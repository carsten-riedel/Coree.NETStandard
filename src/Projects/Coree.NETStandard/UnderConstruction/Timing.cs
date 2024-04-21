using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

using Coree.NETStandard.Classes;


namespace Coree.NETStandard.UnderConstruction
{
    public partial class Timing
    {
        public class Ticker
        {
            private T DeepCloneSerializable<T>(T obj)
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    ms.Position = 0;
                    return (T)formatter.Deserialize(ms);
                }
            }

            private System.Threading.SynchronizationContext TickerSyncContext;

            private event EventHandler<TickerEventArgs> _TickerEventHandler;

            public event EventHandler<TickerEventArgs> TickerEventHandler
            {
                add
                {
                    TickerSyncContext = System.ComponentModel.AsyncOperationManager.SynchronizationContext;
                    _TickerEventHandler += value;
                    if (_StartAtEventAssignment)
                    {
                        Start();
                    }
                }

                remove
                {
                    _TickerEventHandler -= value;
                }
            }

            [Serializable]
            public class TickerEventArgs : EventArgs
            {
                public string name { get; set; }
                public DateTime TickAtDateTime { get; set; }
                public string TimeStringFull { get; set; }
            }

            protected virtual void OnTick(TickerEventArgs e)
            {
                EventHandler<TickerEventArgs> handler = _TickerEventHandler;
                if (handler != null)
                {
                    var clone = DeepCloneSerializable(e);
                    TickerSyncContext.Post(new System.Threading.SendOrPostCallback(delegate (object obj) { handler(this, clone); ; }), null);
                }
            }

            private System.Threading.Thread tickthread = null;
            private DateTime NextTickDate;
            private TimeSpan CheckIfStepToSync;
            private long _ParsedTimeInTicks;
            private long _ParsedOffsetInTicks;
            public bool tickeractive = false;

            private bool _StartAtEventAssignment = false;
            private bool _ForceStartingTick = true;
            private string _Name;

            /// <summary>
            /// A ticker that ticks on a regular basis
            /// </summary>
            /// <param name="Name"></param>
            /// <param name="TickerIntervall"></param>
            /// <param name="TickerOffsetHoursValue"></param>
            /// <param name="TickerOffsetMinutesValue"></param>
            /// <param name="TickerOffsetSecondsValue"></param>
            /// <param name="ForceStartingTick"></param>
            public Ticker(string Name = "default", string TickerIntervall = @"00:00:00:00.50", string TickerOffset = @"00:00:00:00.00", bool ForceStartingTick = true, bool StartAtEventAssignment = true)
            {
                _Name = Name;

                _ForceStartingTick = ForceStartingTick;
                _StartAtEventAssignment = StartAtEventAssignment;

                _ParsedTimeInTicks = TimeSpan.ParseExact(TickerIntervall, @"dd\:hh\:mm\:ss\.ff", null, TimeSpanStyles.None).Ticks;
                _ParsedOffsetInTicks = TimeSpan.ParseExact(TickerOffset, @"dd\:hh\:mm\:ss\.ff", null, TimeSpanStyles.None).Ticks;
            }

            public void Start()
            {
                tickeractive = true;

                DateTime now = DateTime.Now;
                long nowdaytick = now.Ticks;
                long nearestdiff = nowdaytick % _ParsedTimeInTicks;

                long ff = 0;
                if (_ForceStartingTick)
                {
                    ff = nowdaytick - nearestdiff + _ParsedOffsetInTicks;
                }
                else
                {
                    ff = nowdaytick + (_ParsedTimeInTicks - nearestdiff) + _ParsedOffsetInTicks;
                }

                NextTickDate = now.AddTicks(-nowdaytick + ff);

                //CheckIfStepToSync = TimeSpan.FromTicks(System.Convert.ToInt32(tickinc * 0.1));
                CheckIfStepToSync = TimeSpan.Zero;

                tickthread = new System.Threading.Thread(() => TickCheck()) { IsBackground = true };
                tickthread.Start();
                System.Threading.Thread.Sleep(100);
            }

            private void TickCheck()
            {
                while (tickeractive)
                {
                    if (NextTickDate < DateTime.Now)
                    {
                        var futuretick = NextTickDate.AddTicks(_ParsedTimeInTicks);
                        var span = futuretick - DateTime.Now;
                        var ispos = span.CompareTo(CheckIfStepToSync);
                        if (ispos > 0)
                        {
                            OnTick(new TickerEventArgs() { TickAtDateTime = DateTime.Now, name = _Name, TimeStringFull = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.fff") });
                        }
                        else
                        {
                            Console.WriteLine("step over");
                        }

                        NextTickDate = futuretick;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(33);
                    }
                }
            }

            public void Stop()
            {
                tickeractive = false;
                System.Threading.Thread.Sleep(100);
                tickthread.Abort();
            }
        }

        public class TickerSync
        {
            private readonly string? _tickerName;

            private readonly string? _tickIntervall;

            public TickerSync(string tickerName = "default", string tickerIntervall = @"00:00:60:00.00", string TickerOffset = @"00:00:00:00.00", bool ForceStartingTick = true, bool StartAtEventAssignment = true)
            {

                //TimeOnly startTime;
                _tickerName = tickerName;
                _tickIntervall = tickerIntervall;

                var datetime = DateTime.Now;
                datetime.AddMonths(1);

                //this._ForceStartingTick = ForceStartingTick;
                //this._StartAtEventAssignment = StartAtEventAssignment;

                //this._ParsedTimeInTicks = TimeSpan.ParseExact(TickerIntervall, @"dd\:hh\:mm\:ss\.ff", null, TimeSpanStyles.None).Ticks;
                //this._ParsedOffsetInTicks = TimeSpan.ParseExact(TickerOffset, @"dd\:hh\:mm\:ss\.ff", null, TimeSpanStyles.None).Ticks;
            }
        }

        public class Scheduler
        {
            public event EventHandler<TickerEventArgs> TickerEventHandler;

            public class TickerEventArgs : EventArgs
            {
                public DateTime TickAtDateTime { get; set; }
                public string TimeStringFull { get; set; }
            }

            protected virtual void OnTick(TickerEventArgs e)
            {
                EventHandler<TickerEventArgs> handler = TickerEventHandler;
                if (handler != null)
                {
                    handler.Invoke(this, e);
                }
            }



            private System.Threading.Thread? tickCheckThread = null;
            private readonly TimeOfDay _recurSyncOffset;
            private readonly TimeOfDay _recurFrequency;
            private readonly bool _initalTrigger;
            private readonly DateTime _startDate;
            private readonly DateTime _endDate;
            private readonly TimeOfDay _timeOfDay;
            private readonly TickerType _tickerType;
            private readonly int _dailyRecurEvery;
            private readonly int _schedulerPreCalcLimit;

            private List<DateTime> _schedules = new List<DateTime>();

            public List<DateTime> Schedules
            {
                get { return _schedules; }
            }

            private enum TickerType
            {
                timespancRecur,
                daily
            }


            //Constructor for Daily mode initial trigger won't be added to the schedule list it will just fire at start
            public Scheduler(DateTime startDate, DateTime endDate, TimeOfDay timeOfDay, int dailyRecurEvery = 1, bool initalTrigger = true)
            {
                _startDate = startDate;
                _endDate = endDate;
                _timeOfDay = timeOfDay;
                _dailyRecurEvery = dailyRecurEvery;
                _tickerType = TickerType.daily;
                _initalTrigger = initalTrigger;
                _schedulerPreCalcLimit = 5;
                CalculateDailySchedules();

                //tickCheckThread = new System.Threading.Thread(() => TickCheck()) { IsBackground = true };
                //tickCheckThread.Start();
            }

            //Constructor for recurence mode , initial trigger won't be added to the schedule list it will just fire at start
            public Scheduler(DateTime startDate, DateTime endDate, TimeOfDay recurFrequency, TimeOfDay recurSyncOffset, bool initalTrigger)
            {
                _startDate = startDate;
                _endDate = endDate;
                _recurFrequency = recurFrequency;
                _recurSyncOffset = recurSyncOffset;
                _tickerType = TickerType.timespancRecur;
                _initalTrigger = initalTrigger;
                _schedulerPreCalcLimit = 100;
                CalculateRecuringSchedules();
                //tickCheckThread = new System.Threading.Thread(() => TickCheck()) { IsBackground = true };
                //tickCheckThread.Start();
            }

            /// <summary>
            /// Calculates or updates the recurring schedules up to a specified limit.
            /// </summary>
            public void CalculateRecuringSchedulesOld()
            {
                TimeSpan frequency = _recurFrequency.ToTimeSpan();
                TimeSpan offset = _recurSyncOffset.ToTimeSpan();

                // Calculate the first possible tick moment with offset applied
                DateTime firstPossibleTick = new DateTime(_startDate.Year, _startDate.Month, _startDate.Day, _startDate.Hour, _startDate.Minute, _startDate.Second, _startDate.Millisecond);
                long ticksToNextAlignedTime = (offset.Ticks - firstPossibleTick.Ticks % frequency.Ticks + frequency.Ticks) % frequency.Ticks;
                DateTime firstTick = firstPossibleTick.AddTicks(ticksToNextAlignedTime);

                // Ensure the first tick starts after the actual start date
                if (firstTick < _startDate)
                {
                    firstTick = firstTick.Add(frequency);
                }

                DateTime current = firstTick;
                int count = 0;  // Initialize count based on the existing number of schedules

                // Calculate the next times until the end date or the limit is reached.
                while (current <= _endDate && count < _schedulerPreCalcLimit)
                {
                    _schedules.Add(current);
                    current = current.Add(frequency);
                    count++;
                }
            }


            /// <summary>
            /// Calculates or updates the recurring schedules up to a specified limit.
            /// </summary>
            public void CalculateRecuringSchedules()
            {
                //no additional schedules needed
                if (_schedules.Count >= _schedulerPreCalcLimit)
                {
                    return;
                }

                //Add schedules if schudles missing. sync with last highest date.
                if (_schedules.Count != 0)
                {
                    var lastItemOnScheduler = _schedules.Last(); //Maybe highest date is better
                    var itemsneedToBeCalculated = _schedulerPreCalcLimit - _schedules.Count;
                    for (int i = 0; i < itemsneedToBeCalculated; i++)
                    {
                        var nextItem = lastItemOnScheduler.Add(_recurFrequency.ToTimeSpan());
                        _schedules.Add(nextItem);
                        lastItemOnScheduler = nextItem;
                    }
                    return;
                }

                long nowTicks = DateTime.Now.Ticks;
                long nearestDiff = nowTicks % _recurFrequency.Ticks; // Ensure conversion to TimeSpan
                long firstTickOffset = nowTicks + (_recurFrequency.Ticks - nearestDiff) + _recurSyncOffset.Ticks;
                DateTime firstNextTickDate = new DateTime(firstTickOffset); // Directly use ticks to create DateTime
                _schedules.Add(firstNextTickDate);
                CalculateRecuringSchedules();
            }

            /// <summary>
            /// Calculates or updates the daily schedules up to a specified limit.
            /// </summary>
            public void CalculateDailySchedules()
            {
                // Determine the starting point for new calculations
                DateTime current = _schedules.Count > 0 ? _schedules[_schedules.Count - 1].AddDays(_dailyRecurEvery) : new DateTime(_startDate.Year, _startDate.Month, _startDate.Day, _timeOfDay.Hour, _timeOfDay.Minute, _timeOfDay.Second);

                if (current < _startDate)
                {
                    current = _startDate;
                }

                int count = 0;
                while (current <= _endDate && count < _schedulerPreCalcLimit)
                {
                    _schedules.Add(current);
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
                _schedules.RemoveAll(schedule => schedule < clearDate);

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

}