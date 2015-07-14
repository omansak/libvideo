using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public interface IAsyncService
    {
        Task<byte[]> DownloadAsync(string videoUri);
        Task<IEnumerable<byte[]>> DownloadManyAsync(string videoUri);

        Task<string> GetUriAsync(string videoUri);
        Task<IEnumerable<string>> GetAllUrisAsync(string videoUri);

        Task<Video> GetVideoAsync(string videoUri);
        Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri);
    }
}
