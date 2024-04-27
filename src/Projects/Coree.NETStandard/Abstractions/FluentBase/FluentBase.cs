using System;
using System.Collections.Generic;
using System.Linq;

namespace Coree.NETStandard.Abstractions.FluentBase
{
    /// <summary>
    /// Acts as an abstract foundation for structured error handling in classes using a fluent interface pattern. Offers mechanisms to either immediately throw or accumulate exceptions, alongside managing continuation after validation errors.
    /// </summary>
    /// <remarks>
    /// Implements the <see cref="IFluentBase"/> interface to supply default methods and properties, facilitating the easy integration of a uniform error handling strategy across derived classes. This base class is designed to empower your fluent interfaces with sophisticated error handling capabilities, allowing for the graceful accumulation of errors for later analysis or immediate failure upon error detection to prevent further processing under invalid conditions.
    /// <para>
    /// Example usage in a simplified Product class inheriting from FluentBase and an extension method to adjust its price:
    /// <code>
    /// // Inheriting FluentBase in a Product class
    /// public class Product : FluentBase
    /// {
    ///     public string Name { get; set; }
    ///     public decimal Price { get; set; }
    /// }
    ///
    /// // Extension method for the Product class to adjust its price
    /// public static class ProductExtensions
    /// {
    ///     public static Product AdjustPrice(this Product product, decimal adjustment)
    ///     {
    ///         // Early exit if the product is invalid and continuation on validation error is not enabled
    ///         if (!product.IsValid &amp;&amp; !product.ContinueOnValidationError) return product;
    ///
    ///         try
    ///         {
    ///             // Attempt to adjust the product's price
    ///             var newPrice = product.Price + adjustment;
    ///             if (newPrice &lt; 0)
    ///             {
    ///                 throw new ArgumentOutOfRangeException(nameof(adjustment), "Adjustment results in a negative price.");
    ///             }
    ///             product.Price = newPrice;
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             // Add a validation error in case of an exception
    ///             product.AddValidationError($"Error adjusting price by {adjustment}: {ex.Message}", ex);
    ///         }
    ///
    ///         return product;
    ///     }
    /// }
    /// </code>
    /// This exemplifies how `Product`, by inheriting `FluentBase`, utilizes built-in error handling for a price adjustment operation, demonstrating the class's utility in creating fluent interfaces with integrated error management.
    /// </para>
    /// </remarks>
    public abstract class FluentBase : IFluentBase
    {
        /// <summary>
        /// Gets a value indicating whether exceptions are thrown immediately upon encountering a validation error.
        /// This property is set through <see cref="EnableThrowOnError"/> and <see cref="DisableThrowOnError"/> methods.
        /// </summary>
        public bool ThrowOnError { get; private set; } = true;

        /// <summary>
        /// Gets a collection of exceptions that have been accumulated during operation executions.
        /// </summary>
        public List<Exception> ValidationErrors { get; private set; } = new List<Exception>();

        /// <summary>
        /// Indicates whether the current object is in a valid state, based on the absence of validation errors.
        /// </summary>
        public bool IsValid => !ValidationErrors.Any();

        /// <summary>
        /// Indicates whether operations should continue after a validation error has occurred, based on the current configuration.
        /// </summary>
        public bool ContinueOnValidationError { get; private set; } = false;

        /// <summary>
        /// Adds a specified validation error to the collection, throwing an exception if configured to do so.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that represents the error.</param>
        public void AddValidationError(string message, Exception innerException)
        {
            var exception = new Exception(message, innerException);
            ValidationErrors.Add(exception);

            if (ThrowOnError)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Configures the class to throw exceptions immediately upon encountering a validation error.
        /// </summary>
        /// <returns>The current instance of <see cref="FluentBase"/>, enabling fluent configuration.</returns>
        public IFluentBase EnableThrowOnError()
        {
            ThrowOnError = true;
            ContinueOnValidationError = false;
            return this;
        }

        /// <summary>
        /// Configures the class to accumulate validation errors without throwing exceptions immediately, with an option to continue operations after encountering a validation error.
        /// </summary>
        /// <param name="continueOnValidationError">Indicates whether to continue operations after a validation error has occurred.</param>
        /// <returns>The current instance of <see cref="FluentBase"/>, enabling fluent configuration.</returns>
        public IFluentBase DisableThrowOnError(bool continueOnValidationError = false)
        {
            ThrowOnError = false;
            ContinueOnValidationError = continueOnValidationError;
            return this;
        }
    }
}