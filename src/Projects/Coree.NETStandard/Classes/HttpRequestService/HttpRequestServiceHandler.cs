using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    public class HttpRequestServiceHandler : HttpClientHandler, IDisposable
    {
        public HttpRequestServiceHandler(ILogger<HttpRequestServiceHandler> logger) : base()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            AllowAutoRedirect = true;
            ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
        }

        public bool ServerCertificateCustomValidation(HttpRequestMessage sender, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
