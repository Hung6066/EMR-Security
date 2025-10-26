# Tài liệu Kiến trúc Bảo mật Hệ thống Bệnh án Điện tử (EMR)

**Phiên bản:** 2.0 (Security Hardened)  
**Ngày cập nhật:** 24/05/2024
**Đối tượng:** Kiến trúc sư, Lập trình viên, Quản trị viên hệ thống, Chuyên viên An ninh thông tin.

---

## 1. Giới thiệu và Triết lý Bảo mật

### 1.1. Mục tiêu
Tài liệu này mô tả kiến trúc bảo mật đa lớp (Defense-in-Depth) được thiết kế và triển khai cho hệ thống EMR. Mục tiêu là xây dựng một hệ thống không chỉ mạnh mẽ về mặt chức năng mà còn đạt và vượt các tiêu chuẩn bảo mật quốc tế nghiêm ngặt nhất trong ngành y tế (như HIPAA), đảm bảo tính **Bí mật (Confidentiality)**, **Toàn vẹn (Integrity)**, và **Sẵn sàng (Availability)** của dữ liệu bệnh nhân.

### 1.2. Triết lý Bảo mật
Kiến trúc được xây dựng dựa trên các nguyên tắc cốt lõi:
-   **Defense-in-Depth:** Không tin tưởng vào một lớp bảo vệ duy nhất. Hệ thống được xây dựng với nhiều lớp phòng thủ độc lập, nếu một lớp bị xuyên thủng, các lớp khác sẽ ngăn chặn cuộc tấn công.
-   **Zero Trust (Không tin cậy bất kỳ ai):** Mọi yêu cầu, dù từ bên trong hay bên ngoài mạng, đều phải được xác thực, ủy quyền và kiểm tra trước khi được cấp quyền truy cập. "Never trust, always verify."
-   **Principle of Least Privilege (Nguyên tắc Đặc quyền Tối thiểu):** Mỗi người dùng và dịch vụ chỉ được cấp quyền tối thiểu cần thiết để thực hiện công việc của mình.
-   **Secure by Design & Default:** Bảo mật được tích hợp ngay từ khâu thiết kế, không phải là một tính năng "thêm vào sau". Các cài đặt mặc định luôn là cài đặt an toàn nhất.
-   **Automation & Orchestration:** Tự động hóa việc phát hiện, phản ứng và khắc phục sự cố để giảm thiểu thời gian phản ứng và sai sót của con người.

---

## 2. Các Lớp Bảo mật (Security Layers)

Hệ thống được bảo vệ bởi các lớp tuần tự, từ ngoài vào trong và từ tầng hạ tầng lên tầng ứng dụng.

#### **Lớp 1: Kiểm soát Truy cập & Nhận dạng (Identity & Access Management)**

Lớp này là cửa ngõ đầu tiên, đảm bảo "đúng người" được vào hệ thống.

1.  **Xác thực Mạnh & Đa yếu tố (MFA):**
    -   **Cách vận hành:** Ngoài mật khẩu, người dùng bắt buộc phải cung cấp một yếu tố xác thực thứ hai. Hệ thống hỗ trợ:
        -   **TOTP (Time-based One-Time Password):** Dùng ứng dụng Authenticator (Google, Microsoft, Authy) để quét mã QR và nhận mã 6 số thay đổi mỗi 30 giây.
        -   **WebAuthn/FIDO2 (Sinh trắc học):** Cho phép đăng nhập không mật khẩu bằng vân tay, khuôn mặt, hoặc khóa bảo mật vật lý (YubiKey). Đây là tiêu chuẩn chống phishing mạnh nhất.
    -   **Thành phần:** `ITwoFactorService`, `IWebAuthnService`, các controller và component tương ứng.

2.  **Quản lý Chính sách Mật khẩu Động (Password Policy Management):**
    -   **Cách vận hành:** Admin có thể cấu hình động các yêu cầu về mật khẩu mà không cần deploy lại code.
        -   **Độ phức tạp:** Độ dài, chữ hoa/thường, số, ký tự đặc biệt.
        -   **Lịch sử:** Ngăn người dùng sử dụng lại N mật khẩu cũ.
        -   **Hết hạn:** Bắt buộc đổi mật khẩu sau X ngày.
        -   **Kiểm tra lộ lọt:** Tích hợp API của "Have I Been Pwned" để tự động từ chối các mật khẩu đã từng xuất hiện trong các vụ rò rỉ dữ liệu lớn.
    -   **Thành phần:** `PasswordPolicyController`, `IPasswordPolicyService`, `CustomPasswordValidator`.

3.  **Quản lý Phiên & Thiết bị Tin cậy (Session & Trusted Device Management):**
    -   **Cách vận hành:** Mỗi lần đăng nhập thành công, một "phiên" được tạo và gắn với "dấu vân tay thiết bị" (`Device Fingerprint`).
        -   Hệ thống ghi nhận các thiết bị người dùng thường xuyên sử dụng.
        -   Người dùng có thể xem danh sách các thiết bị đang đăng nhập và đăng xuất từ xa khỏi một thiết bị đáng ngờ.
        -   Đăng nhập từ một thiết bị hoàn toàn mới có thể kích hoạt các yêu cầu xác thực bổ sung.
    -   **Thành phần:** `SecurityController`, `IDeviceFingerprintService`, `TrustedDevicesComponent`.

4.  **Phân quyền Chi tiết (Granular Authorization):**
    -   **Cách vận hành:** Sử dụng cơ chế Role-Based Access Control (RBAC) kết hợp với các `Claim` và `Policy` của .NET.
        -   **Roles:** Admin, Security, Doctor, Nurse, Patient...
        -   **Policies:** Các chính sách phức tạp hơn, ví dụ `[Authorize(Policy = "DoctorOrNurse")]`.
        -   **Permissions (Claims):** Các quyền chi tiết hơn như `permission:ViewFullPII`, `permission:ExportReport` được gán cho các vai trò và kiểm tra trong code.
    -   **Thành phần:** Cấu hình trong `Program.cs`, các attribute `[Authorize]` trên Controller/Action.

#### **Lớp 2: Bảo vệ Ứng dụng & API (Application & API Security)**

Lớp này bảo vệ ứng dụng đang chạy khỏi các cuộc tấn công nhắm vào logic và giao diện.

5.  **Runtime Application Self-Protection (RASP):**
    -   **Cách vận hành:** Một middleware thông minh (`RaspMiddleware`) được đặt ở đầu pipeline request. Nó hoạt động như một "hệ miễn dịch" bên trong ứng dụng.
        -   **Phân tích Real-time:** Tự động phân tích mọi request đến để tìm dấu hiệu của các cuộc tấn công phổ biến như SQL Injection, Cross-Site Scripting (XSS), Path Traversal.
        -   **Tự phản ứng:** Nếu phát hiện mối đe dọa, nó sẽ ngay lập tức **chặn** request, **ghi log** chi tiết, tự động **tạo sự cố** trong hệ thống SOAR, và **chặn IP** của kẻ tấn công mà không cần can thiệp từ bên ngoài.
    -   **Thành phần:** `RaspMiddleware`, tích hợp với `ISecurityIncidentService` và `IThreatIntelligenceService`.

6.  **Moving Target Defense (MTD):**
    -   **Cách vận hành:** Làm cho bề mặt tấn công của hệ thống liên tục thay đổi, gây khó khăn cho việc do thám tự động.
        -   **API Endpoint Rotation:** Các đường dẫn API không cố định. Ví dụ `/api/patients` sẽ trở thành `/api/rt-xxxx/patients`, trong đó `xxxx` là một chuỗi được tạo ra hàng ngày dựa trên một secret key. Frontend sẽ tự tính toán và gọi đến đúng đường dẫn. Bất kỳ request nào đến đường dẫn cũ đều bị từ chối.
        -   **Just-in-Time Credentials:** (Kiến trúc) Ứng dụng không lưu chuỗi kết nối database vĩnh viễn. Thay vào đó, nó yêu cầu một credential tạm thời từ một hệ thống quản lý bí mật (như HashiCorp Vault) khi khởi động. Credential này chỉ có hiệu lực trong thời gian ngắn và được tự động xoay vòng.
    -   **Thành phần:** `MtdMiddleware`, `ApiUrlService` (Angular), tích hợp với Vault.

7.  **Deception Technology (Công nghệ Đánh lừa):**
    -   **Cách vận hành:** Tạo ra các "cạm bẫy" trong hệ thống. Bất kỳ tương tác nào với chúng đều là một cảnh báo an ninh nghiêm trọng.
        -   **Honey Tokens:** Các bản ghi dữ liệu giả (bệnh nhân, bác sĩ, thuốc...) với ID đặc biệt. Logic trong các service sẽ kiểm tra nếu có ai đó truy vấn đến các ID này.
        -   **Honeypots:** Các API endpoint giả, không được sử dụng (`/wp-admin`, `/api/backup/download-all`). Bất kỳ request nào đến đây đều được xác định là hành vi quét lỗ hổng.
    -   **Cách phản ứng:** Khi bẫy được kích hoạt, hệ thống sẽ ngay lập tức tạo một sự cố **Critical**, chặn vĩnh viễn IP kẻ tấn công, và có thể đánh lạc hướng bằng cách trả về dữ liệu giả.
    -   **Thành phần:** `DeceptionService`, `HoneypotController`, logic kiểm tra trong các service nghiệp vụ.

8.  **Security Headers, Rate Limiting & CAPTCHA:**
    -   **Cách vận hành:** Các cơ chế phòng thủ tiêu chuẩn nhưng cực kỳ hiệu quả.
        -   **Security Headers:** Cấu hình CSP, HSTS, X-Frame-Options... để ngăn chặn các cuộc tấn công từ phía trình duyệt (`SecurityHeadersMiddleware`).
        -   **Rate Limiting:** Giới hạn số lượng request từ một IP trong một khoảng thời gian nhất định, chống tấn công Brute Force và DoS (`IpRateLimitingMiddleware`).
        -   **CAPTCHA:** Yêu cầu xác thực "tôi không phải robot" sau nhiều lần đăng nhập thất bại để chặn bot.
    -   **Thành phần:** Middleware tương ứng, tích hợp `ng-recaptcha` ở frontend.

#### **Lớp 3: Bảo vệ Dữ liệu (Data Protection)**

Lớp này tập trung vào việc bảo vệ tài sản quý giá nhất: dữ liệu bệnh nhân.

9.  **Mã hóa Toàn diện (Encryption Everywhere):**
    -   **At-Transit:** Mọi giao tiếp giữa client và server, giữa các microservice đều phải dùng TLS 1.3.
    -   **At-Rest:** Dữ liệu trong CSDL được mã hóa bằng Transparent Data Encryption (TDE) của SQL Server. Các file được mã hóa bằng mã hóa của hệ thống lưu trữ (ví dụ: SSE của S3/Azure Blob).
    -   **In-Use (Confidential Computing):** Các xử lý dữ liệu nhạy cảm nhất (như phân tích AI trên bệnh án) được thực hiện trong một **secure enclave**. Dữ liệu được mã hóa ngay cả khi đang ở trong RAM, chống lại sự xâm nhập từ quản trị viên hệ thống hoặc lỗ hổng phần cứng.
    -   **Application-Level Encryption (ALE):** Các trường dữ liệu cực kỳ nhạy cảm (như số CCCD) được mã hóa ngay tại tầng ứng dụng trước khi lưu vào CSDL, sử dụng `AdvancedEncryptionService`.

10. **Quản lý Khóa & Vòng đời (Key Management & Rotation):**
    -   **Cách vận hành:** Hệ thống tự quản lý vòng đời của các khóa mã hóa.
        -   Sử dụng một **Master Key** được lưu trữ an toàn (trong Azure Key Vault, AWS KMS) để mã hóa các khóa dữ liệu (Data Encryption Keys - DEK).
        -   Các DEK được tạo ra với mục đích và thời gian sống cụ thể.
        -   Hệ thống có cơ chế **xoay vòng khóa (key rotation)** tự động hoặc thủ công. Khi một khóa được xoay vòng, một khóa mới sẽ được tạo ra, và dữ liệu cũ có thể được mã hóa lại bằng khóa mới (chạy nền).
    -   **Thành phần:** `EncryptionDashboardComponent`, `IAdvancedEncryptionService`.

11. **Dynamic Data Masking & Obfuscation:**
    -   **Cách vận hành:** Dữ liệu nhạy cảm được tự động che đi (masking) ở tầng API trước khi trả về cho client, dựa trên quyền của người dùng.
        -   Một `JsonConverter` tùy chỉnh (`DataMaskingConverter`) sẽ kiểm tra quyền `permission:ViewFullPII` của người dùng.
        -   Nếu không có quyền, các trường được đánh dấu `[DataMasking]` trong DTO sẽ bị che (ví dụ: `*********1234`, `n***@***.com`).
    -   **Lợi ích:** Đảm bảo nguyên tắc "need-to-know" và giảm thiểu rủi ro lộ dữ liệu trên giao diện người dùng.

12. **Data Leakage Prevention (DLP) & Watermarking:**
    -   **Cách vận hành:**
        -   **DLP:** Một middleware (`DlpMiddleware`) quét nội dung của tất cả các API response. Nếu phát hiện dữ liệu khớp với các pattern nhạy cảm (số thẻ tín dụng, CCCD...) trong các quy tắc DLP, nó sẽ tự động **chặn** hoặc **che** (redact) dữ liệu đó.
        -   **Watermarking:** Mọi file PDF được xuất ra từ hệ thống (bệnh án, báo cáo) sẽ được tự động đóng một dấu chìm (watermark) chứa thông tin về người truy cập, thời gian, và địa chỉ IP, giúp truy vết nguồn gốc nếu file bị rò rỉ.
    -   **Thành phần:** `DlpMiddleware`, `IDlpService`, cập nhật `IPdfService`.

#### **Lớp 4: Giám sát, Phát hiện & Phản ứng (Monitoring, Detection & Response)**

Lớp này đảm bảo chúng ta có "đôi mắt" để quan sát mọi thứ và khả năng phản ứng khi có sự cố.

13. **Blockchain Audit Trail (Nhật ký Kiểm toán Bất biến):**
    -   **Cách vận hành:** Mọi hành động quan trọng (thay đổi bệnh án, cấp quyền, truy cập dữ liệu nhạy cảm) không chỉ được ghi vào `AuditLog` thông thường mà còn được tạo thành một "giao dịch" và đưa vào một chuỗi blockchain riêng tư.
        -   Mỗi khối được liên kết với khối trước đó bằng hash và được "mine" (Proof of Work) để đảm bảo tính bất biến.
        -   Hệ thống có cơ chế tự động **xác thực toàn bộ chuỗi (Validate Chain)** định kỳ để phát hiện bất kỳ sự giả mạo nào đối với lịch sử kiểm toán.
    -   **Lợi ích:** Tạo ra một bằng chứng không thể chối cãi về mọi thay đổi trong hệ thống, đáp ứng yêu cầu cao nhất về tính toàn vẹn dữ liệu.
    -   **Thành phần:** `IBlockchainService`, `BlockchainExplorerComponent`.

14. **User and Entity Behavior Analytics (UEBA):**
    -   **Cách vận hành:** Đây là phiên bản nâng cao của phát hiện bất thường.
        -   **Học Baseline:** Một background job (Hangfire) định kỳ phân tích `AuditLog` để "học" hành vi bình thường của mỗi người dùng: giờ làm việc quen thuộc, dải IP hay sử dụng, các loại hành động/tài nguyên thường truy cập.
        -   **Phát hiện Sai lệch:** Khi có một hành động mới, hệ thống sẽ so sánh nó với baseline đã học. Nếu có sự sai lệch lớn (ví dụ: một bác sĩ đột nhiên đăng nhập lúc 3 giờ sáng từ một quốc gia lạ và xuất hàng loạt bệnh án), một `UebaAlert` sẽ được tạo ra với "Điểm Sai lệch" (Deviation Score).
    -   **Lợi ích:** Phát hiện hiệu quả các mối đe dọa từ bên trong (insider threats) và các tài khoản bị chiếm đoạt.
    -   **Thành phần:** `IUebaService`, `UebaDashboardComponent`.

15. **Advanced Threat Hunting:**
    -   **Cách vận hành:** Cung cấp cho đội ngũ an ninh một "giao diện săn lùng" để chủ động tìm kiếm các mối đe dọa thay vì chờ cảnh báo.
        -   **Query Builder:** Cho phép tạo các truy vấn phức tạp trên nhiều nguồn dữ liệu (AuditLog, LoginAttempts, ThreatLog). Ví dụ: "Tìm tất cả các user đã đăng nhập thất bại hơn 5 lần từ nhiều hơn 3 quốc gia trong 24 giờ qua".
        -   **IOC Management:** Quản lý danh sách các Chỉ số Tấn công (Indicators of Compromise) như IP, domain, hash file độc hại. Hệ thống sẽ liên tục quét log để tìm sự trùng khớp.
    -   **Thành phần:** `IThreatHuntingService`, `ThreatHuntingComponent`.

16. **SOAR (Security Orchestration, Automation, and Response):**
    -   **Cách vận hành:** Đây là "bộ não" tự động hóa của trung tâm an ninh.
        -   Khi một sự cố được tạo (từ RASP, UEBA, Honeypot...), SOAR sẽ tự động kích hoạt một "playbook" (kịch bản phản ứng).
        -   **Ví dụ Playbook "Tài khoản bị chiếm đoạt":**
            1.  **Enrich:** Tự động lấy thông tin về IP đáng ngờ, kiểm tra lịch sử hành vi của người dùng.
            2.  **Contain:** Tự động **chặn IP**, **đăng xuất tất cả các phiên** của người dùng.
            3.  **Notify:** Gửi cảnh báo đến kênh Slack/Teams của đội an ninh với đầy đủ thông tin đã làm giàu.
            4.  **Eradicate:** Yêu cầu người dùng **đổi mật khẩu** và **xác thực lại MFA** trong lần đăng nhập tiếp theo.
    -   **Thành phần:** `ISoarService` (workflow engine), tích hợp chặt chẽ với tất cả các service bảo mật khác.

#### **Lớp 5: Bảo vệ Hạ tầng & Chuỗi Cung ứng (Infrastructure & Supply Chain Security)**

Lớp này bảo vệ nền tảng mà ứng dụng đang chạy trên đó.

17. **File Integrity Monitoring (FIM):**
    -   **Cách vận hành:** Một background job định kỳ quét tất cả các file quan trọng của ứng dụng (DLLs, `appsettings.json`, file trong `wwwroot`) và so sánh hash của chúng với một "baseline" (bản gốc) đã được lưu trữ.
    -   **Phát hiện:** Bất kỳ sự thay đổi, thêm mới, hoặc xóa file nào không mong muốn đều sẽ bị phát hiện và cảnh báo ngay lập tức.
    -   **Lợi ích:** Chống lại việc kẻ tấn công chèn mã độc hoặc thay đổi cấu hình trực tiếp trên máy chủ.
    -   **Thành phần:** `IFimService`, `FimComponent`.

18. **Code Attestation & Supply Chain Security:**
    -   **Cách vận hành:**
        -   **Trong CI/CD:** Toàn bộ mã nguồn và các thư viện phụ thuộc (SBOM) được hash và ký số (code signing) trong pipeline build.
        -   **Khi khởi động:** Ứng dụng tự kiểm tra chữ ký số của chính nó và xác thực hash của các file so với manifest đã được ký. Nếu có bất kỳ sự không khớp nào, ứng dụng sẽ từ chối khởi động và gửi cảnh báo nghiêm trọng.
        -   **Quét phụ thuộc:** Tự động quét SBOM để tìm các thư viện có lỗ hổng đã biết.
    -   **Lợi ích:** Đảm bảo mã nguồn đang chạy trên production chính xác là những gì đã được build và phê duyệt, chống lại các cuộc tấn công chuỗi cung ứng.
    -   **Thành phần:** `ICodeAttestationService`, quy trình trong CI/CD.

---

## 3. Hướng dẫn Cài đặt & Khởi chạy

### 3.1. Yêu cầu Môi trường

-   **.NET 9 SDK**
-   **Node.js** (v18.x hoặc 20.x) và npm
-   **Angular CLI:** `npm install -g @angular/cli`
-   **SQL Server** (2019 hoặc mới hơn, có thể dùng LocalDB hoặc Docker)
-   **(Tùy chọn) Docker Desktop** để chạy CSDL hoặc triển khai.
-   **(Tùy chọn) Một công cụ dòng lệnh `zip`** để chạy script đóng gói.

### 3.2. Cấu hình Backend

1.  **Mở file `backend/EMRSystem.API/appsettings.json`:**
    -   Cập nhật `ConnectionStrings.DefaultConnection` để trỏ đến instance SQL Server của bạn.
    -   Thay đổi các giá trị `JwtSettings.Secret`, `Encryption.MasterKey`, `Encryption.IV` thành các chuỗi ngẫu nhiên, an toàn của riêng bạn.
    -   Cấu hình `EmailSettings` để sử dụng dịch vụ SMTP của bạn.
    -   Cấu hình `RecaptchaSettings` với Site Key và Secret Key từ Google reCAPTCHA.

2.  **Tạo và Cập nhật Cơ sở dữ liệu:**
    -   Mở terminal trong thư mục `backend/EMRSystem.API/`.
    -   Chạy lệnh để áp dụng các migrations và tạo CSDL:
        ```bash
        dotnet ef database update
        ```

3.  **Chạy Backend:**
    -   Trong cùng terminal, chạy lệnh:
        ```bash
        dotnet run
        ```
    -   API sẽ khởi động và lắng nghe trên `https://localhost:5001` (hoặc port tương tự).

### 3.3. Cấu hình Frontend

1.  **Cài đặt các gói phụ thuộc:**
    -   Mở terminal trong thư mục `frontend/emr-frontend/`.
    -   Chạy lệnh:
        ```bash
        npm install
        ```

2.  **Cập nhật Biến Môi trường:**
    -   Mở file `frontend/emr-frontend/src/environments/environment.ts`.
    -   Cập nhật `apiUrl` nếu backend của bạn chạy trên một port khác.
    -   Cập nhật `recaptcha.siteKey` với Site Key của bạn.

3.  **Chạy Frontend:**
    -   Trong cùng terminal, chạy lệnh:
        ```bash
        ng serve
        ```
    -   Ứng dụng Angular sẽ khởi động và có thể truy cập tại `http://localhost:4200`.

### 3.4. Tài khoản Quản trị Mặc định

-   **Email:** `admin@emr.com`
-   **Mật khẩu:** `Admin@123`

Sau khi đăng nhập lần đầu, bạn nên đổi mật khẩu ngay lập tức.

## 4. Hướng dẫn Đóng gói & Triển khai

Dự án đi kèm các script để tự động hóa việc build và đóng gói.

### 4.1. Chạy trên máy cá nhân

1.  Mở terminal tại thư mục gốc của dự án.
2.  Chạy một trong các lệnh sau:
    -   **Linux/macOS:**
        ```bash
        chmod +x ./scripts/pack.sh
        ./scripts/pack.sh 1.0.0
        ```
    -   **Windows (PowerShell):**
        ```powershell
        .\scripts\pack.ps1 -Version 1.0.0
        ```
3.  Kết quả là một file `.zip` (ví dụ: `out/emr-system-1.0.0.zip`) chứa bản build sẵn sàng để triển khai.

### 4.2. Triển khai Tự động (CI/CD)

-   Dự án đã có sẵn file workflow `.github/workflows/release.yml`.
-   Khi bạn tạo một tag mới trên Git (ví dụ: `v1.0.0`), GitHub Actions sẽ tự động build và tạo ra một artifact `.zip` trong tab "Actions" của repository, sẵn sàng để tải về và triển khai.

## 5. Sơ đồ Luồng Vận hành Bảo mật Tổng thể

Một request của người dùng sẽ đi qua các lớp bảo vệ như sau:
[ User Request ]
|
v
[ WAF / External Firewall / CDN ] (Lớp ngoài cùng)
|
v
[ Web Server (Kestrel) ]
|
v
[ 1. Rate Limiting Middleware ] -> (Nếu vượt ngưỡng -> Block)
|
v
[ 2. RASP Middleware (Tự bảo vệ) ] -> (Nếu là tấn công -> Block, Tạo Incident, Chặn IP)
|
v
[ 3. Authentication Middleware (JWT/API Key) ] -> (Nếu không hợp lệ -> 401 Unauthorized)
|
v
[ 4. Zero Trust Middleware ]
| - Tính Trust Score (Device, Location, Behavior)
| - Đánh giá theo Policy
| -> (Nếu không đủ tin cậy -> 403 Forbidden, Yêu cầu MFA)
v
[ 5. Controller Action ] -> [ 6. Application Service ]
| |
| - Logic nghiệp vụ
| - Kiểm tra Honey Token
| - Ghi Audit Log -> Kích hoạt UEBA phân tích
v v
[ 7. Response Generation ] [ Database (TDE, ALE) ]
|
v
[ 8. DLP Middleware ] -> (Quét response -> Nếu có rò rỉ -> Redact/Block)
|
v
[ User Response (Dữ liệu đã được bảo vệ) ]
[ BACKGROUND PROCESSES (Hangfire) ]

UEBA Baseline Training
FIM Scans
Report Generation
Certificate Transparency Monitoring
Blockchain Mining

## 6. Liên hệ & Báo cáo Lỗ hổng

-   Nếu bạn phát hiện bất kỳ vấn đề bảo mật nào, vui lòng tạo một "Issue" trên repository hoặc liên hệ trực tiếp qua email: [your-security-email@example.com].
-   Chúng tôi hoan nghênh và đánh giá cao mọi đóng góp từ cộng đồng để làm cho hệ thống này an toàn hơn.

## Áp dụng thực hành

+---------------------------------------------------------------------------------+
| [ USER / CLIENT ] |
+---------------------------------------------------------------------------------+
| (HTTPS/TLS 1.3)
+---------------------------------------------------------------------------------+
| Lớp 0: Mạng & Chu vi (Network & Perimeter) |
| - Tường lửa (WAF) |
| - Chống DDoS |
| - Geo-Blocking |
| - DNS Security (DNSSEC, DMARC) |
+---------------------------------------------------------------------------------+
|
+---------------------------------------------------------------------------------+
| Lớp 1: Cổng vào Ứng dụng (Application Gateway) |
| - Rate Limiting: Chặn Brute Force, DoS |
| - CAPTCHA: Chặn Bot |
| - RASP (Runtime Self-Protection): Chặn SQLi, XSS, Path Traversal real-time |
| - Security Headers: CSP, HSTS, X-Frame-Options... |
+---------------------------------------------------------------------------------+
|
+---------------------------------------------------------------------------------+
| Lớp 2: Nhận dạng & Xác thực (Identity & Authentication) |
| - Xác thực đa yếu tố (MFA): TOTP, FIDO2/WebAuthn (Sinh trắc học) |
| - Quản lý Chính sách Mật khẩu Động (Độ mạnh, Hết hạn, Lịch sử, HIBP) |
| - Device Fingerprinting: Nhận diện thiết bị |
| - Just-in-Time (JIT) Credentials (Tích hợp Vault) |
+---------------------------------------------------------------------------------+
|
+---------------------------------------------------------------------------------+
| Lớp 3: Zero Trust - Ủy quyền & Đánh giá Truy cập (Authorization & Access |
| Evaluation) |
| - Zero Trust Policy Engine: |
| - Tính Trust Score (Device, Location, Behavior, Network, Time) |
| - RBAC + ABAC (Attribute-Based Access Control) |
| - Đánh giá liên tục -> Yêu cầu Step-up Auth nếu Trust Score giảm |
+---------------------------------------------------------------------------------+
|
+---------------------------------------------------------------------------------+
| Lớp 4: Logic Ứng dụng & Dữ liệu (Application Logic & Data) |
| - Deception Tech: Honeypots (API giả), Honey Tokens (Dữ liệu giả) |
| - Dynamic Data Masking: Che dữ liệu nhạy cảm theo quyền |
| - Data Leakage Prevention (DLP): Quét response, chặn/che dữ liệu rò rỉ |
| - Watermarking: Đóng dấu file PDF/ảnh được xuất |
| - Mã hóa Tầng Ứng dụng (ALE) & Quản lý Khóa (KMS) |
| - Confidential Computing: Xử lý dữ liệu trong Secure Enclave |
+---------------------------------------------------------------------------------+
| |
v v
+------------------------+ +-----------------------------------------+
| CSDL (At-Rest | | Lớp 5: Giám sát, Phát hiện & Phản ứng |
| Encryption - TDE) | | (Logging, Detection & Response) |
+------------------------+ | - Immutable Audit Trail (Blockchain) |
| - UEBA: Phân tích hành vi người dùng |
| - Threat Hunting: Chủ động săn lùng |
| - SOAR: Tự động hóa phản ứng sự cố |
| - FIM: Giám sát toàn vẹn file |
| - SBOM & Code Attestation: Bảo vệ chuỗi |
| cung ứng |
+-----------------------------------------+


---

## 3. Chi tiết Vận hành các Lớp Bảo mật

### Lớp 1: Identity & Access Management (IAM)

_Mục tiêu: Đảm bảo chỉ đúng người, đúng thời điểm, đúng thiết bị được phép vào hệ thống._

1.  **Xác thực Mạnh & Đa yếu tố (MFA):**
    -   **Cách vận hành:** Sau khi nhập đúng mật khẩu, hệ thống yêu cầu một yếu tố thứ hai.
        -   **TOTP:** Người dùng nhập mã 6 số từ ứng dụng Google Authenticator/Authy.
        -   **WebAuthn/FIDO2:** Trình duyệt yêu cầu người dùng xác thực bằng sinh trắc học (vân tay, khuôn mặt trên điện thoại/laptop) hoặc cắm khóa bảo mật vật lý (YubiKey). Đây là phương thức ưu tiên vì chống được phishing.
    -   **Thành phần:** `ITwoFactorService`, `IWebAuthnService`, `WebAuthnSetupComponent`.

2.  **Quản lý Chính sách Mật khẩu Động:**
    -   **Cách vận hành:** Admin cấu hình các quy tắc trong UI. Khi người dùng tạo/đổi mật khẩu, `CustomPasswordValidator` sẽ gọi `IPasswordPolicyService` để kiểm tra tất cả các quy tắc theo thời gian thực.
        -   **Kiểm tra HIBP:** Hash mật khẩu (SHA-1), lấy 5 ký tự đầu, gửi đến API của Have I Been Pwned. Nếu hash suffix có trong kết quả trả về, mật khẩu bị từ chối.
        -   **Lịch sử:** So sánh hash của mật khẩu mới với N hash gần nhất được lưu trong `PasswordHistory`.
    -   **Thành phần:** `PasswordPolicyController`, `IPasswordPolicyService`, `PasswordPolicyComponent`.

3.  **Device Fingerprinting & Quản lý Phiên:**
    -   **Cách vận hành:**
        1.  **Thu thập:** Khi người dùng tương tác, `DeviceFingerprintService` (Angular) thu thập hàng chục thông số (User Agent, độ phân giải, múi giờ, fonts, canvas, WebGL...) và tạo ra một hash SHA-256 duy nhất.
        2.  **Gửi:** Hash này được gửi lên backend qua header `X-Device-Fingerprint` trong mỗi request API.
        3.  **Phân tích:** Backend lưu lại các hash này, liên kết chúng với người dùng, và xây dựng một "hồ sơ thiết bị". Đăng nhập từ một hash hoàn toàn mới sẽ bị coi là rủi ro hơn.
        4.  **Quản lý:** Người dùng có thể xem danh sách các phiên đang hoạt động (kèm thông tin thiết bị) và thực hiện "Đăng xuất từ xa".
    -   **Thành phần:** `DeviceFingerprintService`, `TrustedDevicesComponent`, `IDeviceFingerprintService`.

### Lớp 2: Application & API Security

_Mục tiêu: Chặn các cuộc tấn công phổ biến nhắm vào ứng dụng web ngay tại cổng vào._

4.  **Runtime Application Self-Protection (RASP):**
    -   **Cách vận hành:** `RaspMiddleware` là lớp phòng thủ đầu tiên sau web server. Nó kiểm tra mọi phần của request (URL, query string, body) với các bộ regex được định nghĩa trước để phát hiện các mẫu tấn công.
        -   Nếu phát hiện `... OR 1=1 --`, nó xác định là **SQL Injection**.
        -   Nếu phát hiện `<script>alert(1)</script>`, nó xác định là **XSS**.
        -   Nếu khớp, nó không chuyển request đến controller. Thay vào đó, nó ngay lập tức gọi `ISecurityIncidentService` để tạo sự cố `Critical` và gọi `IThreatIntelligenceService` để chặn IP, sau đó trả về lỗi `400 Bad Request`.
    -   **Thành phần:** `RaspMiddleware`.

5.  **Moving Target Defense (MTD):**
    -   **Cách vận hành:**
        1.  Cả backend (`MtdMiddleware`) và frontend (`ApiUrlService`) đều giữ một `RoutingSecret` giống hệt nhau.
        2.  Khi frontend cần gọi API `/patients`, nó sẽ tính toán: `hash("patients" + "YYYY-MM-DD" + routingSecret)` để ra một chuỗi động, ví dụ `a1b2`. URL cuối cùng sẽ là `/api/rt-a1b2/patients`.
        3.  Khi backend nhận được request, `MtdMiddleware` sẽ thực hiện phép tính tương tự. Nếu hash khớp, nó sẽ rewrite URL về `/api/patients` và cho phép request đi tiếp. Nếu không khớp, request bị coi là không hợp lệ và trả về `404 Not Found`.
    -   **Lợi ích:** Vô hiệu hóa các công cụ quét lỗ hổng tự động dựa trên các đường dẫn API phổ biến.

6.  **Deception Technology:**
    -   **Cách vận hành:**
        -   **Honey Token:** Trong `PatientService`, logic `if (id == 999999)` được cài cắm. Nếu một truy vấn nhắm vào ID này, thay vì đi vào CSDL, nó sẽ kích hoạt `IDeceptionService`. Service này tạo sự cố `Critical` và chặn IP vĩnh viễn, sau đó trả về một bản ghi giả mạo.
        -   **Honeypot:** `HoneypotController` được đăng ký với route `/wp-admin`. Bất kỳ request nào đến đây, dù là GET hay POST, đều sẽ kích hoạt `IDeceptionService` và bị làm chậm 10 giây trước khi nhận lỗi.
    -   **Mục đích:** Phát hiện sớm các hành vi do thám và đánh lạc hướng kẻ tấn công.

### Lớp 3: Data Protection

_Mục tiêu: Bảo vệ dữ liệu ở mọi trạng thái: khi truyền đi, khi lưu trữ, và ngay cả khi đang được xử lý._

7.  **Mã hóa & Quản lý Khóa:**
    -   **Cách vận hành:**
        -   **ALE:** Khi lưu một bệnh nhân mới, `EncryptionInterceptor` của EF Core sẽ tự động tìm các thuộc tính có `[SensitiveData]`, gọi `IAdvancedEncryptionService` để mã hóa chúng trước khi câu lệnh `INSERT` được thực thi.
        -   **Key Rotation:** Admin có thể bấm nút "Rotate Key" trong UI. Backend sẽ tạo một khóa mới, đánh dấu khóa cũ là "đã xoay vòng", và kích hoạt một background job để đọc dữ liệu cũ, giải mã bằng khóa cũ, và mã hóa lại bằng khóa mới.
    -   **Thành phần:** `EncryptionInterceptor`, `IAdvancedEncryptionService`, `EncryptionDashboardComponent`.

8.  **Dynamic Data Masking:**
    -   **Cách vận行:** Khi một controller trả về một `PatientDto`, `JsonSerializer` của .NET sẽ thấy các thuộc tính có `[DataMasking]`. Nó sẽ gọi `DataMaskingConverter`.
        -   Converter này lấy `HttpContext` hiện tại, kiểm tra xem người dùng có `Claim("permission", "ViewFullPII")` hay không.
        -   Nếu có, nó ghi giá trị gốc vào JSON.
        -   Nếu không, nó áp dụng quy tắc che (ví dụ: `ShowLast4`) và ghi giá trị đã được che vào JSON.
    -   **Kết quả:** Một y tá sẽ thấy số CCCD là `********5678`, trong khi một bác sĩ sẽ thấy số đầy đủ.

9.  **DLP & Watermarking:**
    -   **Cách vận hành:**
        -   **DLP:** Sau khi controller thực thi xong, `DlpMiddleware` chặn response body. Nó quét nội dung JSON dựa trên các regex trong `DlpRule`. Nếu phát hiện chuỗi trông giống số thẻ tín dụng và quy tắc là "Block", nó sẽ hủy response và trả về lỗi `403`. Nếu quy tắc là "Redact", nó sẽ thay thế chuỗi đó bằng `***REDACTED***` rồi mới gửi response đi.
        -   **Watermarking:** Khi gọi `IPdfService.GenerateMedicalRecordPdfAsync`, service này sẽ lấy thông tin người dùng và IP từ `HttpContext`, sau đó dùng thư viện QuestPDF để vẽ một lớp text mờ, xoay 45 độ lên trên nội dung chính của file PDF.

### Lớp 4: Monitoring, Detection & Response

_Mục tiêu: "Thấy" mọi thứ, phát hiện điều bất thường, và phản ứng một cách tự động và thông minh._

10. **Blockchain Audit Trail:**
    -   **Cách vận hành:** `AuditInterceptor` sau khi ghi một `AuditLog`, nó sẽ gọi `IBlockchainService.AddTransaction`. Giao dịch này được đưa vào một "mempool".
        -   Một background job (hoặc khi đủ 10 giao dịch) sẽ được kích hoạt để "mine" một khối mới. Nó lấy các giao dịch đang chờ, tính Merkle Root, tìm một `nonce` thỏa mãn yêu cầu Proof-of-Work, sau đó thêm khối mới vào chuỗi và cập nhật trạng thái các giao dịch là "confirmed".
    -   **UI:** `BlockchainExplorerComponent` hiển thị các khối và giao dịch này, cho phép Admin xác thực lại toàn bộ chuỗi bất cứ lúc nào.

11. **UEBA - Phân tích Hành vi:**
    -   **Cách vận hành:**
        -   **Học (Offline):** Một Hangfire job chạy hàng đêm, gọi `IUebaService.UpdateBehavioralBaselinesAsync`. Service này truy vấn `AuditLog` của 90 ngày qua để tính toán và lưu lại các "baseline" hành vi cho mỗi người dùng.
        -   **Phân tích (Real-time):** Sau mỗi hành động, `AuditInterceptor` gọi `IUebaService.AnalyzeAndAlertOnActivityAsync`. Service này so sánh hành động hiện tại với baseline đã học, tính toán "Điểm Sai lệch" cho từng yếu tố (giờ, IP, hành động...) và tổng hợp lại. Nếu điểm tổng hợp vượt ngưỡng, một `UebaAlert` được tạo.
    -   **UI:** `UebaDashboardComponent` hiển thị các alert này, cho phép nhà phân tích xem xét và đánh dấu là "False Positive" hoặc leo thang thành một sự cố.

12. **SOAR - Tự động hóa Phản ứng:**
    -   **Cách vận hành:** `SecurityIncidentService` sau khi tạo một sự cố (`SecurityIncident`), nó sẽ ngay lập tức gọi `ISoarService.HandleIncident`.
        -   SOAR Service tìm một "playbook" phù hợp với loại và mức độ nghiêm trọng của sự cố.
        -   Nó thực thi từng bước trong playbook một cách tuần tự:
            1.  `EnrichData`: Gọi `IThreatIntelligenceService` để lấy thông tin về IP.
            2.  `Contain_BlockIP`: Gọi `IThreatIntelligenceService` để chặn IP.
            3.  `Contain_RevokeSessions`: Gọi `ISecurityService` để hủy tất cả các phiên đăng nhập của người dùng.
            4.  `Notify_SecurityTeam`: Gọi `IEmailService` (hoặc API của Slack/Teams) để gửi cảnh báo.
    -   **Lợi ích:** Giảm thời gian phản ứng từ vài giờ xuống còn vài giây, đảm bảo các bước ngăn chặn cơ bản được thực hiện ngay lập tức và nhất quán.

---

## 5. Hướng dẫn Vận hành và Bảo trì

-   **Định kỳ:**
    -   Chạy FIM scan hàng giờ và xem lại các cảnh báo.
    -   Review UEBA alerts hàng ngày.
    -   Cập nhật và chạy các Threat Hunting query hàng tuần.
    -   Review và tối ưu hóa các SOAR playbook hàng tháng.
    -   Thực hiện Key Rotation hàng quý hoặc hàng năm.
    -   Chạy Vulnerability Scan sau mỗi lần cập nhật thư viện và review dashboard hàng tuần.
-   **Khi có sự cố:**
    1.  Nhận thông báo từ SOAR.
    2.  Truy cập `SecurityIncidentsComponent` để xem chi tiết sự cố.
    3.  Sử dụng `UebaDashboardComponent`, `ThreatHuntingComponent`, `SessionPlaybackComponent` để điều tra.
    4.  Cập nhật trạng thái sự cố và ghi lại các hành động khắc phục.

Tài liệu này cung cấp một cái nhìn tổng quan toàn diện về kiến trúc bảo mật của hệ thống EMR, làm cơ sở cho việc phát triển, triển khai, và vận hành một cách an toàn và tuân thủ.