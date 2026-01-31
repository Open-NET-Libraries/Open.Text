namespace Open.Text;

public static partial class TextExtensions
{
	/// <summary>
	/// Finds the first instance of a string and returns a StringSegment for subsequent use.
	/// </summary>
	/// <param name="source">The source string to search.</param>
	/// <param name="search">The string pattern to look for.</param>
	/// <param name="comparisonType">The string comparison type to use.  Default is Ordinal.</param>
	/// <returns>
	/// The segment representing the found string.
	/// If not found, the result will have a length of 0 and its <see cref="StringSegment.HasValue"/> property will be <see langword="false"/>.
	/// </returns>
	public static StringSegment First(this StringSegment source, StringSegment search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();

		if (search.Length == 0)
			return default;

		int i = source.IndexOf(search, comparisonType);
		return i == -1 ? default : source.Subsegment(i, search.Length);
	}

	/// <inheritdoc cref="First(StringSegment, StringSegment, StringComparison)"/>
	public static StringSegment First(this StringSegment source, char search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();

		int i = source.IndexOf(search, comparisonType);
		return i == -1 ? default : source.Subsegment(i, 1);
	}

	/// <inheritdoc cref="First(StringSegment, StringSegment, StringComparison)"/>
	public static StringSegment First(this string source, char search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		int i = source.IndexOf(search, comparisonType);
		return i == -1 ? default : source.AsSegment(i, 1);
	}

	/// <inheritdoc cref="First(StringSegment, StringSegment, StringComparison)"/>
	public static StringSegment First(this string source, StringSegment search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return First(source.AsSegment(), search, comparisonType);
	}

	/// <summary>
	/// Finds the first instance of a pattern and returns a StringSegment for subsequent use.
	/// </summary>
	/// <param name="source">The source string to search.</param>
	/// <param name="pattern">The pattern to look for.</param>
	/// <returns>
	/// The segment representing the found string.
	/// If not found, the StringSegment.HasValue property will be false.
	/// </returns>
	/// <remarks>If the pattern is right-to-left, then it will return the first segment from the right.</remarks>
	[ExcludeFromCodeCoverage] // Reason: would just test already tested code.
	public static StringSegment First(this string source, Regex pattern)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		Contract.EndContractBlock();

		Match match = pattern.Match(source);
		return match.Success ? new(source, match.Index, match.Length) : default;
	}

	/// <inheritdoc cref="First(string, StringSegment, StringComparison)" />
	public static StringSegment First(this StringSegment source, ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();

		if (search.IsEmpty)
			return default;

		int i = source.AsSpan().IndexOf(search, comparisonType);
		return i == -1 ? default : new(source.Buffer, source.Offset + i, search.Length);
	}

	/// <summary>
	/// Finds the last instance of a string and returns a StringSegment for subsequent use.
	/// </summary>
	/// <inheritdoc cref="First(StringSegment, StringSegment, StringComparison)"/>
	public static StringSegment Last(this StringSegment source, StringSegment search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();

		if (search.Length == 0)
			return default;

		int i = source.AsSpan().LastIndexOf(search, comparisonType);
		return i == -1 ? default : source.Subsegment(i, search.Length);
	}

	/// <inheritdoc cref="Last(StringSegment, StringSegment, StringComparison)" />
	public static StringSegment Last(this string source, StringSegment search, StringComparison comparisonType = StringComparison.Ordinal)
		=> Last(source.AsSegment(), search, comparisonType);

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

		MatchCollection matches = pattern.Matches(source);
		if (matches.Count == 0) return default;
		Match match = matches[matches.Count - 1];
		return new(source, match.Index, match.Length);
	}

	/// <inheritdoc cref="Last(StringSegment, StringSegment, StringComparison)" />
	public static StringSegment Last(this StringSegment source, ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue) throw new ArgumentException(MustBeSegmentWithValue, nameof(source));
		Contract.EndContractBlock();
		if (search.IsEmpty)
			return default;

		int i = source.AsSpan().LastIndexOf(search, comparisonType);
		return i == -1 ? default : new(source.Buffer, source.Offset + i, search.Length);
	}
}
