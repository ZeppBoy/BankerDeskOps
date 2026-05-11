namespace BankerDeskOps.Application.Jobs
{
    public interface IBusinessDateProvider
    {
        DateOnly GetBusinessDate();
    }
}
