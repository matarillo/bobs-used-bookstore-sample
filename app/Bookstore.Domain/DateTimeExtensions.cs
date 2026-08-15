namespace Bookstore.Domain
{
    public static class DateTimeExtensions
    {
        public static DateTime OneSecondToMidnight(this DateTime dateTime)
        {
            return dateTime.Date.AddSeconds(86399);
        }

        // The first moment of the month, not the same time of day on the first. Asked at 14:00 on
        // the 15th, "this month" has to begin at midnight on the 1st: beginning it at 14:00 would
        // drop that morning's records, and money is counted per month.
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
        }
    }
}
