// IBehavioralBiometricsService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IBehavioralBiometricsService
    {
        Task RecordBehaviorAsync(int userId, BehaviorData data);
        Task<BehaviorProfile> GetUserProfileAsync(int userId);
        Task<BehaviorAnalysis> AnalyzeBehaviorAsync(int userId, BehaviorData data);
        Task<bool> IsSuspiciousBehaviorAsync(BehaviorAnalysis analysis);
    }
    
    public class BehaviorData
    {
        public List<KeystrokeDynamics> Keystrokes { get; set; }
        public MouseMovement MouseData { get; set; }
        public TouchBehavior TouchData { get; set; }
        public NavigationPattern Navigation { get; set; }
    }
    
    public class KeystrokeDynamics
    {
        public string Key { get; set; }
        public double PressTime { get; set; }
        public double ReleaseTime { get; set; }
        public double FlightTime { get; set; }
    }
    
    public class MouseMovement
    {
        public List<MousePoint> Points { get; set; }
        public double AverageSpeed { get; set; }
        public double Acceleration { get; set; }
    }
    
    public class MousePoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public long Timestamp { get; set; }
    }
    
    public class TouchBehavior
    {
        public double AveragePressure { get; set; }
        public double AverageDuration { get; set; }
        public List<TouchPoint> Points { get; set; }
    }
    
    public class TouchPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double Pressure { get; set; }
        public long Timestamp { get; set; }
    }
    
    public class NavigationPattern
    {
        public List<string> VisitedPages { get; set; }
        public double AverageTimePerPage { get; set; }
        public int BackButtonClicks { get; set; }
    }
    
    public class BehaviorProfile
    {
        public double TypingSpeed { get; set; }
        public double TypingRhythm { get; set; }
        public double MouseSpeed { get; set; }
        public string NavigationStyle { get; set; }
    }
    
    public class BehaviorAnalysis
    {
        public double SimilarityScore { get; set; }
        public List<string> Anomalies { get; set; }
        public bool RequiresVerification { get; set; }
    }
}