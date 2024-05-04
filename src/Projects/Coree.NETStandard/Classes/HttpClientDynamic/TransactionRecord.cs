using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Coree.NETStandard.Extensions.Http.HttpHeader;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    public class TransactionRecord
    {
        public enum OperationStatus
        {
            Success,
            Failure
        }

        public byte[]? ContentBytes { get; set; }
        public HttpResponseHeaders? ResponseHeaders { get; set; }
        public HttpRequestHeaders? RequestHeaders { get; set; }
        public bool IsFromCache { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Exception? Exception { get; set; } // Exception information
        public OperationStatus Status { get; set; }
        public long? ContentLength { get; set; }

        public string ContentString
        {
            get
            {
                if (ContentBytes == null || ResponseHeaders == null)
                {
                    return string.Empty;
                }
                else
                {
                    try
                    {
                        return ResponseHeaders.GetContentEncoding().GetString(ContentBytes);
                    }
                    catch
                    {
                        return string.Empty; // In case decoding fails
                    }
                }
            }
        }

        // Constructor for successful response
        public TransactionRecord(byte[]? contentBytes, HttpResponseHeaders? responseHeaders, HttpRequestHeaders? requestHeaders, bool isFromCache, HttpStatusCode statusCode)
        {
            ContentBytes = contentBytes;
            ResponseHeaders = responseHeaders;
            RequestHeaders = requestHeaders;
            IsFromCache = isFromCache;
            StatusCode = statusCode;
            Status = OperationStatus.Success;
        }

        // Constructor for failure
        public TransactionRecord(Exception? exception, HttpStatusCode statusCode)
        {
            Exception = exception;
            StatusCode = statusCode;
            Status = OperationStatus.Failure;
        }
    }


    public class TransactionRecord2
    {
        public bool IsFromCache { get; set; }
        public HttpRequestMessage? httpRequestMessage { get; set; }
        public HttpResponseMessage? httpResponseMessage { get; set; }

        public byte[]? ContentBytes
        {
            get
            {
                if (httpResponseMessage == null || httpResponseMessage.Content == null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        return httpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public string? ContentString
        {
            get
            {
                if (httpResponseMessage == null || httpResponseMessage.Content == null || httpResponseMessage.Headers == null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        return httpResponseMessage.Headers.GetContentEncoding().GetString(httpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
                    }
                    catch
                    {
                        return null; // In case decoding fails
                    }
                }
            }
        }

        public bool IsSuccess
        {
            get
            {
                if (httpResponseMessage == null || httpResponseMessage.Content == null || httpResponseMessage.Headers == null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        return httpResponseMessage.IsSuccessStatusCode;
                    }
                    catch
                    {
                        return false;
                    }
                }

            }
        }

        // Constructor for successful response
        public TransactionRecord2(HttpRequestMessage? httpRequestMessage, HttpResponseMessage? httpResponseMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.httpResponseMessage = httpResponseMessage;
        }

    }
}
