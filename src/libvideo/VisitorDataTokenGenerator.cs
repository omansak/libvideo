using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace VideoLibrary
{
    internal class VisitorDataTokenGenerator : IDisposable
    {
        private bool _disposed;
        private static string _visitorData = string.Empty;


        public static async Task<string> GetVisitorDataFromYouTube(HttpClient http)
        {
            // Return cached visitor data if available
            if (!string.IsNullOrEmpty(_visitorData))
                return _visitorData;

            try
            {
                // Configure request headers
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                const string url = "https://www.youtube.com/sw.js_data";
                var response = await http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                // Remove the ")]}'" prefix if present
                jsonString = jsonString.StartsWith(")]}'") ? jsonString.Substring(4) : jsonString;

                var doc = JsonDocument.Parse(jsonString);
                var value = doc.RootElement[0]
                    .EnumerateArray()
                    .ElementAt(2)
                    .EnumerateArray()
                    .ElementAt(0)
                    .EnumerateArray()
                    .ElementAt(0)
                    .EnumerateArray()
                    .ElementAt(13)
                    .GetString();

                if (value == null)
                    throw new Exception("Failed to fetch visitor data");

                _visitorData = value;
                return _visitorData;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to fetch data from YouTube", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to parse JSON response", ex);
            }
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        ~VisitorDataTokenGenerator()
        {
            Dispose();
        }
    }
}
