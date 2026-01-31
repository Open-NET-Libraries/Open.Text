using ZLinq;

namespace Open.Text;

public static partial class TextExtensions
{
	/// <summary>
	/// Enumerates a string by segments that are separated by the split character.
	/// </summary>
	/// <remarks>
	/// Note: Each segment allocates a new string via ToString(). For true zero-allocation,
	/// use <see cref="SplitAsSegmentsNoAlloc(string, char, StringSplitOptions)"/> which returns StringSegments.
	/// </remarks>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitCharacter">The character to find.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <returns>A ValueEnumerable of string segments (allocates per segment).</returns>
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSplitEnumerator, StringSegment, string>, string> SplitToEnumerableNoAlloc(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return source.AsSegment()
			.SplitAsSegmentsNoAlloc(splitCharacter, options)
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
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSequenceSplitEnumerator, StringSegment, string>, string> SplitToEnumerableNoAlloc(
		this string source,
		StringSegment splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return source.AsSegment()
			.SplitAsSegmentsNoAlloc(splitSequence, options, comparisonType)
			.Select(s => s.ToString());
	}

	/// <returns>A zero-allocation enumerable of ReadOnlyMemory&lt;char&gt; segments.</returns>
	/// <inheritdoc cref="SplitToEnumerableNoAlloc(string, char, StringSplitOptions)" />
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSplitEnumerator, StringSegment, ReadOnlyMemory<char>>, ReadOnlyMemory<char>> SplitAsMemoryNoAlloc(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return SplitAsSegmentsNoAlloc(source.AsSegment(), splitCharacter, options)
			.Select(s => s.AsMemory());
	}

	/// <returns>A zero-allocation enumerable of ReadOnlyMemory&lt;char&gt; segments.</returns>
	/// <inheritdoc cref="SplitToEnumerable(string, StringSegment, StringSplitOptions, StringComparison)"/>
	[CLSCompliant(false)]
	public static ValueEnumerable<ZLinq.Linq.Select<StringSegmentSequenceSplitEnumerator, StringSegment, ReadOnlyMemory<char>>, ReadOnlyMemory<char>> SplitAsMemoryNoAlloc(
		this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return SplitAsSegmentsNoAlloc(source.AsSegment(), splitSequence, options, comparisonType)
			.Select(s => s.AsMemory());
	}
}
