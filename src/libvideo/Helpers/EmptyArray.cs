using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    internal static class EmptyArray<T>
    {
        public static readonly T[] Value = new T[0];
    }
}
