// AdvancedEncryptionService.cs
using System.Security.Cryptography;
using System.Text;

namespace EMRSystem.Application.Services
{
    public class AdvancedEncryptionService : IAdvancedEncryptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdvancedEncryptionService> _logger;
        private readonly byte[] _masterKey;

        public AdvancedEncryptionService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AdvancedEncryptionService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            
            // Master key should be stored in Azure Key Vault or similar
            _masterKey = Convert.FromBase64String(configuration["Encryption:MasterKey"]);
        }

        public async Task<EncryptedData> EncryptDataAsync(string plainText, string purpose)
        {
            var key = await GetActiveKeyAsync(purpose);
            if (key == null)
            {
                throw new Exception($"No active encryption key found for purpose: {purpose}");
            }

            var decryptedKey = DecryptKey(key.EncryptedKey);

            using var aes = Aes.Create();
            aes.Key = decryptedKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            
            swEncrypt.Write(plainText);
            swEncrypt.Close();

            var encrypted = new EncryptedData
            {
                CipherText = msEncrypt.ToArray(),
                IV = aes.IV,
                KeyId = key.Id,
                Algorithm = "AES-256-CBC"
            };

            // Audit
            await LogEncryptionOperationAsync(key.Id, "Encrypt", "Data", null);

            return encrypted;
        }

        public async Task<string> DecryptDataAsync(EncryptedData encryptedData)
        {
            var key = await _context.EncryptionKeys.FindAsync(encryptedData.KeyId);
            if (key == null)
            {
                throw new Exception("Encryption key not found");
            }

            var decryptedKey = DecryptKey(key.EncryptedKey);

            using var aes = Aes.Create();
            aes.Key = decryptedKey;
            aes.IV = encryptedData.IV;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(encryptedData.CipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            var plainText = srDecrypt.ReadToEnd();

            // Audit
            await LogEncryptionOperationAsync(key.Id, "Decrypt", "Data", null);

            return plainText;
        }

        public async Task<EncryptionKey> CreateKeyAsync(CreateKeyDto dto)
        {
            byte[] keyBytes;
            byte[] iv = null;

            switch (dto.KeyType)
            {
                case "AES":
                    using (var aes = Aes.Create())
                    {
                        aes.KeySize = dto.KeySize;
                        aes.GenerateKey();
                        aes.GenerateIV();
                        keyBytes = aes.Key;
                        iv = aes.IV;
                    }
                    break;

                case "RSA":
                    using (var rsa = RSA.Create(dto.KeySize))
                    {
                        keyBytes = rsa.ExportRSAPrivateKey();
                    }
                    break;

                default:
                    throw new ArgumentException("Unsupported key type");
            }

            var encryptedKey = EncryptKey(keyBytes);

            var key = new EncryptionKey
            {
                KeyName = dto.KeyName,
                KeyType = dto.KeyType,
                EncryptedKey = encryptedKey,
                IV = iv,
                Purpose = dto.Purpose,
                KeySize = dto.KeySize,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(dto.ValidityDays),
                IsActive = true,
                IsRotated = false,
                CreatedByUserId = 1 // Get from context
            };

            _context.EncryptionKeys.Add(key);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created encryption key: {dto.KeyName} ({dto.KeyType}-{dto.KeySize})");

            return key;
        }

        public async Task RotateKeyAsync(int keyId)
        {
            var oldKey = await _context.EncryptionKeys.FindAsync(keyId);
            if (oldKey == null)
            {
                throw new Exception("Key not found");
            }

            // Create new key with same parameters
            var newKey = await CreateKeyAsync(new CreateKeyDto
            {
                KeyName = oldKey.KeyName + "_rotated",
                KeyType = oldKey.KeyType,
                Purpose = oldKey.Purpose,
                KeySize = oldKey.KeySize,
                ValidityDays = 365
            });

            // Mark old key as rotated
            oldKey.IsRotated = true;
            oldKey.RotatedToKeyId = newKey.Id;
            oldKey.RotatedAt = DateTime.Now;
            oldKey.IsActive = false;

            await _context.SaveChangesAsync();

            // Re-encrypt data with new key
            await ReencryptDataAsync(oldKey.Id, newKey.Id);

            _logger.LogWarning($"Key rotated: {oldKey.KeyName} -> {newKey.KeyName}");
        }

        public async Task<string> GetFromVaultAsync(string keyName)
        {
            var vaultEntry = await _context.SecureVaults
                .Include(v => v.EncryptionKey)
                .FirstOrDefaultAsync(v => v.KeyName == keyName);

            if (vaultEntry == null)
            {
                throw new Exception("Vault entry not found");
            }

            var encryptedData = new EncryptedData
            {
                CipherText = vaultEntry.EncryptedValue,
                IV = vaultEntry.IV,
                KeyId = vaultEntry.EncryptionKeyId,
                Algorithm = "AES-256-CBC"
            };

            return await DecryptDataAsync(encryptedData);
        }

        public async Task StoreInVaultAsync(string keyName, string value, string description)
        {
            var encryptedData = await EncryptDataAsync(value, "VaultStorage");

            var vaultEntry = new SecureVault
            {
                KeyName = keyName,
                EncryptedValue = encryptedData.CipherText,
                IV = encryptedData.IV,
                EncryptionKeyId = encryptedData.KeyId,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _context.SecureVaults.Add(vaultEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> HashWithSaltAsync(string data)
        {
            using var sha256 = SHA256.Create();
            var salt = GenerateSalt();
            var combined = Encoding.UTF8.GetBytes(data + Convert.ToBase64String(salt));
            var hash = sha256.ComputeHash(combined);
            
            // Combine salt and hash
            var hashWithSalt = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, hashWithSalt, salt.Length, hash.Length);
            
            return hashWithSalt;
        }

        public async Task<bool> VerifyHashAsync(string data, byte[] hashWithSalt, byte[] salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(data + Convert.ToBase64String(salt));
            var hash = sha256.ComputeHash(combined);
            
            var storedHash = new byte[hash.Length];
            Buffer.BlockCopy(hashWithSalt, salt.Length, storedHash, 0, hash.Length);
            
            return hash.SequenceEqual(storedHash);
        }

        private async Task<EncryptionKey> GetActiveKeyAsync(string purpose)
        {
            return await _context.EncryptionKeys
                .Where(k => k.Purpose == purpose && 
                           k.IsActive && 
                           !k.IsRotated &&
                           k.ExpiresAt > DateTime.Now)
                .OrderByDescending(k => k.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private byte[] EncryptKey(byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = _masterKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            
            // Write IV first
            ms.Write(aes.IV, 0, aes.IV.Length);
            
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(key, 0, key.Length);
            cs.FlushFinalBlock();
            
            return ms.ToArray();
        }

        private byte[] DecryptKey(byte[] encryptedKey)
        {
            using var aes = Aes.Create();
            aes.Key = _masterKey;
            
            // Extract IV
            var iv = new byte[16];
            Buffer.BlockCopy(encryptedKey, 0, iv, 0, 16);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(encryptedKey, 16, encryptedKey.Length - 16);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var resultStream = new MemoryStream();
            
            cs.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        private byte[] GenerateSalt()
        {
            var salt = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private async Task LogEncryptionOperationAsync(
            int keyId, 
            string operation, 
            string dataType, 
            int? recordId)
        {
            var audit = new DataEncryptionAudit
            {
                EncryptionKeyId = keyId,
                DataType = dataType,
                RecordId = recordId,
                Operation = operation,
                PerformedByUserId = 1, // Get from context
                PerformedAt = DateTime.Now
            };

            _context.DataEncryptionAudits.Add(audit);
            await _context.SaveChangesAsync();
        }

        private async Task ReencryptDataAsync(int oldKeyId, int newKeyId)
        {
            // Re-encrypt sensitive data with new key
            // This is a simplified example
            var patients = await _context.Patients
                .Where(p => !string.IsNullOrEmpty(p.IdentityCard))
                .ToListAsync();

            foreach (var patient in patients)
            {
                // Decrypt with old key, encrypt with new key
                // Implementation depends on how data is stored
            }

            await _context.SaveChangesAsync();
        }
    }
}