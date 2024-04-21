using System;

namespace Coree.NETStandard.Classes
{


    /// <summary>
    /// Represents a specific time of day, independent of any date.
    /// </summary>
    public class TimeOfDay
    {
        private readonly TimeSpan _time;

        /// <summary>
        /// Gets the hour component of the time.
        /// </summary>
        public int Hour => _time.Hours;

        /// <summary>
        /// Gets the minute component of the time.
        /// </summary>
        public int Minute => _time.Minutes;

        /// <summary>
        /// Gets the second component of the time.
        /// </summary>
        public int Second => _time.Seconds;

        /// <summary>
        /// Gets the millisecond component of the time.
        /// </summary>
        public int Millisecond => _time.Milliseconds;

        /// <summary>
        /// Gets the number of ticks that represent the time.
        /// </summary>
        public long Ticks => _time.Ticks;

        /// <summary>
        /// Initializes a new instance of the TimeOfDay class to a specified number of hours, minutes, seconds, and milliseconds.
        /// </summary>
        /// <param name="hour">The hour component of the time.</param>
        /// <param name="minute">The minute component of the time.</param>
        /// <param name="second">The second component of the time.</param>
        /// <param name="millisecond">The millisecond component of the time.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified time components are out of their expected range.</exception>
        public TimeOfDay(int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
        {
            _time = new TimeSpan(0, hour, minute, second, millisecond);
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999)
                throw new ArgumentOutOfRangeException("Invalid time specified");
        }

        /// <summary>
        /// Creates a TimeOfDay from a TimeSpan object.
        /// </summary>
        /// <param name="time">The TimeSpan object to convert.</param>
        /// <returns>A TimeOfDay object.</returns>
        public static TimeOfDay FromTimeSpan(TimeSpan time)
        {
            return new TimeOfDay(time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        /// <summary>
        /// Converts a string representation of a time into a TimeOfDay object.
        /// </summary>
        /// <param name="timeString">A string that represents the time in "hh:mm:ss" format.</param>
        /// <returns>A TimeOfDay object.</returns>
        /// <exception cref="FormatException">Thrown when the time string is not in a valid format.</exception>
        public static TimeOfDay FromTimeString(string timeString)
        {
            if (TimeSpan.TryParseExact(timeString, "hh\\:mm\\:ss", null, out TimeSpan parsedTime))
            {
                return FromTimeSpan(parsedTime);
            }
            throw new FormatException("Invalid time string format. Could not parse the string into a time.");
        }

        /// <summary>
        /// Creates a TimeOfDay from individual time components.
        /// </summary>
        /// <param name="hours">Hour component.</param>
        /// <param name="minutes">Minute component.</param>
        /// <param name="seconds">Second component.</param>
        /// <param name="milliseconds">Millisecond component.</param>
        /// <returns>A new TimeOfDay instance.</returns>
        public static TimeOfDay FromTime(int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            return new TimeOfDay(hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        /// Converts this TimeOfDay instance to a TimeSpan.
        /// </summary>
        /// <returns>A TimeSpan that represents the time.</returns>
        public TimeSpan ToTimeSpan()
        {
            return _time;
        }

        /// <summary>
        /// Adds a specified number of seconds to this TimeOfDay instance.
        /// </summary>
        /// <param name="duration">The number of seconds to add.</param>
        /// <returns>A new TimeOfDay instance representing the added time.</returns>
        public TimeOfDay AddSeconds(double duration)
        {
            return Add(TimeSpan.FromSeconds(duration));
        }

        /// <summary>
        /// Adds a specified number of minutes to this TimeOfDay instance.
        /// </summary>
        /// <param name="duration">The number of minutes to add.</param>
        /// <returns>A new TimeOfDay instance representing the added time.</returns>
        public TimeOfDay AddMinutes(double duration)
        {
            return Add(TimeSpan.FromMinutes(duration));
        }

        /// <summary>
        /// Adds a specified number of hours to this TimeOfDay instance.
        /// </summary>
        /// <param name="duration">The number of hours to add.</param>
        /// <returns>A new TimeOfDay instance representing the added time.</returns>
        public TimeOfDay AddHours(double duration)
        {
            return Add(TimeSpan.FromHours(duration));
        }

        /// <summary>
        /// Adds a TimeSpan to this TimeOfDay instance.
        /// </summary>
        /// <param name="duration">The TimeSpan to add.</param>
        /// <returns>A new TimeOfDay instance representing the added time.</returns>
        public TimeOfDay Add(TimeSpan duration)
        {
            TimeSpan newTime = _time.Add(duration);
            newTime = new TimeSpan(newTime.Ticks % TimeSpan.TicksPerDay);
            return FromTimeSpan(newTime);
        }

        /// <summary>
        /// Subtracts a TimeSpan from this TimeOfDay instance.
        /// </summary>
        /// <param name="duration">The TimeSpan to subtract.</param>
        /// <returns>A new TimeOfDay instance representing the subtracted time.</returns>
        public TimeOfDay Subtract(TimeSpan duration)
        {
            TimeSpan newTime = _time.Subtract(duration);
            if (newTime < TimeSpan.Zero)
            {
                newTime += TimeSpan.FromDays(1);
            }
            return FromTimeSpan(newTime);
        }

        /// <summary>
        /// Defines an implicit conversion of a string to a TimeOfDay.
        /// </summary>
        /// <param name="timeString">The string to convert to TimeOfDay.</param>
        /// <returns>A TimeOfDay equivalent to the time contained in the string.</returns>
        public static implicit operator TimeOfDay(string timeString)
        {
            return FromTimeString(timeString);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object in "hh:mm:ss" format.</returns>
        public override string ToString()
        {
            return _time.ToString("hh\\:mm\\:ss");
        }
    }

}
