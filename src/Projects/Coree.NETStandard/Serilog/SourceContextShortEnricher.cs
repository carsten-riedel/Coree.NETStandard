using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog.Core;
using Serilog.Events;

namespace Coree.NETStandard.Serilog
{
    /// <summary>
    /// A Serilog enricher that adds the "SourceContextShort" property of log events by trimming generic type indicators,
    /// simplifying namespaces, and optionally removing specified suffixes. This can make the log source context more readable
    /// and concise in log outputs.
    /// </summary>
    public class SourceContextShortEnricher : ILogEventEnricher
    {
        private readonly string[]? suffixesToRemove;
        private readonly bool trimGenericTypeIndicator;
        private readonly bool simplifyNamespace;
        private readonly int sourceContextPadding;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceContextShortEnricher"/> class with default settings.
        /// By default, it trims generic type indicators, simplifies namespaces to their last component, applies a default padding,
        /// and removes common suffixes like "Service", "Services", "AsyncCommand", and "Command".
        /// </summary>
        public SourceContextShortEnricher()
        {
            this.trimGenericTypeIndicator = true;
            this.simplifyNamespace = true;
            this.sourceContextPadding = 15;
            this.suffixesToRemove = new[] { "Service", "Services", "AsyncCommand", "Command" };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceContextShortEnricher"/> class with customizable settings.
        /// </summary>
        /// <param name="trimGenericTypeIndicator">If true, trims the generic type indicator from the source context.</param>
        /// <param name="simplifyNamespace">If true, simplifies the namespace to only include the last component.</param>
        /// <param name="sourceContextPadding">The padding applied to the modified source context for alignment in logs.</param>
        /// <param name="suffixesToRemove">An array of suffixes to remove from the source context, enhancing readability.</param>
        public SourceContextShortEnricher(bool trimGenericTypeIndicator = true, bool simplifyNamespace = true, int sourceContextPadding = 15, string[]? suffixesToRemove = null)
        {
            this.trimGenericTypeIndicator = trimGenericTypeIndicator;
            this.simplifyNamespace = simplifyNamespace;
            this.sourceContextPadding = sourceContextPadding;
            this.suffixesToRemove = suffixesToRemove;
        }

        /// <summary>
        /// A Serilog enricher that adds the "SourceContextShort" property of log events by trimming generic type indicators,
        /// simplifying namespaces, and optionally removing specified suffixes. This can make the log source context more readable
        /// and concise in log outputs.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">The factory used to create new log event properties.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue value))
            {
                if (value is ScalarValue scalarValue && scalarValue.Value is string sourceContext)
                {
                    var shortSourceContext = sourceContext;

                    if (trimGenericTypeIndicator)
                    {
                        var TType = shortSourceContext.IndexOf("`1");
                        if (TType > -1)
                        {
                            shortSourceContext = shortSourceContext.Substring(0, TType);
                        }
                    }

                    if (simplifyNamespace)
                    {
                        var shortSourceContexts = shortSourceContext.Split('.');
                        shortSourceContext = shortSourceContexts[shortSourceContexts.Length - 1];
                    }

                    if (suffixesToRemove != null)
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

                    shortSourceContext = shortSourceContext.PadRight(sourceContextPadding);
                    // Create a new property with the modified value and add/update it in the log event.
                    var modifiedProperty = propertyFactory.CreateProperty("SourceContextShort", shortSourceContext);
                    logEvent.AddOrUpdateProperty(modifiedProperty);
                }
            }
        }
    }
}
