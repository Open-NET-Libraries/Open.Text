using Microsoft.Extensions.Primitives;
using System;

namespace Open.Text;

/// <summary>
/// A StringComparison variable struct for comparing a string against other values.
/// </summary>
public readonly struct StringComparable
	: IEquatable<StringComparable>, IEquatable<string>, IEquatable<StringSegment>
{
	/// <summary>
	/// Constructs a StringComparable using the provided string and comparison type.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="segment"/> does not contain a value (is default).</exception>
	public StringComparable(StringSegment segment, StringComparison type)
	{
		if (!segment.HasValue) throw new ArgumentException("The provided segment must have a value.", nameof(segment));
		Segment = segment;
		Type = type;
	}

	/// <summary>
	/// Constructs a StringComparable using the provided string and comparison type.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
	public StringComparable(string value, StringComparison type)
	{
		if (value is null) throw new ArgumentNullException(nameof(value));
		Segment = value;
		Type = type;
	}
	/// <summary>
	/// The string to use for comparison.
	/// </summary>
	public readonly StringSegment Segment { get; }

	/// <summary>
	/// The type of string comparison.
	/// </summary>
	public readonly StringComparison Type { get; }

	/// <summary>
	/// The length of the string.
	/// </summary>
	public int Length => Segment.Length;

	/// <summary>
	/// Compares <paramref name="other"/> if its characters matches this instance.
	/// </summary>
	/// <returns>true if the value of <paramref name="other"/> matches; otherwise false. </returns>
	public bool Equals(string? other)
		=> other is not null && Segment.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(ReadOnlySpan<char> other)
		=> Segment.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(StringComparable other)
		=> Segment.Equals(other.Segment, Type)
		|| Type != other.Type && other.Equals(Segment);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(in SpanComparable other)
		=> Segment.Equals(other.Source, Type)
		|| Type != other.Type && other.Equals(Segment);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(StringSegment other)
		=> Equals(other.AsSpan());

	/// <summary>
	/// Compares <paramref name="obj"/> if it is a string or StringComparable and if the value matches this instance.
	/// </summary>
	/// <returns>true if the value of <paramref name="obj"/> matches; otherwise false. </returns>
	public override bool Equals(object obj) => obj switch
	{
		StringComparable sc => Equals(sc),
		StringSegment se => Equals(se),
		string s => Equals(s),
		_ => false
	};

	/// <inheritdoc />
#if NETSTANDARD2_1_OR_GREATER
	public override int GetHashCode()
		=> HashCode.Combine(Segment, Type);
#else
	public override int GetHashCode()
	{
		int hashCode = 141257509;
		hashCode = hashCode * -1521134295 + Segment.GetHashCode();
		hashCode = hashCode * -1521134295 + Type.GetHashCode();
		return hashCode;
	}
#endif

	/// <summary>
	/// Compares two StringComparables for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, StringComparable b) => a.Equals(b);

	/// <summary>
	/// Compares two StringComparables for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, StringComparable b) => !a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a string for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, string? b) => a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a string for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, string? b) => !a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a StringSegment for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, StringSegment b) => a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a StringSegment for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, StringSegment b) => !a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a span for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, ReadOnlySpan<char> b) => a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a span for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, ReadOnlySpan<char> b) => !a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a SpanComparable for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, SpanComparable b) => a.Equals(b);

	/// <summary>
	/// Compares a StringComparable and a SpanComparable for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, SpanComparable b) => !a.Equals(b);

}

/// <summary />
public static class StringComparableExtensions
{
	/// <summary>
	/// Prepares a string for a specific StringComparison operation.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsComparable(this string source, StringComparison type)
		=> new(source, type);

	/// <summary>
	/// Prepares a string to be case insensitive when comparing equality.
	/// </summary>
	/// <returns>A StringComparable that can be compared (== or !=) against other StringComparables, SpanComparables, strings, and ReadOnlySpan&lt;char&gt;.</returns>
	public static StringComparable AsCaseInsensitive(this string source)
		=> new(source, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Prepares a string to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveInvariantCulture(this string source)
		=> new(source, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Prepares a string to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveCurrentCulture(this string source)
		=> new(source, StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// Prepares a string to be current culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCurrentCulture(this string source)
		=> new(source, StringComparison.CurrentCulture);

	/// <summary>
	/// Prepares a string to be invariant culture and case sensitive when comparing equality.
	/// </summary>
	public static StringComparable AsInvariantCulture(this string source)
		=> new(source, StringComparison.InvariantCulture);

	/// <summary>
	/// Prepares a StringSegment for a specific StringComparison operation.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsComparable(this StringSegment source, StringComparison type)
		=> new(source, type);

	/// <summary>
	/// Prepares a StringSegment to be case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitive(this StringSegment source)
		=> new(source, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Prepares a StringSegment to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveInvariantCulture(this StringSegment source)
		=> new(source, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Prepares a StringSegment to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveCurrentCulture(this StringSegment source)
		=> new(source, StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// Prepares a StringSegment to be current culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCurrentCulture(this StringSegment source)
		=> new(source, StringComparison.CurrentCulture);

	/// <summary>
	/// Prepares a StringSegment to be invariant culture and case sensitive when comparing equality.
	/// </summary>
	public static StringComparable AsInvariantCulture(this StringSegment source)
		=> new(source, StringComparison.InvariantCulture);

}
