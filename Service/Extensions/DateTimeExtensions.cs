using System;

namespace Service.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime SpecifyKindUtc(this DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }
}
