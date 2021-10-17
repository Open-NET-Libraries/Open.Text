using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Open.Text;

public static partial class TextExtensions
{
	private const string MustBeSegmentWithValue = "Must be a StringSegment that has a value (is not null).";

	/// <inheritdoc cref="AsSegment(string, int, int)"/>
	public static StringSegment AsSegment(this string buffer)
		=> buffer is null ? default : new(buffer);

	/// <inheritdoc cref="AsSegment(string, int, int)"/>
	public static StringSegment AsSegment(this string buffer, int start)
		=> AsSegment(buffer, start, (buffer?.Length ?? 0) - start);

	/// <summary>
	/// Creates a StringSegment representing the provided string.
	/// </summary>
	/// <param name="buffer">The string the segment belongs to.</param>
	/// <param name="offset">The starting point of the string to use as the index of the segment.</param>
	/// <param name="length">The length of the segment.</param>
	/// <exception cref="ArgumentNullException">If the source is null.</exception>
	public static StringSegment AsSegment(this string buffer, int offset, int length)
	{
		if (buffer is null) throw new ArgumentNullException(nameof(buffer));
		//Debug.Assert(offset >= 0);
		//Debug.Assert(length >= 0);
		//Debug.Assert(buffer.Length >= offset + length);
		return new(buffer, offset, length);
	}

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
		return i == -1 ? default : new(source, i, search.Length);
	}

	/// <summary>
	/// Finds the first instance of a pattern and returns a StringSegment for subsequent use.
	/// </summary>
	/// <param name="source">The source string to search.</param>
	/// <param name="pattern">The pattern to look for.</param>
	/// <returns>
	/// The segment representing the found string.
	/// If not found, the StringSegment.IsValid property will be false.
	/// </returns>
	/// <remarks>If the pattern is right-to-left, then it will return the first segment from the right.</remarks>
	public static StringSegment First(this string source, Regex pattern)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		Contract.EndContractBlock();

		var match = pattern.Match(source);
		return match.Success ? new(source, match.Index, match.Length) : default;
	}

	/// <inheritdoc cref="First(string, string, StringComparison)" />
	public static StringSegment First(this StringSegment source, ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();

		if (search.IsEmpty)
			return default;

		var i = source.AsSpan().IndexOf(search, comparisonType);
		return i == -1 ? default : new(source.Buffer, source.Offset + i, search.Length);
	}

	/// <inheritdoc cref="First(string, string, StringComparison)" />
	public static StringSegment First(this StringSegment source, string search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (search is null) throw new ArgumentNullException(nameof(search));
		Contract.EndContractBlock();

		return First(source, search.AsSpan(), comparisonType);
	}

	/// <summary>
	/// Finds the last instance of a string and returns a StringSegment for subsequent use.
	/// </summary>
	/// <inheritdoc cref="First(string, string, StringComparison)"/>
	public static StringSegment Last(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (search is null) throw new ArgumentNullException(nameof(search));
		Contract.EndContractBlock();

		if (search.Length == 0)
			return default;

		var i = source.LastIndexOf(search, comparisonType);
		return i == -1 ? default : new(source, i, search.Length);
	}

	/// <summary>
	/// Finds the last instance of a pattern and returns a StringSegment for subsequent use.
	/// </summary>
	/// <remarks>If the pattern is right-to-left, then it will return the last segment from the right (first segment from the left).</remarks>
	/// <inheritdoc cref="First(string, Regex)"/>
	public static StringSegment Last(this string source, Regex pattern)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		Contract.EndContractBlock();

		var matches = pattern.Matches(source);
		if (matches.Count == 0) return default;
		var match = matches[matches.Count - 1];
		return new(source, match.Index, match.Length);
	}

	/// <inheritdoc cref="Last(string, string, StringComparison)" />
	public static StringSegment Last(this StringSegment source, ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();
		if (search.IsEmpty)
			return default;

		var i = source.AsSpan().LastIndexOf(search, comparisonType);
		return i == -1 ? default : new(source.Buffer, source.Offset + i, search.Length);
	}

	/// <inheritdoc cref="Last(string, string, StringComparison)" />
	public static StringSegment Last(this StringSegment source, string search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (search is null) throw new ArgumentNullException(nameof(search));
		Contract.EndContractBlock();

		return Last(source, search.AsSpan(), comparisonType);
	}

	/// <inheritdoc cref="SplitToEnumerable(string, char, StringSplitOptions)"/>
	public static IEnumerable<StringSegment> SplitAsSegments(this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return options switch
		{
			StringSplitOptions.None => source.Length == 0
				? Enumerable.Repeat(StringSegment.Empty, 1)
				: SplitAsSegmentsCore(source, splitCharacter),
			StringSplitOptions.RemoveEmptyEntries => source.Length == 0
				? Enumerable.Empty<StringSegment>()
				: SplitAsSegmentsCoreOmitEmpty(source, splitCharacter),
			_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
		};

		static IEnumerable<StringSegment> SplitAsSegmentsCore(
			string source,
			char splitCharacter)
		{
			var startIndex = 0;
			var len = source.Length;

		loop:
			var nextIndex = source.IndexOf(splitCharacter, startIndex);
			if (nextIndex == -1)
			{
				yield return source.AsSegment(startIndex);
				yield break;
			}
			else if (nextIndex == len)
			{
				yield return new(source, nextIndex, 0);
				yield break;
			}
			else
			{
				yield return new(source, startIndex, nextIndex - startIndex);
				++nextIndex;
			}
			startIndex = nextIndex;
			goto loop;
		}

		static IEnumerable<StringSegment> SplitAsSegmentsCoreOmitEmpty(
			string source,
			char splitCharacter)
		{
			var startIndex = 0;
			var len = source.Length;

			do
			{
				var nextIndex = source.IndexOf(splitCharacter, startIndex);
				if (nextIndex == len)
					yield break;

				if (nextIndex == -1)
				{
					yield return source.AsSegment(startIndex);
					yield break;
				}
				else
				{
					var length = nextIndex - startIndex;
					if (length != 0) yield return new (source, startIndex, length);
					++nextIndex;
				}
				startIndex = nextIndex;
			}
			while (startIndex != len);
		}

	}


	/// <inheritdoc cref="SplitToEnumerable(string, string, StringSplitOptions, StringComparison)"/>
	public static IEnumerable<StringSegment> SplitAsSegments(this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
		if (splitSequence.Length == 0)
			throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
		Contract.EndContractBlock();

		return options switch
		{
			StringSplitOptions.None => source.Length == 0
				? Enumerable.Repeat(StringSegment.Empty, 1)
				: SplitAsSegmentsCore(source, splitSequence, comparisonType),
			StringSplitOptions.RemoveEmptyEntries => source.Length == 0
				? Enumerable.Empty<StringSegment>()
				: SplitAsSegmentsCoreOmitEmpty(source, splitSequence, comparisonType),
			_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
		};

		static IEnumerable<StringSegment> SplitAsSegmentsCore(
			string source,
			string splitSequence,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			var startIndex = 0;
			var len = source.Length;
			var s = splitSequence.Length;

		loop:
			var nextIndex = source.IndexOf(splitSequence, startIndex, comparisonType);
			if (nextIndex == -1)
			{
				yield return source.AsSegment(startIndex);
				yield break;
			}
			else if (nextIndex == len)
			{
				yield return new(source, nextIndex, 0);
				yield break;
			}
			else
			{
				yield return new(source, startIndex, nextIndex - startIndex);
				nextIndex += s;
			}
			startIndex = nextIndex;
			goto loop;
		}

		static IEnumerable<StringSegment> SplitAsSegmentsCoreOmitEmpty(
			string source,
			string splitSequence,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			var startIndex = 0;
			var len = source.Length;
			var s = splitSequence.Length;

			do
			{
				var nextIndex = source.IndexOf(splitSequence, startIndex, comparisonType);
				if (nextIndex == len)
					yield break;

				if (nextIndex == -1)
				{
					yield return source.AsSegment(startIndex);
					yield break;
				}
				else
				{
					var length = nextIndex - startIndex;
					if (length != 0) yield return new(source, startIndex, length);
					nextIndex += s;
				}
				startIndex = nextIndex;
			}
			while (startIndex != len);
		}

	}


	/// <inheritdoc cref="Preceding(StringSegment, int, bool)"/>
	public static StringSegment Preceding(this StringSegment source, bool includeSegment = false)
		=> source.HasValue
		? new(source.Buffer, 0, includeSegment ? source.Offset + source.Length : source.Offset)
		: default;

	/// <inheritdoc cref="Following(StringSegment, int, bool)"/>
	public static StringSegment Following(this StringSegment source, bool includeSegment = false)
		=> source.HasValue
		? source.Buffer.AsSegment(includeSegment ? source.Offset : source.Offset + source.Length)
		: default;

	/// <summary>
	/// Gets the string segment that precedes this one.
	/// </summary>
	/// <param name="source">The segment to work from.</param>
	/// <param name="maxCharacters">The max number of characters to get.</param>
	/// <param name="includeSegment">When true, will include this segment.</param>
	public static StringSegment Preceding(this StringSegment source,int maxCharacters, bool includeSegment = false)
	{
		if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
		if (!source.HasValue) return source;
		if (maxCharacters == 0) return includeSegment ? source : new(source.Buffer, source.Offset, 0);
		var start = Math.Max(0, source.Offset - maxCharacters);
		return new(source.Buffer, start, includeSegment ? (source.Offset + source.Length - start) : (source.Offset - start));
	}

	/// <summary>
	/// Gets the string segment that follows this one.
	/// </summary>
	/// <param name="source">The segment to work from.</param>
	/// <param name="maxCharacters">The max number of characters to get.</param>
	/// <param name="includeSegment">When true, will include this segment.</param>
	public static StringSegment Following(this StringSegment source, int maxCharacters, bool includeSegment = false)
	{
		if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
		if (!source.HasValue) return source;
		var start = includeSegment ? source.Offset : source.Offset + source.Length;
		if (maxCharacters == 0) return includeSegment ? source : new(source.Buffer, start, 0);
		var len = Math.Min(includeSegment ? (maxCharacters + source.Length) : maxCharacters, source.Buffer.Length - start);
		return new(source.Buffer, start, len);
	}


	/// <summary>
	/// Creates a StringSegment with the offset adjusted by the value provided.
	/// </summary>
	/// <param name="segment">The segment to work from.</param>
	/// <param name="offset">The value (positive or negative) to move the index by and adjust the length.</param>
	public static StringSegment OffsetStart(this StringSegment segment, int offset)
	{
		if (!segment.HasValue) return segment;
		var newIndex = segment.Offset + offset;
		var sLen = segment.Length;
		return newIndex < 0
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index less than the start of the string.")
			: offset > sLen
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index greater than the length of the segment.")
			: (new(segment.Buffer, newIndex, sLen - offset));
	}

	/// <summary>
	/// Creates a StringSegment with the length adjusted by the value provided.
	/// </summary>
	/// <param name="segment">The segment to work from.</param>
	/// <param name="offset">The value (positive or negative) to adjust the length.</param>
	public static StringSegment OffsetEnd(this StringSegment segment, int offset)
	{
		if (!segment.HasValue) return segment;
		var sLen = segment.Length;
		var sOff = segment.Offset;
		var newLength = sLen + offset;
		return newLength < 0
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot shrink less than the start of the string.")
			: sOff + newLength > segment.Buffer.Length
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot expand greater than the length of the source.")
			: (new(segment.Buffer, sOff, newLength));
	}


	/// <summary>
	/// Creates a StringSegment that starts offset by the value provided and extends by the length provided.
	/// </summary>
	/// <param name="segment">The segment to work from.</param>
	/// <param name="offset">The value (positive or negative) to move the index by.</param>
	/// <param name="length">The length desired.</param>
	/// <param name="ignoreLengthBoundary">
	///	If true, the length can exceed the segment length but not past the full length of the source string.
	///	If false (default), an ArgumentOutOfRangeException will be thrown if the expected length exeeds the segment.
	///	</param>
	public static StringSegment Slice(this StringSegment segment, int offset, int length, bool ignoreLengthBoundary = false)
	{
		if (!segment.HasValue)
		{
			return offset == 0 && length == 0 ? segment
				: throw new InvalidOperationException("Cannot slice a segment with no value.");
		}
		var newIndex = segment.Offset + offset;
		if (newIndex < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index less than the start of the string.");
		var newEnd = newIndex + length;
		if (ignoreLengthBoundary)
		{
			var end = segment.Buffer.Length;
			if (newEnd > end)
			{
				if (newIndex > end) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Index is greater than the end of the source string.");
				throw new ArgumentOutOfRangeException(nameof(length), length, "Desired length will extend greater than the end of the source string.");
			}
			return new(segment.Buffer, newIndex, length);
		}
		else
		{
			var end = segment.Offset + segment.Length;
			if (newEnd > end)
			{
				if (newIndex > end) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Index is greater than the length of the segment.");
				throw new ArgumentOutOfRangeException(nameof(length), length, "Desired length will extend greater than the length of the segment.");
			}
			return new(segment.Buffer, newIndex, length);
		}

	}

	private static int TrimStartCore(StringSegment segment, ReadOnlySpan<char> span, char trim)
	{
		int o = segment.Offset, i = o;
		int end = i + segment.Length;

		while (end != i)
		{
			if (span[i] != trim) break;
			++i;
		}

		return i - o;
	}

	private static int TrimStartCore(StringSegment segment, ReadOnlySpan<char> span, ReadOnlySpan<char> trim)
	{
		int o = segment.Offset, i = o;
		int end = i + segment.Length;

		while (end != i)
		{
			if (trim.IndexOf(span[i]) == -1) break;
			++i;
		}

		return i - o;
	}

	private static int TrimEndCore(StringSegment segment, ReadOnlySpan<char> span, char trim)
	{
		int o = segment.Offset;
		int end = o + segment.Length, e = end;

		while (e != o)
		{
			if (span[e - 1] != trim) break;
			--e;
		}

		return end - e;
	}

	private static int TrimEndCore(StringSegment segment, ReadOnlySpan<char> span, ReadOnlySpan<char> trim)
	{
		int o = segment.Offset;
		int end = o + segment.Length, e = end;

		while (e != o)
		{
			if (trim.IndexOf(span[e - 1]) == -1) break;
			--e;
		}

		return end - e;
	}


	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the beginning.
	/// </summary>
	/// <param name="segment">The segment to work from.</param>
	/// <param name="trim">The character to skip over.</param>
	public static StringSegment TrimStart(this StringSegment segment, char trim)
	{
		var length = segment.Length;
		if (segment.Length == 0) return segment;

		var trimmed = TrimStartCore(segment, segment.Buffer.AsSpan(), trim);
		return trimmed == 0 ? segment
			: new(segment.Buffer, segment.Offset + trimmed, length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(StringSegment, char)"/>
	public static StringSegment TrimEnd(this StringSegment segment, char trim)
	{
		var length = segment.Length;
		if (segment.Length == 0) return segment;

		var trimmed = TrimEndCore(segment, segment.Buffer.AsSpan(), trim);
		return trimmed == 0 ? segment
			: new(segment.Buffer, segment.Offset, length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have any of the trim characters at the beginning.
	/// </summary>
	/// <param name="segment">The segment to work from.</param>
	/// <param name="trim">The characters to skip over.</param>
	public static StringSegment TrimStart(this StringSegment segment, ReadOnlySpan<char> trim)
	{
		var length = segment.Length;
		if (segment.Length == 0) return segment;

		var trimmed = TrimStartCore(segment, segment.Buffer.AsSpan(), trim);
		return trimmed == 0 ? segment
			: new(segment.Buffer, segment.Offset + trimmed, length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have any of the trim characters at the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(StringSegment, char)"/>
	public static StringSegment TrimEnd(this StringSegment segment, ReadOnlySpan<char> trim)
	{
		var length = segment.Length;
		if (segment.Length == 0) return segment;

		var trimmed = TrimEndCore(segment, segment.Buffer.AsSpan(), trim);
		return trimmed == 0 ? segment
			: new(segment.Buffer, segment.Offset, length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the beginning nor the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(StringSegment, char)"/>
	public static StringSegment Trim(this StringSegment segment, char trim)
	{
		var length = segment.Length;
		if (segment.Length == 0) return segment;

		var span = segment.Buffer.AsSpan();
		var trimmedEnd = TrimEndCore(segment, span, trim);
		if (trimmedEnd == length) return new(segment.Buffer, segment.Offset, 0);
		var trimmedStart = TrimStartCore(segment, span, trim);
		return trimmedEnd == 0 && trimmedStart == 0 ? segment
			: new(segment.Buffer, segment.Offset + trimmedStart, length - trimmedEnd - trimmedStart);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the beginning nor the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(StringSegment, ReadOnlySpan{char})"/>
	public static StringSegment Trim(this StringSegment segment, ReadOnlySpan<char> trim)
	{
		var length = segment.Length;
		if (segment.Length == 0) return segment;

		var span = segment.Buffer.AsSpan();
		var trimmedEnd = TrimEndCore(segment, span, trim);
		if (trimmedEnd == length) return new(segment.Buffer, segment.Offset, 0);
		var trimmedStart = TrimStartCore(segment, span, trim);
		return trimmedEnd == 0 && trimmedStart == 0 ? segment
			: new(segment.Buffer, segment.Offset + trimmedStart, length - trimmedEnd - trimmedStart);
	}


	/// <summary>
	/// Determines whether this StringSegment and a specified ReadOnlySpan have the same characters.
	/// </summary>
	/// <inheritdoc cref="string.Equals(string, StringComparison)"/>
	public static bool Equals(this StringSegment source, ReadOnlySpan<char> other, StringComparison stringComparison = StringComparison.Ordinal)
		=> other.Length == source.Length && source.AsSpan().Equals(other, stringComparison);

}
