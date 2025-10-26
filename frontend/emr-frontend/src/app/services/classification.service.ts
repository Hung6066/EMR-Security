@Injectable({ providedIn: 'root' })
export class ClassificationService {
  private api = 'http://localhost:5000/api/classification';
  constructor(private http: HttpClient) {}
  getLabels() { return this.http.get<Label[]>(`${this.api}/labels`); }
  upsertLabel(l: Label) { return this.http.post<Label>(`${this.api}/labels`, l); }
  assign(type: string, id: number, labelId: number, reason?: string) {
    return this.http.post(`${this.api}/assign?type=${type}&id=${id}&labelId=${labelId}&reason=${encodeURIComponent(reason||'')}`, {});
  }
  getTags(type: string, id: number) { return this.http.get<Tag[]>(`${this.api}/tags?type=${type}&id=${id}`); }
  addTag(type: string, id: number, tag: string) { return this.http.post(`${this.api}/tags?type=${type}&id=${id}&tag=${encodeURIComponent(tag)}`, {}); }
}
export interface Label { id: number; name: string; level: number; color: string; isActive: boolean; }
export interface Tag { id: number; resourceType: string; resourceId: number; tag: string; taggedAt: Date; }