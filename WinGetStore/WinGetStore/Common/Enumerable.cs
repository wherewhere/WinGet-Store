using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WinGetStore.Common
{
    public static class Enumerable
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="ICollection{TSource}"/> to be added.</param>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ICollection{TSource}"/>.
        /// The collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>, if type <typeparamref name="TSource"/> is a reference type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collection"/> is null.</exception>
        public static void AddRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (source is List<TSource> list)
            {
                list.AddRange(collection);
            }
            else if (source is TSource[] array)
            {
                int count = collection.Count();
                if (count > 0)
                {
                    int _size = Array.FindLastIndex(array, (x) => x != null) + 1;
                    if (array.Length - _size < count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(array));
                    }

                    if (collection is ICollection<TSource> c)
                    {
                        c.CopyTo(array, _size);
                    }
                    else
                    {
                        foreach (TSource item in collection)
                        {
                            array[_size++] = item;
                        }
                    }
                }
            }
            else if (source is ISet<TSource> set)
            {
                set.UnionWith(collection);
            }
            else
            {
                foreach (TSource item in collection)
                {
                    source.Add(item);
                }
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source"></param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="List{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (source is List<TSource> list)
            {
                list.ForEach(action);
            }
            else if (source is TSource[] array)
            {
                Array.ForEach(array, action);
            }
            else if (source is ImmutableList<TSource> immutableList)
            {
                immutableList.ForEach(action);
            }
            else
            {
                foreach (TSource item in source)
                {
                    action(item);
                }
            }
        }
    }
}
