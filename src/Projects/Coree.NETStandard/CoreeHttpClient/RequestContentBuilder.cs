using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Coree.NETStandard.CoreeHttpClient
{
    public class RequestContentBuilder
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

        public RequestContentBuilder() { }

        public RequestContentBuilder(string data, Encoding? encoding = null, string? name = null)
        {
            AddContent(data, encoding, name);
        }

        public RequestContentBuilder(Dictionary<string, string> formData, string? name = null)
        {
            AddContent(formData, name);
        }

        public RequestContentBuilder(byte[] bytes, string? name = null)
        {
            AddContent(bytes, name);
        }

        public RequestContentBuilder(string data, string mediaType, Encoding? encoding = null, string? name = null)
        {
            AddContent(data, mediaType, encoding, name);
        }

        public RequestContentBuilder(Stream stream, string? name = null, string? filename = null)
        {
            AddContent(stream, name, filename);
        }

        public void AddContent(string data, string mediaType, Encoding? encoding = null, string? name = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            contents.Add(new NamedHttpContent(new StringContent(data, encoding, mediaType), name));
        }

        public void AddContent(string data, Encoding? encoding = null, string? name = null)
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

        public void AddContent(Dictionary<string, string> formData, string? name = null)
        {
            contents.Add(new NamedHttpContent(new FormUrlEncodedContent(formData), name));
        }

        // Add binary data as application/octet-stream
        public void AddContent(byte[] bytes, string? name = null)
        {
            var byteContent = new ByteArrayContent(bytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            contents.Add(new NamedHttpContent(byteContent, name));
        }

        public void AddContent(Stream stream, string? name = null, string? filename = null)
        {
            var streamContent = new StreamContent(stream);
            contents.Add(new NamedHttpContent(streamContent, name, filename));
        }

        // Consolidate all added content into a single HttpContent instance
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