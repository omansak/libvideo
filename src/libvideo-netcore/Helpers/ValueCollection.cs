using System;
using System.Collections;
using System.Collections.Generic;

namespace VideoLibraryNetCore.Helpers
{
    internal partial class Query
    {
        public class ValueCollection : ICollection<string>, IReadOnlyCollection<string>
        {
            private readonly Query query;

            public ValueCollection(Query query)
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
                    if (item == pair.Value)
                        return true;
                }
                return false;
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < query.Count; i++)
                    array[arrayIndex++] = query.Pairs[i].Value;
            }

            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < query.Count; i++)
                    yield return query.Pairs[i].Value;
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}