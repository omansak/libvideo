using System.Linq;
using System.Net;
using System.Net.Http;

namespace VideoLibrary.Debug
{
    class CustomHandler
    {
        public HttpMessageHandler GetHandler()
        {
            HttpMessageHandler handler = new HttpClientHandler();
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("CONSENT", "YES+cb", "/", "youtube.com"));
            return new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

        }
    }
    class CustomYouTube : YouTube
    {
        protected override HttpClient MakeClient(HttpMessageHandler handler)
        {
            return base.MakeClient(handler);
        }

        protected override HttpMessageHandler MakeHandler()
        {
            return new CustomHandler().GetHandler();
        }
    }
    class CustomYoutubeClient : VideoClient
    {
        protected override HttpClient MakeClient(HttpMessageHandler handler)
        {
            return base.MakeClient(handler);
        }

        protected override HttpMessageHandler MakeHandler()
        {
            return new CustomHandler().GetHandler();
        }
    }
    class Test
    {
        public void Run()
        {
            // Custom Youtube
            var youtube = new CustomYouTube();
            var foo1 = youtube.GetAllVideosAsync("https://www.youtube.com/watch?v=qK_NeRZOdq4").GetAwaiter().GetResult();
            var video = foo1.ToList().FirstOrDefault();
            var byte1 = video.GetBytesAsync().GetAwaiter().GetResult();
            // Custom youtube with custom client
            var client = new CustomYoutubeClient();
            var byte2 = client.GetBytesAsync(video).GetAwaiter().GetResult();
        }
    }
}
