using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Coree.NETStandard.Serilog
{
    /// <summary>
    /// A custom log event sink that conditionally changes the log event level based on predefined criteria.
    /// </summary>
    /// <remarks>
    /// This sink wraps another log event sink and delegates the emission of log events to it, potentially with modified log event levels.
    /// </remarks>
    public class ConditionalLevelSink : ILogEventSink
    {
        private readonly ILogEventSink _innerSink;
        private readonly Dictionary<string, LogEventLevel>? _conditionalLevel;

        /// <summary>
        /// Initializes a new instance of the ConditionalLevelSink class.
        /// </summary>
        /// <param name="innerSink">The inner sink to which log events will be forwarded.</param>
        /// <param name="conditionalLevel">A dictionary mapping specific criteria to log event levels.</param>
        /// <exception cref="ArgumentNullException">Thrown when innerSink is null.</exception>
        public ConditionalLevelSink(ILogEventSink innerSink, Dictionary<string, LogEventLevel>? conditionalLevel)
        {
            _innerSink = innerSink ?? throw new ArgumentNullException(nameof(innerSink));
            _conditionalLevel = conditionalLevel;
        }

        /// <summary>
        /// Processes and potentially modifies a log event before forwarding it to the inner sink.
        /// </summary>
        /// <param name="logEvent">The log event to be emitted.</param>
        /// <remarks>
        /// If a matching criterion is found in the conditionalLevel dictionary, the log event's level is modified accordingly before it is forwarded.
        /// </remarks>
        public void Emit(LogEvent logEvent)
        {
            if (_conditionalLevel == null)
            {
                _innerSink.Emit(logEvent);
                return;
            }

            string? scalarValueValue = null;
            var sourceContextIsPrensent = logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value);
            if (sourceContextIsPrensent && value is ScalarValue scalarValue)
            {
                if (scalarValue != null)
                {
                    if (scalarValue.Value != null)
                    {
                        scalarValueValue = scalarValue.Value.ToString();
                    }
                }
            }
            else
            {
                _innerSink.Emit(logEvent);
            }

            if (sourceContextIsPrensent && (scalarValueValue != null) && _conditionalLevel.TryGetValue(key: scalarValueValue, out LogEventLevel newLevel))
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
