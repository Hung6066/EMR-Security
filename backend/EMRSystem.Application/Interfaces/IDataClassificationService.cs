public interface IDataClassificationService
{
    Task<List<ClassificationLabel>> GetLabelsAsync();
    Task<ClassificationLabel> UpsertLabelAsync(ClassificationLabel label);
    Task AssignLabelAsync(string type, long id, int labelId, string? reason);
    Task<List<EntityTag>> GetTagsAsync(string type, long id);
    Task AddTagAsync(string type, long id, string tag);
}