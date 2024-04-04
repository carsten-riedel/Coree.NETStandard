using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.CoreeHttpClient
{
    public class RequestParamBuilder
    {
        public Dictionary<string, string>? QueryParams { get; private set; }
        public Dictionary<string, string>? Headers { get; private set; }
        public Dictionary<string, string>? Cookies { get; private set; }

        public RequestParamBuilder(Dictionary<string, string>? queryParams = null, Dictionary<string, string>? headers = null, Dictionary<string, string>? cookies = null)
        {
            QueryParams = queryParams;
            Headers = headers;
            Cookies = cookies;
        }
    }

}
