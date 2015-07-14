using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public interface IService
    {
        byte[] Download(string videoUri);
        IEnumerable<byte[]> DownloadMany(string videoUri);

        string GetUri(string videoUri);
        IEnumerable<string> GetAllUris(string videoUri);

        Video GetVideo(string videoUri);
        IEnumerable<Video> GetAllVideos(string videoUri);
    }
}
