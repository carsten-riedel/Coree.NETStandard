using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using Json.More;
using Json.Path;

namespace Coree.NETStandard.Extensions.Conversions.String
{
    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class ConversionsStringExtensions
    {
        /// <summary>
        /// Converts a JSON-formatted string to a JsonNode.
        /// </summary>
        /// <param name="jsonString">The JSON string to convert.</param>
        /// <returns>A JsonNode representation of the JSON string, or null if the string is null.</returns>
        public static JsonNode? ToJsonNode(this string? jsonString)
        {
            if (jsonString == null) { return null; }
            return jsonString.ToJsonDocument()?.RootElement.AsNode();
        }

        /// <summary>
        /// Converts a JSON-formatted string to a JsonDocument.
        /// </summary>
        /// <param name="jsonString">The JSON string to convert.</param>
        /// <returns>A JsonDocument representation of the JSON string, or null if the string is null.</returns>
        public static JsonDocument? ToJsonDocument(this string? jsonString)
        {
            if (jsonString == null) { return null; }
            return JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        }

        /// <summary>
        /// Evaluates a JSON Path expression on a JSON-formatted string and returns the result.
        /// </summary>
        /// <param name="jsonString">The JSON string to evaluate.</param>
        /// <param name="jsonPath">The JSON Path expression to evaluate.</param>
        /// <returns>A PathResult containing the evaluation result, or null if the string is null.</returns>
        public static PathResult? ToJsonPathResult(this string? jsonString, string jsonPath)
        {
            if (jsonString == null) { return null; }
            var path = JsonPath.Parse(jsonPath, new PathParsingOptions() { });
            var result = path.Evaluate(jsonString.ToJsonNode());
            return result;
        }

        /// <summary>
        /// Evaluates a JSON Path expression on a JSON-formatted string and returns a list of values.
        /// </summary>
        /// <param name="jsonString">The JSON string to evaluate.</param>
        /// <param name="jsonPath">The JSON Path expression to evaluate.</param>
        /// <returns>A list of values matching the JSON Path expression, or null if the string or results are null.</returns>
        public static List<T>? ToJsonPathResultValues<T>(this string? jsonString, string jsonPath)
        {
            var pathResult = jsonString.ToJsonPathResult(jsonPath);
            if (pathResult == null) { return null; }
            var result = pathResult.Matches?.Where(e=>e.Value != null).Select(e => e.Value!.GetValue<T>()).ToList();
            return result;
        }
    }
}
