// behavioral-analytics.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

interface KeystrokeData {
  key: string;
  pressTime: number;
  releaseTime: number;
  flightTime: number;
}

interface MouseData {
  x: number;
  y: number;
  timestamp: number;
}

@Injectable({
  providedIn: 'root'
})
export class BehavioralAnalyticsService {
  private apiUrl = 'http://localhost:5000/api/behavioral-analytics';
  
  private keystrokes: KeystrokeData[] = [];
  private mouseMovements: MouseData[] = [];
  private lastKeyPressTime = 0;
  private dataSubject = new Subject<void>();

  constructor(private http: HttpClient) {
    this.dataSubject.pipe(
      debounceTime(30000) // Send data every 30 seconds
    ).subscribe(() => {
      this.sendBehavioralData();
    });
  }

  startTracking(): void {
    this.trackKeystrokes();
    this.trackMouseMovements();
    this.trackTouchEvents();
  }

  stopTracking(): void {
    // Clean up event listeners
    document.removeEventListener('keydown', this.handleKeyDown);
    document.removeEventListener('keyup', this.handleKeyUp);
    document.removeEventListener('mousemove', this.handleMouseMove);
  }

  private trackKeystrokes(): void {
    let keyDownTime: { [key: string]: number } = {};

    const handleKeyDown = (event: KeyboardEvent) => {
      const now = Date.now();
      keyDownTime[event.key] = now;
    };

    const handleKeyUp = (event: KeyboardEvent) => {
      const now = Date.now();
      const pressTime = keyDownTime[event.key];
      
      if (pressTime) {
        const flightTime = this.lastKeyPressTime > 0 ? pressTime - this.lastKeyPressTime : 0;
        
        this.keystrokes.push({
          key: event.key.length === 1 ? event.key : '[special]',
          pressTime: pressTime,
          releaseTime: now,
          flightTime: flightTime
        });

        this.lastKeyPressTime = now;
        delete keyDownTime[event.key];

        // Limit stored data
        if (this.keystrokes.length > 100) {
          this.keystrokes.shift();
        }

        this.dataSubject.next();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('keyup', handleKeyUp);
  }

  private trackMouseMovements(): void {
    const handleMouseMove = (event: MouseEvent) => {
      this.mouseMovements.push({
        x: event.clientX,
        y: event.clientY,
        timestamp: Date.now()
      });

      // Limit stored data
      if (this.mouseMovements.length > 200) {
        this.mouseMovements.shift();
      }

      this.dataSubject.next();
    };

    // Throttle mouse events
    let throttleTimeout: any;
    document.addEventListener('mousemove', (event) => {
      if (!throttleTimeout) {
        handleMouseMove(event);
        throttleTimeout = setTimeout(() => {
          throttleTimeout = null;
        }, 100);
      }
    });
  }

  private trackTouchEvents(): void {
    document.addEventListener('touchmove', (event) => {
      const touch = event.touches[0];
      this.mouseMovements.push({
        x: touch.clientX,
        y: touch.clientY,
        timestamp: Date.now()
      });

      if (this.mouseMovements.length > 200) {
        this.mouseMovements.shift();
      }

      this.dataSubject.next();
    });
  }

  private sendBehavioralData(): void {
    if (this.keystrokes.length === 0 && this.mouseMovements.length === 0) {
      return;
    }

    const data = {
      keystrokes: this.keystrokes,
      mouseMovements: this.mouseMovements,
      timestamp: new Date()
    };

    this.http.post(`${this.apiUrl}/record`, data).subscribe(
      () => {
        // Clear sent data
        this.keystrokes = [];
        this.mouseMovements = [];
      },
      error => console.error('Error sending behavioral data:', error)
    );
  }

  getTypingSpeed(): number {
    if (this.keystrokes.length < 2) return 0;

    const timeSpan = this.keystrokes[this.keystrokes.length - 1].releaseTime - 
                     this.keystrokes[0].pressTime;
    
    return (this.keystrokes.length / timeSpan) * 60000; // Keys per minute
  }

  getAverageFlightTime(): number {
    if (this.keystrokes.length === 0) return 0;

    const total = this.keystrokes.reduce((sum, k) => sum + k.flightTime, 0);
    return total / this.keystrokes.length;
  }
}