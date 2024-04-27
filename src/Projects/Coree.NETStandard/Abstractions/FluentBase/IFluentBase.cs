using System;
using System.Collections.Generic;

namespace Coree.NETStandard.Abstractions.FluentBase
{
    /// <summary>
    /// Serves as a foundational class for fluent interface pattern error handling, enabling mechanisms for both error accumulation and immediate exception throwing, alongside optional continuation after encountering validation errors. This abstract class standardizes error management for classes utilizing fluent interfaces.
    /// </summary>
    /// <remarks>
    /// Implements IFluentBase, offering:
    /// - Error accumulation during operations.
    /// - Conditional continuation of operations post-errors.
    /// - Flexible toggling between immediate exception throwing or error accumulation, adapting to various error handling strategies.
    /// </remarks>
    /// <example>
    /// Illustrates the use of FluentBase with a concrete Product class and an extension method for price adjustment with comprehensive error handling:
    /// <code>
    /// public abstract class FluentBase : IFluentBase
    /// {
    ///     private bool throwOnError = true;
    ///     public List&lt;Exception&gt; ValidationErrors { get; private set; } = new List&lt;Exception&gt;();
    ///     public bool IsValid =&gt; !ValidationErrors.Any();
    ///     public bool ContinueOnValidationError { get; private set; } = false;
    ///
    ///     public void AddValidationError(string message, Exception innerException)
    ///     {
    ///         ValidationErrors.Add(new Exception(message, innerException));
    ///         if (throwOnError)
    ///         {
    ///             throw innerException;
    ///         }
    ///     }
    ///
    ///     public IFluentBase EnableThrowOnError()
    ///     {
    ///         throwOnError = true;
    ///         ContinueOnValidationError = false;
    ///         return this;
    ///     }
    ///
    ///     public IFluentBase DisableThrowOnError(bool continueOnValidationError = false)
    ///     {
    ///         throwOnError = false;
    ///         ContinueOnValidationError = continueOnValidationError;
    ///         return this;
    ///     }
    /// }
    ///
    /// // Simplified Product class inheriting FluentBase
    /// public class Product : FluentBase
    /// {
    ///     public string Name { get; set; }
    ///     public decimal Price { get; set; }
    /// }
    ///
    /// // Extension method for Product
    /// public static class ProductExtensions
    /// {
    ///     public static Product AdjustPrice(this Product product, decimal adjustment)
    ///     {
    ///         if (!product.IsValid &amp;&amp; !product.ContinueOnValidationError) return product;
    ///
    ///         try
    ///         {
    ///             var newPrice = product.Price + adjustment;
    ///             if (newPrice &lt; 0)
    ///             {
    ///                 throw new ArgumentOutOfRangeException(nameof(adjustment), "Adjustment results in a negative price.");
    ///             }
    ///             product.Price = newPrice;
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             product.AddValidationError($"Error adjusting price by {adjustment}: {ex.Message}", ex);
    ///         }
    ///
    ///         return product;
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IFluentBase
    {
        /// <summary>
        /// Indicates whether the current object is in a valid state.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// A collection of exceptions that have been accumulated during operations.
        /// </summary>
        List<Exception> ValidationErrors { get; }

        /// <summary>
        /// Indicates whether operations should continue after a validation error has occurred.
        /// </summary>
        bool ContinueOnValidationError { get; }

        /// <summary>
        /// Adds a validation error to the collection with a custom message and the exception that occurred.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that occurred during validation.</param>
        void AddValidationError(string message, Exception innerException);

        /// <summary>
        /// Enables the behavior to throw exceptions immediately when a validation error occurs.
        /// Resets <c>ContinueOnValidationError</c> to <c>false</c>.
        /// </summary>
        /// <returns>The current instance of <see cref="IFluentBase"/>.</returns>
        IFluentBase EnableThrowOnError();

        /// <summary>
        /// Disables the behavior of throwing exceptions immediately, allowing operations to accumulate validation errors.
        /// Optionally allows continuation after validation errors based on the <paramref name="continueOnValidationError"/> parameter.
        /// </summary>
        /// <param name="continueOnValidationError">Determines whether to continue operations after a validation error has occurred.</param>
        /// <returns>The current instance of <see cref="IFluentBase"/>.</returns>
        IFluentBase DisableThrowOnError(bool continueOnValidationError = false);
    }
}