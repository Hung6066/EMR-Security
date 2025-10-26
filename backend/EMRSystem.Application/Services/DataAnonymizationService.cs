// DataAnonymizationService.cs
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EMRSystem.Application.Services
{
    public class DataAnonymizationService : IDataAnonymizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public DataAnonymizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T> AnonymizeAsync<T>(T entity) where T : class
        {
            var anonymized = CloneObject(entity);
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<SensitiveDataAttribute>();
                if (attribute != null)
                {
                    var value = property.GetValue(anonymized);
                    if (value != null)
                    {
                        var anonymizedValue = AnonymizeValue(value.ToString(), property.PropertyType);
                        property.SetValue(anonymized, Convert.ChangeType(anonymizedValue, property.PropertyType));
                    }
                }
            }

            return anonymized;
        }

        public async Task<T> PseudonymizeAsync<T>(T entity, string salt) where T : class
        {
            var pseudonymized = CloneObject(entity);
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<SensitiveDataAttribute>();
                if (attribute != null)
                {
                    var value = property.GetValue(pseudonymized);
                    if (value != null)
                    {
                        var pseudonym = GeneratePseudonym(value.ToString(), salt);
                        property.SetValue(pseudonymized, pseudonym);
                    }
                }
            }

            return pseudonymized;
        }

        public async Task<List<T>> GenerateSyntheticDataAsync<T>(int count) where T : class, new()
        {
            var syntheticData = new List<T>();
            
            // Analyze real data patterns
            var realData = await _context.Set<T>().Take(1000).ToListAsync();
            var patterns = AnalyzeDataPatterns(realData);

            for (int i = 0; i < count; i++)
            {
                var synthetic = new T();
                var properties = typeof(T).GetProperties();

                foreach (var property in properties)
                {
                    var syntheticValue = GenerateSyntheticValue(property.PropertyType, patterns);
                    if (syntheticValue != null)
                    {
                        property.SetValue(synthetic, syntheticValue);
                    }
                }

                syntheticData.Add(synthetic);
            }

            return syntheticData;
        }

        public async Task<DifferentialPrivacyResult> ApplyDifferentialPrivacyAsync(
            string query, 
            double epsilon)
        {
            // Simplified differential privacy implementation
            // In production, use a proper DP library like Google's Differential Privacy
            
            var result = await ExecuteQueryAsync(query);
            
            // Add Laplace noise
            var sensitivity = CalculateSensitivity(query);
            var scale = sensitivity / epsilon;
            var noise = GenerateLaplaceNoise(scale);
            
            var noisyResult = AddNoiseToResult(result, noise);

            return new DifferentialPrivacyResult
            {
                Result = noisyResult,
                NoiseAdded = noise,
                PrivacyBudget = epsilon
            };
        }

        public async Task<string> TokenizeAsync(string sensitiveData, string tokenType)
        {
            var token = GenerateToken();
            
            var tokenMapping = new TokenMapping
            {
                Token = token,
                TokenType = tokenType,
                EncryptedValue = await EncryptValueAsync(sensitiveData),
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddYears(1)
            };

            _context.TokenMappings.Add(tokenMapping);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<string> DetokenizeAsync(string token)
        {
            var mapping = await _context.TokenMappings
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt > DateTime.Now);

            if (mapping == null)
            {
                throw new Exception("Invalid or expired token");
            }

            return await DecryptValueAsync(mapping.EncryptedValue);
        }

        private string AnonymizeValue(string value, Type propertyType)
        {
            if (propertyType == typeof(string))
            {
                // Email
                if (Regex.IsMatch(value, @"^[^@]+@[^@]+\.[^@]+$"))
                {
                    return "***@***.***";
                }
                
                // Phone
                if (Regex.IsMatch(value, @"^\d{9,11}$"))
                {
                    return "***-***-****";
                }
                
                // Default: mask middle characters
                if (value.Length > 4)
                {
                    return value.Substring(0, 2) + new string('*', value.Length - 4) + value.Substring(value.Length - 2);
                }
                
                return new string('*', value.Length);
            }

            return value;
        }

        private string GeneratePseudonym(string value, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = value + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash).Substring(0, 16);
        }

        private Dictionary<string, object> AnalyzeDataPatterns<T>(List<T> data)
        {
            // Analyze patterns in real data for synthetic generation
            var patterns = new Dictionary<string, object>();
            
            if (data.Count == 0) return patterns;

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var values = data.Select(d => property.GetValue(d)?.ToString())
                        .Where(v => !string.IsNullOrEmpty(v))
                        .ToList();
                    
                    if (values.Count > 0)
                    {
                        patterns[property.Name] = values;
                    }
                }
            }

            return patterns;
        }

        private object GenerateSyntheticValue(Type propertyType, Dictionary<string, object> patterns)
        {
            if (propertyType == typeof(string))
            {
                return GenerateRandomString(10);
            }
            else if (propertyType == typeof(int))
            {
                return _random.Next(1, 1000);
            }
            else if (propertyType == typeof(DateTime))
            {
                return DateTime.Now.AddDays(-_random.Next(0, 365));
            }
            else if (propertyType == typeof(bool))
            {
                return _random.Next(0, 2) == 1;
            }

            return null;
        }

        private double GenerateLaplaceNoise(double scale)
        {
            var u = _random.NextDouble() - 0.5;
            return -scale * Math.Sign(u) * Math.Log(1 - 2 * Math.Abs(u));
        }

        private double CalculateSensitivity(string query)
        {
            // Simplified - in practice, calculate based on query type
            return 1.0;
        }

        private async Task<object> ExecuteQueryAsync(string query)
        {
            // Execute the query and return result
            // This is a placeholder
            return 0;
        }

        private object AddNoiseToResult(object result, double noise)
        {
            if (result is int intResult)
            {
                return intResult + (int)Math.Round(noise);
            }
            else if (result is double doubleResult)
            {
                return doubleResult + noise;
            }
            
            return result;
        }

        private string GenerateToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Substring(0, 32);
        }

        private async Task<byte[]> EncryptValueAsync(string value)
        {
            // Use encryption service
            return Encoding.UTF8.GetBytes(value); // Placeholder
        }

        private async Task<string> DecryptValueAsync(byte[] encrypted)
        {
            // Use encryption service
            return Encoding.UTF8.GetString(encrypted); // Placeholder
        }

        private T CloneObject<T>(T source) where T : class
        {
            var json = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }

    public class TokenMapping
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }
        public byte[] EncryptedValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}