public class DataClassificationService : IDataClassificationService
{
    private readonly ApplicationDbContext _ctx;
    public DataClassificationService(ApplicationDbContext ctx) { _ctx = ctx; }

    public Task<List<ClassificationLabel>> GetLabelsAsync() =>
        _ctx.ClassificationLabels.Where(x => x.IsActive).OrderBy(x => x.Level).ToListAsync();

    public async Task<ClassificationLabel> UpsertLabelAsync(ClassificationLabel label)
    {
        if (label.Id == 0) _ctx.ClassificationLabels.Add(label);
        else _ctx.ClassificationLabels.Update(label);
        await _ctx.SaveChangesAsync();
        return label;
    }

    public async Task AssignLabelAsync(string type, long id, int labelId, string? reason)
    {
        _ctx.EntityClassifications.Add(new EntityClassification
        {
            ResourceType = type, ResourceId = id, LabelId = labelId,
            Reason = reason, ClassifiedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();
    }

    public Task<List<EntityTag>> GetTagsAsync(string type, long id) =>
        _ctx.EntityTags.Where(t => t.ResourceType == type && t.ResourceId == id)
            .OrderByDescending(t => t.TaggedAt).ToListAsync();

    public async Task AddTagAsync(string type, long id, string tag)
    {
        _ctx.EntityTags.Add(new EntityTag { ResourceType = type, ResourceId = id, Tag = tag, TaggedAt = DateTime.UtcNow });
        await _ctx.SaveChangesAsync();
    }
}