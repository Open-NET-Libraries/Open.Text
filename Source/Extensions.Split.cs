using ZLinq;

namespace Open.Text;

internal static class SingleEmpty
{
	public static readonly IReadOnlyList<string> Instance
		= Array.AsReadOnly([string.Empty]);
}

public static partial class TextExtensions
{
	#region First Split
	private static ReadOnlySpan<char> FirstSplitSpan(ReadOnlySpan<char> source, int start, int i, int n, out int nextIndex)
	{
		Debug.Assert(start >= 0);
		if (i == -1)
		{
			nextIndex = -1;
			return start == 0 ? source : source.Slice(start);
		}

		nextIndex = i + n;
		int segmentLen = i - start;
		return segmentLen == 0 ? [] : source.Slice(start, segmentLen);
	}

	private static ReadOnlySpan<char> FirstSplitSpan(ReadOnlySpan<char> rest, int i, int n, out int nextIndex)
	{
		if (i == -1)
		{
			nextIndex = -1;
			return rest;
		}

		nextIndex = i + n;
		return i == 0
			? []
			: rest.Slice(0, i);
	}

	/// <summary>
	/// Finds the first instance of a character and returns the set of characters up to that character.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitCharacter">The character to find.</param>
	/// <param name="nextIndex">The next possible index following the current one.</param>
	/// <param name="startIndex">The index to start the split.</param>
	/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		char splitCharacter,
		out int nextIndex,
		int startIndex = 0)
	{
		if (startIndex == 0)
			return FirstSplitSpan(source, source.IndexOf(splitCharacter), 1, out nextIndex);

		int len = source.Length;
		if (startIndex > len) throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Is greater than the length of the provided string.");
		Contract.EndContractBlock();

		if (startIndex == len)
		{
			nextIndex = -1;
			return [];
		}

		return FirstSplitSpan(source, startIndex, source.IndexOf(splitCharacter, startIndex, StringComparison.Ordinal), 1, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, char, out int, int)"/>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		char splitCharacter,
		out int nextIndex,
		int startIndex = 0) => source.HasValue
			? FirstSplit(source.AsSpan(), splitCharacter, out nextIndex, startIndex)
			: throw new ArgumentException("Cannot split a null string or sequence that has no value.", nameof(source));

	/// <inheritdoc cref="FirstSplit(StringSegment, char, out int, int)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		char splitCharacter,
		out int nextIndex,
		int startIndex = 0) =>
		// AsSegment will throw if source is null
		FirstSplit(source.AsSegment(), splitCharacter, out nextIndex, startIndex);

	/// <summary>
	/// Finds the first instance of a character sequence and returns the set of characters up to that sequence.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitSequence">The sequence to find.</param>
	/// <param name="nextIndex">The next possible index following the current one.</param>
	/// <param name="startIndex">The index to start the split.</param>
	/// <param name="comparisonType">The string comparison type to use.</param>
	/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> splitSequence,
		out int nextIndex,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (splitSequence.Length == 0)
			throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
		if (startIndex == 0)
			return FirstSplitSpan(source, source.IndexOf(splitSequence, comparisonType), splitSequence.Length, out nextIndex);

		int len = source.Length;
		if (startIndex > source.Length)
			throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Is greater than the length of the provided string.");
		Contract.EndContractBlock();

		if (startIndex == len)
		{
			nextIndex = -1;
			return [];
		}

		return FirstSplitSpan(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), splitSequence.Length, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		ReadOnlySpan<char> splitSequence,
		out int nextIndex,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.HasValue
			? FirstSplit(source.AsSpan(), splitSequence, out nextIndex, startIndex, comparisonType)
			: throw new ArgumentException("Cannot split a null string or sequence that has no value.", nameof(source));

	/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		ReadOnlySpan<char> splitSequence,
		out int nextIndex,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.AsSegment().FirstSplit(splitSequence, out nextIndex, startIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (splitSequence.Length == 0)
			throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
		Contract.EndContractBlock();

		if (source.Length == 0)
		{
			nextIndex = -1;
			return [];
		}

		int i = source.IndexOf(splitSequence, comparisonType);
		return FirstSplitSpan(source, i, splitSequence.Length, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		ReadOnlySpan<char> splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.HasValue
			? FirstSplit(source.AsSpan(), splitSequence, out nextIndex, comparisonType)
			: throw new ArgumentException("Cannot split a null string or sequence that has no value.", nameof(source));

	/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		ReadOnlySpan<char> splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.AsSegment().FirstSplit(splitSequence, out nextIndex, comparisonType);

#if NETSTANDARD2_0
	// NET Standard 2.0 String does not implicitly convert to ReadOnlySpan<char>.
	// Therefore we need these extra overloads.

	/// <inheritdoc cref="FirstSplit(StringSegment, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		string splitSequence,
		out int nextIndex,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.AsSegment().FirstSplit(splitSequence.AsSpan(), out nextIndex, startIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(StringSegment, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		string splitSequence,
		out int nextIndex,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.FirstSplit(splitSequence.AsSpan(), out nextIndex, startIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(StringSegment, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		string splitSequence,
		out int nextIndex,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.FirstSplit(splitSequence.AsSpan(), out nextIndex, startIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(StringSegment, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		string splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.AsSegment().FirstSplit(splitSequence.AsSpan(), out nextIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(StringSegment, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		string splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.FirstSplit(splitSequence.AsSpan(), out nextIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(StringSegment, ReadOnlySpan{char}, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		string splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal) => source.FirstSplit(splitSequence.AsSpan(), out nextIndex, comparisonType);

#endif
	#endregion

	/// <summary>
	/// Enumerates a string by segments that are separated by the split character.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitCharacter">The character to find.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <returns>A zero-allocation enumerable of the string segments.</returns>
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSplitEnumerator, StringSegment, string>, string> SplitToEnumerable(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return source.AsSegment()
			.SplitAsSegments(splitCharacter, options)
			.Select(s => s.ToString());
	}

	/// <summary>
	/// Enumerates a string by segments that are separated by the split sequence.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitSequence">The sequence to find.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <param name="comparisonType">The string comparison type to use.</param>
	/// <returns>A zero-allocation enumerable of the string segments.</returns>
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSequenceSplitEnumerator, StringSegment, string>, string> SplitToEnumerable(
		this string source,
		StringSegment splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return source.AsSegment()
			.SplitAsSegments(splitSequence, options, comparisonType)
			.Select(s => s.ToString());
	}

	/// <returns>A zero-allocation enumerable of ReadOnlyMemory&lt;char&gt; segments.</returns>
	/// <inheritdoc cref="SplitToEnumerable(string, char, StringSplitOptions)" />
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSplitEnumerator, StringSegment, ReadOnlyMemory<char>>, ReadOnlyMemory<char>> SplitAsMemory(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return SplitAsSegments(source.AsSegment(), splitCharacter, options)
			.Select(s => s.AsMemory());
	}

	/// <returns>A zero-allocation enumerable of ReadOnlyMemory&lt;char&gt; segments.</returns>
	/// <inheritdoc cref="SplitToEnumerable(string, StringSegment, StringSplitOptions, StringComparison)"/>
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSequenceSplitEnumerator, StringSegment, ReadOnlyMemory<char>>, ReadOnlyMemory<char>> SplitAsMemory(
		this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return SplitAsSegments(source.AsSegment(), splitSequence, options, comparisonType)
			.Select(s => s.AsMemory());
	}

	/// <summary>
	/// Splits a sequence of characters into strings using the character provided.
	/// </summary>
	/// <param name="source">The source string to split up.</param>
	/// <param name="splitCharacter">The character to split by.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <returns>The resultant list of string segments.</returns>
	public static IReadOnlyList<string> Split(
		this ReadOnlySpan<char> source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source.IsEmpty)
		{
			return options.HasFlag(StringSplitOptions.RemoveEmptyEntries)
				? Array.Empty<string>()
				: SingleEmpty.Instance;
		}

		var list = new List<string>();

#if NET5_0_OR_GREATER
		bool trimEach = options.HasFlag(StringSplitOptions.TrimEntries);
#endif
		if (options.HasFlag(StringSplitOptions.RemoveEmptyEntries))
		{
		loop:
			ReadOnlySpan<char> result = source.FirstSplit(splitCharacter, out int nextIndex);
#if NET5_0_OR_GREATER
			if (trimEach) result = result.Trim();
#endif
			if (!result.IsEmpty) list.Add(result.ToString());
			if (nextIndex == -1) return list;
			source = source.Slice(nextIndex);
			goto loop;
		}

		{
		loop:
			ReadOnlySpan<char> result = source.FirstSplit(splitCharacter, out int nextIndex);
#if NET5_0_OR_GREATER
			if (trimEach) result = result.Trim();
#endif
			list.Add(result.IsEmpty ? string.Empty : result.ToString());
			if (nextIndex == -1) return list;
			source = source.Slice(nextIndex);
			goto loop;
		}
	}

	/// <summary>
	/// Splits a sequence of characters into strings using the character sequence provided.
	/// </summary>
	/// <param name="source">The source string to split up.</param>
	/// <param name="splitSequence">The sequence to split by.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <param name="comparisonType">The optional comparison type.</param>
	/// <returns>The resultant list of string segments.</returns>
	public static IReadOnlyList<string> Split(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source.IsEmpty)
		{
			return options.HasFlag(StringSplitOptions.RemoveEmptyEntries)
				? Array.Empty<string>()
				: SingleEmpty.Instance;
		}

		var list = new List<string>();
#if NET5_0_OR_GREATER
		bool trimEach = options.HasFlag(StringSplitOptions.TrimEntries);
#endif
		if (options.HasFlag(StringSplitOptions.RemoveEmptyEntries))
		{
		loop:
			ReadOnlySpan<char> result = source.FirstSplit(splitSequence, out int nextIndex, comparisonType);
#if NET5_0_OR_GREATER
			if (trimEach) result = result.Trim();
#endif
			if (!result.IsEmpty) list.Add(result.ToString());
			if (nextIndex == -1) return list;
			source = source.Slice(nextIndex);
			goto loop;
		}

		{
		loop:
			ReadOnlySpan<char> result = source.FirstSplit(splitSequence, out int nextIndex, comparisonType);
#if NET5_0_OR_GREATER
			if (trimEach) result = result.Trim();
#endif
			list.Add(result.IsEmpty ? string.Empty : result.ToString());
			if (nextIndex == -1) return list;
			source = source.Slice(nextIndex);
			goto loop;
		}
	}

	/// <inheritdoc cref="Split(ReadOnlySpan{char}, ReadOnlySpan{char}, StringSplitOptions, StringComparison)"/>
	public static IReadOnlyList<string> Split(this ReadOnlySpan<char> source,
		StringSegment splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal) => source.Split(splitSequence.AsSpan(), options, comparisonType);
}
