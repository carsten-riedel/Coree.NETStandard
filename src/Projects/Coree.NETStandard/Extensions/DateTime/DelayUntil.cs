using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Coree.NETStandard.Extensions
{
    /// <summary>
    /// Contains extension methods for enhancing DateTime functionalities.
    /// </summary>
    public static partial class DateTimeExtensions
    {
        /// <summary>
        /// Asynchronously pauses execution until a specified future point in time.
        /// </summary>
        /// <param name="futureDate">The date and time at which the delay concludes. Execution resumes as soon as this specified moment is reached or exceeded.</param>
        /// <remarks>
        /// This method computes the time remaining until the 'futureDate' and initiates a non-blocking wait. 
        /// If the system's date is adjusted, or if the calculated delay exceeds what <see cref="Task.Delay(int)"/> can handle (Int32.MaxValue milliseconds),
        /// it recalculates and continues delaying. The method uses a default check interval of one minute, limiting the delay precision to roughly one minute.
        /// For more precise delay intervals, consider adjusting the check interval.
        /// </remarks>
        /// <example>
        /// Example to pause execution until 5 minutes into the future:
        /// <code>
        /// DateTime futureTime = DateTime.Now.AddMinutes(5);
        /// await futureTime.DelayUntil();
        /// </code>
        /// </example>
        public static async Task DelayUntil(this DateTime futureDate)
        {
            const int checkIntervalMillis = 60000;  // 60,000 milliseconds = 1 minute
            const int maxDelayMillis = Int32.MaxValue;  // Maximum delay per single Task.Delay call

            while (DateTime.Now < futureDate)
            {
                var now = DateTime.Now;
                var timeSpanRemaining = futureDate - now;

                // Calculate the next delay, not exceeding one minute
                int nextDelayMillis = (int)Math.Min(timeSpanRemaining.TotalMilliseconds, checkIntervalMillis);

                // Ensure the delay does not exceed Int32.MaxValue
                nextDelayMillis = Math.Min(nextDelayMillis, maxDelayMillis);

                if (nextDelayMillis > 0)
                {
                    await Task.Delay(nextDelayMillis);
                }
            }
        }

    }
}
