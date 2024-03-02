using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

namespace Coree.NETStandard.Logging
{

    public class FromLogContextEmpty : ILogEventEnricher
    {
        private readonly string alternativeSourceContext;

        public FromLogContextEmpty(string alternativeSourceContext)
        {
            this.alternativeSourceContext = alternativeSourceContext;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value))
            {
                // Create a new property with the modified value and add/update it in the log event.
                var modifiedProperty = propertyFactory.CreateProperty("SourceContext", alternativeSourceContext);
                logEvent.AddOrUpdateProperty(modifiedProperty);
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

                    var TType = shortSourceContext.IndexOf("`1");
                    if (TType > -1)
                    {
                        shortSourceContext = shortSourceContext.Substring(0, TType);
                    }

                    var shortSourceContexts = shortSourceContext.Split('.');
                    shortSourceContext = shortSourceContexts[shortSourceContexts.Length - 1];

                    // Check and remove any of the specified suffixes
                    foreach (var suffix in suffixesToRemove)
                    {
                        if (shortSourceContext.EndsWith(suffix))
                        {
                            shortSourceContext = shortSourceContext.Substring(0, shortSourceContext.Length - suffix.Length);
                            break; // Assuming you only want to remove the first matching suffix
                        }
                    }
                    shortSourceContext = shortSourceContext.PadRight(35);
                    // Create a new property with the modified value and add/update it in the log event.
                    var modifiedProperty = propertyFactory.CreateProperty("SourceContext", shortSourceContext);
                    logEvent.AddOrUpdateProperty(modifiedProperty);
                }
            }
        }
    }
}