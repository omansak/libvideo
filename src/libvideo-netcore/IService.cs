using System.Collections.Generic;

namespace VideoLibraryNetCore
{
    internal interface IService<T> where T : Video
    {
        T GetVideo(string uri);
        IEnumerable<T> GetAllVideos(string uri);
    }
}