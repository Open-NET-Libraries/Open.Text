using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Open.Text;

public static partial class Extensions
{
	private static ReadOnlySpan<char> TrimStartPatternCore(ReadOnlySpan<char> source, ReadOnlySpan<char> pattern, StringComparison comparisonType, int max)
	{
		var pLen = pattern.Length;

		if (max == -1)
		{
			while (source.IndexOf(pattern, comparisonType) == 0)
			{
				source = source.Slice(pLen);
				if (pattern.Length > source.Length) break;
			}
		}
		else
		{
			while (0 != max-- && source.IndexOf(pattern, comparisonType) == 0)
			{
				source = source.Slice(pLen);
				if (pattern.Length > source.Length) break;
			}
		}

		return source;
	}

	/// <param name="source">The source sequence of characters.</param>
	/// <param name="pattern">The pattern to search for.</param>
	/// <param name="comparisonType">The comparison type to use when searching.  Default is ordinal.</param>
	/// <param name="max">The maximum number of times to remove the specified sequence.  -1 (default) = all instances.</param>
	/// <inheritdoc cref="TrimStartPattern(string, Regex, int)"/>
	public static ReadOnlySpan<char> TrimStartPattern(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> pattern,
		StringComparison comparisonType = StringComparison.Ordinal,
		int max = -1)
	{
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		return max == 0 || source.IsEmpty || pattern.IsEmpty || pattern.Length > source.Length
			? source
			: TrimStartPatternCore(source, pattern, comparisonType, max);
	}

	/// <inheritdoc cref="TrimStartPattern(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison, int)"/>
	public static ReadOnlySpan<char> TrimStartPattern(this ReadOnlySpan<char> source,
		string pattern,
		StringComparison comparisonType = StringComparison.Ordinal,
		int max = -1)
	{
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		return max == 0 || source.IsEmpty || pattern.Length == 0 || pattern.Length > source.Length
			? source
			: TrimStartPatternCore(source, pattern.AsSpan(), comparisonType, max);
	}

	/// <remarks>To any allocations, call .AsSpan() before calling this method name.</remarks>
	/// <inheritdoc cref="TrimStartPattern(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison, int)"/>
	public static string TrimStartPattern(this string source,
		string pattern,
		StringComparison comparisonType = StringComparison.Ordinal,
		int max = -1)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		return max == 0 || source.Length == 0 || pattern.Length == 0 || pattern.Length > source.Length
			? source
			: max != 1
            ? TrimStartPatternCore(source.AsSpan(), pattern.AsSpan(), comparisonType, max).ToString()
            : source.IndexOf(pattern, comparisonType) == 0
            ? source.Substring(pattern.Length)
            : source;
	}

	/// <summary>
	/// Trims (omits) a matching pattern from the start of a sequence of characters.
	/// </summary>
	/// <param name="source">The source sequence of characters.</param>
	/// <param name="pattern">The pattern to search for.</param>
	/// <param name="max">The maximum number of times to remove the specified sequence.  -1 (default) = all instances.</param>
	/// <returns>The resultant trimmed string.</returns>
	public static string TrimStartPattern(this string source,
		Regex pattern,
		int max = -1)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		if (max == 0 || source.Length == 0)
			return source;

		var i = pattern.RightToLeft ? RightToLeft() : LeftToRight();
		return i == 0 ? source : source.Substring(i);

		int RightToLeft()
		{
			var i = 0;
			var matches = pattern.Matches(source);
			if (max == -1)
			{
				for (var m = matches.Count - 1; m >= 0; m--)
				{
					var match = matches[m];
					if (!match.Success || match.Index != i)
						break;
					i += match.Length;
				}
			}
			else
			{
				for (var m = matches.Count - 1; max != 0 && m >= 0; m--)
				{
					var match = matches[m];
					if (!match.Success || match.Index != i)
						break;
					i += match.Length;
					--max;
				}
			}
			return i;
		}

		int LeftToRight()
		{
			var i = 0;
			var match = pattern.Match(source);
			if (max == -1)
			{
				while (match.Success && match.Index == i)
				{
					i += match.Length;
					match = match.NextMatch();
				}
			}
			else
			{
				while (0 != max && match.Success && match.Index == i)
				{
					i += match.Length;
					match = match.NextMatch();
					--max;
				}
			}
			return i;
		}
	}

	private static ReadOnlySpan<char> TrimEndPatternCore(ReadOnlySpan<char> source, ReadOnlySpan<char> pattern, StringComparison comparisonType, int max)
	{
		var pLen = pattern.Length;
		var expectedIndex = source.Length - pLen;

		if (max == -1)
		{
			while (source.LastIndexOf(pattern, comparisonType) == expectedIndex)
			{
				source = source.Slice(0, expectedIndex);
				expectedIndex = source.Length - pattern.Length;
				if (expectedIndex < 0) break;
			}
		}
		else
		{
			while (0 != max-- && source.LastIndexOf(pattern, comparisonType) == expectedIndex)
			{
				source = source.Slice(0, expectedIndex);
				expectedIndex = source.Length - pattern.Length;
				if (expectedIndex < 0) break;
			}
		}

		return source;
	}

	/// <param name="source">The source sequence of characters.</param>
	/// <param name="comparisonType">The comparison type to use when searching.  Default is ordinal.</param>
	/// <param name="pattern">The pattern to search for.</param>
	/// <param name="max">The maximum number of times to remove the specified sequence.  -1 (default) = all instances.</param>
	/// <returns>The resultant trimmed span.</returns>
	/// <inheritdoc cref="TrimEndPattern(string, Regex, int)"/>
	public static ReadOnlySpan<char> TrimEndPattern(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> pattern,
		StringComparison comparisonType = StringComparison.Ordinal,
		int max = -1)
	{
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		return max == 0 || source.IsEmpty || pattern.Length == 0 || pattern.Length > source.Length
			? source
			: TrimEndPatternCore(source, pattern, comparisonType, max);
	}

	/// <inheritdoc cref="TrimEndPattern(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison, int)"/>
	public static ReadOnlySpan<char> TrimEndPattern(this ReadOnlySpan<char> source,
		string pattern,
		StringComparison comparisonType = StringComparison.Ordinal,
		int max = -1)
	{
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		return max == 0 || source.IsEmpty || pattern.Length == 0 || pattern.Length > source.Length
			? source
			: TrimEndPatternCore(source, pattern.AsSpan(), comparisonType, max);
	}

	/// <remarks>To any allocations, call .AsSpan() before calling this method name.</remarks>
	/// <inheritdoc cref="TrimEndPattern(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison, int)"/>
	public static string TrimEndPattern(this string source,
		string pattern,
		StringComparison comparisonType = StringComparison.Ordinal,
		int max = -1)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		if (max == 0 || source.Length == 0 || pattern.Length == 0 || pattern.Length > source.Length) return source;
		if (max != 1) return TrimEndPatternCore(source.AsSpan(), pattern.AsSpan(), comparisonType, max).ToString();

		var expectedIndex = source.Length - pattern.Length;
		var result = source.LastIndexOf(pattern, comparisonType);
		return result == expectedIndex
			? source.Substring(0, expectedIndex)
			: source;
	}

	/// <summary>
	/// Trims (omits) a matching pattern from the end of a sequence of characters.
	/// </summary>
	/// <param name="source">The source sequence of characters.</param>
	/// <param name="pattern">The pattern to search for.</param>
	/// <param name="max">The maximum number of times to remove the specified sequence.  -1 (default) = all instances.</param>
	/// <returns>The resultant trimmed string.</returns>
	public static string TrimEndPattern(this string source,
		Regex pattern,
		int max = -1)
	{

		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (max < -1) throw new ArgumentOutOfRangeException(nameof(max), max, "Must be -1 or greater.");
		Contract.EndContractBlock();

		int len;
		if (max == 0 || (len = source.Length) == 0)
			return source;

		var i = pattern.RightToLeft ? RightToLeft() : LeftToRight();
		return i == len ? source : source.Substring(0, i);

		int RightToLeft()
		{
			var i = len;
			var match = pattern.Match(source);
			if (max == -1)
			{
				int n;
				while (match.Success && match.Index == (n = i - match.Length))
				{
					i = n;
					match = match.NextMatch();
				}
			}
			else
			{
				int n;
				while (0 != max && match.Success && match.Index == (n = i - match.Length))
				{
					i = n;
					match = match.NextMatch();
					--max;
				}
			}
			return i;
		}

		int LeftToRight()
		{
			var i = len;
			var matches = pattern.Matches(source);
			if (max == -1)
			{
				int n;
				for (var m = matches.Count - 1; m >= 0; m--)
				{
					var match = matches[m];
					if (!match.Success || match.Index != (n = i - match.Length))
						break;
					i = n;
				}
			}
			else
			{
				int n;
				for (var m = matches.Count - 1; max != 0 && m >= 0; m--)
				{
					var match = matches[m];
					if (!match.Success || match.Index != (n = i - match.Length))
						break;
					i = n;
					--max;
				}
			}
			return i;
		}
	}
}
