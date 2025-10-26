// Core/Entities/Security/UebaProfile.cs
public class UserBehaviorProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Baseline hành vi
    public string TypicalLoginHours { get; set; } // JSON: {"hour": count}
    public string TypicalIpSubnets { get; set; } // JSON: ["192.168.1.0/24", ...]
    public string FrequentActions { get; set; } // JSON: {"action": count}
    public string FrequentResources { get; set; } // JSON: {"resource": count}
    public double AvgActionsPerSession { get; set; }
    
    public DateTime LastUpdatedAt { get; set; }
    public ApplicationUser User { get; set; }
}

public class UebaAlert
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string AlertType { get; set; } // LoginTime, Location, ActionSequence, DataVolume
    public string Description { get; set; }
    public double DeviationScore { get; set; }
    public DateTime DetectedAt { get; set; }
    public string Context { get; set; } // JSON
}