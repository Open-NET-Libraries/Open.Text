using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Text;

/// <summary>
/// A comparer for <see cref="StringSegment"/> that uses existing <see cref="StringSegment.Compare(StringSegment, StringSegment, StringComparison)"/>
/// and <see cref="StringSegment.Equals(StringSegment, StringSegment, StringComparison)"/> for its comparisons
/// and <see cref="TextExtensions.GetHashCodeFromChars(ReadOnlySpan{char}, StringComparison, int)"/> for hash codes.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="StringSegmentComparer"/>
/// with the specified <paramref name="comparisonType"/>
/// and <paramref name="maxHashChars"/>.
/// </remarks>
/// <param name="comparisonType">The string comparison type.</param>
/// <param name="maxHashChars">
/// The max number of characters to generate a hash from.
/// See <see cref="TextExtensions.GetHashCodeFromChars(ReadOnlySpan{char}, StringComparison, int)"/> for more details.
/// </param>
public class StringSegmentComparer(StringComparison comparisonType = StringComparison.Ordinal, int maxHashChars = int.MaxValue)
		: IComparer<StringSegment>, IEqualityComparer<StringSegment>
{
	/// <summary>
	/// A <see cref="StringSegmentComparer"/> that uses <see cref="StringComparison.Ordinal"/>.
	/// </summary>
	public static readonly StringSegmentComparer Ordinal = new();

	/// <summary>
	/// A <see cref="StringSegmentComparer"/> that uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
	/// </summary>
	public static readonly StringSegmentComparer OrdinalIgnoreCase = new(StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// A <see cref="StringSegmentComparer"/> that uses <see cref="StringComparison.CurrentCulture"/>.
	/// </summary>
	public static readonly StringSegmentComparer CurrentCulture = new(StringComparison.CurrentCulture);

	/// <summary>
	/// A <see cref="StringSegmentComparer"/> that uses <see cref="StringComparison.CurrentCultureIgnoreCase"/>.
	/// </summary>
	public static readonly StringSegmentComparer CurrentCultureIgnoreCase = new(StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// A <see cref="StringSegmentComparer"/> that uses <see cref="StringComparison.InvariantCulture"/>.
	/// </summary>
	public static readonly StringSegmentComparer InvariantCulture = new(StringComparison.InvariantCulture);

	/// <summary>
	/// A <see cref="StringSegmentComparer"/> that uses <see cref="StringComparison.InvariantCultureIgnoreCase"/>.
	/// </summary>
	public static readonly StringSegmentComparer InvariantCultureIgnoreCase = new(StringComparison.InvariantCultureIgnoreCase);

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
