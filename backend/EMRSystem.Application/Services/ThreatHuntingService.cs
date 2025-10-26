// ThreatHuntingService.cs
using System.Text.Json;
using System.Linq.Dynamic.Core;

namespace EMRSystem.Application.Services
{
    public class ThreatHuntingService : IThreatHuntingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ThreatHuntingService> _logger;

        public ThreatHuntingService(
            ApplicationDbContext context,
            ILogger<ThreatHuntingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ThreatHuntingResult> ExecuteQueryAsync(int queryId)
        {
            var query = await _context.ThreatHuntingQueries.FindAsync(queryId);
            if (query == null)
                throw new Exception("Query not found");

            var definition = JsonSerializer.Deserialize<QueryDefinition>(query.QueryDefinition);
            var results = await ExecuteQueryDefinitionAsync(definition);

            var huntingResult = new ThreatHuntingResult
            {
                QueryId = queryId,
                ExecutionTime = DateTime.Now,
                MatchCount = results.Count,
                Results = JsonSerializer.Serialize(results),
                HasThreats = results.Count > 0,
                Summary = $"Found {results.Count} matches"
            };

            _context.ThreatHuntingResults.Add(huntingResult);
            
            query.LastExecutedAt = DateTime.Now;
            query.ExecutionCount++;
            
            await _context.SaveChangesAsync();

            return huntingResult;
        }

        public async Task<ThreatHuntingQuery> CreateQueryAsync(CreateThreatQueryDto dto)
        {
            var query = new ThreatHuntingQuery
            {
                Name = dto.Name,
                Description = dto.Description,
                QueryDefinition = JsonSerializer.Serialize(dto.Definition),
                Severity = dto.Severity,
                IsActive = true,
                CreatedByUserId = 1, // Get from context
                CreatedAt = DateTime.Now,
                ExecutionCount = 0
            };

            _context.ThreatHuntingQueries.Add(query);
            await _context.SaveChangesAsync();

            return query;
        }

        public async Task<List<ThreatHuntingQuery>> GetQueriesAsync()
        {
            return await _context.ThreatHuntingQueries
                .Include(q => q.CreatedBy)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ThreatHuntingResult>> GetResultsAsync(int queryId)
        {
            return await _context.ThreatHuntingResults
                .Where(r => r.QueryId == queryId)
                .OrderByDescending(r => r.ExecutionTime)
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<ThreatIndicator>> GetIndicatorsAsync()
        {
            return await _context.ThreatIndicators
                .Where(i => i.IsActive && (!i.ExpiresAt.HasValue || i.ExpiresAt > DateTime.Now))
                .OrderByDescending(i => i.Severity)
                .ToListAsync();
        }

        public async Task<ThreatIndicator> AddIndicatorAsync(AddThreatIndicatorDto dto)
        {
            var indicator = new ThreatIndicator
            {
                IndicatorType = dto.IndicatorType,
                Value = dto.Value,
                Description = dto.Description,
                Severity = dto.Severity,
                Source = dto.Source,
                AddedAt = DateTime.Now,
                IsActive = true,
                MatchCount = 0
            };

            _context.ThreatIndicators.Add(indicator);
            await _context.SaveChangesAsync();

            return indicator;
        }

        public async Task<ThreatHuntingSummary> GetSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var results = await _context.ThreatHuntingResults
                .Where(r => r.ExecutionTime >= startDate && r.ExecutionTime <= endDate)
                .ToListAsync();

            var queries = await _context.ThreatHuntingQueries.ToListAsync();

            var summary = new ThreatHuntingSummary
            {
                TotalQueries = queries.Count,
                TotalExecutions = results.Count,
                ThreatsDetected = results.Count(r => r.HasThreats),
                ThreatsBySeverity = queries
                    .GroupBy(q => q.Severity)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return summary;
        }

        public async Task<List<SuspiciousActivity>> HuntSuspiciousActivitiesAsync(HuntingCriteria criteria)
        {
            var activities = new List<SuspiciousActivity>();

            // Hunt for unusual login patterns
            var loginActivities = await HuntUnusualLoginsAsync(criteria);
            activities.AddRange(loginActivities);

            // Hunt for data exfiltration attempts
            var dataExfil = await HuntDataExfiltrationAsync(criteria);
            activities.AddRange(dataExfil);

            // Hunt for privilege escalation attempts
            var privEsc = await HuntPrivilegeEscalationAsync(criteria);
            activities.AddRange(privEsc);

            // Hunt for IOC matches
            var iocMatches = await HuntIOCMatchesAsync(criteria);
            activities.AddRange(iocMatches);

            return activities.OrderByDescending(a => a.ThreatScore).ToList();
        }

        private async Task<List<object>> ExecuteQueryDefinitionAsync(QueryDefinition definition)
        {
            var results = new List<object>();

            switch (definition.DataSource)
            {
                case "AuditLogs":
                    var auditLogs = await QueryAuditLogsAsync(definition);
                    results.AddRange(auditLogs);
                    break;

                case "LoginAttempts":
                    var loginAttempts = await QueryLoginAttemptsAsync(definition);
                    results.AddRange(loginAttempts);
                    break;

                case "ThreatLogs":
                    var threatLogs = await QueryThreatLogsAsync(definition);
                    results.AddRange(threatLogs);
                    break;
            }

            return results;
        }

        private async Task<List<object>> QueryAuditLogsAsync(QueryDefinition definition)
        {
            IQueryable<AuditLog> query = _context.AuditLogs;

            foreach (var condition in definition.Conditions)
            {
                query = ApplyCondition(query, condition);
            }

            var results = await query.Take(1000).ToListAsync();
            return results.Cast<object>().ToList();
        }

        private async Task<List<object>> QueryLoginAttemptsAsync(QueryDefinition definition)
        {
            IQueryable<LoginAttempt> query = _context.LoginAttempts;

            foreach (var condition in definition.Conditions)
            {
                query = ApplyCondition(query, condition);
            }

            var results = await query.Take(1000).ToListAsync();
            return results.Cast<object>().ToList();
        }

        private async Task<List<object>> QueryThreatLogsAsync(QueryDefinition definition)
        {
            IQueryable<ThreatLog> query = _context.ThreatLogs;

            foreach (var condition in definition.Conditions)
            {
                query = ApplyCondition(query, condition);
            }

            var results = await query.Take(1000).ToListAsync();
            return results.Cast<object>().ToList();
        }

        private IQueryable<T> ApplyCondition<T>(IQueryable<T> query, QueryCondition condition)
        {
            var expression = condition.Operator switch
            {
                "equals" => $"{condition.Field} == \"{condition.Value}\"",
                "contains" => $"{condition.Field}.Contains(\"{condition.Value}\")",
                "greater" => $"{condition.Field} > {condition.Value}",
                "less" => $"{condition.Field} < {condition.Value}",
                _ => null
            };

            if (!string.IsNullOrEmpty(expression))
            {
                query = query.Where(expression);
            }

            return query;
        }

        private async Task<List<SuspiciousActivity>> HuntUnusualLoginsAsync(HuntingCriteria criteria)
        {
            var activities = new List<SuspiciousActivity>();

            // Multiple failed logins
            var failedLogins = await _context.LoginAttempts
                .Where(l => l.AttemptedAt >= criteria.StartDate && 
                           l.AttemptedAt <= criteria.EndDate &&
                           !l.IsSuccessful)
                .GroupBy(l => new { l.Email, l.IpAddress })
                .Where(g => g.Count() >= 5)
                .ToListAsync();

            foreach (var group in failedLogins)
            {
                activities.Add(new SuspiciousActivity
                {
                    ActivityType = "Multiple Failed Logins",
                    Description = $"{group.Count()} failed login attempts for {group.Key.Email}",
                    Timestamp = group.Max(l => l.AttemptedAt),
                    ThreatScore = Math.Min(group.Count() / 5.0, 1.0),
                    Indicators = new List<string> { "Brute Force", "Credential Stuffing" }
                });
            }

            return activities;
        }

        private async Task<List<SuspiciousActivity>> HuntDataExfiltrationAsync(HuntingCriteria criteria)
        {
            var activities = new List<SuspiciousActivity>();

            // Large data exports
            var exports = await _context.AuditLogs
                .Where(a => a.Timestamp >= criteria.StartDate && 
                           a.Timestamp <= criteria.EndDate &&
                           a.Action == "EXPORT")
                .ToListAsync();

            foreach (var export in exports)
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(export.AdditionalInfo ?? "{}");
                if (metadata.ContainsKey("recordCount") && 
                    int.Parse(metadata["recordCount"].ToString()) > 100)
                {
                    activities.Add(new SuspiciousActivity
                    {
                        ActivityType = "Large Data Export",
                        Description = $"User exported {metadata["recordCount"]} records",
                        UserId = export.UserId ?? 0,
                        Timestamp = export.Timestamp,
                        ThreatScore = 0.7,
                        Indicators = new List<string> { "Data Exfiltration" }
                    });
                }
            }

            return activities;
        }

        private async Task<List<SuspiciousActivity>> HuntPrivilegeEscalationAsync(HuntingCriteria criteria)
        {
            var activities = new List<SuspiciousActivity>();

            // Unauthorized access attempts
            var accessDenied = await _context.AuditLogs
                .Where(a => a.Timestamp >= criteria.StartDate && 
                           a.Timestamp <= criteria.EndDate &&
                           !a.IsSuccess &&
                           a.FailureReason != null &&
                           a.FailureReason.Contains("Unauthorized"))
                .ToListAsync();

            foreach (var attempt in accessDenied)
            {
                activities.Add(new SuspiciousActivity
                {
                    ActivityType = "Unauthorized Access Attempt",
                    Description = $"Attempted to access {attempt.EntityType}",
                    UserId = attempt.UserId ?? 0,
                    Timestamp = attempt.Timestamp,
                    ThreatScore = 0.6,
                    Indicators = new List<string> { "Privilege Escalation" }
                });
            }

            return activities;
        }

        private async Task<List<SuspiciousActivity>> HuntIOCMatchesAsync(HuntingCriteria criteria)
        {
            var activities = new List<SuspiciousActivity>();
            var indicators = await GetIndicatorsAsync();

            foreach (var indicator in indicators)
            {
                switch (indicator.IndicatorType)
                {
                    case "IP":
                        var ipMatches = await _context.LoginAttempts
                            .Where(l => l.IpAddress == indicator.Value &&
                                       l.AttemptedAt >= criteria.StartDate &&
                                       l.AttemptedAt <= criteria.EndDate)
                            .ToListAsync();

                        foreach (var match in ipMatches)
                        {
                            activities.Add(new SuspiciousActivity
                            {
                                ActivityType = "IOC Match - Malicious IP",
                                Description = $"Activity from known malicious IP: {indicator.Value}",
                                Timestamp = match.AttemptedAt,
                                ThreatScore = 0.9,
                                Indicators = new List<string> { "Known Threat" }
                            });
                        }
                        break;
                }
            }

            return activities;
        }
    }
}