using System;
using System.Collections.Generic;

namespace piqey.Utilities.Extensions
{
	public static class IListExtensions
	{
		/// <summary>
		/// Selects a random element from an <see cref="IList{T}"/> and optionally removes it.
		/// </summary>
		/// <typeparam name="T">The type of elements in the <see cref="IList{T}"/>.</typeparam>
		/// <param name="src">The source <see cref="IList{T}"/> from which to select an element.</param>
		/// <param name="remove">Whether to remove the selected element from the <see cref="IList{T}"/>. Defaults to <c>false</c>.</param>
		/// <returns>A randomly selected element from the <see cref="IList{T}"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the source <see cref="IList{T}"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the source <see cref="IList{T}"/> is empty.</exception>
		public static T Random<T>(this IList<T> src, bool remove = false)
		{
			if (src == null) throw new ArgumentNullException(nameof(src));
			if (src.Count == 0) throw new InvalidOperationException("Cannot perform this operation on an empty list.");

			int i = UnityEngine.Random.Range(0, src.Count);
			T item = src[i];
			if (remove) src.RemoveAt(i);

			return item;
		}
	}
}
