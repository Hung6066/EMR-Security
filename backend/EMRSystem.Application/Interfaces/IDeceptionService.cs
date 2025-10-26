public interface IDeceptionService
{
    Task TriggerHoneypotAsync(string honeypotName, string description);
}