using BankerDeskOps.Application.Jobs;

namespace BankerDeskOps.Jobs.Tests.Fakes
{
    public sealed class FakeBusinessDateProvider : IBusinessDateProvider
    {
        public DateOnly Date { get; set; }

        public FakeBusinessDateProvider(DateOnly date) => Date = date;

        public DateOnly GetBusinessDate() => Date;
    }
}
