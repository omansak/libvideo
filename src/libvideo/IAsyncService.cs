using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    internal interface IAsyncService<T> where T : Video
    {
        Task<T> GetVideoAsync(string uri);
        Task<IEnumerable<T>> GetAllVideosAsync(string uri);
    }
}
