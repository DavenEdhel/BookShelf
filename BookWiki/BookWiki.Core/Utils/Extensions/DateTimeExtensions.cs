using System;

namespace Keurig.IQ.Core.CrossCutting.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime SetDateToNow(this DateTime value, int? hours = null, int? minutes = null, int? seconds = null)
        {
            return new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                hours ?? value.Hour,
                minutes ?? value.Minute,
                seconds ?? value.Second);
        }

        public static DateTime SetDateToBeFuture(this DateTime value, int? hours = null, int? minutes = null, int? seconds = null)
        {
            var nowDate = SetDateToNow(value, seconds: 0);

            if (nowDate < DateTime.Now)
            {
                return nowDate.AddDays(1);
            }

            return nowDate;
        }

        public static bool InRange(this DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck >= startDate && dateToCheck < endDate;
        }
    }
}