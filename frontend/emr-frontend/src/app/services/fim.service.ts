// src/app/services/fim.service.ts
@Injectable({ providedIn: 'root' })
export class FimService {
  private api = 'http://localhost:5000/api/security/fim';
  constructor(private http: HttpClient) {}
  getStatus(): Observable<FileIntegrityRecord[]> { return this.http.get<FileIntegrityRecord[]>(`${this.api}/status`); }
  createBaseline(): Observable<any> { return this.http.post(`${this.api}/baseline`, {}); }
  scan(): Observable<FileIntegrityRecord[]> { return this.http.post<FileIntegrityRecord[]>(`${this.api}/scan`, {}); }
  acknowledge(id: number): Observable<any> { return this.http.post(`${this.api}/acknowledge/${id}`, {}); }
}