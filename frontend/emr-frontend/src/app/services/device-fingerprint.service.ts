// device-fingerprint.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

declare const FingerprintJS: any;

export interface DeviceFingerprint {
  userAgent: string;
  screenResolution: string;
  timezone: string;
  language: string;
  platform: string;
  cookiesEnabled: boolean;
  plugins: string[];
  canvasFingerprint: string;
  webGLFingerprint: string;
  audioFingerprint: string;
  fonts: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DeviceFingerprintService {
  private apiUrl = 'http://localhost:5000/api/device-fingerprint';

  constructor(private http: HttpClient) {}

  async generateFingerprint(): Promise<DeviceFingerprint> {
    const fingerprint: DeviceFingerprint = {
      userAgent: navigator.userAgent,
      screenResolution: `${screen.width}x${screen.height}`,
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      language: navigator.language,
      platform: navigator.platform,
      cookiesEnabled: navigator.cookieEnabled,
      plugins: this.getPlugins(),
      canvasFingerprint: await this.getCanvasFingerprint(),
      webGLFingerprint: this.getWebGLFingerprint(),
      audioFingerprint: await this.getAudioFingerprint(),
      fonts: await this.detectFonts()
    };

    return fingerprint;
  }

  sendFingerprint(fingerprint: DeviceFingerprint): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit`, fingerprint);
  }

  private getPlugins(): string[] {
    const plugins: string[] = [];
    for (let i = 0; i < navigator.plugins.length; i++) {
      plugins.push(navigator.plugins[i].name);
    }
    return plugins;
  }

  private async getCanvasFingerprint(): Promise<string> {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    if (!ctx) return '';

    const text = 'EMR System 2024';
    ctx.textBaseline = 'top';
    ctx.font = '14px Arial';
    ctx.textBaseline = 'alphabetic';
    ctx.fillStyle = '#f60';
    ctx.fillRect(125, 1, 62, 20);
    ctx.fillStyle = '#069';
    ctx.fillText(text, 2, 15);
    ctx.fillStyle = 'rgba(102, 204, 0, 0.7)';
    ctx.fillText(text, 4, 17);

    return canvas.toDataURL();
  }

  private getWebGLFingerprint(): string {
    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl') as WebGLRenderingContext;
    
    if (!gl) return '';

    const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
    if (!debugInfo) return '';

    const vendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL);
    const renderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);

    return `${vendor}~${renderer}`;
  }

  private async getAudioFingerprint(): Promise<string> {
    try {
      const AudioContext = (window as any).AudioContext || (window as any).webkitAudioContext;
      const context = new AudioContext();
      const oscillator = context.createOscillator();
      const analyser = context.createAnalyser();
      const gainNode = context.createGain();
      const scriptProcessor = context.createScriptProcessor(4096, 1, 1);

      gainNode.gain.value = 0;
      oscillator.type = 'triangle';
      oscillator.connect(analyser);
      analyser.connect(scriptProcessor);
      scriptProcessor.connect(gainNode);
      gainNode.connect(context.destination);

      oscillator.start(0);

      return new Promise((resolve) => {
        scriptProcessor.addEventListener('audioprocess', (event) => {
          const output = event.outputBuffer.getChannelData(0);
          const fingerprint = output.slice(0, 30).join('');
          oscillator.stop();
          context.close();
          resolve(fingerprint);
        });
      });
    } catch (e) {
      return '';
    }
  }

  private async detectFonts(): Promise<string[]> {
    const baseFonts = ['monospace', 'sans-serif', 'serif'];
    const testFonts = [
      'Arial', 'Verdana', 'Times New Roman', 'Courier New',
      'Georgia', 'Palatino', 'Garamond', 'Bookman',
      'Comic Sans MS', 'Trebuchet MS', 'Impact'
    ];

    const detected: string[] = [];
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    if (!ctx) return detected;

    const testString = 'mmmmmmmmmmlli';
    const testSize = '72px';

    const baseWidths: { [key: string]: number } = {};
    
    // Get baseline widths
    for (const baseFont of baseFonts) {
      ctx.font = `${testSize} ${baseFont}`;
      baseWidths[baseFont] = ctx.measureText(testString).width;
    }

    // Test each font
    for (const font of testFonts) {
      let detected_font = false;
      
      for (const baseFont of baseFonts) {
        ctx.font = `${testSize} ${font}, ${baseFont}`;
        const width = ctx.measureText(testString).width;
        
        if (width !== baseWidths[baseFont]) {
          detected_font = true;
          break;
        }
      }
      
      if (detected_font) {
        detected.push(font);
      }
    }

    return detected;
  }
}