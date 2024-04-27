using System;
using System.Collections.Generic;
using System.Web;

namespace Coree.NETStandard.CoreeHttpClient
{
    /// <summary>
    /// Provides extension methods for Uri objects.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Adds new query parameters to the URI or updates them if they already exist.
        /// </summary>
        /// <param name="uri">The URI to add or update query parameters for.</param>
        /// <param name="parametersToAddOrUpdate">The parameters to add or update in the URI query.</param>
        /// <returns>A new Uri object with the added or updated query parameters.</returns>
        /// <remarks>
        /// If the input dictionary is null, the original Uri is returned without modifications. Parameters are URL-encoded as necessary.
        /// </remarks>
        public static Uri AddOrUpdateQueryParameters(this Uri uri, Dictionary<string, string>? parametersToAddOrUpdate)
        {
            if (parametersToAddOrUpdate == null)
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var param in parametersToAddOrUpdate)
            {
                query[param.Key] = param.Value;
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }
    }
}