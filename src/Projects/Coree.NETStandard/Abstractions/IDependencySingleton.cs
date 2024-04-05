using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Abstractions
{
    public interface IDependencySingleton
    {
        public void SetMinimumLogLevel(LogLevel logLevel = LogLevel.Trace);
    }
}
