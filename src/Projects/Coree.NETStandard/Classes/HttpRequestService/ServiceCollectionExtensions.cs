using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Coree.NETStandard.CoreeHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to facilitate the registration
    /// of hosted services with specific configurations.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures and adds HTTP request services and handlers to the service collection with pre-defined request headers and content types.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <returns>The updated IServiceCollection with HTTP request services and handlers configured.</returns>
        public static IServiceCollection AddHttpRequestService(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHttpClient(nameof(HttpRequestService), (provider, client) =>
            {
                client.DefaultRequestHeaders.Accept.Clear();
                // Basic types: JSON, XML, Plain Text
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json",1.0));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 1.0));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain", 1.0));

                // Additional common types
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html",0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/javascript", 0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/csv", 0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data", 0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded", 0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf", 0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png", 0.8));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/jpeg", 0.8));

                // Accept any other types with a lower quality value
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream",0.1));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.1));

                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));

            }).ConfigurePrimaryHttpMessageHandler<HttpRequestServiceHandler>();


            services.AddTransient<HttpRequestService>();
            services.AddTransient<HttpRequestServiceHandler>();

            return services;
        }
    }
}
