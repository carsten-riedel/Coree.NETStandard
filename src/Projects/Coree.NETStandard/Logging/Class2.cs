using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Coree.NETStandard.Logging
{
    public class CustomSourceContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value))
            {
                // Assume value is a ScalarValue and we want to modify it.
                if (value is ScalarValue scalarValue && scalarValue.Value is string sourceContext)
                {
                    // Example modification: append "Modified" to the original SourceContext.
                    // You can replace this logic with whatever modification you need.
                    var modifiedSourceContext = $"{sourceContext}Modified";

                    // Create a new property with the modified value and add/update it in the log event.
                    var modifiedProperty = propertyFactory.CreateProperty("SourceContext", modifiedSourceContext);
                    logEvent.AddOrUpdateProperty(modifiedProperty);
                }
            }
        }
    }

    public class SourceContextShortEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value))
            {
                if (value is ScalarValue scalarValue && scalarValue.Value is string sourceContext)
                {
                    // Remove trailing "Service" from the source context
                    var shortSourceContext = sourceContext.EndsWith("Service")
                        ? sourceContext.Substring(0, sourceContext.Length - "Service".Length)
                        : sourceContext;

                    // Create a new property with the shortened source context
                    var newProperty = propertyFactory.CreateProperty("SourceContextShort", shortSourceContext);
                    logEvent.AddPropertyIfAbsent(newProperty);
                }
            }
        }
    }

    public class EnhancedSourceContextShortEnricher : ILogEventEnricher
    {
        private readonly List<string> suffixesToRemove = new List<string>() { "Service", "Services" };

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value))
            {
                if (value is ScalarValue scalarValue && scalarValue.Value is string sourceContext)
                {
                    var shortSourceContext = sourceContext;

                    // Check and remove any of the specified suffixes
                    foreach (var suffix in suffixesToRemove)
                    {
                        if (shortSourceContext.EndsWith(suffix))
                        {
                            shortSourceContext = shortSourceContext.Substring(0, shortSourceContext.Length - suffix.Length);
                            break; // Assuming you only want to remove the first matching suffix
                        }
                    }

                    // Create a new property with the potentially modified source context
                    var newProperty = propertyFactory.CreateProperty("SourceContextShort", shortSourceContext);
                    logEvent.AddPropertyIfAbsent(newProperty);
                }
            }
        }
    }

    public class MethodNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var stack = new StackTrace();
            // Skip frames to get to the method of interest, adjust the skipFrames value as needed
            // 0 would be this method, 1 would be the method calling Enrich, etc.
            var frame = stack.GetFrame(3); // You might need to adjust this number based on where you're logging from
            if (frame != null)
            {
                var method = frame.GetMethod();
                if (method != null)
                {
                    var methodName = method.Name;
                    var property = propertyFactory.CreateProperty("MethodName", methodName);
                    logEvent.AddPropertyIfAbsent(property);
                }
            }
        }
    }
}