using System.Text;

namespace Open.Text;

public static partial class TextExtensions
{
	private const string MustBeSegmentWithValue = "Must be a StringSegment that has a value (is not null).";

	/// <summary>
	/// Creates a StringSegment representing the provided string.
	/// </summary>
	/// <param name="buffer">The string the segment belongs to.</param>
	public static StringSegment AsSegment(this string? buffer)
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
	public static StringSegment AsSegment(this string buffer, int offset, int length)
	{
		if (buffer is null) throw new ArgumentNullException(nameof(buffer));
		//Debug.Assert(offset >= 0);
		//Debug.Assert(length >= 0);
		//Debug.Assert(buffer.Length >= offset + length);
		return new(buffer, offset, length);
	}

	/// <summary>Shortcut for .AsSegment().Trim().</summary>
	/// <inheritdoc cref="AsSegment(string)"/>
	[ExcludeFromCodeCoverage] // Reason: would just test already tested code.
	public static StringSegment TrimAsSegment(this string buffer)
		=> buffer is null ? default : new StringSegment(buffer).Trim();

	/// <summary>
	/// Creates a <see cref="StringSegmentEnumerable"/> for enumerating over the characters in the <paramref name="segment"/>.
	/// </summary>
	public static StringSegmentEnumerable AsEnumerable(this StringSegment segment)
		=> new(segment);

	/// <inheritdoc cref="SplitToEnumerable(string, char, StringSplitOptions)"/>
	public static IEnumerable<StringSegment> SplitAsSegments(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return SplitAsSegments(source.AsSegment(), splitCharacter, options);
	}

	/// <inheritdoc cref="SplitToEnumerable(string, char, StringSplitOptions)"/>
	public static IEnumerable<StringSegment> SplitAsSegments(
		this StringSegment source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
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
			StringSegment source,
			char splitCharacter)
		{
			var startIndex = 0;
			var len = source.Length;

		loop:
			var nextIndex = source.IndexOf(splitCharacter, startIndex);
			if (nextIndex == -1)
			{
				yield return source.Subsegment(startIndex);
				yield break;
			}
			else if (nextIndex == len)
			{
				yield return source.Subsegment(nextIndex, 0);
				yield break;
			}
			else
			{
				yield return source.Subsegment(startIndex, nextIndex - startIndex);
				++nextIndex;
			}

			startIndex = nextIndex;
			goto loop;
		}

		static IEnumerable<StringSegment> SplitAsSegmentsCoreOmitEmpty(
			StringSegment source,
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
					yield return source.Subsegment(startIndex);
					yield break;
				}
				else
				{
					var length = nextIndex - startIndex;
					if (length != 0) yield return source.Subsegment(startIndex, length);
					++nextIndex;
				}

				startIndex = nextIndex;
			}
			while (startIndex != len);
		}
	}

	/// <summary>
	/// Enumerates a string by segments that are separated by the regular expression matches.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="pattern">The pattern to split by.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <returns>An enumerable of the segments.</returns>
	public static IEnumerable<StringSegment> SplitAsSegments(
		this string source,
		Regex pattern,
		StringSplitOptions options = StringSplitOptions.None)
	{
		return source is null
			? throw new ArgumentNullException(nameof(source))
			: pattern is null
			? throw new ArgumentNullException(nameof(pattern))
			: source.Length == 0
			? options == StringSplitOptions.RemoveEmptyEntries
				? Enumerable.Empty<StringSegment>()
				: Enumerable.Repeat(StringSegment.Empty, 1)
			: SplitCore(source, pattern, options);

		static IEnumerable<StringSegment> SplitCore(string source, Regex pattern, StringSplitOptions options)
		{
			int len;
			var nextStart = 0;
			var match = pattern.Match(source);
			while (match.Success)
			{
				len = match.Index - nextStart;
				if (len != 0 || options == StringSplitOptions.None)
					yield return new(source, nextStart, match.Index - nextStart);
				nextStart = match.Index + match.Length;
				match = match.NextMatch();
			}
			len = source.Length - nextStart;
			if (len != 0 || options == StringSplitOptions.None)
				yield return source.AsSegment(nextStart, len);
		}
	}

	/// <returns>An IEnumerable&lt;StringSegment&gt; of the segments.</returns>
	/// <inheritdoc cref="SplitToEnumerable(string, string, StringSplitOptions, StringComparison)"/>
	public static IEnumerable<StringSegment> SplitAsSegments(
		this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
		Contract.EndContractBlock();

		return SplitAsSegments(source.AsSegment(), splitSequence.AsSegment(), options, comparisonType);
	}

	/// <inheritdoc cref="SplitAsSegments(string, string, StringSplitOptions, StringComparison)"/>
	public static IEnumerable<StringSegment> SplitAsSegments(
		this StringSegment source,
		StringSegment splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
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
			StringSegment source,
			StringSegment splitSequence,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			var startIndex = 0;
			var len = source.Length;
			var s = splitSequence.Length;

		loop:
			var nextIndex = source.IndexOf(splitSequence, startIndex, comparisonType);
			if (nextIndex == -1)
			{
				yield return source.Subsegment(startIndex);
				yield break;
			}
			else if (nextIndex == len)
			{
				yield return source.Subsegment(nextIndex, 0);
				yield break;
			}
			else
			{
				yield return source.Subsegment(startIndex, nextIndex - startIndex);
				nextIndex += s;
			}

			startIndex = nextIndex;
			goto loop;
		}

		static IEnumerable<StringSegment> SplitAsSegmentsCoreOmitEmpty(
			StringSegment source,
			StringSegment splitSequence,
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
					yield return source.Subsegment(startIndex);
					yield break;
				}
				else
				{
					var length = nextIndex - startIndex;
					if (length != 0) yield return source.Subsegment(startIndex, length);
					nextIndex += s;
				}

				startIndex = nextIndex;
			}
			while (startIndex != len);
		}
	}

	/// <summary>
	/// Joins a sequence of segments with a separator sequence.
	/// </summary>
	/// <param name="source">The segments to join.</param>
	/// <param name="between">The segment to place between each segment.</param>
	/// <returns>An IEnumerable&lt;StringSegment&gt; of the segments.</returns>
	/// <exception cref="ArgumentNullException">The source is null.</exception>
	public static IEnumerable<StringSegment> Join(this IEnumerable<StringSegment> source, StringSegment between)
	{
		return source is null
			? throw new ArgumentNullException(nameof(source))
			: JoinCore(source, between);

		static IEnumerable<StringSegment> JoinCore(IEnumerable<StringSegment> source, StringSegment between)
		{
			using var e = source.GetEnumerator();
			var ok = e.MoveNext();
			Debug.Assert(ok);
			var c = e.Current;
			if (c.Length != 0) yield return c;
			while (e.MoveNext())
			{
				if (between.HasValue) yield return between;
				c = e.Current;
				if (c.Length != 0) yield return c;
			}
		}
	}

	/// <summary>
	/// Joins a sequence of segments with an optional separator sequence.
	/// </summary>
	/// <returns>A StringBuilder of the segments.</returns>
	/// <inheritdoc cref="Join(IEnumerable{StringSegment}, StringSegment)"/>
	public static StringBuilder JoinToStringBuilder(this IEnumerable<StringSegment> source, StringSegment between = default)
	{
		var sb = new StringBuilder();
		foreach (var segment in Join(source, between))
			sb.Append(segment.AsSpan());
		return sb;
	}

	/// <returns>A joined string of the segments.</returns>
	/// <inheritdoc cref="JoinToStringBuilder(IEnumerable{StringSegment}, StringSegment)"/>
	public static string JoinToString(this IEnumerable<StringSegment> source, StringSegment between = default)
		=> JoinToStringBuilder(source, between).ToString();

	/// <summary>
	/// Splits a sequence and replaces the removed sequences with the replacement sequence.
	/// </summary>
	/// <inheritdoc cref="SplitAsSegments(string, string, StringSplitOptions, StringComparison)"/>
	public static IEnumerable<StringSegment> Replace(
		this StringSegment source,
		StringSegment splitSequence,
		StringSegment replacement,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> Join(SplitAsSegments(source, splitSequence, comparisonType: comparisonType), replacement);

	/// <returns>The resultant string.</returns>
	/// <inheritdoc cref="Replace(StringSegment, StringSegment, StringSegment, StringComparison)"/>
	public static string ReplaceToString(this StringSegment source,
		StringSegment splitSequence,
		StringSegment replacement,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> JoinToString(SplitAsSegments(source, splitSequence, comparisonType: comparisonType), replacement);

	/// <inheritdoc cref="Replace(StringSegment, StringSegment, StringSegment, StringComparison)"/>
	public static IEnumerable<StringSegment> ReplaceAsSegments(
		this string source,
		Regex splitSequence,
		StringSegment replacement)
		=> Join(SplitAsSegments(source, splitSequence), replacement);

	/// <inheritdoc cref="Replace(StringSegment, StringSegment, StringSegment, StringComparison)"/>
	public static IEnumerable<StringSegment> ReplaceAsSegments(
		this string source,
		StringSegment splitSequence,
		StringSegment replacement,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> Replace(source, splitSequence, replacement, comparisonType);

	/// <summary>
	/// Splits each sequence and replaces the removed sequences with the replacement sequence.
	/// </summary>
	/// <inheritdoc cref="SplitAsSegments(string, string, StringSplitOptions, StringComparison)"/>
	public static IEnumerable<StringSegment> ReplaceEach(
		this IEnumerable<StringSegment> source,
		StringSegment splitSequence,
		StringSegment replacement,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		return source is null
			? throw new ArgumentNullException(nameof(source))
			: ReplaceEachCore(source, splitSequence, replacement, comparisonType);

		static IEnumerable<StringSegment> ReplaceEachCore(IEnumerable<StringSegment> source, StringSegment splitSequence, StringSegment replacement, StringComparison comparisonType)
		{
			// Instead of source.SelectMany(s => s.Replace(splitSequence, replacement, comparisonType));
			// we manually yield to reduce allocations.
			foreach (var s in source)
			{
				foreach (var e in Replace(s, splitSequence, replacement, comparisonType))
					yield return e;
			}
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
	public static StringSegment Preceding(this StringSegment source, int maxCharacters, bool includeSegment = false)
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
			: new(segment.Buffer, newIndex, sLen - offset);
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
			: new(segment.Buffer, sOff, newLength);
	}

	/// <summary>
	/// Creates a StringSegment that starts offset by the value provided and extends by the length provided.
	/// </summary>
	/// <param name="segment">The segment to work from.</param>
	/// <param name="offset">The value (positive or negative) to move the index by.</param>
	/// <param name="length">The length desired.</param>
	/// <param name="ignoreLengthBoundary">
	///	If true, the length can exceed the segment length but not past the full length of the source string.
	///	If false (default), an ArgumentOutOfRangeException will be thrown if the expected length exceeds the segment.
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
			: new(segment.Buffer!, segment.Offset + trimmed, length - trimmed);
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
			: new(segment.Buffer!, segment.Offset, length - trimmed);
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
			: new(segment.Buffer!, segment.Offset + trimmed, length - trimmed);
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
			: new(segment.Buffer!, segment.Offset, length - trimmed);
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
		if (trimmedEnd == length) return new(segment.Buffer!, segment.Offset, 0);
		var trimmedStart = TrimStartCore(segment, span, trim);
		return trimmedEnd == 0 && trimmedStart == 0 ? segment
			: new(segment.Buffer!, segment.Offset + trimmedStart, length - trimmedEnd - trimmedStart);
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
		if (trimmedEnd == length) return new(segment.Buffer!, segment.Offset, 0);
		var trimmedStart = TrimStartCore(segment, span, trim);
		return trimmedEnd == 0 && trimmedStart == 0 ? segment
			: new(segment.Buffer!, segment.Offset + trimmedStart, length - trimmedEnd - trimmedStart);
	}
}
