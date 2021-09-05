using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Open.Text
{
	public static partial class Extensions
	{

		/// <summary>
		/// Trims a matching string from the start of a sequence of characters.
		/// </summary>
		/// <param name="source">The source sequence of characters.</param>
		/// <param name="pattern">The pattern to search for.</param>
		/// <param name="comparisonType">The comparison type to use when searching.  Default is ordinal.</param>
		/// <param name="max">The maximum number of times to remove the specified sequence from the start.  -1 (default) = all instances.</param>
		/// <returns>The resultant trimmed string.</returns>
		public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> source,
			string pattern,
			StringComparison comparisonType = StringComparison.Ordinal,
			int max = -1)
		{
			if (pattern is null) throw new ArgumentNullException(nameof(pattern));
			Contract.EndContractBlock();

			if (max == 0 || source.IsEmpty || pattern.Length == 0 || pattern.Length > source.Length) return source;
			var pLen = pattern.Length;
			var pSpan = pattern.AsSpan();

			while (0 != max-- && source.IndexOf(pSpan, comparisonType) == 0)
			{
				source = source.Slice(pLen);
				if (pattern.Length > source.Length) break;
			}

			return source;
		}

		/// <summary>
		/// Trims a matching string from the start of a sequence of characters.
		/// </summary>
		/// <param name="source">The source sequence of characters.</param>
		/// <param name="pattern">The pattern to search for.</param>
		/// <param name="comparisonType">The comparison type to use when searching.  Default is ordinal.</param>
		/// <param name="max">The maximum number of times to remove the specified sequence from the start.  -1 (default) = all instances.</param>
		/// <returns>The resultant trimmed string.</returns>
		public static string TrimStart(this string source,
			string pattern,
			StringComparison comparisonType = StringComparison.Ordinal,
			int max = -1)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (pattern is null) throw new ArgumentNullException(nameof(pattern));
			Contract.EndContractBlock();

			if (max == 0 || source.Length == 0 || pattern.Length == 0 || pattern.Length > source.Length) return source;
			if (max != 1) return TrimStart(source.AsSpan(), pattern, comparisonType, max).ToString();

			return source.IndexOf(pattern, comparisonType) == 0
				? source.Substring(pattern.Length)
				: source;
		}

		/// <summary>
		/// Trims a matching string from the end of a sequence of characters.
		/// </summary>
		/// <param name="source">The source sequence of characters.</param>
		/// <param name="pattern">The pattern to search for.</param>
		/// <param name="comparisonType">The comparison type to use when searching.  Default is ordinal.</param>
		/// <param name="max">The maximum number of times to remove the specified sequence from the end.  -1 (default) = all instances.</param>
		/// <returns>The resultant trimmed string.</returns>
		public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> source,
			string pattern,
			StringComparison comparisonType = StringComparison.Ordinal,
			int max = -1)
		{
			if (pattern is null) throw new ArgumentNullException(nameof(pattern));
			Contract.EndContractBlock();

			if (max == 0 || source.IsEmpty || pattern.Length == 0 || pattern.Length > source.Length) return source;
			var pLen = pattern.Length;
			var pSpan = pattern.AsSpan();

			var expectedIndex = source.Length - pattern.Length;
			while (0 != max-- && source.LastIndexOf(pSpan, comparisonType) == expectedIndex)
			{
				source = source.Slice(pLen);
				expectedIndex = source.Length - pattern.Length;
				if (expectedIndex < 0) break;
			}

			return source;
		}

		/// <summary>
		/// Trims a matching string from the end of a sequence of characters.
		/// </summary>
		/// <param name="source">The source sequence of characters.</param>
		/// <param name="pattern">The pattern to search for.</param>
		/// <param name="comparisonType">The comparison type to use when searching.  Default is ordinal.</param>
		/// <param name="max">The maximum number of times to remove the specified sequence from the end.  -1 (default) = all instances.</param>
		/// <returns>The resultant trimmed string.</returns>
		public static string TrimEnd(this string source,
			string pattern,
			StringComparison comparisonType = StringComparison.Ordinal,
			int max = -1)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (pattern is null) throw new ArgumentNullException(nameof(pattern));
			Contract.EndContractBlock();

			if (max == 0 || source.Length == 0 || pattern.Length == 0 || pattern.Length > source.Length) return source;
			if (max != 1) return TrimEnd(source.AsSpan(), pattern, comparisonType, max).ToString();

			var expectedIndex = source.Length - pattern.Length;
			var result = source.LastIndexOf(pattern, comparisonType);
			return result == expectedIndex
				? source.Substring(0, expectedIndex)
				: source;
		}
	}
}
