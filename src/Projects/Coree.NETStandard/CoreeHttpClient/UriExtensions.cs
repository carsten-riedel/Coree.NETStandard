using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Coree.NETStandard.CoreeHttpClient
{
    public static class UriExtensions
    {
        public static Uri AddOrUpdateQueryParameters(this Uri uri, Dictionary<string, string>? parametersToAddOrUpdate)
        {
            // If the dictionary is null, return the original Uri without modifications
            if (parametersToAddOrUpdate == null)
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var param in parametersToAddOrUpdate)
            {
                query[param.Key] = param.Value; // This adds or updates as needed
            }

            // This will URL-encode the parameters as necessary
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }
    }
}
