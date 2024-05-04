using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Classes.DebugService
{
    public class DebugService : IDebugService
    {
        private readonly ILogger<DebugService> logger;

        public DebugService(ILogger<DebugService> logger)
        {
            this.logger = logger;
            Debug.WriteLine("DebugService ctor");
        }

        public void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
