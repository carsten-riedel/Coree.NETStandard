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
        /// Asynchronously delays execution until the specified future date and time.
        /// </summary>
        /// <param name="futureDate">The future date and time at which the delay should end. Execution will resume shortly after this time is reached or passed.</param>
        /// <remarks>
        /// This method calculates the remaining time to the specified future date and performs a non-blocking delay.
        /// If the system date changes or if the delay exceeds the maximum interval that <see cref="Task.Delay(int)"/> can handle, 
        /// the method recalculates and continues to delay until the specified date is reached.
        /// The check interval is set to one minute, which means the delay accuracy is about one minute. If you need more precise control over the delay, consider decreasing the interval.
        /// </remarks>
        /// <example>
        /// Using the method to delay execution until 5 minutes from now:
        /// <code>
        /// DateTime futureTime = DateTime.Now.AddMinutes(5);
        /// await DateTimeExtensions.DelayUntil(futureTime);
        /// </code>
        /// </example>
        public static async Task DelayUntil(DateTime futureDate)
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
