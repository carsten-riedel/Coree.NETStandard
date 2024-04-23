using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Coree.NETStandard.Classes
{
    public class Scheduler
    {
        public event EventHandler<TickerEventArgs> TickOccurred;

        public class TickerEventArgs : EventArgs
        {
            public TimeSpan diviation { get; set; }
            public DateTime TickTime { get; set; }
            public string FormattedTickTime { get; set; }
        }

        private System.Threading.Thread? _tickMonitorThread = null;
        private readonly TimeOfDay _synchronizationOffset;
        private readonly TimeOfDay _recurrenceInterval;
        private readonly bool _triggerAtStart;
        private readonly DateTime _startDateTime;
        private readonly DateTime _endDateTime;
        private readonly TimeOfDay _timeOfDay;
        private readonly ScheduleType _scheduleType;
        private readonly int _dailyRecurEvery;
        private readonly int _schedulerPreCalcLimit;

        private List<DateTime> _scheduledDates = new List<DateTime>();

        public List<DateTime> ScheduledDates
        {
            get { return _scheduledDates; }
        }

        private enum ScheduleType
        {
            timespancRecur,
            daily
        }

        protected virtual void RaiseTickEvent(TickerEventArgs args)
        {
            var handler = TickOccurred;
            if (handler != null)
            {
                foreach (var singleHandler in handler.GetInvocationList())
                {
                    Task.Run(() => singleHandler.DynamicInvoke(this, args));
                }
            }
        }

        private async void TickMonitor()
        {
            while (true)
            {
                var nextSchedule = _scheduledDates.First();
                var now = DateTime.Now;
                if (nextSchedule < now)
                {
                    RaiseTickEvent(new TickerEventArgs() { TickTime = now, diviation = now - nextSchedule, FormattedTickTime = now.ToString("MM.dd.yyyy HH:mm:ss.fff") });
                    //ClearSchedulesBefore(now);
                    _scheduledDates.RemoveAll(schedule => schedule < now);
                    //_scheduledDates.Remove(nextSchedule);

                    CalculateRecuringSchedules();
                }
                else
                {
                    System.Threading.Thread.Sleep(33);
                }
                await Task.Delay(100);
            }
        }

        //Constructor for Daily mode initial trigger won't be added to the schedule list it will just fire at start
        public Scheduler(DateTime startDateTime, DateTime endDateTime, TimeOfDay timeOfDay, int dailyRecurEvery = 1, bool initalTrigger = true)
        {
            _startDateTime = startDateTime;
            _endDateTime = endDateTime;
            _timeOfDay = timeOfDay;
            _dailyRecurEvery = dailyRecurEvery;
            _scheduleType = ScheduleType.daily;
            _triggerAtStart = initalTrigger;
            _schedulerPreCalcLimit = 5;
            CalculateDailySchedules();

            _tickMonitorThread = new System.Threading.Thread(() => TickMonitor()) { IsBackground = true };
            _tickMonitorThread.Start();
        }

        //Constructor for recurence mode , initial trigger won't be added to the schedule list it will just fire at start
        public Scheduler(DateTime startDateTime, DateTime endDateTime, TimeOfDay recurFrequency, TimeOfDay recurSyncOffset, bool initalTrigger)
        {
            _startDateTime = startDateTime;
            _endDateTime = endDateTime;
            _recurrenceInterval = recurFrequency;
            _synchronizationOffset = recurSyncOffset;
            _scheduleType = ScheduleType.timespancRecur;
            _triggerAtStart = initalTrigger;
            _schedulerPreCalcLimit = 100;
            CalculateRecuringSchedules();
            _tickMonitorThread = new System.Threading.Thread(() => TickMonitor()) { IsBackground = true };
            _tickMonitorThread.Start();
        }

        /// <summary>
        /// Calculates or updates the recurring schedules up to a specified limit.
        /// </summary>
        public void CalculateRecuringSchedules()
        {
            //Add schedules if schudles missing. sync with last highest date.
            if (_scheduledDates.Count != 0)
            {
                var lastItemOnScheduler = _scheduledDates.Last(); //Maybe highest date is better
                var itemsneedToBeCalculated = _schedulerPreCalcLimit - _scheduledDates.Count;
                for (int i = 0; i < itemsneedToBeCalculated; i++)
                {
                    var nextItem = lastItemOnScheduler.Add(_recurrenceInterval.ToTimeSpan());
                    _scheduledDates.Add(nextItem);
                    //lastItemOnScheduler = nextItem;
                }
                return;
            }

            //no additional schedules needed
            if (_scheduledDates.Count >= _schedulerPreCalcLimit)
            {
                return;
            }

            long nowTicks = DateTime.Now.Ticks;
            long nearestDiff = nowTicks % _recurrenceInterval.Ticks; // Ensure conversion to TimeSpan
            long firstTickOffset = nowTicks + (_recurrenceInterval.Ticks - nearestDiff) + _synchronizationOffset.Ticks;
            DateTime firstNextTickDate = new DateTime(firstTickOffset); // Directly use ticks to create DateTime
            _scheduledDates.Add(firstNextTickDate);
            CalculateRecuringSchedules();
        }

        /// <summary>
        /// Calculates or updates the daily schedules up to a specified limit.
        /// </summary>
        public void CalculateDailySchedules()
        {
            // Determine the starting point for new calculations
            DateTime current = _scheduledDates.Count > 0 ? _scheduledDates[_scheduledDates.Count - 1].AddDays(_dailyRecurEvery) : new DateTime(_startDateTime.Year, _startDateTime.Month, _startDateTime.Day, _timeOfDay.Hour, _timeOfDay.Minute, _timeOfDay.Second);

            if (current < _startDateTime)
            {
                current = _startDateTime;
            }

            int count = 0;
            while (current <= _endDateTime && count < _schedulerPreCalcLimit)
            {
                _scheduledDates.Add(current);
                current = current.AddDays(_dailyRecurEvery);
                count++;
            }
        }

        /// <summary>
        /// Clears all scheduled times before the specified date.
        /// </summary>
        /// <param name="clearDate">The date before which all schedules will be removed.</param>
        public void ClearSchedulesBefore(DateTime clearDate)
        {
            // Option 1: Use List.RemoveAll for efficiency
            _scheduledDates.RemoveAll(schedule => schedule < clearDate);

            // Option 2: Manual iteration and removal - more explicit
            // for (int i = _schedules.Count - 1; i >= 0; i--)
            // {
            //     if (_schedules[i] < clearDate)
            //     {
            //         _schedules.RemoveAt(i);
            //     }
            // }
        }
    }
}