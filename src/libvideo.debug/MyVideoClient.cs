using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Debug
{
    class MyVideoClient : VideoClient
    {
        protected override HttpClient MakeClient(HttpMessageHandler handler)
        {
            return base.MakeClient(handler);
        }

        protected override HttpMessageHandler MakeHandler()
        {
            return base.MakeHandler();
        }
    }
}
