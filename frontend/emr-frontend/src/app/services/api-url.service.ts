import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiUrlService {
  // Key bí mật này PHẢI giống với key trong appsettings.json của backend
  private readonly routingSecret = "YourSuperSecretKeyForMTDRoutingMustMatchBackend";

  constructor() { }

  /**
   * Tạo URL động cho một resource.
   * @param resource Tên resource, ví dụ: 'patients', 'medicalrecords'
   */
  public getUrl(resource: string): string {
    const dynamicPart = this.generateDynamicPart(resource);
    return `http://localhost:5000/api/rt-${dynamicPart}/${resource}`;
  }

  private async generateDynamicPart(resource: string): Promise<string> {
    const timeFactor = new Date().toISOString().split('T')[0]; // yyyy-MM-dd
    const dataToHash = `${resource}:${timeFactor}:${this.routingSecret}`;
    
    const hashBuffer = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(dataToHash));
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    
    return hashHex.substring(0, 4);
  }
}