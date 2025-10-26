// AnomalyDetectionService.cs
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EMRSystem.Application.Services
{
    public class AnomalyDetectionService : IAnomalyDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly ILogger<AnomalyDetectionService> _logger;

        public AnomalyDetectionService(
            ApplicationDbContext context,
            ILogger<AnomalyDetectionService> logger)
        {
            _context = context;
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<AnomalyScore> DetectUserBehaviorAnomalyAsync(
            int userId, 
            UserActivity activity)
        {
            var score = new AnomalyScore
            {
                Reasons = new List<string>(),
                FeatureScores = new Dictionary<string, double>()
            };

            // Get user's historical behavior
            var historicalActivities = await GetUserHistoricalActivitiesAsync(userId);
            
            if (historicalActivities.Count < 10)
            {
                // Not enough data for detection
                score.Score = 0.5;
                score.IsAnomaly = false;
                return score;
            }

            // Feature extraction
            var features = ExtractActivityFeatures(activity, historicalActivities);

            // Calculate anomaly scores for each feature
            score.FeatureScores["TimeOfDay"] = CalculateTimeAnomalyScore(
                activity.Timestamp.Hour, 
                historicalActivities.Select(a => a.Timestamp.Hour).ToList()
            );

            score.FeatureScores["ActionFrequency"] = CalculateActionFrequencyScore(
                activity.Action,
                historicalActivities.Select(a => a.Action).ToList()
            );

            score.FeatureScores["IpAddress"] = CalculateIpAnomalyScore(
                activity.IpAddress,
                historicalActivities.Select(a => a.IpAddress).ToList()
            );

            score.FeatureScores["ResourceAccess"] = CalculateResourceAccessScore(
                activity.Resource,
                historicalActivities.Select(a => a.Resource).ToList()
            );

            // Overall score (weighted average)
            score.Score = (
                score.FeatureScores["TimeOfDay"] * 0.2 +
                score.FeatureScores["ActionFrequency"] * 0.3 +
                score.FeatureScores["IpAddress"] * 0.3 +
                score.FeatureScores["ResourceAccess"] * 0.2
            );

            score.IsAnomaly = score.Score > 0.7;

            if (score.IsAnomaly)
            {
                foreach (var feature in score.FeatureScores.Where(f => f.Value > 0.7))
                {
                    score.Reasons.Add($"Unusual {feature.Key}");
                }

                // Create alert
                await CreateAnomalyAlertAsync(userId, activity, score);
            }

            return score;
        }

        public async Task<AnomalyScore> DetectDataAccessAnomalyAsync(DataAccessPattern pattern)
        {
            var score = new AnomalyScore
            {
                Reasons = new List<string>(),
                FeatureScores = new Dictionary<string, double>()
            };

            // Get user's typical access patterns
            var historicalPatterns = await GetUserDataAccessPatternsAsync(pattern.UserId);

            // Volume anomaly
            var avgRecordCount = historicalPatterns.Average(p => p.RecordCount);
            var stdDev = CalculateStdDev(historicalPatterns.Select(p => (double)p.RecordCount).ToList());
            var zScore = Math.Abs((pattern.RecordCount - avgRecordCount) / stdDev);
            
            score.FeatureScores["Volume"] = Math.Min(zScore / 3.0, 1.0);

            // Speed anomaly
            var recordsPerMinute = pattern.RecordCount / pattern.Duration.TotalMinutes;
            var avgSpeed = historicalPatterns.Average(p => p.RecordCount / p.Duration.TotalMinutes);
            var speedZScore = Math.Abs((recordsPerMinute - avgSpeed) / 
                CalculateStdDev(historicalPatterns.Select(p => p.RecordCount / p.Duration.TotalMinutes).ToList()));
            
            score.FeatureScores["Speed"] = Math.Min(speedZScore / 3.0, 1.0);

            // Data type anomaly
            var dataTypeFrequency = historicalPatterns.Count(p => p.DataType == pattern.DataType) / 
                (double)historicalPatterns.Count;
            score.FeatureScores["DataType"] = dataTypeFrequency < 0.1 ? 0.8 : 0.2;

            score.Score = score.FeatureScores.Values.Average();
            score.IsAnomaly = score.Score > 0.7;

            if (score.IsAnomaly)
            {
                score.Reasons.Add($"Unusual data access pattern detected");
                if (score.FeatureScores["Volume"] > 0.7)
                    score.Reasons.Add("Accessing unusually large number of records");
                if (score.FeatureScores["Speed"] > 0.7)
                    score.Reasons.Add("Accessing data at unusual speed");
            }

            return score;
        }

        public async Task<List<AnomalyAlert>> GetAnomaliesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AnomalyAlerts
                .Where(a => a.DetectedAt >= startDate && a.DetectedAt <= endDate)
                .OrderByDescending(a => a.Score)
                .ToListAsync();
        }

        public async Task TrainModelAsync()
        {
            // Load historical data
            var trainingData = await LoadTrainingDataAsync();

            // Define data pipeline
            var pipeline = _mlContext.Transforms.Concatenate("Features", 
                    "TimeOfDay", "DayOfWeek", "ActionType", "ResourceType")
                .Append(_mlContext.AnomalyDetection.Trainers.RandomizedPca(
                    featureColumnName: "Features",
                    rank: 4,
                    ensureZeroMean: true));

            // Train the model
            _model = pipeline.Fit(trainingData);

            _logger.LogInformation("Anomaly detection model trained successfully");
        }

        public async Task<PredictionResult> PredictSecurityRiskAsync(SecurityContext context)
        {
            var result = new PredictionResult
            {
                RiskFactors = new List<string>()
            };

            // Feature engineering
            var features = ExtractSecurityFeatures(context);

            // Calculate risk score based on multiple factors
            double riskScore = 0;

            // Time-based risk
            var hour = context.Timestamp.Hour;
            if (hour >= 0 && hour <= 5) // Night time access
            {
                riskScore += 0.3;
                result.RiskFactors.Add("Off-hours access");
            }

            // IP-based risk
            var threatAssessment = await AssessIpThreatAsync(context.IpAddress);
            riskScore += threatAssessment.ThreatScore / 100.0 * 0.4;
            if (threatAssessment.IsThreat)
            {
                result.RiskFactors.Add("Suspicious IP address");
            }

            // User behavior risk
            var behaviorScore = await CalculateUserBehaviorRiskAsync(context.UserId);
            riskScore += behaviorScore * 0.3;

            result.RiskScore = Math.Min(riskScore, 1.0);
            result.RiskLevel = result.RiskScore switch
            {
                >= 0.8 => "Critical",
                >= 0.6 => "High",
                >= 0.4 => "Medium",
                _ => "Low"
            };
            result.Confidence = 0.85; // Simplified

            return result;
        }

        private async Task<List<UserActivityData>> GetUserHistoricalActivitiesAsync(int userId)
        {
            var activities = await _context.AuditLogs
                .Where(a => a.UserId == userId && a.Timestamp > DateTime.Now.AddDays(-30))
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();

            return activities.Select(a => new UserActivityData
            {
                Timestamp = a.Timestamp,
                Action = a.Action,
                Resource = a.EntityType,
                IpAddress = a.IpAddress
            }).ToList();
        }

        private double CalculateTimeAnomalyScore(int currentHour, List<int> historicalHours)
        {
            var hourCounts = historicalHours.GroupBy(h => h)
                .ToDictionary(g => g.Key, g => g.Count());

            var totalCount = historicalHours.Count;
            var currentHourFreq = hourCounts.ContainsKey(currentHour) 
                ? hourCounts[currentHour] / (double)totalCount 
                : 0;

            return 1.0 - currentHourFreq;
        }

        private double CalculateActionFrequencyScore(string action, List<string> historicalActions)
        {
            var frequency = historicalActions.Count(a => a == action) / (double)historicalActions.Count;
            return 1.0 - frequency;
        }

        private double CalculateIpAnomalyScore(string currentIp, List<string> historicalIps)
        {
            var isKnown = historicalIps.Contains(currentIp);
            return isKnown ? 0.0 : 0.9;
        }

        private double CalculateResourceAccessScore(string resource, List<string> historicalResources)
        {
            var frequency = historicalResources.Count(r => r == resource) / (double)historicalResources.Count;
            return frequency < 0.1 ? 0.8 : 0.2;
        }

        private double CalculateStdDev(List<double> values)
        {
            var avg = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }

        private async Task CreateAnomalyAlertAsync(int userId, UserActivity activity, AnomalyScore score)
        {
            var alert = new AnomalyAlert
            {
                Type = "UserBehavior",
                Score = score.Score,
                Description = $"Anomalous behavior detected for user {userId}: {string.Join(", ", score.Reasons)}",
                DetectedAt = DateTime.Now,
                IsResolved = false
            };

            _context.AnomalyAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogWarning($"Anomaly detected: {alert.Description}");
        }

        private async Task<List<DataAccessPattern>> GetUserDataAccessPatternsAsync(int userId)
        {
            // Retrieve historical data access patterns
            return new List<DataAccessPattern>(); // Placeholder
        }

        private Dictionary<string, double> ExtractActivityFeatures(
            UserActivity activity, 
            List<UserActivityData> historical)
        {
            return new Dictionary<string, double>
            {
                ["TimeOfDay"] = activity.Timestamp.Hour,
                ["DayOfWeek"] = (int)activity.Timestamp.DayOfWeek,
                ["ActionType"] = HashString(activity.Action),
                ["ResourceType"] = HashString(activity.Resource)
            };
        }

        private Dictionary<string, double> ExtractSecurityFeatures(SecurityContext context)
        {
            return new Dictionary<string, double>
            {
                ["Hour"] = context.Timestamp.Hour,
                ["DayOfWeek"] = (int)context.Timestamp.DayOfWeek,
                ["ActionHash"] = HashString(context.Action)
            };
        }

        private async Task<IDataView> LoadTrainingDataAsync()
        {
            // Load and prepare training data
            var data = new List<AnomalyData>();
            return _mlContext.Data.LoadFromEnumerable(data);
        }

        private async Task<(bool IsThreat, double ThreatScore)> AssessIpThreatAsync(string ipAddress)
        {
            // Use threat intelligence service
            return (false, 0); // Placeholder
        }

        private async Task<double> CalculateUserBehaviorRiskAsync(int userId)
        {
            // Calculate based on recent suspicious activities
            return 0.0; // Placeholder
        }

        private double HashString(string input)
        {
            return input.GetHashCode() % 1000 / 1000.0;
        }
    }

    public class UserActivityData
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public string IpAddress { get; set; }
    }

    public class AnomalyData
    {
        public float TimeOfDay { get; set; }
        public float DayOfWeek { get; set; }
        public float ActionType { get; set; }
        public float ResourceType { get; set; }
    }

    public class AnomalyAlert
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public double Score { get; set; }
        public string Description { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}