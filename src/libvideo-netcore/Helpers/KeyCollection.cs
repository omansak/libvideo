using System;
using System.Collections;
using System.Collections.Generic;

namespace VideoLibraryNetCore.Helpers
{
    internal partial class Query : IDictionary<string, string>
    {
        public class KeyCollection : ICollection<string>, IReadOnlyCollection<string>
        {
            private readonly Query query;

            public KeyCollection(Query query)
            {
                this.query = query;
            }

            public int Count => query.Count;

            public bool IsReadOnly => true;

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                for (int i = 0; i < query.Count; i++)
                {
                    var pair = query.Pairs[i];
                    if (item == pair.Key)
                        return true;
                }
                return false;
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < query.Count; i++)
                    array[arrayIndex++] = query.Pairs[i].Key;
            }

            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < query.Count; i++)
                    yield return query.Pairs[i].Key;
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}