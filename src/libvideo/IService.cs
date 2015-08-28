using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    internal interface IService
    {
        Video GetVideo(string videoUri);
        IEnumerable<Video> GetAllVideos(string videoUri);
    }
}
