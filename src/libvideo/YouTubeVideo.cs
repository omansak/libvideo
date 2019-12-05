using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo : Video
    {
        private readonly string jsPlayer;

        private string uri;
        private bool encrypted;
        //Your Service
        // https://dl.dropboxusercontent.com/s/ccmwnfmcmmwdspf/dfunctionregex.txt is not reliable please use own file
        /// <summary>
        /// <para>Url to a remote txt file. The file contains the current Regex-string for decrypting some videos.</para> 
        /// Used as fallback in case of a breaking update of Youtubes javascript. This allows an update of the Regex even after deployment of the application.
        /// </summary>
        public static string DFunctionRegexService = "https://dl.dropboxusercontent.com/s/ccmwnfmcmmwdspf/dfunctionregex.txt"; //For Dynamic Service
        internal YouTubeVideo(string title,
            UnscrambledQuery query, string jsPlayer)
        {
            this.Title = title;
            this.uri = query.Uri;
            this.jsPlayer = jsPlayer;
            if (DFunctionRegex_Dynamic == null) //For Dynamic
            {
                DFunctionRegex_Dynamic = new Regex(Task.Run(GetDecryptRegex).Result);
            }
            this.encrypted = query.IsEncrypted;
            this.FormatCode = int.Parse(new Query(uri)["itag"]);
        }
        private async Task<string> GetDecryptRegex() // For Dynamic
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var r = await httpClient.GetAsync(DFunctionRegexService);
                return await r.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public override string Title { get; }
        public override WebSites WebSite => WebSites.YouTube;

        public override string Uri =>
            GetUriAsync().GetAwaiter().GetResult();

        public string GetUri(Func<DelegatingClient> makeClient) =>
            GetUriAsync(makeClient).GetAwaiter().GetResult();

        public override Task<string> GetUriAsync() =>
            GetUriAsync(() => new DelegatingClient());

        public async Task<string> GetUriAsync(Func<DelegatingClient> makeClient)
        {
            if (encrypted)
            {
                uri = await
                    DecryptAsync(uri, makeClient)
                    .ConfigureAwait(false);
                encrypted = false;
            }

            return uri;
        }

        public int FormatCode { get; }
        public bool IsEncrypted => encrypted;
    }
}
