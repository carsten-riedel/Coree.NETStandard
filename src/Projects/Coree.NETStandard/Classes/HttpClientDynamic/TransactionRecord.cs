using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Coree.NETStandard.Extensions.Http.HttpHeader;

namespace Coree.NETStandard.Classes.HttpRequestService
{

    public class TransactionRecord
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

        public TransactionRecord(HttpRequestMessage? httpRequestMessage, HttpResponseMessage? httpResponseMessage)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.httpResponseMessage = httpResponseMessage;
        }

    }
}
