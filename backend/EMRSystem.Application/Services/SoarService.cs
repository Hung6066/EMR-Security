public class SoarService : ISoarService // Tạo interface này
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SoarService> _logger;

    public SoarService(IServiceProvider services, ILogger<SoarService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task HandleIncident(SecurityIncident incident)
    {
        _logger.LogInformation("SOAR: Starting playbook for incident #{IncidentId} of category '{Category}' and severity '{Severity}'", incident.Id, incident.Category, incident.Severity);

        using var scope = _services.CreateScope();
        var playbook = await GetPlaybookForIncident(incident, scope);
        if (playbook == null) {
            _logger.LogWarning("SOAR: No playbook found for incident #{IncidentId}", incident.Id);
            return;
        }

        _logger.LogInformation("SOAR: Executing playbook '{PlaybookName}'", playbook.Name);

        foreach (var step in playbook.Steps)
        {
            await ExecutePlaybookStep(incident, step, scope);
        }
    }

    private async Task ExecutePlaybookStep(SecurityIncident incident, PlaybookStep step, IServiceScope scope)
    {
        var incidentService = scope.ServiceProvider.GetRequiredService<ISecurityIncidentService>();
        var threatIntelService = scope.ServiceProvider.GetRequiredService<IThreatIntelligenceService>();
        var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();

        _logger.LogInformation("SOAR: Executing step '{Action}' for incident #{IncidentId}", step.Action, incident.Id);

        try
        {
            switch (step.Action)
            {
                case "Enrich_IP_Info":
                    if (!string.IsNullOrEmpty(incident.IpAddress))
                    {
                        var assessment = await threatIntelService.AssessIpAddressAsync(incident.IpAddress);
                        await incidentService.AddCommentAsync(incident.Id, 1, // System User
                            $"IP Enrichment: Score={assessment.ThreatScore}, Country={assessment.CountryCode}, VPN={assessment.IsVPN}");
                    }
                    break;
                
                case "Contain_BlockIP":
                    if (!string.IsNullOrEmpty(incident.IpAddress))
                    {
                        await threatIntelService.BlockIpAddressAsync(incident.IpAddress, $"SOAR: Auto-block for incident #{incident.Id}", TimeSpan.FromDays(step.DurationDays ?? 1));
                        await incidentService.AddActionAsync(incident.Id, new IncidentActionDto { ActionType = "Containment", Description = $"IP {incident.IpAddress} blocked for {step.DurationDays ?? 1} day(s)." });
                    }
                    break;
                
                case "Contain_RevokeSessions":
                    if (incident.AffectedUserId.HasValue)
                    {
                        await securityService.RevokeAllSessionsAsync(incident.AffectedUserId.Value);
                        await incidentService.AddActionAsync(incident.Id, new IncidentActionDto { ActionType = "Containment", Description = $"All sessions for user #{incident.AffectedUserId} revoked." });
                    }
                    break;
                
                case "Notify_SecurityTeam":
                    await incidentService.NotifySecurityTeamAsync(incident, "SOAR Action Required", step.Details);
                    break;

                default:
                    _logger.LogWarning("SOAR: Unknown playbook action '{Action}'", step.Action);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SOAR: Failed to execute step '{Action}' for incident #{IncidentId}", step.Action, incident.Id);
        }
    }

    private async Task<IncidentPlaybook?> GetPlaybookForIncident(SecurityIncident incident, IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.IncidentPlaybooks
            .FirstOrDefaultAsync(p => p.IncidentCategory == incident.Category && p.Severity == incident.Severity && p.IsActive);
    }
}