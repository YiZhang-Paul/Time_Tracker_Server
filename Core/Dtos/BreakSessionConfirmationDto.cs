namespace Core.Dtos
{
    public class BreakSessionConfirmationDto
    {
        public bool IsSkip { get; set; } = true;
        public int TargetDuration { get; set; } = -1;
    }
}
