using System.Collections.Generic;

namespace SnipInsight.Forms.Features.Library
{
    public class Grouping<K, T> : List<T>
    {
        public Grouping(K key)
        {
            this.Key = key;
        }

        public Grouping(K key, IEnumerable<T> items)
            : this(key)
        {
            this.AddRange(items);
        }

        public K Key { get; private set; }
    }
}