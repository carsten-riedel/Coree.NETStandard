using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


using Microsoft.Extensions.DependencyInjection;

namespace Coree.NETStandard.CoreeHttpClient
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreeHttpClient(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHttpClient("CoreeHttpClient", clientConfig =>
            {
                clientConfig.DefaultRequestHeaders.Accept.Clear();
                clientConfig.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")
                {
                    Parameters = { new NameValueHeaderValue("charset", "utf-8") }
                });
                clientConfig.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                clientConfig.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            });

            // Register CoreeHttpClient as a singleton implementation of ICoreeHttpClient
            services.AddSingleton<ICoreeHttpClient, CoreeHttpClient>();

            return services;
        }
    }
}
