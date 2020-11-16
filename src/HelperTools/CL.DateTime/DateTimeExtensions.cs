using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateTimeCL.Extensions
{
    public static partial class DateTimeExtensions
    {
        #region Week
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var _firstDayOfWeek = DayOfWeek.Monday;  //CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek = 
            var diff = dt.DayOfWeek - _firstDayOfWeek;
            if (diff < 0) diff += 7;

            return dt.AddDays(-diff);
        }
        public static DateTime LastDayOfWeek(this DateTime dt) => dt.FirstDayOfWeek().AddDays(6);
        public static DateTime FirstDayOfLastWeek(this DateTime dt) => dt.FirstDayOfWeek().AddDays(-7);
        public static DateTime LastDayOfLastWeek(this DateTime dt) => dt.FirstDayOfWeek().AddDays(-1);
        public static DateTime FirstDayOfNextWeek(this DateTime dt) => dt.LastDayOfWeek().AddDays(1);
        public static DateTime LastDayOfNextWeek(this DateTime dt) => dt.FirstDayOfNextWeek().AddDays(6);

        /// <summary>
        /// Returns week number of the year using ISO week date standard (ISO-8601)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int IsoWeek(this DateTime dt)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) dt = dt.AddDays(3);

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Returns week number of the year.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="calendarWeekRule"></param>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        public static int Week(this DateTime dt, CalendarWeekRule calendarWeekRule = CalendarWeekRule.FirstFourDayWeek, DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, calendarWeekRule, dayOfWeek);
        }
        #endregion

        #region Month
        public static DateTime FirstDayOfMonth(this DateTime dt) => new DateTime(dt.Year, dt.Month, 1);
        public static DateTime LastDayOfMonth(this DateTime dt) => dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);
        public static DateTime FirstDayOfLastMonth(this DateTime dt) => dt.FirstDayOfMonth().AddMonths(-1);
        public static DateTime LastDayOfLastMonth(this DateTime dt) => dt.FirstDayOfMonth().AddDays(-1);
        public static DateTime FirstDayOfNextMonth(this DateTime dt) => dt.FirstDayOfMonth().AddMonths(1);
        public static DateTime LastDayOfNextMonth(this DateTime dt) => dt.FirstDayOfNextMonth().AddMonths(1).AddDays(-1);
        #endregion

        #region Year
        public static DateTime FirstDayOfYear(this DateTime dt) => new DateTime(dt.Year, 1, 1);
        public static DateTime LastDayOfYear(this DateTime dt) => dt.FirstDayOfYear().AddYears(1).AddDays(-1);
        public static DateTime FirstDayOfLastYear(this DateTime dt) => dt.FirstDayOfYear().AddYears(-1);
        public static DateTime LastDayOfLastYear(this DateTime dt) => dt.FirstDayOfYear().AddDays(-1);
        public static DateTime FirstDayOfNextYear(this DateTime dt) => dt.LastDayOfYear().AddDays(1);
        public static DateTime LastDayOfNextYear(this DateTime dt) => dt.LastDayOfYear().AddYears(1);
        #endregion

        #region Relative Time Span
        /// <summary>
        /// Formats relative date from the current date. For example, by formatting a past date as 'x days ago' or today as 'today'
        /// </summary>
        /// <param name="x"></param>
        /// <param name="descriptorPast"></param>
        /// <param name="descriptorFuture"></param>
        /// <returns></returns>
        public static (double timeSpan, string relativePeriod, string prettify) RelativeTimeSpan(this DateTime x, string descriptorPast = "ago", string descriptorFuture = "in")
        {
            var isInFuture = x > DateTime.Now;
            var descriptor = isInFuture ? descriptorFuture : descriptorPast;
            var i = isInFuture ? x.Subtract(DateTime.Now) : DateTime.Now.Subtract(x);

            (double, string) ts = i switch
            {
                TimeSpan t when Math.Round(t.TotalDays, 0) > 365.2425 => (Math.Round(t.TotalDays, 0) / 365.2425, "Year"),
                TimeSpan t when Math.Round(t.TotalDays, 0) > 365.2425 / 12 => (Math.Round(t.TotalDays, 0) / (365.2425 / 12), "Month"),
                TimeSpan t when Math.Round(t.TotalDays, 0) >= 1 && Math.Round(t.TotalDays, 0) <= 365.2425 / 12 && Math.Round(t.TotalDays, 0) % 7 == 0 => (Math.Round(t.TotalDays, 0) / 7, "Week"),
                TimeSpan t when Math.Round(t.TotalDays, 0) >= 1 && Math.Round(t.TotalDays, 0) < 365.2425 / 12 => (Math.Round(t.TotalDays, 0), "Day"),
                TimeSpan t when Math.Round(t.TotalHours, 0) < 24 && Math.Round(t.TotalMinutes, 0) >= 60 => (Math.Round(t.TotalHours, 0), "Hour"),
                TimeSpan t when Math.Round(t.TotalMinutes, 0) < 60 && Math.Round(t.TotalSeconds, 0) >= 60 => (Math.Round(t.TotalMinutes, 0), "Minute"),
                TimeSpan t when Math.Round(t.TotalSeconds, 0) < 60 => (Math.Round(t.TotalSeconds, 0), "Second"),
                _ => (i.TotalMilliseconds, "Milliseconds"),
            };

            ts.Item1 = Math.Round(ts.Item1, 0);

            return (ts.Item1, ts.Item2, ToPrettyTimeSpan(x, ts.Item1, ts.Item2, descriptor, isInFuture));
        }

        public static string ToPrettyTimeSpan(DateTime dt, double timeSpan, string relativePeriod, string descriptor, bool isInFuture)
        {
            var relativePeriodQuantified = timeSpan == 1 ? relativePeriod : $"{relativePeriod}s";
            var str = isInFuture ? $"{descriptor} {timeSpan} {relativePeriodQuantified}" : $"{timeSpan} {relativePeriodQuantified} {descriptor}";

            if (timeSpan == 1 && relativePeriod == "Day") str = isInFuture ? "tomorrow" : "yesterday";
            if (timeSpan == 0 && relativePeriod == "Second") str = "just now";
            if (timeSpan == 1 && relativePeriod == "Year")
            {
                if (DateTime.Now.FirstDayOfNextYear() <= dt && dt <= DateTime.Now.LastDayOfNextYear()) str = "next year";
                if (DateTime.Now.FirstDayOfLastYear() <= dt && dt <= DateTime.Now.LastDayOfLastYear()) str = "last year";
            }

            return str;
        }
        #endregion
    }
}
