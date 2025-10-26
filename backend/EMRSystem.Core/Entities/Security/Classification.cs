public class ClassificationLabel
{
    public int Id { get; set; }
    public string Name { get; set; } // Public/Internal/PII/PHI/Confidential
    public int Level { get; set; } // 1..5
    public string Color { get; set; } // #hex
    public bool IsActive { get; set; } = true;
}

public class EntityClassification
{
    public long Id { get; set; }
    public string ResourceType { get; set; } // Patient, MedicalRecord,...
    public long ResourceId { get; set; }
    public int LabelId { get; set; }
    public string? Reason { get; set; }
    public DateTime ClassifiedAt { get; set; }
}

public class EntityTag
{
    public long Id { get; set; }
    public string ResourceType { get; set; }
    public long ResourceId { get; set; }
    public string Tag { get; set; }
    public DateTime TaggedAt { get; set; }
}