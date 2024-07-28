namespace Open.Text;

/// <summary>
/// A <see cref="StringComparison"/> struct for comparing a <see cref="string"/> against other values.
/// </summary>
public readonly struct StringComparable
	: IEquatable<StringComparable>, IEquatable<string>, IEquatable<StringSegment>
{
	/// <summary>
	/// Constructs a <see cref="StringComparable"/> using the provided string and comparison type.
	/// </summary>
	public StringComparable(StringSegment segment, StringComparison type)
	{
		Segment = segment;
		Type = type;
	}

	/// <summary>
	/// Constructs a <see cref="StringComparable"/> using the provided string and comparison type.
	/// </summary>
	public StringComparable(string? value, StringComparison type)
	{
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
	[ExcludeFromCodeCoverage]
	public int Length => Segment.Length;

	/// <summary>
	/// Compares <paramref name="other"/> if its characters matches this instance.
	/// </summary>
	/// <returns>true if the value of <paramref name="other"/> matches; otherwise false. </returns>
	public bool Equals(string? other)
		=> Segment.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(ReadOnlySpan<char> other)
		=> Segment.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(StringComparable other)
		=> Segment.Equals(other.Segment, Type)
		|| Type != other.Type && other.Equals(Segment);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(SpanComparable other)
		=> Segment.Equals(other.Source, Type)
		|| Type != other.Type && other.Equals(Segment);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(StringSegment other)
		=> Equals(other.AsSpan());

	/// <summary>
	/// Compares <paramref name="obj"/> if it is a <see cref="string"/> or a <see cref="StringComparable"/> and if the value matches this instance.
	/// </summary>
	/// <returns>true if the value of <paramref name="obj"/> matches; otherwise false. </returns>
	[ExcludeFromCodeCoverage]
	public override bool Equals(object? obj) => obj switch
	{
		StringComparable sc => Equals(sc),
		StringSegment se => Equals(se),
		string s => Equals(s),
		_ => false
	};

	/// <summary>
	/// Checks if <paramref name="value"/> value is contained in the sequence using the comparison type.
	/// </summary>
	/// <returns>true if the value of <paramref name="value"/> is contained (using the comparison type); otherwise false. </returns>
	public bool Contains(string value)
		=> Segment.Contains(value, Type);

	/// <inheritdoc cref="Contains(string)"/>
	public bool Contains(StringSegment value)
		=> Segment.Contains(value, Type);

	/// <inheritdoc cref="Contains(string)"/>
	public bool Contains(ReadOnlySpan<char> value)
		=> Segment.Contains(value, Type);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
#if NETSTANDARD2_0
	public override int GetHashCode()
	{
		int hashCode = 141257509;
		hashCode = hashCode * -1521134295 + Segment.GetHashCode();
		hashCode = hashCode * -1521134295 + Type.GetHashCode();
		return hashCode;
	}
#else
	public override int GetHashCode()
		=> HashCode.Combine(Segment, Type);
#endif

	/// <summary>
	/// Compares two <see cref="StringComparable"/>s for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, StringComparable b) => a.Equals(b);

	/// <summary>
	/// Compares two <see cref="StringComparable"/>s for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, StringComparable b) => !a.Equals(b);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a <see cref="string"/> for equality.
	/// </summary>
	public static bool operator ==(StringComparable comparable, string? str) => comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a <see cref="string"/> for inequality.
	/// </summary>
	public static bool operator !=(StringComparable comparable, string? str) => !comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="string"/> and a <see cref="StringComparable"/> for equality.
	/// </summary>
	public static bool operator ==(string? str, StringComparable comparable) => comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="string"/> and a <see cref="StringComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(string? str, StringComparable comparable) => !comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a <see cref="StringSegment"/> for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, StringSegment b) => a.Equals(b);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a <see cref="StringSegment"/> for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, StringSegment b) => !a.Equals(b);

	/// <summary>
	/// Compares a <see cref="StringSegment"/> and a <see cref="StringComparable"/> for equality.
	/// </summary>
	public static bool operator ==(StringSegment a, StringComparable b) => b.Equals(a);

	/// <summary>
	/// Compares a <see cref="StringSegment"/> and a <see cref="StringComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(StringSegment a, StringComparable b) => !b.Equals(a);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a span for equality.
	/// </summary>
	public static bool operator ==(StringComparable a, ReadOnlySpan<char> b) => a.Equals(b);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a span for inequality.
	/// </summary>
	public static bool operator !=(StringComparable a, ReadOnlySpan<char> b) => !a.Equals(b);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a <see cref="SpanComparable"/> for equality.
	/// </summary>
	public static bool operator ==(StringComparable str, SpanComparable span) => str.Equals(span);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a <see cref="SpanComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(StringComparable str, SpanComparable span) => !str.Equals(span);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a span for equality.
	/// </summary>
	public static bool operator ==(ReadOnlySpan<char> a, StringComparable b) => b.Equals(a);

	/// <summary>
	/// Compares a <see cref="StringComparable"/> and a span for inequality.
	/// </summary>
	public static bool operator !=(ReadOnlySpan<char> a, StringComparable b) => !b.Equals(a);
}

/// <summary>Extensions for StringComparable.</summary>
[ExcludeFromCodeCoverage]
public static class StringComparableExtensions
{
	/// <summary>
	/// Prepares a <see cref="string"/> for a specific StringComparison operation.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsComparable(this string? source, StringComparison type)
		=> new(source, type);

	/// <summary>
	/// Prepares a <see cref="string"/> to be case insensitive when comparing equality.
	/// </summary>
	/// <returns>A <see cref="StringComparable"/> that can be compared (== or !=) against other StringComparables, SpanComparables, strings, and ReadOnlySpan&lt;char&gt;.</returns>
	public static StringComparable AsCaseInsensitive(this string? source)
		=> new(source, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Prepares a <see cref="string"/> to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveInvariantCulture(this string? source)
		=> new(source, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Prepares a <see cref="string"/> to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveCurrentCulture(this string? source)
		=> new(source, StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// Prepares a <see cref="string"/> to be current culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCurrentCulture(this string? source)
		=> new(source, StringComparison.CurrentCulture);

	/// <summary>
	/// Prepares a <see cref="string"/> to be invariant culture and case sensitive when comparing equality.
	/// </summary>
	public static StringComparable AsInvariantCulture(this string? source)
		=> new(source, StringComparison.InvariantCulture);

	/// <summary>
	/// Prepares a <see cref="StringSegment"/> for a specific <see cref="StringComparison"/> operation.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsComparable(this StringSegment source, StringComparison type)
		=> new(source, type);

	/// <summary>
	/// Prepares a <see cref="StringSegment"/> to be case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitive(this StringSegment source)
		=> new(source, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Prepares a <see cref="StringSegment"/> to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveInvariantCulture(this StringSegment source)
		=> new(source, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Prepares a <see cref="StringSegment"/> to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCaseInsensitiveCurrentCulture(this StringSegment source)
		=> new(source, StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// Prepares a <see cref="StringSegment"/> to be current culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static StringComparable AsCurrentCulture(this StringSegment source)
		=> new(source, StringComparison.CurrentCulture);

	/// <summary>
	/// Prepares a <see cref="StringSegment"/> to be invariant culture and case sensitive when comparing equality.
	/// </summary>
	public static StringComparable AsInvariantCulture(this StringSegment source)
		=> new(source, StringComparison.InvariantCulture);
}
