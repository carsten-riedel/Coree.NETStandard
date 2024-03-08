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
    public class EnhancedSourceContextShortEnricher : ILogEventEnricher
    {
        private readonly List<string> suffixesToRemove = new List<string>() { "Service", "Services" };
        private readonly bool cutTTypes;
        private readonly bool removeDots;
        private readonly bool removeSuffixes;

        public EnhancedSourceContextShortEnricher(bool cutTTypes = true, bool removeDots = true, bool removeSuffixes = true)
        {
            this.cutTTypes = cutTTypes;
            this.removeDots = removeDots;
            this.removeSuffixes = removeSuffixes;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value))
            {
                if (value is ScalarValue scalarValue && scalarValue.Value is string sourceContext)
                {
                    var shortSourceContext = sourceContext;

                    if (cutTTypes)
                    {
                        var TType = shortSourceContext.IndexOf("`1");
                        if (TType > -1)
                        {
                            shortSourceContext = shortSourceContext.Substring(0, TType);
                        }
                    }

                    if (removeDots)
                    {
                        var shortSourceContexts = shortSourceContext.Split('.');
                        shortSourceContext = shortSourceContexts[shortSourceContexts.Length - 1];
                    }

                    if (removeSuffixes)
                    {
                        // Check and remove any of the specified suffixes
                        foreach (var suffix in suffixesToRemove)
                        {
                            if (shortSourceContext.EndsWith(suffix))
                            {
                                shortSourceContext = shortSourceContext.Substring(0, shortSourceContext.Length - suffix.Length);
                                break; // Assuming you only want to remove the first matching suffix
                            }
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