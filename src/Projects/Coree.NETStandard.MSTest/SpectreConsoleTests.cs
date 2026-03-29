using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Coree.NETStandard.SpectreConsole;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Coree.NETStandard.MSTest
{
    [TestClass]
    public class SpectreConsoleTests
    {
        private sealed class MarkerDependency
        {
        }

        private sealed class DummyAsyncCommand : AsyncCommand<DummyAsyncCommand.Settings>
        {
            public sealed class Settings : CommandSettings
            {
            }

            public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
            {
                return Task.FromResult(0);
            }
        }

        private sealed class DependencyBackedAsyncCommand : AsyncCommand<DependencyBackedAsyncCommand.Settings>
        {
            private readonly MarkerDependency dependency;

            public sealed class Settings : CommandSettings
            {
            }

            public DependencyBackedAsyncCommand(MarkerDependency dependency)
            {
                this.dependency = dependency;
            }

            public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
            {
                return Task.FromResult(dependency != null ? 0 : 1);
            }
        }

        private static ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MarkerDependency>();
            services.AddSpectreConsole(config =>
            {
                config.SetApplicationName("testapp");
                config.AddCommand<DummyAsyncCommand>("dummy");
                config.AddCommand<DependencyBackedAsyncCommand>("needs-di");
            });

            return services.BuildServiceProvider();
        }

        [TestMethod]
        public void CommandAppExtensions_GetCommandTypes_DoesNotDiscoverBuiltInCliCommandsBeforeFirstRun()
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationName("testapp");
                config.AddCommand<DummyAsyncCommand>("dummy");
            });

            var commandTypes = app.GetCommandTypes();

            CollectionAssert.Contains(commandTypes, typeof(DummyAsyncCommand));
            Assert.IsFalse(commandTypes.Any(type => string.Equals(type.FullName, "Spectre.Console.Cli.XmlDocCommand", StringComparison.Ordinal)));
        }

        [TestMethod]
        public void ProviderBackedSpectreConsoleTypeRegistrar_ResolvesRuntimeRegistrations()
        {
            using var provider = new ServiceCollection().BuildServiceProvider();
            var registrar = new SpectreConsoleTypeRegistrar(provider, false);

            registrar.Register(typeof(MarkerDependency), typeof(MarkerDependency));
            registrar.RegisterInstance(typeof(string), "registered-instance");
            registrar.RegisterLazy(typeof(Guid), () => Guid.Empty);

            var resolver = registrar.Build();

            Assert.IsNotNull(resolver.Resolve(typeof(MarkerDependency)));
            Assert.AreEqual("registered-instance", resolver.Resolve(typeof(string)));
            Assert.AreEqual(Guid.Empty, resolver.Resolve(typeof(Guid)));
        }

        [TestMethod]
        public void ProviderBackedSpectreConsoleTypeRegistrar_PrefersPrimaryProviderOverRuntimeRegistrations()
        {
            using var provider = new ServiceCollection()
                .AddSingleton("primary-instance")
                .BuildServiceProvider();
            var registrar = new SpectreConsoleTypeRegistrar(provider, false);

            registrar.RegisterInstance(typeof(string), "runtime-instance");

            var resolver = registrar.Build();

            Assert.AreEqual("primary-instance", resolver.Resolve(typeof(string)));
        }

        [TestMethod]
        public void AddSpectreConsole_TypeResolver_DoesNotResolveBuiltInXmlDocCommandBeforeRuntimeRegistration()
        {
            using var provider = CreateServiceProvider();
            var xmlDocCommandType = typeof(CommandApp).Assembly.GetType("Spectre.Console.Cli.XmlDocCommand");

            Assert.IsNotNull(xmlDocCommandType, "The current Spectre package should contain the hidden XmlDoc command type.");

            var resolver = new SpectreConsoleTypeRegistrar(provider, false).Build();
            Assert.IsNull(resolver.Resolve(xmlDocCommandType));
        }

        [TestMethod]
        public async Task AddSpectreConsole_CommandApp_UserCommand_Succeeds()
        {
            using var provider = CreateServiceProvider();
            var app = provider.GetRequiredService<ICommandApp>();

            var exitCode = await app.RunAsync(new[] { "dummy" });

            Assert.AreEqual(0, exitCode);
        }

        [TestMethod]
        public async Task AddSpectreConsole_CommandApp_HostInjectedDependency_Succeeds()
        {
            using var provider = CreateServiceProvider();
            var app = provider.GetRequiredService<ICommandApp>();

            var exitCode = await app.RunAsync(new[] { "needs-di" });

            Assert.AreEqual(0, exitCode);
        }

        [TestMethod]
        public async Task AddSpectreConsole_CommandApp_CliXmldoc_Succeeds()
        {
            using var provider = CreateServiceProvider();
            var app = provider.GetRequiredService<ICommandApp>();

            using var output = new StringWriter();
            var originalOut = Console.Out;
            var originalError = Console.Error;

            try
            {
                Console.SetOut(output);
                Console.SetError(output);

                var exitCode = await app.RunAsync(new[] { "cli", "xmldoc" });
                var text = output.ToString();

                Assert.AreEqual(0, exitCode);
                StringAssert.Contains(text, "<Model>");
                StringAssert.Contains(text, "DummyAsyncCommand");
                Assert.IsFalse(text.Contains("Could not resolve type", StringComparison.Ordinal));
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        [TestMethod]
        public async Task PlainSpectreCommandApp_CliXmldoc_Succeeds()
        {
            using var output = new StringWriter();
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(output),
            });

            var app = new CommandApp();
            app.Configure(config =>
            {
                config.Settings.Console = console;
                config.SetApplicationName("testapp");
                config.AddCommand<DummyAsyncCommand>("dummy");
            });

            var exitCode = await app.RunAsync(new[] { "cli", "xmldoc" });
            var text = output.ToString();

            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(text, "<Model>");
            StringAssert.Contains(text, "DummyAsyncCommand");
            Assert.IsFalse(text.Contains("Could not resolve type", StringComparison.Ordinal));
        }

        [TestMethod]
        public void SpectreConsoleHostedService_NormalizeProcessExitCode_MapsHelpRequestToHelpVersionDisplayed()
        {
            var exitCode = SpectreConsoleHostedService.NormalizeProcessExitCode(0, new[] { "--help" });

            Assert.AreEqual((int)SpectreConsoleHostedService.ExitCode.HelpVersionDisplayed, exitCode);
        }

        [TestMethod]
        public void SpectreConsoleHostedService_NormalizeProcessExitCode_MapsNoArgsToHelpVersionDisplayed()
        {
            var exitCode = SpectreConsoleHostedService.NormalizeProcessExitCode(0, Array.Empty<string>());

            Assert.AreEqual((int)SpectreConsoleHostedService.ExitCode.HelpVersionDisplayed, exitCode);
        }

        [TestMethod]
        public void SpectreConsoleHostedService_NormalizeProcessExitCode_KeepsCliXmldocSuccessAtZero()
        {
            var exitCode = SpectreConsoleHostedService.NormalizeProcessExitCode(0, new[] { "cli", "xmldoc" });

            Assert.AreEqual(0, exitCode);
        }

        [TestMethod]
        public void SpectreConsoleHostedService_ShouldStopApplication_OnlyContinuesForSuccessAndContinue()
        {
            Assert.IsFalse(SpectreConsoleHostedService.ShouldStopApplication((int)SpectreConsoleHostedService.ExitCode.SuccessAndContinue));
            Assert.IsTrue(SpectreConsoleHostedService.ShouldStopApplication(0));
            Assert.IsTrue(SpectreConsoleHostedService.ShouldStopApplication((int)SpectreConsoleHostedService.ExitCode.SuccessAndExit));
        }
    }
}
