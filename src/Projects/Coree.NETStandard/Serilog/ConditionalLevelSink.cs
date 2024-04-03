using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Serilog.Core;
using Serilog.Events;

namespace Coree.NETStandard.Serilog
{
    public class ConditionalLevelSink : ILogEventSink
    {
        private readonly ILogEventSink _innerSink;
        private readonly Dictionary<string, LogEventLevel>? _conditionalLevel;

        public ConditionalLevelSink(ILogEventSink innerSink, Dictionary<string, LogEventLevel>? conditionalLevel)
        {
            _innerSink = innerSink ?? throw new ArgumentNullException(nameof(innerSink));
            _conditionalLevel = conditionalLevel;
        }

        public void Emit(LogEvent logEvent)
        {
            if (_conditionalLevel == null)
            {
                _innerSink.Emit(logEvent);
            }
            else if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value) && value is ScalarValue scalarValue && _conditionalLevel.TryGetValue(scalarValue.Value.ToString(), out LogEventLevel newLevel))
            {
                var modifiedEvent = new LogEvent(
                    logEvent.Timestamp,
                    newLevel,
                    logEvent.Exception,
                    logEvent.MessageTemplate,
                    logEvent.Properties.Select(kv => new LogEventProperty(kv.Key, kv.Value)));
                _innerSink.Emit(modifiedEvent);
            }
            else
            {
                _innerSink.Emit(logEvent);
            }
        }
    }

}
