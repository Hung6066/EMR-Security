// IDataAnonymizationService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IDataAnonymizationService
    {
        Task<T> AnonymizeAsync<T>(T entity) where T : class;
        Task<T> PseudonymizeAsync<T>(T entity, string salt) where T : class;
        Task<List<T>> GenerateSyntheticDataAsync<T>(int count) where T : class, new();
        Task<DifferentialPrivacyResult> ApplyDifferentialPrivacyAsync(string query, double epsilon);
        Task<string> TokenizeAsync(string sensitiveData, string tokenType);
        Task<string> DetokenizeAsync(string token);
    }
    
    public class DifferentialPrivacyResult
    {
        public object Result { get; set; }
        public double NoiseAdded { get; set; }
        public double PrivacyBudget { get; set; }
    }
}