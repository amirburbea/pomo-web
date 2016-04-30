using System;
using System.Collections.Generic;

namespace PoMo.Common
{
    public static class BinarySearchMethods
    {
        public static int BinarySearchByValue<T, TValue>(this IReadOnlyList<T> collection, TValue searchValue, Func<T, TValue> projection, IComparer<TValue> comparer = null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }
            if (comparer == null)
            {
                comparer = Comparer<TValue>.Default;
            }
            int startIndex = 0;
            int endIndex = collection.Count - 1;
            while (endIndex >= startIndex)
            {
                int midPoint = startIndex + (endIndex - startIndex) / 2;
                TValue itemValue = projection(collection[midPoint]);
                switch (comparer.Compare(itemValue, searchValue))
                {
                    case 0:
                        return midPoint;
                    case -1:
                        startIndex = midPoint + 1;
                        break;
                    case 1:
                        endIndex = midPoint - 1;
                        break;
                }
            }
            return ~startIndex;
        }

        public static int BinarySearch<T, TValue>(this IReadOnlyList<T> collection, T searchItem, Func<T, TValue> projection, IComparer<TValue> comparer = null)
        {
            return collection.BinarySearchByValue(projection(searchItem), projection, comparer);
        }
    }
}