using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Actions
{
    /// <summary>
    /// Makes an HTTP request.
    /// </summary>
    public class HttpRequestAction : ActionBase
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public override string TypeId => "http_request";
        public override string DisplayName => "HTTP Request";

        /// <summary>
        /// Target URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE).
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// Request body for POST/PUT.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Content type (e.g., "application/json").
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        public override async Task ExecuteAsync(KeyboardEventArgs keyEvent)
        {
            if (string.IsNullOrWhiteSpace(Url))
                return;

            try
            {
                HttpResponseMessage response;

                switch (Method?.ToUpperInvariant())
                {
                    case "POST":
                        var content = new StringContent(Body ?? "", Encoding.UTF8, ContentType);
                        response = await _httpClient.PostAsync(Url, content);
                        break;

                    case "PUT":
                        var putContent = new StringContent(Body ?? "", Encoding.UTF8, ContentType);
                        response = await _httpClient.PutAsync(Url, putContent);
                        break;

                    case "DELETE":
                        response = await _httpClient.DeleteAsync(Url);
                        break;

                    default: // GET
                        response = await _httpClient.GetAsync(Url);
                        break;
                }

                // Fire and forget - we don't need the response
                response.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP request failed: {ex.Message}");
            }
        }

        public override bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                error = "URL is required";
                return false;
            }

            if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
            {
                error = "Invalid URL format";
                return false;
            }

            error = null;
            return true;
        }
    }
}
