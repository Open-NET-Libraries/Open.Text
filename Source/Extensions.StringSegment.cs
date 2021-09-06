using System;
using System.Diagnostics.Contracts;

namespace Open.Text
{
	public static partial class Extensions
	{
		/// <summary>
		/// Returns a StringSegment representing the source string.
		/// </summary>
		public static StringSegment AsSegment(this string source)
			=> StringSegment.Create(source);

		/// <inheritdoc cref="AsSegment(string)"/>
		/// <param name="start">The index to start the segment.</param>
		public static StringSegment AsSegment(this string source, int start)
			=> StringSegment.Create(source, start);

		/// <inheritdoc cref="AsSegment(string, int)"/>
		/// <param name="length">The length of the segment.</param>
		public static StringSegment AsSegment(this string source, int start, int length)
			=> StringSegment.Create(source, start, length);

		/// <summary>
		/// Finds the first instance of a string and returns a StringSegment for subsequent use.
		/// </summary>
		/// <param name="source">The source string to search.</param>
		/// <param name="search">The string pattern to look for.</param>
		/// <param name="comparisonType">The string comparision type to use.  Default is Ordinal.</param>
		/// <returns>
		/// The segment representing the found string.
		/// If not found, the StringSegment.IsValid property will be false.
		/// </returns>
		public static StringSegment First(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (search is null) throw new ArgumentNullException(nameof(search));
			Contract.EndContractBlock();

			if (search.Length == 0)
				return default;

			var i = source.IndexOf(search, comparisonType);
			return i == -1 ? default : StringSegment.Create(source, i, search.Length);
		}

		/// <inheritdoc cref="First(string, string, StringComparison)" />
		public static StringSegment First(this StringSegment source, in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (!source.IsValid) throw new ArgumentException("Must be a valid segment.", nameof(source));
			Contract.EndContractBlock();

			if (search.IsEmpty)
				return default;

			var i = source.AsSpan().IndexOf(search, comparisonType);
			return i == -1 ? default : StringSegment.Create(source.Source, source.Index + i, search.Length);
		}

		/// <inheritdoc cref="First(string, string, StringComparison)" />
		public static StringSegment First(this StringSegment source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (search is null) throw new ArgumentNullException(nameof(search));
			Contract.EndContractBlock();

			return First(source, search.AsSpan(), comparisonType);
		}

		/// <inheritdoc cref="First(string, string, StringComparison)"/>
		/// <summary>
		/// Finds the last instance of a string and returns a StringSegment for subsequent use.
		/// </summary>
		public static StringSegment Last(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (search is null) throw new ArgumentNullException(nameof(search));
			Contract.EndContractBlock();

			if (search.Length == 0)
				return default;

			var i = source.LastIndexOf(search, comparisonType);
			return i == -1 ? default : StringSegment.Create(source, i, search.Length);
		}

		/// <inheritdoc cref="Last(string, string, StringComparison)" />
		public static StringSegment Last(this StringSegment source, in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (!source.IsValid) throw new ArgumentException("Must be a valid segment.", nameof(source));
			Contract.EndContractBlock();
			if (search.IsEmpty)
				return default;

			var i = source.AsSpan().LastIndexOf(search, comparisonType);
			return i == -1 ? default : StringSegment.Create(source.Source, source.Index + i, search.Length);
		}

		/// <inheritdoc cref="Last(string, string, StringComparison)" />
		public static StringSegment Last(this StringSegment source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (search is null) throw new ArgumentNullException(nameof(search));
			Contract.EndContractBlock();

			return Last(source, search.AsSpan(), comparisonType);
		}


	}
}
