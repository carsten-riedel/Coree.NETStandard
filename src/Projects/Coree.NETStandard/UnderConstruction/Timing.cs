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

    


    }

}