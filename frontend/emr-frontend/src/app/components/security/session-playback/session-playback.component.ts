// src/app/components/security/session-playback/session-playback.component.ts
import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import 'rrweb-player/dist/style.css'; // Import styles

interface SessionRecording {
  id: number;
  userId: number;
  userName: string;
  startedAt: Date;
  endedAt?: Date;
  sizeBytes: number;
}

@Component({
  selector: 'app-session-playback',
  templateUrl: './session-playback.component.html',
  styleUrls: ['./session-playback.component.css']
})
export class SessionPlaybackComponent implements OnInit, OnDestroy {
  @ViewChild('playerTarget') playerTarget!: ElementRef;
  
  recordings: SessionRecording[] = [];
  selectedRecording: SessionRecording | null = null;
  player: any = null; // rrweb-player instance
  loading = false;
  
  displayedColumns = ['userName', 'startedAt', 'duration', 'size', 'actions'];

  constructor(
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadRecordings();
  }

  ngOnDestroy(): void {
    if (this.player) {
      this.player.pause();
    }
  }

  loadRecordings(): void {
    this.http.get<SessionRecording[]>('http://localhost:5000/api/sessionrecordings').subscribe(
      data => {
        this.recordings = data;
      },
      error => console.error('Error loading recordings:', error)
    );
  }

  async playSession(recording: SessionRecording): Promise<void> {
    this.selectedRecording = recording;
    this.loading = true;
    
    // Cleanup old player
    if (this.player) {
      this.player.pause();
      this.playerTarget.nativeElement.innerHTML = '';
    }

    try {
      const events = await this.http.get<any[]>(`http://localhost:5000/api/sessionrecordings/${recording.id}`).toPromise();
      
      const { default: rrwebPlayer } = await import('rrweb-player');

      this.player = new rrwebPlayer({
        target: this.playerTarget.nativeElement,
        props: {
          events,
          showController: true,
          skipInactive: true,
          unpackFn: (event: any) => event,
          UNSAFE_replay_globals: true, // Be careful with this in production
        },
      });

      this.loading = false;
    } catch (error) {
      this.loading = false;
      this.snackBar.open('Lỗi khi tải phiên ghi', 'Đóng', { duration: 3000 });
      console.error('Error playing session:', error);
    }
  }

  getDuration(start: Date, end?: Date): string {
    if (!end) return 'In progress';
    const durationMs = new Date(end).getTime() - new Date(start).getTime();
    const minutes = Math.floor(durationMs / 60000);
    const seconds = ((durationMs % 60000) / 1000).toFixed(0);
    return `${minutes}:${seconds.padStart(2, '0')}`;
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}