using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

namespace Utilities
{
    public static class EnumerableExtensions
    {
        // TODO: this is really fancy but it's not the best solution for performance or my sanity
        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            
            var count = 0;
            T selectedElement = default(T);

            foreach (var element in enumerable)
            {
                count++;
                if (Random.Range(0, count) == 0)
                {
                    selectedElement = element;
                }
            }

            if (count == 0)
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            return selectedElement;
        }

        public static T RandomElement<T>(this IEnumerable<T> enumerable, System.Random random)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            
            var count = 0;
            T selectedElement = default(T);

            foreach (var element in enumerable)
            {
                count++;
                if (random.Next(0, count) == 0)
                {
                    selectedElement = element;
                }
            }

            if (count == 0)
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            return selectedElement;       
        }
        
        [CanBeNull]
        public static T RandomElementOrDefault<T>(this IEnumerable<T> enumerable) where T : class
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            // ReSharper disable once PossibleMultipleEnumeration
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;

            // Rewind using ToList and pass to the original method
            // ReSharper disable once PossibleMultipleEnumeration
            return enumerable.RandomElement();
        }

        [CanBeNull]
        public static T RandomElementOrDefault<T>(this IEnumerable<T> enumerable, System.Random random) where T : class
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            // ReSharper disable once PossibleMultipleEnumeration
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;

            // ReSharper disable once PossibleMultipleEnumeration
            return enumerable.RandomElement(random);
        }
        
        public static T? RandomElementOrDefaultStruct<T>(this IEnumerable<T> enumerable) where T : struct
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            // ReSharper disable once PossibleMultipleEnumeration
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;

            // ReSharper disable once PossibleMultipleEnumeration
            return enumerable.RandomElement();
        }
        
        public static T? RandomElementOrDefaultStruct<T>(this IEnumerable<T> enumerable, System.Random random) where T : struct
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            // ReSharper disable once PossibleMultipleEnumeration
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;

            // ReSharper disable once PossibleMultipleEnumeration
            return enumerable.RandomElement(random);
        }
    }
}