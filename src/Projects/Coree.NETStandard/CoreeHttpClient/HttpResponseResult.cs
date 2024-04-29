using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.FluentBase;

namespace Coree.NETStandard.CoreeHttpClient
{
    public class HttpResponseResult
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

        private string? _contentString;

        public string ContentString
        {
            get
            {
                if (_contentString != null)
                {
                    return _contentString;
                }

                if (ContentBytes == null || ResponseHeaders == null)
                {
                    _contentString = string.Empty;
                }
                else
                {
                    try
                    {
                        _contentString = ResponseHeaders.GetContentEncoding().GetString(ContentBytes);
                    }
                    catch
                    {
                        _contentString = string.Empty; // In case decoding fails
                    }
                }
                return _contentString;
            }
        }

        // Constructor for successful response
        public HttpResponseResult(byte[]? contentBytes, HttpResponseHeaders? responseHeaders, HttpRequestHeaders? requestHeaders, bool isFromCache, HttpStatusCode statusCode)
        {
            ContentBytes = contentBytes;
            ResponseHeaders = responseHeaders;
            RequestHeaders = requestHeaders;
            IsFromCache = isFromCache;
            StatusCode = statusCode;
            Status = OperationStatus.Success;
        }

        // Constructor for failure
        public HttpResponseResult(Exception? exception, HttpStatusCode statusCode)
        {
            Exception = exception;
            StatusCode = statusCode;
            Status = OperationStatus.Failure;
        }
    }

    public class Product : FluentBase
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
    }

    public static class ProductExtensions
    {
        // Simplified synchronous method using Task.FromResult
        public static Product AdjustPrice(this Product product, decimal adjustment)
        {
            return AdjustPriceAsync(product, adjustment).GetAwaiter().GetResult();
        }

        public static async Task<Product> AdjustPriceAsync(this Task<Product> productTask, decimal adjustment)
        {
            Product product = await productTask;
            return await AdjustPriceAsync(product, adjustment);
        }

        // Remaining asynchronous extension method with adjusted signature
        public static async Task<Product> AdjustPriceAsync(this Product product, decimal adjustment)
        {
            // Simulate asynchronous operation with a delay
            await Task.Delay(100); // Replace with actual async operation

            // Same validation logic as AdjustPrice method
            if (!product.IsValid && !product.ContinueOnValidationError) return product;

            try
            {
                var newPrice = product.Price + adjustment;
                if (newPrice < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(adjustment), "Adjustment results in a negative price.");
                }
                product.Price = newPrice;
            }
            catch (Exception ex)
            {
                // Add a validation error asynchronously
                product.AddValidationError($"Error adjusting price by {adjustment}: {ex.Message}", ex);
            }

            return product;
        }
    }
}