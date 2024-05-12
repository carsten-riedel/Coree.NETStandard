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
    /// <summary>
    /// Handles HTTP requests with custom server certificate validation and automatic decompression.
    /// </summary>
    public class HttpRequestServiceHandler : HttpClientHandler, IDisposable
    {
        private readonly ILogger<HttpRequestServiceHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestServiceHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger used to log SSL policy errors and other information.</param>
        public HttpRequestServiceHandler(ILogger<HttpRequestServiceHandler> logger) : base()
        {
            this.logger = logger;
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            AllowAutoRedirect = true;
            ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
        }

        /// <summary>
        /// Validates the server certificate with custom logic.
        /// </summary>
        /// <param name="sender">The originating request message that triggered the validation.</param>
        /// <param name="certificate">The certificate to validate.</param>
        /// <param name="chain">The chain of certificates associated with the server's certificate.</param>
        /// <param name="errors">SSL policy errors encountered during the validation.</param>
        /// <returns>true if the certificate is considered valid; otherwise, false.</returns>
        /// <remarks>
        /// This method logs any SSL policy errors. It currently returns true for all certificates, which is insecure for production environments.
        /// Consider implementing a more stringent validation logic.
        /// </remarks>
        public bool ServerCertificateCustomValidation(HttpRequestMessage sender, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors != SslPolicyErrors.None)
            {
                logger.LogError($"SSL policy errors detected: {errors}. Certificate Subject: {certificate.Subject}");
            }

            // Always returning true is insecure; this should be modified to implement proper validation logic.
            return true;
        }
    }
}
