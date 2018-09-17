using System.Collections.Generic;

namespace SnipInsight.Forms.Extensions
{
    internal static class ICollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> toAdd)
        {
            foreach (var item in toAdd)
            {
                list.Add(item);
            }
        }
    }
}
