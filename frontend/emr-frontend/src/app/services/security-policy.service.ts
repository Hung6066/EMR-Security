@Injectable({ providedIn: 'root' })
export class SecurityPolicyService {
  private api = 'http://localhost:5000/api/security/password-policy';
  constructor(private http: HttpClient) {}
  getPolicy() { return this.http.get<PasswordPolicy>(this.api); }
  updatePolicy(p: PasswordPolicy) { return this.http.put<PasswordPolicy>(this.api, p); }
}
export interface PasswordPolicy {
  minLength: number; requireLowercase: boolean; requireUppercase: boolean;
  requireDigit: boolean; requireNonAlphanumeric: boolean; requiredUniqueChars: number;
  expireDays: number; passwordHistory: number; checkCommonPasswords: boolean;
  checkPwnedPasswords: boolean; maxFailedAccessAttempts: number; lockoutMinutes: number;
}