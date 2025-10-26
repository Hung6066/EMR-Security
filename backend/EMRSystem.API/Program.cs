using System.Text;
using AspNetCoreRateLimit;
using EMRSystem.API.Authentication;
using EMRSystem.API.Middleware;
using EMRSystem.Application.Interfaces;
using EMRSystem.Application.Services;
using EMRSystem.Core.Entities;
using EMRSystem.Core.Entities.Security;
using EMRSystem.Core.Settings;
using EMRSystem.Infrastructure.Data;
using EMRSystem.Infrastructure.Interceptors;
using EMRSystem.Infrastructure.Repositories;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Settings ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// --- Database & Identity ---
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
    var encryptionInterceptor = serviceProvider.GetRequiredService<EncryptionInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor, encryptionInterceptor);
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Cấu hình Identity (được override bởi PasswordPolicy)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
    var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddApiKeySupport();

// --- Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SecurityTeam", policy => policy.RequireRole("Admin", "Security"));
    options.AddPolicy("DoctorOrNurse", policy => policy.RequireRole("Doctor", "Nurse"));
});

// --- Services ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient();

// Interceptors
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<EncryptionInterceptor>();

// Core Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Security Services
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<IDlpService, DlpService>();
builder.Services.AddScoped<IAnomalyDetectionService, AnomalyDetectionService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IAdvancedEncryptionService, AdvancedEncryptionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBehavioralAnalyticsService, BehavioralAnalyticsService>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IDataAnonymizationService, DataAnonymizationService>();
builder.Services.AddScoped<IDeviceFingerprintService, DeviceFingerprintService>();
builder.Services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
builder.Services.AddScoped<ISecurityIncidentService, SecurityIncidentService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IThreatHuntingService, ThreatHuntingService>();
builder.Services.AddScoped<IThreatIntelligenceService, ThreatIntelligenceService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IWebAuthnService, WebAuthnService>();
builder.Services.AddScoped<IZeroTrustService, ZeroTrustService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddTransient<IPasswordValidator<ApplicationUser>, CustomPasswordValidator>();

builder.Services.AddSingleton<IObjectFactory>(sp => new ObjectFactory(sp));

// --- Rate Limiting ---
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// --- Hangfire for Background Jobs ---
builder.Services.AddHangfire(config => config
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// --- Controllers & API Explorer ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EMR System API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --- WebAuthn/FIDO2 ---
builder.Services.AddFido2(options =>
{
    options.ServerDomain = builder.Configuration["Fido2:ServerDomain"];
    options.ServerName = "EMR System";
    options.Origins.Add(builder.Configuration["Fido2:Origin"]);
});

// --- Build App ---
var app = builder.Build();

// --- HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseIpRateLimiting();
app.UseRasp(); // Thêm dòng này
app.UseCors("AllowAngular");

app.UseRouting();

// Thêm MTD Middleware ngay trước UseEndpoints
app.UseMovingTargetDefense();

app.UseAuthentication();
app.UseAuthorization();
app.UseZeroTrust();

app.UseStaticFiles();
app.UseDefaultFiles();

app.MapControllers()
.AddJsonOptions(options =>
{
    // Lấy service provider để truyền vào factory
    var serviceProvider = builder.Services.BuildServiceProvider();
    var objectFactory = serviceProvider.GetRequiredService<IObjectFactory>();
    
    // Converter factory của chúng ta sẽ được gọi khi có attribute [DataMasking]
    // Không cần thêm trực tiếp vào options.Converters
});
app.MapFallbackToFile("index.html");
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Quan trọng: Sử dụng lớp filter tùy chỉnh của chúng ta
    Authorization = new[] { new HangfireAuthorizationFilter() },
    // Tắt tính năng ghi lại trạng thái (có thể gây ra cookie lớn)
    IsReadOnlyFunc = (DashboardContext context) => true,
    // (Tùy chọn) Thêm một số tùy chỉnh khác cho Dashboard
    DashboardTitle = "EMR System Jobs"
});
app.UseDlp(); // Thêm dòng này

using (var scope = app.Services.CreateScope())
{
    var attestationService = scope.ServiceProvider.GetRequiredService<ICodeAttestationService>();
    var verificationResult = await attestationService.VerifySelfAsync();
    
    if (!verificationResult.IsValid)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogCritical("CODE ATTESTATION FAILED! Application has been tampered with. Files: {Files}", 
            string.Join(", ", verificationResult.MismatchedFiles));
        
        // Ngăn ứng dụng khởi động hoàn toàn
        // Có thể throw exception hoặc Environment.Exit(1)
        throw new ApplicationException("Application integrity check failed. Shutting down.");
    }
}

// --- Seed Data ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // await SeedData.Initialize(services);
}

RecurringJob.AddOrUpdate<FileIntegrityService>(
    "fim-scan", 
    service => service.ScanForChangesAsync(), 
    Cron.Hourly);

    // Program.cs
RecurringJob.AddOrUpdate<IUebaService>(
    "ueba-baseline-update", 
    service => service.UpdateBehavioralBaselinesAsync(), 
    Cron.Daily(2)); // Chạy vào 2 giờ sáng mỗi ngày

app.Run();