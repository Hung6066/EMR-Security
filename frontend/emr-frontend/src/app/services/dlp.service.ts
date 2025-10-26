// src/app/services/dlp.service.ts
@Injectable({ providedIn: 'root' })
export class DlpService {
  private api = 'http://localhost:5000/api/security/dlp';
  constructor(private http: HttpClient) {}
  getRules(): Observable<DlpRule[]> { return this.http.get<DlpRule[]>(`${this.api}/rules`); }
  upsertRule(rule: DlpRule): Observable<DlpRule> { return this.http.post<DlpRule>(`${this.api}/rules`, rule); }
  getIncidents(from: string, to: string): Observable<DlpIncident[]> {
    return this.http.get<DlpIncident[]>(`${this.api}/incidents?f=${from}&t=${to}`);
  }
}
// (Định nghĩa interface DlpRule, DlpIncident tương ứng)


// (Định nghĩa interface FileIntegrityRecord)

// src/app/services/vulnerability.service.ts

// (Định nghĩa interface Vulnerability)