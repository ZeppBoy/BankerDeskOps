using BankerDeskOps.Application.Jobs;
using BankerDeskOps.Jobs.Tests.Fakes;

namespace BankerDeskOps.Jobs.Tests
{
    public class BusinessDateProviderTests
    {
        [Fact]
        public void FakeBusinessDateProvider_ReturnsSetDate()
        {
            var expected = new DateOnly(2026, 5, 11);
            var provider = new FakeBusinessDateProvider(expected);

            Assert.Equal(expected, provider.GetBusinessDate());
        }

        [Fact]
        public void FakeBusinessDateProvider_DateCanBeChanged()
        {
            var provider = new FakeBusinessDateProvider(new DateOnly(2026, 1, 1));
            provider.Date = new DateOnly(2026, 6, 30);

            Assert.Equal(new DateOnly(2026, 6, 30), provider.GetBusinessDate());
        }

        [Fact]
        public void SystemBusinessDateProvider_ReturnsToday()
        {
            var provider = new SystemBusinessDateProvider();
            var result = provider.GetBusinessDate();

            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), result);
        }
    }
}
