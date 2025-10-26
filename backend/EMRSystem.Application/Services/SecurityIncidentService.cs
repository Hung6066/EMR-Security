// SecurityIncidentService.cs
using System.Text.Json;

namespace EMRSystem.Application.Services
{
    public class SecurityIncidentService : ISecurityIncidentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SecurityIncidentService> _logger;
        private readonly ISoarService _soarService;

        public SecurityIncidentService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<SecurityIncidentService> logger,
            ISoarService soarService)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _soarService = soarService;
        }

        public async Task<SecurityIncident> CreateIncidentAsync(CreateIncidentDto dto)
        {
            var incident = new SecurityIncident
            {
                Title = dto.Title,
                Description = dto.Description,
                Severity = dto.Severity,
                Status = "New",
                Category = dto.Category,
                AffectedUserId = dto.AffectedUserId,
                AffectedResource = dto.AffectedResource,
                IpAddress = dto.IpAddress,
                DetectedAt = DateTime.Now,
                IsAutoDetected = true,
                Evidence = JsonSerializer.Serialize(dto.Evidence)
            };

            _context.SecurityIncidents.Add(incident);
            await _context.SaveChangesAsync();

            // >>> KÍCH HOẠT SOAR <<<
            await _soarService.HandleIncident(incident);

            // Execute automated response
            await ExecuteAutomatedResponseAsync(incident);

            // Notify security team
            await NotifySecurityTeamAsync(incident);

            _logger.LogWarning($"Security incident created: {incident.Title} (Severity: {incident.Severity})");

            return incident;
        }

        public async Task<SecurityIncident> GetIncidentByIdAsync(long id)
        {
            return await _context.SecurityIncidents
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .Include(i => i.Actions)
                    .ThenInclude(a => a.PerformedBy)
                .Include(i => i.AffectedUser)
                .Include(i => i.AssignedToUser)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<SecurityIncident>> GetActiveIncidentsAsync()
        {
            return await _context.SecurityIncidents
                .Where(i => i.Status != "Closed")
                .OrderByDescending(i => i.Severity)
                .ThenByDescending(i => i.DetectedAt)
                .ToListAsync();
        }

        public async Task UpdateIncidentStatusAsync(long id, string status, string notes)
        {
            var incident = await _context.SecurityIncidents.FindAsync(id);
            if (incident == null) throw new Exception("Incident not found");

            var oldStatus = incident.Status;
            incident.Status = status;

            switch (status)
            {
                case "Investigating":
                    incident.AcknowledgedAt = DateTime.Now;
                    break;
                case "Resolved":
                    incident.ResolvedAt = DateTime.Now;
                    incident.Resolution = notes;
                    break;
                case "Closed":
                    incident.ClosedAt = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();

            // Log status change
            await AddActionAsync(id, new IncidentActionDto
            {
                ActionType = "Status Change",
                Description = $"Status changed from {oldStatus} to {status}. {notes}",
                PerformedByUserId = 1 // System or current user
            });
        }

        public async Task AssignIncidentAsync(long id, int userId)
        {
            var incident = await _context.SecurityIncidents.FindAsync(id);
            if (incident == null) throw new Exception("Incident not found");

            incident.AssignedToUserId = userId;
            await _context.SaveChangesAsync();

            // Notify assigned user
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    $"Security Incident Assigned: {incident.Title}",
                    $"You have been assigned to security incident #{incident.Id}. Severity: {incident.Severity}"
                );
            }
        }

        public async Task AddCommentAsync(long incidentId, int userId, string comment)
        {
            var incidentComment = new IncidentComment
            {
                IncidentId = incidentId,
                UserId = userId,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.IncidentComments.Add(incidentComment);
            await _context.SaveChangesAsync();
        }

        public async Task AddActionAsync(long incidentId, IncidentActionDto action)
        {
            var incidentAction = new IncidentAction
            {
                IncidentId = incidentId,
                ActionType = action.ActionType,
                Description = action.Description,
                PerformedByUserId = action.PerformedByUserId,
                PerformedAt = DateTime.Now,
                Result = action.Result
            };

            _context.IncidentActions.Add(incidentAction);
            await _context.SaveChangesAsync();
        }

        public async Task<IncidentPlaybook> GetPlaybookAsync(string category, string severity)
        {
            return await _context.IncidentPlaybooks
                .FirstOrDefaultAsync(p => p.IncidentCategory == category && 
                                         p.Severity == severity &&
                                         p.IsActive);
        }

        public async Task<bool> ExecuteAutomatedResponseAsync(SecurityIncident incident)
        {
            var playbook = await GetPlaybookAsync(incident.Category, incident.Severity);
            if (playbook == null || string.IsNullOrEmpty(playbook.AutomatedActions))
                return false;

            try
            {
                var actions = JsonSerializer.Deserialize<List<AutomatedAction>>(playbook.AutomatedActions);
                
                foreach (var action in actions)
                {
                    await ExecuteActionAsync(incident, action);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing automated response for incident {incident.Id}");
                return false;
            }
        }

        public async Task<IncidentMetrics> GetIncidentMetricsAsync(DateTime startDate, DateTime endDate)
        {
            var incidents = await _context.SecurityIncidents
                .Where(i => i.DetectedAt >= startDate && i.DetectedAt <= endDate)
                .ToListAsync();

            var resolved = incidents.Where(i => i.Status == "Resolved" || i.Status == "Closed").ToList();

            var metrics = new IncidentMetrics
            {
                TotalIncidents = incidents.Count,
                CriticalIncidents = incidents.Count(i => i.Severity == "Critical"),
                ResolvedIncidents = resolved.Count,
                AverageResolutionTime = resolved
                    .Where(i => i.ResolvedAt.HasValue)
                    .Average(i => (i.ResolvedAt.Value - i.DetectedAt).TotalHours),
                IncidentsByCategory = incidents
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                IncidentsBySeverity = incidents
                    .GroupBy(i => i.Severity)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return metrics;
        }

        private async Task ExecuteActionAsync(SecurityIncident incident, AutomatedAction action)
        {
            switch (action.Type)
            {
                case "BlockIP":
                    if (!string.IsNullOrEmpty(incident.IpAddress))
                    {
                        // Block IP address
                        var blacklist = new IpBlacklist
                        {
                            IpAddress = incident.IpAddress,
                            Reason = $"Auto-blocked due to incident: {incident.Title}",
                            BlockedAt = DateTime.Now,
                            ExpiresAt = DateTime.Now.AddHours(action.DurationHours ?? 24),
                            BlockedBy = 1, // System
                            IsActive = true
                        };
                        _context.IpBlacklists.Add(blacklist);
                    }
                    break;

                case "DisableUser":
                    if (incident.AffectedUserId.HasValue)
                    {
                        var user = await _context.Users.FindAsync(incident.AffectedUserId.Value);
                        if (user != null)
                        {
                            user.IsActive = false;
                            await _emailService.SendEmailAsync(
                                user.Email,
                                "Account Suspended - Security Incident",
                                "Your account has been temporarily suspended due to a security incident."
                            );
                        }
                    }
                    break;

                case "RevokeAllSessions":
                    if (incident.AffectedUserId.HasValue)
                    {
                        var sessions = await _context.UserSessions
                            .Where(s => s.UserId == incident.AffectedUserId.Value && s.IsActive)
                            .ToListAsync();
                        
                        foreach (var session in sessions)
                        {
                            session.IsActive = false;
                        }
                    }
                    break;

                case "NotifyAdmin":
                    await NotifySecurityTeamAsync(incident);
                    break;
            }

            await _context.SaveChangesAsync();

            // Log action
            await AddActionAsync(incident.Id, new IncidentActionDto
            {
                ActionType = action.Type,
                Description = $"Automated action executed: {action.Description}",
                PerformedByUserId = 1, // System
                Result = "Success"
            });
        }

        private async Task NotifySecurityTeamAsync(SecurityIncident incident)
        {
            var securityTeam = await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Security" || ur.Role.Name == "Admin"))
                .ToListAsync();

            foreach (var user in securityTeam)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    $"Security Incident Alert: {incident.Title}",
                    $@"
                        <h2>Security Incident Detected</h2>
                        <p><strong>Severity:</strong> {incident.Severity}</p>
                        <p><strong>Category:</strong> {incident.Category}</p>
                        <p><strong>Description:</strong> {incident.Description}</p>
                        <p><strong>Detected At:</strong> {incident.DetectedAt}</p>
                        <p>Please review and take appropriate action.</p>
                    "
                );
            }
        }
    }

    public class AutomatedAction
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public int? DurationHours { get; set; }
    }
}