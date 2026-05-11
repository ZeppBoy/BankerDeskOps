namespace BankerDeskOps.Application.Jobs
{
    public sealed class SystemBusinessDateProvider : IBusinessDateProvider
    {
        public DateOnly GetBusinessDate() => DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
