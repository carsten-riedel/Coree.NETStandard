using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    /// <summary>
    /// Facilitates the creation and management of HTTP request content.
    /// </summary>
    public class ContentComposer
    {
        private class NamedHttpContent
        {
            public HttpContent HttpContent { get; set; }
            public string? Name { get; set; }
            public string? Filename { get; set; }

            public NamedHttpContent(HttpContent httpContent, string? name = null, string? filename = null)
            {
                this.HttpContent = httpContent;
                this.Name = name;
                this.Filename = filename;
            }
        }

        private List<NamedHttpContent> contents = new List<NamedHttpContent>();

        private bool IsValidJson(string jsonString)
        {
            try
            {
                JsonSerializer.Deserialize<object>(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private bool IsValidXml(string xmlString)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ContentComposer with no initial content.
        /// </summary>
        public ContentComposer()
        { }

        /// <summary>
        /// Initializes a new instance of the ContentComposer and adds string content automatically determining the media type.
        /// </summary>
        /// <param name="data">The string data to be added as content.</param>
        /// <param name="encoding">The encoding to use for the content. Defaults to UTF-8 if not specified.</param>
        /// <param name="name">An optional name for the content part in a multipart form data context.</param>
        /// <remarks>
        /// The content type is determined based on whether the data is valid JSON or XML. If neither, 'text/plain' is used.
        /// </remarks>
        public ContentComposer(string data, Encoding? encoding = null, string? name = null)
        {
            AddAutoStringContent(data, encoding, name);
        }

        /// <summary>
        /// Initializes a new instance of the ContentComposer with form URL encoded content from a dictionary.
        /// </summary>
        /// <param name="formData">The form data as a dictionary of key-value pairs.</param>
        /// <param name="name">An optional name for the content part in multipart form data.</param>
        /// <remarks>
        /// This constructor is ideal for constructing content for HTTP POST requests using 'application/x-www-form-urlencoded'.
        /// </remarks>
        public ContentComposer(Dictionary<string, string> formData, string? name = null)
        {
            AddFormUrlEncodedContent(formData, name);
        }

        /// <summary>
        /// Initializes a new instance of the ContentComposer and adds byte array content.
        /// </summary>
        /// <param name="bytes">The byte array to be added as content.</param>
        /// <param name="name">An optional name for the content part, useful in multipart form data scenarios.</param>
        /// <remarks>
        /// The content type for the byte array is set to 'application/octet-stream' by default.
        /// </remarks>
        public ContentComposer(byte[] bytes, string? name = null)
        {
            AddByteArrayContent(bytes, name);
        }

        /// <summary>
        /// Initializes a new instance of the ContentComposer and adds plain string content with a specified media type.
        /// </summary>
        /// <param name="data">The string data to add as content.</param>
        /// <param name="mediaType">The media type to use for the content (e.g., "text/plain", "application/json").</param>
        /// <param name="encoding">The encoding of the string content, defaulting to UTF-8 if not specified.</param>
        /// <param name="name">An optional descriptor for the content part, useful in multipart scenarios.</param>
        /// <remarks>
        /// This method directly adds string content with the specified media type, useful for simple text-based content types.
        /// </remarks>
        public ContentComposer(string data, string mediaType, Encoding? encoding = null, string? name = null)
        {
            AddPlainStringContent(data, mediaType, encoding, name);
        }

        /// <summary>
        /// Initializes a new instance of the ContentComposer and adds stream content, suitable for large data uploads like files.
        /// </summary>
        /// <param name="stream">The stream representing the content to be uploaded.</param>
        /// <param name="name">An optional name for the content part, used in multipart form data.</param>
        /// <param name="filename">An optional filename for the content part, used when adding file-based content in multipart form data.</param>
        /// <remarks>
        /// Stream content is not read into memory all at once, making this method suitable for large file uploads.
        /// </remarks>
        public ContentComposer(Stream stream, string? name = null, string? filename = null)
        {
            AddStreamContent(stream, name, filename);
        }

        /// <summary>
        /// Adds plain string content with a specified media type to the content composer.
        /// </summary>
        /// <param name="data">The string data to add as content.</param>
        /// <param name="mediaType">The media type to use for the content (e.g., "text/plain", "application/json").</param>
        /// <param name="encoding">The encoding of the string content, defaulting to UTF-8 if not specified.</param>
        /// <param name="name">An optional descriptor for the content part, useful in multipart scenarios.</param>
        /// <remarks>
        /// This method directly adds string content with the specified media type, useful for simple text-based content types.
        /// </remarks>
        public void AddPlainStringContent(string data, string mediaType, Encoding? encoding = null, string? name = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            contents.Add(new NamedHttpContent(new StringContent(data, encoding, mediaType), name));
        }

        /// <summary>
        /// Adds string content to the composer, automatically determining the appropriate media type based on the content.
        /// </summary>
        /// <param name="data">The string data to be added as content.</param>
        /// <param name="encoding">The encoding to use for the content. Defaults to UTF-8 if not specified.</param>
        /// <param name="name">An optional name for the content part in a multipart form data context.</param>
        /// <remarks>
        /// This method checks if the data is valid JSON or XML and sets the content type accordingly.
        /// If neither JSON nor XML is detected, the content type defaults to 'text/plain'.
        /// </remarks>
        public void AddAutoStringContent(string data, Encoding? encoding = null, string? name = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;

            if (IsValidJson(data))
            {
                contents.Add(new NamedHttpContent(new StringContent(data, encoding, "application/json"), name));
            }
            else if (IsValidXml(data))
            {
                contents.Add(new NamedHttpContent(new StringContent(data, encoding, "application/xml"), name));
            }
            else
            {
                contents.Add(new NamedHttpContent(new StringContent(data, encoding, "text/plain"), name));
            }
        }

        /// <summary>
        /// Adds form URL encoded content constructed from a dictionary of key-value pairs.
        /// </summary>
        /// <param name="formData">The dictionary containing form data as key-value pairs.</param>
        /// <param name="name">An optional name for the content part in multipart form data.</param>
        /// <remarks>
        /// Use this method to encode dictionary data as 'application/x-www-form-urlencoded', which is commonly used in HTTP POST requests.
        /// </remarks>
        public void AddFormUrlEncodedContent(Dictionary<string, string> formData, string? name = null)
        {
            contents.Add(new NamedHttpContent(new FormUrlEncodedContent(formData), name));
        }

        /// <summary>
        /// Adds byte array content to the composer.
        /// </summary>
        /// <param name="bytes">The byte array to be added as content.</param>
        /// <param name="name">An optional name for the content part, useful in multipart form data scenarios.</param>
        /// <remarks>
        /// This method wraps the provided byte array into a ByteArrayContent and sets the content type to 'application/octet-stream' by default.
        /// </remarks>
        public void AddByteArrayContent(byte[] bytes, string? name = null)
        {
            var byteContent = new ByteArrayContent(bytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            contents.Add(new NamedHttpContent(byteContent, name));
        }

        /// <summary>
        /// Adds stream content to the composer, useful for large data uploads like files.
        /// </summary>
        /// <param name="stream">The stream representing the content to be uploaded.</param>
        /// <param name="name">An optional name for the content part, used in multipart form data.</param>
        /// <param name="filename">An optional filename for the content part, used when adding file-based content in multipart form data.</param>
        /// <remarks>
        /// Stream content is not read into memory all at once, making this method suitable for large file uploads.
        /// </remarks>
        public void AddStreamContent(Stream stream, string? name = null, string? filename = null)
        {
            var streamContent = new StreamContent(stream);
            contents.Add(new NamedHttpContent(streamContent, name, filename));
        }

        /// <summary>
        /// Consolidates all added content into a single HttpContent instance, suitable for HTTP transmission.
        /// </summary>
        /// <returns>A HttpContent instance containing all the previously added content pieces, or null if no content was added.</returns>
        /// <remarks>
        /// If multiple content pieces are added, they are combined into a MultipartFormDataContent. If only one piece is added, it is returned directly.
        /// </remarks>
        public HttpContent? Build()
        {
            if (contents.Count == 0)
            {
                return null;
            }
            else if (contents.Count == 1)
            {
                return contents[0].HttpContent;
            }
            else
            {
                var multiPartContent = new MultipartFormDataContent();
                foreach (var content in contents)
                {
                    if (content.Name != null && content.Filename != null)
                    {
                        multiPartContent.Add(content.HttpContent, content.Name, content.Filename);
                    }
                    else if (content.Name != null)
                    {
                        multiPartContent.Add(content.HttpContent, content.Name);
                    }
                    else
                    {
                        multiPartContent.Add(content.HttpContent);
                    }
                }
                return multiPartContent;
            }
        }
    }
}
