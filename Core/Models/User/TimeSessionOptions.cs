namespace Core.Models.User
{
    public class TimeSessionOptions
    {
        public int DailyWorkDuration { get; set; } = 8 * 60 * 60 * 1000;
        public int WorkSessionDuration { get; set; } = 50 * 60 * 1000;
        public int BreakSessionDuration { get; set; } = 10 * 60 * 1000;
    }
}
