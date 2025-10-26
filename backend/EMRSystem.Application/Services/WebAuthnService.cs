// WebAuthnService.cs
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace EMRSystem.Application.Services
{
    public class WebAuthnService : IWebAuthnService
    {
        private readonly IFido2 _fido2;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public WebAuthnService(
            IFido2 fido2,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuthService authService)
        {
            _fido2 = fido2;
            _context = context;
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<CredentialCreateOptions> GetCredentialOptionsAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found");

            var existingKeys = await _context.WebAuthnCredentials
                .Where(c => c.UserId == userId && c.IsActive)
                .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                .ToListAsync();

            var fidoUser = new Fido2User
            {
                DisplayName = user.FullName,
                Name = user.Email,
                Id = Encoding.UTF8.GetBytes(user.Id.ToString())
            };

            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = false,
                UserVerification = UserVerificationRequirement.Preferred,
                AuthenticatorAttachment = AuthenticatorAttachment.CrossPlatform
            };

            var options = _fido2.RequestNewCredential(
                fidoUser,
                existingKeys,
                authenticatorSelection,
                AttestationConveyancePreference.None
            );

            // Store challenge in session/cache
            await StoreChallenge(user.Id, options.Challenge);

            return options;
        }

        public async Task<bool> RegisterCredentialAsync(
            int userId, 
            AuthenticatorAttestationRawResponse attestationResponse)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found");

            var challenge = await GetStoredChallenge(user.Id);
            if (challenge == null)
                throw new Exception("Challenge not found");

            var options = new CredentialCreateOptions
            {
                Challenge = challenge,
                User = new Fido2User
                {
                    DisplayName = user.FullName,
                    Name = user.Email,
                    Id = Encoding.UTF8.GetBytes(user.Id.ToString())
                },
                Rp = new PublicKeyCredentialRpEntity(
                    "EMR System", 
                    "localhost", 
                    ""
                ),
                Attestation = AttestationConveyancePreference.None
            };

            var success = await _fido2.MakeNewCredentialAsync(
                attestationResponse,
                options,
                async (args, cancellationToken) => true
            );

            if (success.Result != null)
            {
                var credential = new WebAuthnCredential
                {
                    UserId = userId,
                    CredentialId = success.Result.CredentialId,
                    PublicKey = success.Result.PublicKey,
                    UserHandle = success.Result.User.Id,
                    SignatureCounter = success.Result.Counter,
                    CredentialType = success.Result.CredType,
                    AAGUID = success.Result.Aaguid?.ToString(),
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    DeviceName = "Security Key" // Can be customized
                };

                _context.WebAuthnCredentials.Add(credential);
                await _context.SaveChangesAsync();

                await ClearChallenge(user.Id);
                return true;
            }

            return false;
        }

        public async Task<AssertionOptions> GetAssertionOptionsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found");

            var existingCredentials = await _context.WebAuthnCredentials
                .Where(c => c.UserId == user.Id && c.IsActive)
                .ToListAsync();

            if (!existingCredentials.Any())
                throw new Exception("No credentials registered");

            var allowedCredentials = existingCredentials
                .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                .ToList();

            var options = _fido2.GetAssertionOptions(
                allowedCredentials,
                UserVerificationRequirement.Preferred
            );

            await StoreChallenge(user.Id, options.Challenge);

            return options;
        }

        public async Task<AuthResponseDto> VerifyAssertionAsync(
            string email, 
            AuthenticatorAssertionRawResponse assertionResponse)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found");

            var credential = await _context.WebAuthnCredentials
                .FirstOrDefaultAsync(c => c.CredentialId.SequenceEqual(assertionResponse.Id) && 
                                         c.IsActive);

            if (credential == null)
                throw new Exception("Credential not found");

            var challenge = await GetStoredChallenge(user.Id);
            if (challenge == null)
                throw new Exception("Challenge not found");

            var options = new AssertionOptions
            {
                Challenge = challenge,
                RpId = "localhost",
                AllowCredentials = new List<PublicKeyCredentialDescriptor>
                {
                    new PublicKeyCredentialDescriptor(credential.CredentialId)
                }
            };

            var storedCounter = credential.SignatureCounter;

            var res = await _fido2.MakeAssertionAsync(
                assertionResponse,
                options,
                credential.PublicKey,
                storedCounter,
                async (args, cancellationToken) => true
            );

            if (res.Status == "ok")
            {
                credential.SignatureCounter = res.Counter;
                credential.LastUsedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                await ClearChallenge(user.Id);

                return await _authService.GenerateAuthResponse(user);
            }

            throw new Exception("Invalid assertion");
        }

        public async Task<List<WebAuthnCredential>> GetUserCredentialsAsync(int userId)
        {
            return await _context.WebAuthnCredentials
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task RevokeCredentialAsync(int credentialId)
        {
            var credential = await _context.WebAuthnCredentials.FindAsync(credentialId);
            if (credential != null)
            {
                credential.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        private async Task StoreChallenge(int userId, byte[] challenge)
        {
            // Store in distributed cache or session
            // Implementation depends on cache strategy
        }

        private async Task<byte[]> GetStoredChallenge(int userId)
        {
            // Retrieve from cache
            return null;
        }

        private async Task ClearChallenge(int userId)
        {
            // Clear from cache
        }
    }
}