using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Helpers
{
    public static class Require
    {
        public static void NotNull<T>(T obj, string name)
            where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }
    }
}
