using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Open.Text;

/// <summary>
/// A struct for representing a subsegment of a <see cref="StringSegment"/>.
/// </summary>
public readonly struct StringSubsegment : IEquatable<StringSubsegment>
{
	/// <summary>
	/// Constructs a new <see cref="StringSubsegment"/> from the specified <see cref="StringSegment"/>.
	/// </summary>
	/// <param name="source"><inheritdoc cref="Source" path="/summary"/></param>
	/// <param name="offset"><inheritdoc cref="Offset" path="/summary"/></param>
	/// <param name="length"><inheritdoc cref="Length" path="/summary"/></param>
	internal StringSubsegment(
		StringSegment source,
		int offset,
		int length)
	{
		Debug.Assert(source.HasValue);
		Debug.Assert(offset >= 0 && offset <= source.Length);
		Debug.Assert(length >= 0 && offset + length <= source.Length);

		Source = source;
		Offset = offset;
		Length = length;
	}

	/// <summary>
	/// Indicates that the <see cref="Source"/> is has a value.
	/// </summary>
	public bool HasValue => Source.HasValue;

	/// <summary>
	/// The source <see cref="StringSegment"/> from which this subsegment was created.
	/// </summary>
	public StringSegment Source { get; }

	/// <summary>
	/// The index of the first character in the source <see cref="StringSegment"/> that is included in this subsegment.
	/// </summary>
	public int Offset { get; }

	/// <summary>
	/// The number of characters in the source <see cref="StringSegment"/> that are included in this subsegment.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// Gets a <see cref="ReadOnlySpan{T}"/> representing the defined subsegment.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<char> AsSpan()
		=> Source.AsSpan(Offset, Length);

	/// <summary>
	/// Returns a new <see cref="StringSegment"/> representing the defined subsegment.
	/// </summary>
	public StringSegment AsSegment()
		=> Offset == 0 && Source.Length == Length ? Source : Source.Subsegment(Offset, Length);

	/// <summary>
	/// Returns a new <see cref="StringSegment"/> representing the defined subsegment.
	/// </summary>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "AsSegment")]
	public static implicit operator StringSegment(StringSubsegment segment)
		=> segment.AsSegment();

	/// <summary>
	/// Compares a read-only span of characters to this <see cref="StringSubsegment"/> for equality.
	/// </summary>
	public bool Equals(ReadOnlySpan<char> other, StringComparison comparisonType = StringComparison.Ordinal)
		=> Source.HasValue && other.Length == Length && AsSpan().Equals(other, comparisonType);

	/// <summary>
	/// Compares two <see cref="StringSubsegment"/> instances for equality.
	/// </summary>
	public bool Equals(StringSubsegment other, StringComparison comparisonType = StringComparison.Ordinal)
		=> Source.HasValue
		? other.Source.HasValue && other.Length == Length && Equals(other.AsSpan(), comparisonType)
		: !other.Source.HasValue;

	/// <inheritdoc />
	public override bool Equals(object obj)
		=> obj is StringSubsegment other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode()
		=> AsSegment().GetHashCode();

	/// <inheritdoc />
	public override string ToString()
		=> AsSpan().ToString();

	bool IEquatable<StringSubsegment>.Equals(StringSubsegment other)
		=> Equals(other);

	/// <summary>
	/// Compares two <see cref="StringSubsegment"/> instances for equality.
	/// </summary>
	public static bool operator ==(StringSubsegment left, StringSubsegment right)
		=> left.Equals(right);

	/// <summary>
	/// Compares two <see cref="StringSubsegment"/> instances for inequality.
	/// </summary>
	public static bool operator !=(StringSubsegment left, StringSubsegment right)
		=> !left.Equals(right);
}