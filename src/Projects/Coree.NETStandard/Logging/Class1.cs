using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Coree.NETStandard.Logging
{
    public class Logger
    {
        public static Serilog.Core.Logger Log { get; set; } = GetSerilogConfiguration(LogEventLevel.Debug).CreateLogger();

        public static LoggerConfiguration GetSerilogConfiguration(LogEventLevel restrictedToMinimumLevel)
        {
            var loggerConfiguration = new LoggerConfiguration();
            ApplyDefaultSerilogConfiguration(loggerConfiguration, restrictedToMinimumLevel);
            return loggerConfiguration;
        }

        public static string DefaultOutputTemplate()
        {
            return "[{Timestamp:HH:mm:ss} {Level:u3}] MethodName {MethodName}, SourceContext {SourceContext}, ProcessId: {ProcessId}, ThreadId: {ThreadId}, MachineName: {MachineName}, EnvironmentUserName: {EnvironmentUserName}, EnvironmentName: {EnvironmentName} | {Message:lj}{NewLine}{Exception}";
        }

        public static void ApplyDefaultSerilogConfiguration(LoggerConfiguration config,LogEventLevel restrictedToMinimumLevel = LogEventLevel.Debug)
        {
           config
           .Enrich.With(new Coree.NETStandard.Logging.MethodNameEnricher())
           .Enrich.FromLogContext()
           .Enrich.WithProcessId()
           .Enrich.WithThreadId()
           .Enrich.WithMachineName()
           .Enrich.WithEnvironmentUserName()
           .Enrich.WithEnvironmentName()
           .WriteTo.Console(outputTemplate: DefaultOutputTemplate(),restrictedToMinimumLevel: restrictedToMinimumLevel);
        }


    }
}