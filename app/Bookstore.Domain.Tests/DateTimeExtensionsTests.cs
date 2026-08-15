namespace Bookstore.Domain.Tests
{
    public class DateTimeExtensionsTests
    {
        // The month begins at midnight on the first. If it began at the same time of day the
        // question was asked, anything recorded earlier on the first would fall outside
        // "this month".
        [Fact]
        public void StartOfMonth_IsMidnightOnTheFirstOfTheMonth_When_Executed()
        {
            var dateTime = new DateTime(2026, 8, 15, 14, 30, 45, DateTimeKind.Utc);

            Assert.Equal(new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), dateTime.StartOfMonth());
        }

        [Fact]
        public void StartOfMonth_IncludesRecordsFromTheMorningOfTheFirst_When_AskedLaterInTheDay()
        {
            var askedAt = new DateTime(2026, 8, 15, 14, 0, 0, DateTimeKind.Utc);
            var recordedAt = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);

            Assert.True(recordedAt >= askedAt.StartOfMonth());
        }

        [Fact]
        public void StartOfMonth_KeepsTheKindOfTheGivenTime_When_Executed()
        {
            var dateTime = new DateTime(2026, 8, 15, 14, 30, 45, DateTimeKind.Utc);

            Assert.Equal(DateTimeKind.Utc, dateTime.StartOfMonth().Kind);
        }
    }
}
