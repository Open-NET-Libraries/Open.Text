namespace Open.Text;
/// <summary>
/// Represents a search operation within a string segment.
/// </summary>
public readonly ref struct StringSegmentSearch
{
	/// <summary>
	/// The source segment where the search is to be performed.
	/// </summary>
	public StringSegment Source { get; }

	/// <summary>
	/// The character sequences to be searched within the source.
	/// </summary>
	public ReadOnlySpan<char> Sequence { get; }

	/// <summary>
	/// The string comparison option used for the search.
	/// </summary>
	public StringComparison Comparison { get; }

	/// <summary>
	/// Indicates whether the search is to be performed from right to left.
	/// </summary>
	public bool RightToLeft { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="StringSegmentSearch"/> struct.
	/// </summary>
	/// <param name="source">
	/// <inheritdoc cref="Source" path="/summary"/>
	/// </param>
	/// <param name="search">
	/// <inheritdoc cref="Sequence" path="/summary"/>
	/// </param>
	/// <param name="comparisonType">
	/// <inheritdoc cref="Comparison" path="/summary"/>
	/// </param>
	/// <param name="rightToLeft">
	/// <inheritdoc cref="RightToLeft" path="/summary"/>
	/// </param>
	internal StringSegmentSearch(
	StringSegment source,
		ReadOnlySpan<char> search,
		StringComparison comparisonType,
		bool rightToLeft)
	{
		Source = source;
		Sequence = search;
		Comparison = comparisonType;
		RightToLeft = rightToLeft;
	}
}

/// <summary>
/// Represents a captured substring within a string segment based on a search operation.
/// </summary>
public readonly ref struct StringSegmentCapture
{
	/// <summary>
	/// Gets the search operation that resulted in this capture.
	/// </summary>
	public StringSegmentSearch Search { get; }

	/// <summary>
	/// Gets the captured substring as a string subsegment.
	/// </summary>
	public StringSubsegment Value { get; }

	/// <summary>
	/// Returns <see langword="true"/> if the capture was successful; otherwise, <see langword="false"/>.
	/// </summary>
	public bool Success => Value.HasValue;

	/// <summary>
	/// The index of the first character in the source <see cref="StringSegment"/> that is included in this subsegment.
	/// </summary>
	public int Index => Value.HasValue ? Value.Offset : -1;

	/// <summary>
	/// Implicitly converts a <see cref="StringSegmentSearch"/> to a <see cref="StringSegmentCapture"/>.
	/// </summary>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator StringSegmentCapture(StringSegmentSearch capture)
		=> capture.First();

	/// <summary>
	/// Implicitly converts a <see cref="StringSegmentCapture"/> to a <see cref="StringSubsegment"/>.
	/// </summary>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator StringSubsegment(StringSegmentCapture capture)
		=> capture.Value;

	/// <summary>
	/// Implicitly converts a <see cref="StringSegmentCapture"/> to a <see cref="StringSegment"/>.
	/// </summary>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator StringSegment(StringSegmentCapture capture)
		=> capture.Value;

	/// <inheritdoc />
	public override string ToString() => Value.ToString();

	/// <summary>
	/// Initializes a new instance of the <see cref="StringSegmentCapture"/> struct.
	/// </summary>
	/// <param name="search">
	/// <inheritdoc cref="Search" path="/summary"/>
	/// </param>
	/// <param name="value">
	/// <inheritdoc cref="Value" path="/summary"/>
	/// </param>
	internal StringSegmentCapture(StringSegmentSearch search, StringSubsegment value)
	{
		Search = search;
		Value = value;
	}
}

public static partial class TextExtensions
{
	/// <summary>
	/// Starts a search for the specified character sequence within the source segment.
	/// </summary>
	/// <param name="source">
	/// <inheritdoc cref="StringSegmentSearch.Source" path="/summary"/>
	/// </param>
	/// <param name="search">
	/// <inheritdoc cref="StringSegmentSearch.Sequence" path="/summary"/>
	/// </param>
	/// <param name="comparisonType">
	/// <inheritdoc cref="StringSegmentSearch.Comparison" path="/summary"/>
	/// </param>
	/// <param name="rightToLeft">
	/// <inheritdoc cref="StringSegmentSearch.RightToLeft" path="/summary"/>
	/// </param>
	public static StringSegmentSearch Find(
		this StringSegment source,
		ReadOnlySpan<char> search,
		StringComparison comparisonType = StringComparison.Ordinal,
		bool rightToLeft = false)
		=> new(source, search, comparisonType, rightToLeft);

	/// <inheritdoc cref="Find(StringSegment, ReadOnlySpan{char}, StringComparison, bool)"/>
	public static StringSegmentSearch Find(
		this string source,
		ReadOnlySpan<char> search,
		StringComparison comparisonType = StringComparison.Ordinal,
		bool rightToLeft = false)
		=> new(source, search, comparisonType, rightToLeft);

	/// <summary>
	/// Finds the next occurrence of the specified character sequence within the source segment.
	/// </summary>
	public static StringSegmentCapture First(
		this StringSegmentSearch search)
	{
		if(search.Source.Length == 0 || search.Sequence.Length == 0)
			return default;

		var i = search.RightToLeft
			? search.Source.LastIndexOf(search.Sequence, search.Comparison)
			: search.Source.IndexOf(search.Sequence, search.Comparison);

		return new(search, i == -1
			? default
			: new(search.Source, i, search.Sequence.Length) );
	}

	/// <summary>
	/// Finds the next occurrence of the specified character sequence within the source segment.
	/// </summary>
	public static StringSegmentCapture Next(
		this StringSegmentCapture capture)
	{
		var value = capture.Value;
		var len = value.Length;
		if (len == 0)
			return default;

		var search = capture.Search;
		var source = search.Source;
		var i = capture.Search.RightToLeft
			? source.Subsegment(0, value.Offset).LastIndexOf(search.Sequence, search.Comparison)
			: source.IndexOf(search.Sequence, value.Offset + value.Length, search.Comparison);

		return new(search, i == -1
			? default
			: new(source, i, len));
	}

	/// <summary>
	/// Finds the next occurrence after the first occurrence of the specified character sequence within the source segment.
	/// </summary>
	public static StringSegmentCapture Next(
		this StringSegmentSearch search)
		=> search.First().Next();

	/// <summary>
	/// Finds the last occurrence of the specified character sequence within the source segment.
	/// </summary>
	public static StringSegmentCapture Last(
		this StringSegmentSearch search)
	{
		if (search.Source.Length == 0 || search.Sequence.Length == 0)
			return default;

		var i = search.RightToLeft
			? search.Source.IndexOf(search.Sequence, search.Comparison)
			: search.Source.LastIndexOf(search.Sequence, search.Comparison);

		return new(search, i == -1
			? default
			: new(search.Source, i, search.Sequence.Length));
	}

	/// <summary>
	/// Returns <see langword="true"/> if the capture has a value; otherwise <see langword="false"/>.
	/// </summary>
	public static bool Exists(this StringSegmentCapture capture)
		=> capture.Value.HasValue;

	/// <summary>
	/// Returns <see langword="true"/> if the search has a value; otherwise <see langword="false"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Exists(this StringSegmentSearch search)
		=> search.First().Exists();

	/// <summary>
	/// Resolves the value of the capture, or returns the specified default value.
	/// </summary>
	public static StringSubsegment Or(
		this StringSegmentCapture capture,
		StringSubsegment defaultValue)
	{
		var value = capture.Value;
		return value.HasValue ? value : defaultValue;
	}

	/// <summary>
	/// Resolves the value of the search, or returns the specified default value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static StringSubsegment Or(
		this StringSegmentSearch capture,
		StringSubsegment defaultValue)
		=> capture.First().Or(defaultValue);
}