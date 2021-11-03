using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal readonly struct UnscrambledQuery
    {
        public UnscrambledQuery(string uri, bool encrypted)
        {
            this.Uri = uri;
            this.IsEncrypted = encrypted;
        }

        public string Uri { get; }
        public bool IsEncrypted { get; }
    }
}
