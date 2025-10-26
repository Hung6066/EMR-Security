import * as rrweb from 'rrweb';
@Injectable({ providedIn: 'root' })
export class SessionRecordingService {
  private stopFn: (() => void) | null = null;
  private sessionId: string | null = null;
  private buffer: rrweb.eventWithTime[] = [];
  private timer?: any;

  constructor(private http: HttpClient) {}

  start(): void {
    if (this.stopFn) return;
    this.http.post<{sessionId:string}>('http://localhost:5000/api/session-recordings/start', {})
      .subscribe(res => { this.sessionId = res.sessionId; });

    this.stopFn = rrweb.record({
      emit: (e) => {
        this.buffer.push(e);
        if (this.buffer.length > 200) this.flush();
      },
      sampling: { mousemove: 50, scroll: 100 },
      maskAllInputs: true,
      blockClass: 'rr-mask' // thêm class này vào các vùng chứa PII
    });

    this.timer = setInterval(() => this.flush(), 3000);
  }

  stop(): void {
    if (this.stopFn) { this.stopFn(); this.stopFn = null; }
    if (this.timer) { clearInterval(this.timer); }
    this.flush(true);
    this.http.post('http://localhost:5000/api/session-recordings/stop', { sessionId: this.sessionId }).subscribe();
    this.sessionId = null;
  }

  flush(final = false): void {
    if (!this.sessionId || this.buffer.length === 0) return;
    const chunk = this.buffer.splice(0, this.buffer.length);
    this.http.post('http://localhost:5000/api/session-recordings/chunk', {
      sessionId: this.sessionId, events: chunk
    }).subscribe();
    if (final) this.buffer = [];
  }
}