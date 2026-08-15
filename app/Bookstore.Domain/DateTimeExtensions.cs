namespace Bookstore.Domain
{
    public static class DateTimeExtensions
    {
        public static DateTime OneSecondToMidnight(this DateTime dateTime)
        {
            return dateTime.Date.AddSeconds(86399);
        }

        // RULE-PERIOD-02, revised by ISSUE-24: the first moment of the month, not the same time of
        // day on the first. Taken at 14:00 on the 15th, the old form began "this month" at 14:00
        // on the 1st and dropped that morning's records — now that money is counted per month,
        // that is a missing morning of takings.
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
        }
    }
}
