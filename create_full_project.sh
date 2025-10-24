#!/usr/bin/env bash

# Script này sẽ tạo toàn bộ cấu trúc thư mục và các file trống
# cho dự án EMR System (.NET 9 Backend + Angular Frontend).

set -e

echo "🚀 Bắt đầu tạo toàn bộ cấu trúc dự án EMR System..."

# --- Hàm trợ giúp ---
function create_file() {
    mkdir -p "$(dirname "$1")" && touch "$1"
}

# --- Cấu trúc thư mục Backend ---
echo "-> Tạo cấu trúc thư mục Backend..."
mkdir -p backend/EMRSystem.API/{Authentication,Controllers,Middleware,Security}
mkdir -p backend/EMRSystem.Application/{DTOs,Interfaces,Mappings,Services}
mkdir -p backend/EMRSystem.Core/Entities/{Main,Security}
mkdir -p backend/EMRSystem.Core/Settings
mkdir -p backend/EMRSystem.Infrastructure/{Data,Interceptors,Repositories}

# --- Cấu trúc thư mục Frontend ---
echo "-> Tạo cấu trúc thư mục Frontend..."
mkdir -p frontend/emr-frontend/src/app/components/appointments/dialogs
mkdir -p frontend/emr-frontend/src/app/components/auth
mkdir -p frontend/emr-frontend/src/app/components/blockchain/dialogs
mkdir -p frontend/emr-frontend/src/app/components/dashboard
mkdir -p frontend/emr-frontend/src/app/components/medical-records
mkdir -p frontend/emr-frontend/src/app/components/patients
mkdir -p frontend/emr-frontend/src/app/components/reports
mkdir -p frontend/emr-frontend/src/app/components/security/anomaly-detection
mkdir -p frontend/emr-frontend/src/app/components/security/api-key-management/dialogs
mkdir -p frontend/emr-frontend/src/app/components/security/compliance-dashboard
mkdir -p frontend/emr-frontend/src/app/components/security/encryption-dashboard/dialogs
mkdir -p frontend/emr-frontend/src/app/components/security/password-policy
mkdir -p frontend/emr-frontend/src/app/components/security/privacy-tools
mkdir -p frontend/emr-frontend/src/app/components/security/security-dashboard
mkdir -p frontend/emr-frontend/src/app/components/security/security-incidents/dialogs
mkdir -p frontend/emr-frontend/src/app/components/security/session-playback
mkdir -p frontend/emr-frontend/src/app/components/security/threat-hunting
mkdir -p frontend/emr-frontend/src/app/components/security/trust-score
mkdir -p frontend/emr-frontend/src/app/components/security/webauthn-setup
mkdir -p frontend/emr-frontend/src/app/components/shared/dialogs
mkdir -p frontend/emr-frontend/src/app/components/shared/layout
mkdir -p frontend/emr-frontend/src/app/components/shared/tagging
mkdir -p frontend/emr-frontend/src/app/guards
mkdir -p frontend/emr-frontend/src/app/interceptors
mkdir -p frontend/emr-frontend/src/app/models
mkdir -p frontend/emr-frontend/src/app/services
mkdir -p frontend/emr-frontend/src/assets/icons
mkdir -p frontend/emr-frontend/src/environments

# --- Cấu trúc thư mục gốc ---
mkdir -p .github/workflows
mkdir -p scripts

# --- Tạo các file trống ---
echo "-> Tạo các file trống..."

# Root
create_file README.md
create_file .gitignore
create_file .github/workflows/release.yml
create_file scripts/pack.sh
create_file scripts/pack.ps1

# --- Backend Files ---

# Solution
create_file backend/EMRSystem.sln

# EMRSystem.API
create_file backend/EMRSystem.API/appsettings.Development.json
create_file backend/EMRSystem.API/appsettings.json
create_file backend/EMRSystem.API/Program.cs
create_file backend/EMRSystem.API/EMRSystem.API.csproj
create_file backend/EMRSystem.API/Authentication/ApiKeyAuthenticationHandler.cs
create_file backend/EMRSystem.API/Authentication/HangfireAuthorizationFilter.cs
create_file backend/EMRSystem.API/Controllers/AdvancedReportsController.cs
create_file backend/EMRSystem.API/Controllers/AnomalyDetectionController.cs
create_file backend/EMRSystem.API/Controllers/ApiKeysController.cs
create_file backend/EMRSystem.API/Controllers/AppointmentsController.cs
create_file backend/EMRSystem.API/Controllers/AuthController.cs
create_file backend/EMRSystem.API/Controllers/BlockchainController.cs
create_file backend/EMRSystem.API/Controllers/ClassificationController.cs
create_file backend/EMRSystem.API/Controllers/ComplianceController.cs
create_file backend/EMRSystem.API/Controllers/MedicalDocumentsController.cs
create_file backend/EMRSystem.API/Controllers/MedicalRecordsController.cs
create_file backend/EMRSystem.API/Controllers/PackagingController.cs
create_file backend/EMRSystem.API/Controllers/PasswordPolicyController.cs
create_file backend/EMRSystem.API/Controllers/PatientsController.cs
create_file backend/EMRSystem.API/Controllers/PrivacyController.cs
create_file backend/EMRSystem.API/Controllers/ReportsController.cs
create_file backend/EMRSystem.API/Controllers/SecurityController.cs
create_file backend/EMRSystem.API/Controllers/SecurityIncidentsController.cs
create_file backend/EMRSystem.API/Controllers/SessionRecordingsController.cs
create_file backend/EMRSystem.API/Controllers/ThreatHuntingController.cs
create_file backend/EMRSystem.API/Controllers/TwoFactorController.cs
create_file backend/EMRSystem.API/Controllers/WebAuthnController.cs
create_file backend/EMRSystem.API/Middleware/GeoBlockingMiddleware.cs
create_file backend/EMRSystem.API/Middleware/SecurityHeadersMiddleware.cs
create_file backend/EMRSystem.API/Middleware/ZeroTrustMiddleware.cs
create_file backend/EMRSystem.API/Security/CustomPasswordValidator.cs

# EMRSystem.Application
create_file backend/EMRSystem.Application/EMRSystem.Application.csproj
create_file backend/EMRSystem.Application/Mappings/MappingProfile.cs

# EMRSystem.Core
create_file backend/EMRSystem.Core/EMRSystem.Core.csproj
create_file backend/EMRSystem.Core/Entities/Main/Appointment.cs
create_file backend/EMRSystem.Core/Entities/Main/Doctor.cs
create_file backend/EMRSystem.Core/Entities/Main/LabTest.cs
create_file backend/EMRSystem.Core/Entities/Main/MedicalDocument.cs
create_file backend/EMRSystem.Core/Entities/Main/MedicalRecord.cs
create_file backend/EMRSystem.Core/Entities/Main/Patient.cs
create_file backend/EMRSystem.Core/Entities/Main/Prescription.cs
create_file backend/EMRSystem.Core/Entities/Security/ApiKey.cs
create_file backend/EMRSystem.Core/Entities/Security/ApplicationUser.cs
create_file backend/EMRSystem.Core/Entities/Security/AuditLog.cs
create_file backend/EMRSystem.Core/Entities/Security/Blockchain.cs
create_file backend/EMRSystem.Core/Entities/Security/Classification.cs
create_file backend/EMRSystem.Core/Entities/Security/Compliance.cs
create_file backend/EMRSystem.Core/Entities/Security/DeviceFingerprint.cs
create_file backend/EMRSystem.Core/Entities/Security/EncryptionKey.cs
create_file backend/EMRSystem.Core/Entities/Security/LoginAttempt.cs
create_file backend/EMRSystem.Core/Entities/Security/PasswordPolicy.cs
create_file backend/EMRSystem.Core/Entities/Security/RefreshToken.cs
create_file backend/EMRSystem.Core/Entities/Security/SecureVault.cs
create_file backend/EMRSystem.Core/Entities/Security/SecurityIncident.cs
create_file backend/EMRSystem.Core/Entities/Security/SessionRecording.cs
create_file backend/EMRSystem.Core/Entities/Security/ThreatHunting.cs
create_file backend/EMRSystem.Core/Entities/Security/ThreatIntelligence.cs
create_file backend/EMRSystem.Core/Entities/Security/TrustScore.cs
create_file backend/EMRSystem.Core/Entities/Security/TwoFactorAuth.cs
create_file backend/EMRSystem.Core/Entities/Security/UserSession.cs
create_file backend/EMRSystem.Core/Entities/Security/WebAuthnCredential.cs
create_file backend/EMRSystem.Core/Entities/Security/ZeroTrustPolicy.cs
create_file backend/EMRSystem.Core/Settings/EmailSettings.cs
create_file backend/EMRSystem.Core/Settings/JwtSettings.cs

# EMRSystem.Infrastructure
create_file backend/EMRSystem.Infrastructure/EMRSystem.Infrastructure.csproj
create_file backend/EMRSystem.Infrastructure/Data/ApplicationDbContext.cs
create_file backend/EMRSystem.Infrastructure/Data/SeedData.cs
create_file backend/EMRSystem.Infrastructure/Interceptors/AuditInterceptor.cs
create_file backend/EMRSystem.Infrastructure/Interceptors/EncryptionInterceptor.cs
create_file backend/EMRSystem.Infrastructure/Repositories/PatientRepository.cs
create_file backend/EMRSystem.Infrastructure/Repositories/MedicalRecordRepository.cs
create_file backend/EMRSystem.Infrastructure/Repositories/Repository.cs

# --- Frontend Files ---

# Root Frontend Files
create_file frontend/emr-frontend/angular.json
create_file frontend/emr-frontend/package.json
create_file frontend/emr-frontend/tsconfig.json
create_file frontend/emr-frontend/tsconfig.app.json
create_file frontend/emr-frontend/tsconfig.spec.json
create_file frontend/emr-frontend/README.md

# src/
create_file frontend/emr-frontend/src/index.html
create_file frontend/emr-frontend/src/main.ts
create_file frontend/emr-frontend/src/styles.css
create_file frontend/emr-frontend/src/favicon.ico

# src/app/
create_file frontend/emr-frontend/src/app/app-routing.module.ts
create_file frontend/emr-frontend/src/app/app.component.css
create_file frontend/emr-frontend/src/app/app.component.html
create_file frontend/emr-frontend/src/app/app.component.ts
create_file frontend/emr-frontend/src/app/app.module.ts
create_file frontend/emr-frontend/src/app/guards/auth.guard.ts
create_file frontend/emr-frontend/src/app/guards/role.guard.ts
create_file frontend/emr-frontend/src/app/interceptors/auth.interceptor.ts
create_file frontend/emr-frontend/src/app/interceptors/error.interceptor.ts

# src/app/services/
create_file frontend/emr-frontend/src/app/services/advanced-reports.service.ts
create_file frontend/emr-frontend/src/app/services/anomaly-detection.service.ts
create_file frontend/emr-frontend/src/app/services/api-key.service.ts
create_file frontend/emr-frontend/src/app/services/appointment.service.ts
create_file frontend/emr-frontend/src/app/services/auth.service.ts
create_file frontend/emr-frontend/src/app/services/behavioral-analytics.service.ts
create_file frontend/emr-frontend/src/app/services/blockchain.service.ts
create_file frontend/emr-frontend/src/app/services/classification.service.ts
create_file frontend/emr-frontend/src/app/services/compliance.service.ts
create_file frontend/emr-frontend/src/app/services/device-fingerprint.service.ts
create_file frontend/emr-frontend/src/app/services/encryption.service.ts
create_file frontend/emr-frontend/src/app/services/file-upload.service.ts
create_file frontend/emr-frontend/src/app/services/medical-record.service.ts
create_file frontend/emr-frontend/src/app/services/notification.service.ts
create_file frontend/emr-frontend/src/app/services/password-policy.service.ts
create_file frontend/emr-frontend/src/app/services/patient.service.ts
create_file frontend/emr-frontend/src/app/services/privacy.service.ts
create_file frontend/emr-frontend/src/app/services/report.service.ts
create_file frontend/emr-frontend/src/app/services/security-incident.service.ts
create_file frontend/emr-frontend/src/app/services/security-policy.service.ts
create_file frontend/emr-frontend/src/app/services/session-recording.service.ts
create_file frontend/emr-frontend/src/app/services/threat-hunting.service.ts
create_file frontend/emr-frontend/src/app/services/threat-intelligence.service.ts
create_file frontend/emr-frontend/src/app/services/two-factor.service.ts
create_file frontend/emr-frontend/src/app/services/webauthn.service.ts
create_file frontend/emr-frontend/src/app/services/zero-trust.service.ts

# src/app/models/ (Các file models)
# ... Bạn có thể tạo file `_models.ts` để chứa tất cả hoặc tạo từng file riêng

# src/app/components/ (Tạo các file .ts, .html, .css cho mỗi component)
# Ví dụ cho một component:
create_file frontend/emr-frontend/src/app/components/dashboard/dashboard.component.ts
create_file frontend/emr-frontend/src/app/components/dashboard/dashboard.component.html
create_file frontend/emr-frontend/src/app/components/dashboard/dashboard.component.css

# (Tương tự cho các component khác... Script này sẽ dài nếu liệt kê tất cả)
# Thay vào đó, chúng ta đã tạo thư mục, bạn có thể dùng `ng generate component`
# hoặc tạo tay các file .ts, .html, .css trong các thư mục đã có.

# src/environments
create_file frontend/emr-frontend/src/environments/environment.ts
create_file frontend/emr-frontend/src/environments/environment.prod.ts

echo "✅ Hoàn thành! Cấu trúc thư mục và các file trống đã được tạo."
echo "➡️  Bước tiếp theo:"
echo "1. Mở dự án trong IDE của bạn (VS Code, Visual Studio, Rider)."
echo "2. Sao chép nội dung code từ các tin nhắn trước vào các file tương ứng."
echo "3. Chạy 'npm install' trong thư mục 'frontend/emr-frontend'."
echo "4. Chạy 'dotnet restore' trong thư mục 'backend'."
echo "5. Bắt đầu phát triển!"
