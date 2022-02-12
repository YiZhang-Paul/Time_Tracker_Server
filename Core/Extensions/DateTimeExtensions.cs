using System;

namespace Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToKindUtc(this DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        public static DateTime? ToKindUtc(this DateTime? date)
        {
            if (!date.HasValue || date.Value.Kind == DateTimeKind.Utc)
            {
                return date;
            }

            return DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
        }
    }
}
