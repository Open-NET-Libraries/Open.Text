namespace Open.Text;

/// <summary>
/// A comparer for <see cref="StringSegment"/> that uses existing <see cref="StringSegment.Compare(StringSegment, StringSegment, StringComparison)"/>
/// and <see cref="StringSegment.Equals(StringSegment, StringSegment, StringComparison)"/> for its comparisons
/// and <see cref="TextExtensions.GetHashCodeFromChars(ReadOnlySpan{char}, StringComparison, int)"/> for hash codes.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="StringSegmentHashComparer"/>
/// with the specified <paramref name="comparisonType"/>
/// and <paramref name="maxHashChars"/>.
/// </remarks>
/// <param name="comparisonType">The string comparison type.</param>
/// <param name="maxHashChars">
/// The max number of characters to generate a hash from.
/// See <see cref="TextExtensions.GetHashCodeFromChars(ReadOnlySpan{char}, StringComparison, int)"/> for more details.
/// </param>
public class StringSegmentHashComparer(StringComparison comparisonType, int maxHashChars)
		: IComparer<StringSegment>, IEqualityComparer<StringSegment>
{
	/// <summary>
	/// Creates a comparer using <see cref="StringComparison.Ordinal"/>.
	/// </summary>
	public static StringSegmentHashComparer Create(int maxHashChars)
		=> new(StringComparison.Ordinal, maxHashChars);

	/// <summary>
	/// Creates a comparer using the specified <paramref name="comparisonType"/>.
	/// </summary>
	public static StringSegmentHashComparer Create(StringComparison comparisonType, int maxHashChars)
		=> new(comparisonType, maxHashChars);

	/// <summary>
	/// Creates a comparer using <see cref="StringComparison.OrdinalIgnoreCase"/>.
	/// </summary>
	public static StringSegmentHashComparer CreateOrdinalIgnoreCase(int maxHashChars)
		=> new(StringComparison.OrdinalIgnoreCase, maxHashChars);

	/// <summary>
	/// Creates a comparer using <see cref="StringComparison.CurrentCulture"/>.
	/// </summary>
	public static StringSegmentHashComparer CreateCurrentCulture(int maxHashChars)
		=> new(StringComparison.CurrentCulture, maxHashChars);

	/// <summary>
	/// Creates a comparer using <see cref="StringComparison.CurrentCultureIgnoreCase"/>.
	/// </summary>
	public static StringSegmentHashComparer CreateCurrentCultureIgnoreCase(int maxHashChars)
		=> new(StringComparison.CurrentCultureIgnoreCase, maxHashChars);

	/// <summary>
	/// Creates a comparer using <see cref="StringComparison.InvariantCulture"/>.
	/// </summary>
	public static StringSegmentHashComparer CreateInvariantCulture(int maxHashChars)
		=> new(StringComparison.InvariantCulture, maxHashChars);

	/// <summary>
	/// Creates a comparer using <see cref="StringComparison.InvariantCultureIgnoreCase"/>.
	/// </summary>
	public static StringSegmentHashComparer CreateInvariantCultureIgnoreCase(int maxHashChars)
		=> new(StringComparison.InvariantCultureIgnoreCase, maxHashChars);

	/// <summary>
	/// The string comparison type.
	/// </summary>
	public StringComparison ComparisonType { get; } = comparisonType;

	/// <summary>
	/// The max number of characters to generate a hash from.
	/// </summary>
	public int MaxHashChars { get; } = maxHashChars;

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Compare(StringSegment x, StringSegment y)
		=> StringSegment.Compare(x, y, ComparisonType);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(StringSegment x, StringSegment y)
		=> StringSegment.Equals(x, y, ComparisonType);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode(StringSegment obj)
		=> obj.GetHashCodeFromChars(ComparisonType, MaxHashChars);
}
