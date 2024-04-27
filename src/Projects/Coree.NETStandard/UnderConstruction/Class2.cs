using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Coree.NETStandard.Extensions;
using Coree.NETStandard.Classes;
using Coree.NETStandard.Utilities;
using Coree.NETStandard.Classes.TimeOfDay;

#pragma warning disable

namespace Coree.NETStandard.UnderConstruction
{
    public class Scheduler2
    {
        public class TickerEventArgs : EventArgs
        {
            public TimeSpan Diviation { get; set; }
            public DateTime TickTime { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public string FormattedTickTime { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        }

        public delegate Task TickerEventDelegate(object sender, TickerEventArgs e, CancellationToken cancellationToken);

        private TickerEventDelegate? _tickOccurred;
        private readonly CancellationToken _cancellationToken;

        public event TickerEventDelegate TickOccurred
        {
            add { EventSubscription.AddHandler(ref _tickOccurred, value); }
            remove { EventSubscription.RemoveHandler(ref _tickOccurred, value); }
        }

        protected virtual void RaiseTickEvent(TickerEventArgs args)
        {
            var handler = _tickOccurred;
            if (handler != null)
            {
                foreach (var singleHandler in handler.GetInvocationList().Cast<TickerEventDelegate>())
                {
                    // Start each handler in a new task and ignore the returned Task object.
                    Task.Run(async () => {
                        try
                        {
                            await singleHandler(this, args, _cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle the exception as necessary
                            Console.WriteLine($"Error in event handler: {ex.Message}");
                        }
                    });
                }
            }
        }

        public List<DateTime> schedules { get; set; } = new List<DateTime>();  

        public Scheduler2(CancellationToken? cancellationToken = null,bool startInConstructor = true)
        {
            _cancellationToken = cancellationToken ?? CancellationToken.None;
            if (startInConstructor)
            {
                Task.Run(StartAsync);
            }
        }

        public async Task StartAsync()
        {
            while (!_cancellationToken.IsCancellationRequested && schedules?.Count > 0)
            {
                var nextSchedule = schedules.First();
                await nextSchedule.DelayUntil();  // Ensure DelayUntil extension method is accessible
                RaiseTickEvent(new TickerEventArgs { TickTime = nextSchedule, Diviation = DateTime.Now - nextSchedule, FormattedTickTime = nextSchedule.ToString("MM.dd.yyyy HH:mm:ss.fff") });
                schedules.RemoveAt(0);
            }
        }
    }

    public class Scheduler3
    {
        public class TickerEventArgs : EventArgs
        {
            public TimeSpan Diviation { get; set; }
            public DateTime TickTime { get; set; }
            public string FormattedTickTime { get; set; }
        }

        public delegate Task TickerEventDelegate(object sender, TickerEventArgs e, CancellationToken cancellationToken);

        private TickerEventDelegate? _tickOccurred;
        private readonly CancellationToken _cancellationToken;

        public event TickerEventDelegate TickOccurred
        {
            add { EventSubscription.AddHandler(ref _tickOccurred, value); }
            remove { EventSubscription.RemoveHandler(ref _tickOccurred, value); }
        }

        protected virtual void RaiseTickEvent(TickerEventArgs args)
        {
            var handler = _tickOccurred;
            if (handler != null)
            {
                foreach (var singleHandler in handler.GetInvocationList().Cast<TickerEventDelegate>())
                {
                    // Start each handler in a new task and ignore the returned Task object.
                    Task.Run(async () => {
                        try
                        {
                            await singleHandler(this, args, _cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle the exception as necessary
                            Console.WriteLine($"Error in event handler: {ex.Message}");
                        }
                    });
                }
            }
        }

        private IScheduleProvider ScheduleProvider { get; set; }
        

        public Scheduler3(IScheduleProvider scheduleProvider, CancellationToken? cancellationToken = null, bool startInConstructor = true)
        {
            ScheduleProvider = scheduleProvider;
            _cancellationToken = cancellationToken ?? CancellationToken.None;
            if (startInConstructor)
            {
                Task.Run(StartAsync);
            }
        }

        public async Task StartAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (ScheduleProvider.GetAndCalcNextSchedule(out var nextSchedule))
                {
                    await nextSchedule.DelayUntil();  // Ensure DelayUntil extension method is accessible
                    RaiseTickEvent(new TickerEventArgs { TickTime = nextSchedule, Diviation = DateTime.Now - nextSchedule, FormattedTickTime = nextSchedule.ToString("MM.dd.yyyy HH:mm:ss.fff") });
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }

    public interface IScheduleProvider
    {
        bool GetAndCalcNextSchedule(out DateTime schedule);
        DateTime? PeekNextSchedule();  // Peeks at the next schedule without moving forward
    }

    public class IntervalScheduleProvider : IScheduleProvider
    {
        private DateTime _nextSchedule;
        private readonly TimeSpan _interval;
        private readonly bool _startImmediately;

        public IntervalScheduleProvider(TimeSpan interval, DateTime? startDateTime = null, bool startImmediately = false)
        {
            _interval = interval;
            _startImmediately = startImmediately;
            // Determine the initial next schedule based on the startImmediately flag
            _nextSchedule = startDateTime ?? DateTime.Now;
            if (!_startImmediately)
            {
                _nextSchedule += _interval; // Start the schedule after one interval if not starting immediately
            }
            Console.WriteLine($"[IntervalScheduleProvider] Initialized with next schedule at {_nextSchedule} and interval {_interval}");
        }

        public bool GetAndCalcNextSchedule(out DateTime schedule)
        {
            var now = DateTime.Now;
            if (_nextSchedule <= now)
            {
                // If it's time for the schedule or past due, update and calculate next
                schedule = _nextSchedule;
                _nextSchedule += _interval;
                Console.WriteLine($"[IntervalScheduleProvider] Advanced next schedule to {_nextSchedule}");
                return true;
            }
            else
            {
                // If it's not time yet, just return the current scheduled time without updating
                schedule = _nextSchedule;
                Console.WriteLine($"[IntervalScheduleProvider] Peeked at next schedule: {schedule}");
                return false;
            }
        }

        public DateTime? PeekNextSchedule()
        {
            Console.WriteLine($"[IntervalScheduleProvider] Peeked at next schedule without changing state: {_nextSchedule}");
            return _nextSchedule;
        }
    }



    public class WeekDayAtScheduleProvider : IScheduleProvider
    {
        private readonly DayOfWeek _dayOfWeek;
        private readonly TimeOfDay _timeOfDay;

        public WeekDayAtScheduleProvider(DayOfWeek dayOfWeek, TimeOfDay timeOfDay)
        {
            _dayOfWeek = dayOfWeek;
            _timeOfDay = timeOfDay;
            Console.WriteLine($"[WeekDayAtScheduleProvider] Initialized for {_dayOfWeek} at {_timeOfDay}");
        }

        public bool GetAndCalcNextSchedule(out DateTime schedule)
        {
            schedule = CalculateNextSchedule();
            Console.WriteLine($"[WeekDayAtScheduleProvider] Calculated and updated next schedule: {schedule}");
            return true;
        }

        public DateTime? PeekNextSchedule()
        {
            var nextSchedule = CalculateNextSchedule();
            Console.WriteLine($"[WeekDayAtScheduleProvider] Peeked at next schedule without changing state: {nextSchedule}");
            return nextSchedule;
        }

        private DateTime CalculateNextSchedule()
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, _timeOfDay.Hour, _timeOfDay.Minute, _timeOfDay.Second, _timeOfDay.Millisecond);

            int daysUntilScheduledDay = (_dayOfWeek - now.DayOfWeek + 7) % 7;
            if (daysUntilScheduledDay == 0 && scheduledTime <= now)
            {
                daysUntilScheduledDay = 7;
            }

            var nextSchedule = scheduledTime.AddDays(daysUntilScheduledDay);
            return nextSchedule;
        }
    }


    public class ScheduleCombiner : IScheduleProvider
    {
        private readonly List<IScheduleProvider> _providers;

        public ScheduleCombiner(params IScheduleProvider[] providers)
        {
            _providers = new List<IScheduleProvider>(providers);
            Console.WriteLine("[ScheduleCombiner] Initialized with multiple providers");
        }

        public DateTime? PeekNextSchedule()
        {
            var next = _providers.Select(p => p.PeekNextSchedule()).Where(d => d.HasValue).Min(d => d);
            Console.WriteLine($"[ScheduleCombiner] Peeked at the earliest next schedule from combined providers: {next}");
            return next;
        }

        public bool GetAndCalcNextSchedule(out DateTime schedule)
        {
            var next = PeekNextSchedule();
            if (next.HasValue)
            {
                schedule = next.Value;
                foreach (var provider in _providers)
                {
                    if (provider.PeekNextSchedule() == schedule)
                    {
                        provider.GetAndCalcNextSchedule(out _);
                        Console.WriteLine($"[ScheduleCombiner] Advanced schedule for the provider that matched: {schedule}");
                        return true;
                    }
                }
            }

            schedule = default;
            Console.WriteLine("[ScheduleCombiner] No valid next schedule found or none was due");
            return false;
        }
    }

}
#pragma warning restore