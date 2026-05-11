namespace BankerDeskOps.Infrastructure.Jobs
{
    public class JobExecution
    {
        public Guid Id { get; set; }
        public string JobName { get; set; } = string.Empty;
        public DateOnly BusinessDate { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public JobExecutionStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
