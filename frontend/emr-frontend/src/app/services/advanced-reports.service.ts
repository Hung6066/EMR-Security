@Injectable({ providedIn: 'root' })
export class AdvancedReportsService {
  private api='http://localhost:5000/api/advanced-reports';
  constructor(private http: HttpClient) {}
  export(type: 'Security'|'Usage'|'Clinical', from: string, to: string, format='PDF') {
    return this.http.post<ReportArchive>(`${this.api}/export?type=${type}&from=${from}&to=${to}&format=${format}`, {});
  }
  archive() { return this.http.get<ReportArchive[]>(`${this.api}/archive`); }
  download(id: number) { return this.http.get(`${this.api}/archive/${id}/download`, { responseType: 'blob' }); }
}
export interface ReportArchive {
  id: number; type: string; title: string; format: string; filePath: string; generatedAt: string;
}