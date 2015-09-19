using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal struct Operations
    {
        public Operations(string reverse, 
            string swap, string splice)
        {
            this.Reverse = reverse;
            this.Swap = swap;
            this.Splice = splice;
        }

        public string Reverse { get; }
        public string Swap { get; }
        public string Splice { get; }
    }
}
