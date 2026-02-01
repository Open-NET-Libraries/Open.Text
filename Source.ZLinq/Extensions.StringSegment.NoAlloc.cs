using System.Text;

namespace Open.Text;

/// <summary>
/// Zero-allocation text extension methods using ZLinq ValueEnumerable patterns.
/// </summary>
public static class NoAllocTextExtensions
{
	private const string MustBeSegmentWithValue = "Must be a StringSegment that has a value (is not null).";

	/// <summary>
	/// Splits a string by character with zero allocations for direct foreach iteration.
	/// Returns a ZLinq ValueEnumerable that avoids heap allocations and supports LINQ operations.
	/// </summary>
	/// <param name="source">The string to split.</param>
	/// <param name="splitCharacter">The character to split by.</param>
	/// <param name="options">String split options.</param>
	/// <returns>A zero-allocation ValueEnumerable of StringSegment.</returns>
	public static ValueEnumerable<StringSegmentSplitEnumerator, StringSegment> SplitAsSegmentsNoAlloc(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
		=> SplitAsSegmentsNoAlloc(source.AsSegment(), splitCharacter, options);

	/// <inheritdoc cref="SplitAsSegmentsNoAlloc(string, char, StringSplitOptions)"/>
	public static ValueEnumerable<StringSegmentSplitEnumerator, StringSegment> SplitAsSegmentsNoAlloc(
		this StringSegment source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
		=> source.HasValue
			? new(new StringSegmentSplitEnumerator(source, splitCharacter, options))
			: throw new ArgumentNullException(nameof(source), MustBeSegmentWithValue);

	/// <summary>
	/// Enumerates a string by segments that are separated by the regular expression matches.
	/// </summary>
	/// <remarks>
	/// Note: Regex matching internally allocates Match objects per match. The enumerator itself
	/// is stack-allocated, but regex operations have unavoidable allocations.
	/// </remarks>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="pattern">The pattern to split by.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <returns>A ValueEnumerable of the segments.</returns>
	public static ValueEnumerable<RegexSplitSegmentEnumerator, StringSegment> SplitAsSegmentsNoAlloc(
		this string source,
		Regex pattern,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		Contract.EndContractBlock();

		return new(new RegexSplitSegmentEnumerator(source, pattern, options));
	}

	/// <summary>
	/// Enumerates a string by segments that are separated by the specified sequence (zero-allocation).
	/// </summary>
	/// <returns>A ValueEnumerable of the segments (zero-allocation when used with foreach or ZLinq).</returns>
	public static ValueEnumerable<StringSegmentSequenceSplitEnumerator, StringSegment> SplitAsSegmentsNoAlloc(
		this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal) => SplitAsSegmentsNoAlloc(source.AsSegment(), splitSequence.AsSegment(), options, comparisonType);

	/// <inheritdoc cref="SplitAsSegmentsNoAlloc(string, string, StringSplitOptions, StringComparison)"/>
	public static ValueEnumerable<StringSegmentSequenceSplitEnumerator, StringSegment> SplitAsSegmentsNoAlloc(
		this StringSegment source,
		StringSegment splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue)
			throw new ArgumentNullException(nameof(source), MustBeSegmentWithValue);
		if (!splitSequence.HasValue)
			throw new ArgumentNullException(nameof(splitSequence), "Cannot split using a null sequence.");
		if (splitSequence.Length == 0)
			throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
		Contract.EndContractBlock();

		return new ValueEnumerable<StringSegmentSequenceSplitEnumerator, StringSegment>(
			new StringSegmentSequenceSplitEnumerator(source, splitSequence, options, comparisonType));
	}

	/// <summary>
	/// Joins a sequence of segments with a separator sequence.
	/// </summary>
	/// <remarks>
	/// Note: This overload accepts IEnumerable which may cause boxing if the source is a struct enumerator.
	/// For true zero-allocation joins, use the ValueEnumerable overloads returned by split operations.
	/// </remarks>
	/// <param name="source">The segments to join.</param>
	/// <param name="between">The segment to place between each segment.</param>
	/// <returns>A ValueEnumerable of the joined segments.</returns>
	/// <exception cref="System.ArgumentNullException">The source is null.</exception>
	public static ValueEnumerable<StringSegmentJoinEnumerator, StringSegment> JoinNoAlloc(
		this IEnumerable<StringSegment> source, StringSegment between)
		=> source is null
			? throw new ArgumentNullException(nameof(source))
			: new(new StringSegmentJoinEnumerator(source, between));

	/// <summary>
	/// Joins a regex split sequence of segments with a separator sequence (zero-allocation).
	/// </summary>
	/// <param name="source">The regex split segments to join.</param>
	/// <param name="between">The segment to place between each segment.</param>
	/// <returns>A ValueEnumerable of the joined segments (zero-allocation when used with foreach or ZLinq).</returns>
	public static ValueEnumerable<RegexSplitJoinEnumerator, StringSegment> JoinNoAlloc(
		this ValueEnumerable<RegexSplitSegmentEnumerator, StringSegment> source,
		StringSegment between)
		=> new(new RegexSplitJoinEnumerator(source.Enumerator, between));

	/// <summary>
	/// Joins a sequence split result with a separator sequence (zero-allocation).
	/// </summary>
	/// <param name="source">The sequence split segments to join.</param>
	/// <param name="between">The segment to place between each segment.</param>
	/// <returns>A ValueEnumerable of the joined segments (zero-allocation when used with foreach or ZLinq).</returns>
	public static ValueEnumerable<SequenceSplitJoinEnumerator, StringSegment> JoinNoAlloc(
		this ValueEnumerable<StringSegmentSequenceSplitEnumerator, StringSegment> source,
		StringSegment between)
		=> new(new SequenceSplitJoinEnumerator(source.Enumerator, between));

	/// <summary>
	/// Splits a sequence and replaces the removed sequences with the replacement sequence (zero-allocation).
	/// </summary>
	/// <returns>A ValueEnumerable of the segments (zero-allocation when used with foreach or ZLinq).</returns>
	public static ValueEnumerable<SequenceSplitJoinEnumerator, StringSegment> ReplaceNoAlloc(
		this StringSegment source,
		StringSegment splitSequence,
		StringSegment replacement,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> JoinNoAlloc(SplitAsSegmentsNoAlloc(source, splitSequence, comparisonType: comparisonType), replacement);

	/// <summary>
	/// Replaces all occurrences of a pattern with a replacement sequence (zero-allocation).
	/// </summary>
	/// <returns>A ValueEnumerable of the segments (zero-allocation when used with foreach or ZLinq).</returns>
	public static ValueEnumerable<RegexSplitJoinEnumerator, StringSegment> ReplaceAsSegmentsNoAlloc(
		this string source,
		Regex splitSequence,
		StringSegment replacement)
		=> JoinNoAlloc(SplitAsSegmentsNoAlloc(source, splitSequence), replacement);

	/// <summary>
	/// Replaces all occurrences of a sequence with a replacement sequence (zero-allocation).
	/// </summary>
	/// <returns>A ValueEnumerable of the segments (zero-allocation when used with foreach or ZLinq).</returns>
	public static ValueEnumerable<SequenceSplitJoinEnumerator, StringSegment> ReplaceAsSegmentsNoAlloc(
		this string source,
		StringSegment splitSequence,
		StringSegment replacement,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> ReplaceNoAlloc(source, splitSequence, replacement, comparisonType);

	/// <summary>
	/// Joins a ValueEnumerable of StringSegments into a single string with an optional separator.
	/// </summary>
	/// <typeparam name="TEnumerator">The type of the value enumerator.</typeparam>
	/// <param name="source">The source ValueEnumerable to join.</param>
	/// <param name="separator">Optional separator to place between segments.</param>
	/// <returns>The joined string.</returns>
	public static string JoinToString<TEnumerator>(
		this ValueEnumerable<TEnumerator, StringSegment> source,
		StringSegment separator = default)
		where TEnumerator : struct, IValueEnumerator<StringSegment>
	{
		using var enumerator = source.GetEnumerator();

		if (!enumerator.MoveNext())
			return string.Empty;

		var current = enumerator.Current;
		if (!enumerator.MoveNext())
			return current.ToString();

		var sb = new StringBuilder();
		sb.Append(current.AsSpan());
		AppendRemaining(separator, enumerator, sb);

		return sb.ToString();
	}

	/// <summary>
	/// Appends the string representations of the elements in the sequence to the specified StringBuilder,
	/// using the specified optional separator between each element.
	/// </summary>
	/// <remarks>If the sequence is empty, the returned StringBuilder is unchanged or empty. The method does not add
	/// a separator after the last element.</remarks>
	/// <typeparam name="TEnumerator">The type of the value enumerator used to iterate the sequence of StringSegment elements.</typeparam>
	/// <param name="source">The sequence of StringSegment elements to join and append.</param>
	/// <param name="stringBuilder">The StringBuilder to append the joined elements to. If null, a new StringBuilder is created.</param>
	/// <param name="separator">The separator to use between each element. If not specified, no separator is used.</param>
	/// <returns>A StringBuilder containing the concatenated string representations of the sequence elements, separated by the
	/// specified separator. Returns the provided StringBuilder if not null; otherwise, returns a new StringBuilder if the source has elements.</returns>
#if !NETSTANDARD2_0
	[return: NotNullIfNotNull(nameof(stringBuilder))]
#endif
	public static StringBuilder? JoinToStringBuilder<TEnumerator>(
		this ValueEnumerable<TEnumerator, StringSegment> source,
		StringBuilder? stringBuilder,
		StringSegment separator = default)
		where TEnumerator : struct, IValueEnumerator<StringSegment>
	{
		using var enumerator = source.GetEnumerator();

		if (!enumerator.MoveNext())
			return stringBuilder;

		var current = enumerator.Current;
		var sb = stringBuilder ?? new StringBuilder();
		sb.Append(current.AsSpan());

		if (enumerator.MoveNext())
		{
			AppendRemaining(separator, enumerator, sb);
		}

		return sb;
	}

	/// <summary>
	/// Concatenates the elements of the sequence, using the specified separator,
	/// and appends the result to a new StringBuilder instance.
	/// </summary>
	/// <typeparam name="TEnumerator">The type of the value-type enumerator used to iterate the sequence.</typeparam>
	/// <param name="source">The sequence of StringSegment values to concatenate.</param>
	/// <param name="separator">The separator to use between each element. If not specified, no separator is used.</param>
	/// <returns>A StringBuilder containing the concatenated elements of the sequence, or null if the sequence is empty.</returns>
	public static StringBuilder? JoinToStringBuilder<TEnumerator>(
		this ValueEnumerable<TEnumerator, StringSegment> source,
		StringSegment separator = default)
		where TEnumerator : struct, IValueEnumerator<StringSegment>
		=> source.JoinToStringBuilder(null, separator);

	private static void AppendRemaining<TEnumerator>(
		StringSegment separator,
		ValueEnumerator<TEnumerator, StringSegment> enumerator,
		StringBuilder sb)
		where TEnumerator : struct, IValueEnumerator<StringSegment>
	{
		if (separator.Length == 0)
		{
			do
			{
				sb.Append(enumerator.Current.AsSpan());
			}
			while (enumerator.MoveNext());
		}
		else
		{
			var sep = separator.AsSpan();
			do
			{
				sb.Append(sep);
				sb.Append(enumerator.Current.AsSpan());
			} while (enumerator.MoveNext());
		}
	}
}
