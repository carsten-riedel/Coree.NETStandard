using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Abstractions.ServiceFactoryEx
{
    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T> : ServiceFactoryEx<T, object, object, object, object, object, object, object, object, object> where T : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K> : ServiceFactoryEx<T, K, object, object, object, object, object, object, object, object> where T : class where K : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R> : ServiceFactoryEx<T, K, R, object, object, object, object, object, object, object> where T : class where K : class where R : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S> : ServiceFactoryEx<T, K, R, S, object, object, object, object, object, object> where T : class where K : class where R : class where S : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="U">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S,U}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S, U> : ServiceFactoryEx<T, K, R, S, U, object, object, object, object, object> where T : class where K : class where R : class where S : class where U : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="U">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="V">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S,U,V}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S, U, V> : ServiceFactoryEx<T, K, R, S, U, V, object, object, object, object> where T : class where K : class where R : class where S : class where U : class where V : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="U">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="V">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="W">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S,U,V,W}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S, U, V, W> : ServiceFactoryEx<T, K, R, S, U, V, W, object, object, object> where T : class where K : class where R : class where S : class where U : class where V : class where W : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="U">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="V">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="W">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="X">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S,U,V,W,X}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S, U, V, W, X> : ServiceFactoryEx<T, K, R, S, U, V, W, X, object, object> where T : class where K : class where R : class where S : class where U : class where V : class where W : class where X : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="U">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="V">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="W">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="X">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="Y">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S,U,V,W,X,Y}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S, U, V, W, X, Y> : ServiceFactoryEx<T, K, R, S, U, V, W, X, Y, object> where T : class where K : class where R : class where S : class where U : class where V : class where W : class where X : class where Y : class {}

    /// <summary>
    /// Represents a generic abstract factory for creating and managing service instances of type <typeparamref name="T"/>.
    /// This factory leverages dependency injection and configuration patterns to facilitate the creation and lifecycle management of services.
    /// </summary>
    /// <typeparam name="T">The type of service to be created and managed. This type must be a class that implements IDisposable.</typeparam>
    /// <typeparam name="K">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="R">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="S">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="U">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="V">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="W">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="X">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="Y">Additional service type managed by this factory.</typeparam>
    /// <typeparam name="Z">Additional service type managed by this factory.</typeparam>
    /// <remarks>
    /// The <see cref="ServiceFactoryEx{T,K,R,S,U,V,W,X,Y,Z}"/> serves as a foundational component in applications requiring robust and configurable service instantiation.
    /// It abstracts the complexity involved in the instantiation and management of services, thereby promoting a clean and maintainable architecture.
    /// This class should be inherited by specific service factory implementations that can provide concrete and custom instantiation logic.
    /// <example>
    /// The following example shows how to use the FileService in a console application:
    /// <code>
    /// private static async Task Main(string[] args)
    /// {
    ///     // Creating a FileService instance without params nullable ILogger.
    ///     var fileService1 = new FileService();
    ///     // Creating a FileService instance using the constructor that accepts an ILogger.
    ///     var fileService2 = new FileService(new Logger&lt;FileService&gt;());
    ///     // Creating a FileService instance using the factory method.
    ///     var fileService3 = FileService.CreateServiceFactory();
    ///
    ///     // Example method calls on fileService instances
    ///     await fileService1.SomeFileOperationAsync();
    ///     await fileService2.SomeFileOperationAsync();
    ///     fileService3.SomeFileOperation();
    /// }
    ///
    /// public partial class FileService : ServiceFactoryEx&lt;FileService&gt;, IFileService
    /// {
    ///    private readonly ILogger&lt;FileService&gt;? _logger;
    ///    public FileService(ILogger&lt; FileService&gt;? logger = null)
    ///    {
    ///       this._logger = logger;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class ServiceFactoryEx<T, K, R, S, U, V, W, X, Y, Z> : IDisposable where T : class where K : class where R : class where S : class where U : class where V : class where W : class where X : class  where Y : class  where Z : class
    {
        private static IHostBuilder? _hostBuilder;
        private static IHost? _host;

        /// <summary>
        /// Creates a service instance with optional host configuration.
        /// </summary>
        /// <param name="configureHost">An action to configure the host. Can be null.</param>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        public static T CreateServiceFactory(Action<IHostBuilder>? configureHost = null)
        {
            return CreateServiceStack(null, null, configureHost);
        }

        /// <summary>
        /// Creates a service instance with optional service collection configuration.
        /// </summary>
        /// <param name="services">An action to configure the services. Can be null.</param>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        public static T CreateServiceFactory(Action<IServiceCollection>? services = null)
        {
            return CreateServiceStack(services, null, null);
        }

        /// <summary>
        /// Creates a service instance with optional configurations for services, logging, and host.
        /// </summary>
        /// <param name="configureServices">An action to configure the services. Can be null.</param>
        /// <param name="configureLogging">An action to configure the logging. Can be null.</param>
        /// <param name="configureHost">An action to configure the host. Can be null.</param>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        public static T CreateServiceFactory(Action<IServiceCollection>? configureServices = null, Action<ILoggingBuilder>? configureLogging = null, Action<IHostBuilder>? configureHost = null)
        {
            return CreateServiceStack(configureServices, configureLogging, configureHost);
        }

        /// <summary>
        /// Creates a service instance with a default logging level set to Trace.
        /// </summary>
        /// <returns>A new instance of type <typeparamref name="T"/> with default logging configuration.</returns>
        /// <remarks>
        /// This method simplifies the creation of a service instance by applying a default Trace logging level.
        /// It delegates to another factory method, providing a standardized logging setup for basic tracing of application activities.
        /// </remarks>
        public static T CreateServiceFactory()
        {
            return CreateServiceFactory(LogLevel.Trace);
        }

        /// <summary>
        /// Creates a service instance with a default logging level.
        /// </summary>
        /// <param name="logLevel">The logging level to be used as default.</param>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        public static T CreateServiceFactory(LogLevel logLevel)
        {
            return CreateServiceStack(configureServices: null, configureLogging: (log) => ConfigureDefaultLogging(log, logLevel), configureHost: (host) => ConfigureDefaultHost(host));
        }

        private static void ConfigureDefaultLogging(ILoggingBuilder log, LogLevel logLevel)
        {
            log.SetMinimumLevel(logLevel);
            log.AddFilter((category, level) => level >= logLevel);
        }

        private static void ConfigureDefaultHost(IHostBuilder host)
        {
            host.UseConsoleLifetime(lifeTimeOptions => { lifeTimeOptions.SuppressStatusMessages = true; });
        }

        /// <summary>
        /// Internally constructs and configures a service stack of type <typeparamref name="T"/> based on provided actions.
        /// </summary>
        /// <param name="configureServices">An optional action to configure additional services.</param>
        /// <param name="configureLogging">An optional action to configure logging within the service stack.</param>
        /// <param name="configureHost">An optional action to configure the hosting environment.</param>
        /// <returns>An instance of <typeparamref name="T"/> created within the configured service environment.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the created service instance is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is a failure in building the service instance.</exception>
        /// <remarks>
        /// This method is a critical part of the service factory infrastructure, encapsulating the complex logic of service setup.
        /// It ensures that the service, logging, and host configurations are applied before building and starting the host.
        /// This method is not exposed publicly and should only be used within the <see cref="ServiceFactoryEx{T,K,R,S,U,V,W,X,Y,Z}"/> class.
        /// </remarks>
        private static T CreateServiceStack(Action<IServiceCollection>? configureServices = null, Action<ILoggingBuilder>? configureLogging = null, Action<IHostBuilder>? configureHost = null)
        {
            try
            {
                _hostBuilder = Host.CreateDefaultBuilder();

                _hostBuilder.ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddMemoryCache();

                    if (typeof(K) != typeof(object)) { services.AddSingleton<K>(); }
                    if (typeof(R) != typeof(object)) { services.AddSingleton<R>(); }
                    if (typeof(S) != typeof(object)) { services.AddSingleton<S>(); }
                    if (typeof(U) != typeof(object)) { services.AddSingleton<U>(); }
                    if (typeof(V) != typeof(object)) { services.AddSingleton<W>(); }
                    if (typeof(X) != typeof(object)) { services.AddSingleton<X>(); }
                    if (typeof(Y) != typeof(object)) { services.AddSingleton<Y>(); }
                    if (typeof(Z) != typeof(object)) { services.AddSingleton<Z>(); }

                    services.AddLogging(loggingBuilder =>
                    {
                        configureLogging?.Invoke(loggingBuilder);
                    });
                    configureServices?.Invoke(services);
                });

                configureHost?.Invoke(_hostBuilder);

                _host = _hostBuilder.Build();

                _host.Start();

                var serviceProvider = _host.Services.GetRequiredService<IServiceProvider>();

                T instance = ActivatorUtilities.CreateInstance<T>(serviceProvider);

                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance));
                }
                return instance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create instance.", ex);
            }
        }

        /// <summary>
        /// Disposes the static instance of the host if it has been created.
        /// </summary>
        public void Dispose()
        {
            _host?.StopAsync().ConfigureAwait(false);
        }
    }
}