using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Serilog
{
    public static class OutputTemplates
    {
        public static string CommonConsoleConfigOutputTemplate()
        {
            return "{Timestamp:HH:mm:ss.ffff} {Level:u3}|{SourceContext}| {EnvironmentUserName} | {EnvironmentName} | {Message:l}{NewLine}{Exception}";
        }

        public static string DefaultShort()
        {
            return "{Timestamp:HH:mm:ss.ffff} | {Level:u3} | {SourceContextShort} | {Message:l}{NewLine}{Exception}";
        }
    }
}
