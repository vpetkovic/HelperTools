using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeCL
{
    public class DateRangeBuilder
    {
        private DateTime _start;
        private DateTime _end;
        private readonly DateRangeSettings _dateRange = new DateRangeSettings();

        /// <summary>
        /// Generates range of dates between two dates or for period of time with optional exclusion of the start or end or both start and end date
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="dateRange"></param>
        public DateRangeBuilder(DateTime? start, DateTime? end, DateRangeSettings dateRange)
        {
            _start = start ?? DateTime.Parse(DateTime.Today.ToString("yyyy-MM-dd H:mm:ss"));
            _end = (end < _start ? _start : end) ?? _start;
            _dateRange = start != null && end != null ? new DateRangeSettings() { Boundary = dateRange.Boundary } : dateRange;

            switch (_dateRange.Boundary)
            {
                case DateRangeBoundary.none:
                    break;
                case DateRangeBoundary.left:
                    _end = _end.AddDays(-1);
                    break;
                case DateRangeBoundary.right:
                    _start = _start.AddDays(1);
                    break;
                case DateRangeBoundary.both:
                    _start = _start.AddDays(1);
                    _end = _end.AddDays(-1);
                    break;
            }
        }

        /// <summary>
        /// Generates range of dates between two dates or for period of time with optional exclusion of the start or end or both start and end date
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public DateRangeBuilder(DateTime start, DateTime end)
        {
            _start = start;
            _end = end;
        }

        private Func<DateTime, int, DateRangeSettings, DateTime> GetDate = (dt, arithmetic, range) =>
        {
            return _ = range.Frequency switch
            {
                DateRangeFrequency.D => dt.AddDays(arithmetic * range.Periods),
                DateRangeFrequency.W => dt.AddDays(arithmetic * (7 * range.Periods)),
                DateRangeFrequency.M => dt.AddMonths(arithmetic * range.Periods),
                DateRangeFrequency.Y => dt.AddYears(arithmetic * range.Periods),
                _ => dt.AddDays(arithmetic * range.Periods),
            };
        };

        public DateTime startDate => GetDate(_start, -1, _dateRange);
        public DateTime endDate => _end;

        public List<DateTime> dates => Enumerable.Range(0, (this.endDate - this.startDate).Days + 1).Select(offset => this.startDate.AddDays(offset)).ToList();

    }

    /// <summary>
    /// Specifies period of time for which builder should generate date range for
    /// </summary>
    public class DateRangeSettings
    {
        public int Periods { get; set; } = 0;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRangeFrequency Frequency { get; set; } = DateRangeFrequency.D;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRangeBoundary Boundary { get; set; } = DateRangeBoundary.none;
    }

    public enum DateRangeFrequency
    {
        D = 0,
        W = 1,
        M = 2,
        Y = 3
    }

    /// <summary>
    /// Make the interval closed with respect to the given frequency to the ‘left’, ‘right’, or both sides (None, the default).
    /// Use boundary='right' to exclude start if it falls on the boundary;  Use boundary='left' to exclude end if it falls on the boundary
    /// </summary>
    public enum DateRangeBoundary
    {
        none,
        left,
        right,
        both
    }
}
