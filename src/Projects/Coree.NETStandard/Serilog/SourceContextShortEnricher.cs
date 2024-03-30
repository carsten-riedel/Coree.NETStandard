using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog.Core;
using Serilog.Events;

namespace Coree.NETStandard.Serilog
{
    public class SourceContextShortEnricher : ILogEventEnricher
    {
        private readonly string[] suffixesToRemove = { "Service", "Services" , "AsyncCommand", "Command"  };
        private readonly bool trimGenericTypeIndicator;
        private readonly bool simplifyNamespace;
        private readonly bool stripDefinedSuffixes;
        private readonly int sourceContextPadding;


        public SourceContextShortEnricher()
        {
            this.trimGenericTypeIndicator = true;
            this.simplifyNamespace = true;
            this.stripDefinedSuffixes = false;
            this.sourceContextPadding = 15;
        }

        public SourceContextShortEnricher(string[] suffixesToRemove)
        {
            this.trimGenericTypeIndicator = true;
            this.simplifyNamespace = true;
            this.stripDefinedSuffixes = true;
            this.sourceContextPadding = 15;
            this.suffixesToRemove = suffixesToRemove;
        }

        public SourceContextShortEnricher(bool trimGenericTypeIndicator = true, bool simplifyNamespace = true, bool stripDefinedSuffixes = true,int sourceContextPadding = 15)
        {
            this.trimGenericTypeIndicator = trimGenericTypeIndicator;
            this.simplifyNamespace = simplifyNamespace;
            this.stripDefinedSuffixes = stripDefinedSuffixes;
            this.sourceContextPadding = sourceContextPadding; 
        }

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

                    if (stripDefinedSuffixes)
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
