using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Coree.NETStandard.Extensions.Http.HttpHeader;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    /// <summary>
    /// Represents a detailed record of an HTTP transaction, including the request, response, and any exception that occurred.
    /// </summary>
    public class TransactionRecord
    {
        /// <summary>
        /// Indicates whether the transaction record was retrieved from cache.
        /// </summary>
        public bool IsFromCache { get; set; }

        /// <summary>
        /// The HTTP request message associated with the transaction.
        /// </summary>
        public HttpRequestMessage? HttpRequestMessage { get; set; }

        /// <summary>
        /// The HTTP response message received for the transaction.
        /// </summary>
        public HttpResponseMessage? HttpResponseMessage { get; set; }

        /// <summary>
        /// The last exception encountered during the transaction, if any.
        /// </summary>
        public Exception? LastException { get; set; }

        /// <summary>
        /// Retrieves the response as a byte array. Returns null if the response is unavailable or an error occurs during retrieval.
        /// </summary>
        public byte[]? ResponseBytes
        {
            get
            {
                if (HttpResponseMessage == null || HttpResponseMessage.Content == null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        return HttpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the response as a string, decoding it according to the response's content encoding. Returns null if the response is unavailable or decoding fails.
        /// </summary>
        public string? ResponseString
        {
            get
            {
                if (HttpResponseMessage == null || HttpResponseMessage.Content == null || HttpResponseMessage.Headers == null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        return HttpResponseMessage.Headers.GetContentEncoding().GetString(HttpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
                    }
                    catch
                    {
                        return null; // In case decoding fails
                    }
                }
            }
        }

        /// <summary>
        /// Indicates whether the HTTP response was successful and no exceptions occurred. Returns false if the response, its content, headers are null, or an exception was recorded.
        /// </summary>
        public bool ResponseSuccess
        {
            get
            {
                if (HttpResponseMessage == null || HttpResponseMessage.Content == null || HttpResponseMessage.Headers == null || LastException != null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        return HttpResponseMessage.IsSuccessStatusCode;
                    }
                    catch
                    {
                        return false;
                    }
                }

            }
        }

        /// <summary>
        /// Initializes a new instance of TransactionRecord with the specified HTTP request and response messages, and an optional exception.
        /// </summary>
        /// <param name="httpRequestMessage">The HTTP request message associated with the transaction.</param>
        /// <param name="httpResponseMessage">The HTTP response message received in response to the request.</param>
        /// <param name="lastException">The exception, if any, that occurred during the transaction.</param>
        public TransactionRecord(HttpRequestMessage? httpRequestMessage, HttpResponseMessage? httpResponseMessage, Exception? lastException = null)
        {
            this.HttpRequestMessage = httpRequestMessage;
            this.HttpResponseMessage = httpResponseMessage;
            this.LastException = lastException;
        }

    }
}
