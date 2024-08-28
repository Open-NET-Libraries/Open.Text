using System.ComponentModel;

namespace Open.Text;

/// <summary>
/// A <see cref="StringComparison"/> struct for comparing a span against other values.
/// </summary>
/// <remarks>
/// Constructs a <see cref="SpanComparable"/> using the provided string and comparison type.
/// </remarks>
public readonly ref struct SpanComparable(ReadOnlySpan<char> source, StringComparison type)
{
	/// <summary>
	/// The span to use for comparison.
	/// </summary>
	public readonly ReadOnlySpan<char> Source { get; } = source;

	/// <summary>
	/// The type of string comparison.
	/// </summary>
	public readonly StringComparison Type { get; } = type;

	/// <summary>
	/// The length of the string.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public int Length => Source.Length;

	/// <summary>
	/// Compares <paramref name="other"/> if its characters matches this instance.
	/// </summary>
	/// <returns>true if the value of <paramref name="other"/> matches; otherwise false. </returns>
	public bool Equals(string? other)
		=> Source.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(ReadOnlySpan<char> other)
		=> Source.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(StringComparable other)
		=> Source.Equals(other.Segment, Type)
		|| Type != other.Type && other.Equals(Source);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(SpanComparable other)
		=> Source.Equals(other.Source, Type)
		|| Type != other.Type && other.Equals(Source);

	/// <summary>
	/// Reports the zero-based index of the first occurrence
	/// of the specified <paramref name="value"/>
	/// starting from the <paramref name="startIndex"/>.
	/// </summary>
	public int IndexOf(StringSegment value, int startIndex = 0)
		=> Source.IndexOf(value, startIndex, Type);

	/// <inheritdoc cref="IndexOf(StringSegment, int)"/>
	public int IndexOf(char value, int startIndex = 0)
		=> Source.IndexOf(value, startIndex, Type);

	/// <inheritdoc cref="IndexOf(StringSegment, int)"/>
	public int IndexOf(ReadOnlySpan<char> value, int startIndex = 0)
		=> Source.IndexOf(value, startIndex, Type);

	/// <summary>
	/// Checks if <paramref name="value"/> value is contained in the sequence using the comparison type.
	/// </summary>
	/// <returns>true if the value of <paramref name="value"/> is contained (using the comparison type); otherwise false. </returns>
	public bool Contains(StringSegment value)
		=> Source.Contains(value, Type);

	/// <inheritdoc cref="Contains(StringSegment)"/>
	public bool Contains(char value)
		=> Source.Contains(value, Type);

	/// <inheritdoc cref="Contains(StringSegment)"/>
	public bool Contains(ReadOnlySpan<char> value)
		=> Source.Contains(value, Type);

	/// <inheritdoc cref="ReadOnlySpan{T}.Equals(object)" />
	[Obsolete("Equals() on StringCompariable will always throw an exception. Use == instead.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "<Pending>")]
	[ExcludeFromCodeCoverage]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	public override bool Equals(object? obj) => throw new NotSupportedException();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0070 // Use 'System.HashCode'
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public override int GetHashCode()
	{
		int hashCode = 1013877538;
		hashCode = hashCode * -1521134295 + Source.GetHashCode();
		hashCode = hashCode * -1521134295 + Type.GetHashCode();
		return hashCode;
	}
#pragma warning restore IDE0070 // Use 'System.HashCode'
#pragma warning restore IDE0079 // Remove unnecessary suppression

	/// <summary>
	/// Compares two <see cref="SpanComparable"/>s for equality.
	/// </summary>
	public static bool operator ==(SpanComparable a, SpanComparable b) => a.Equals(b);

	/// <summary>
	/// Compares two <see cref="SpanComparable"/>s for inequality.
	/// </summary>
	public static bool operator !=(SpanComparable a, SpanComparable b) => !a.Equals(b);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> and a <see cref="StringSegment"/> for equality.
	/// </summary>
	public static bool operator ==(SpanComparable comparable, StringSegment segment) => comparable.Equals(segment);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> and a <see cref="StringSegment"/> for inequality.
	/// </summary>
	public static bool operator !=(SpanComparable comparable, StringSegment segment) => !comparable.Equals(segment);

	/// <summary>
	/// Compares a <see cref="StringSegment"/> and a <see cref="SpanComparable"/> for equality.
	/// </summary>
	public static bool operator ==(StringSegment segment, SpanComparable comparable) => comparable.Equals(segment);

	/// <summary>
	/// Compares a <see cref="StringSegment"/> and a <see cref="SpanComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(StringSegment segment, SpanComparable comparable) => !comparable.Equals(segment);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> and a <see cref="string"/> for equality.
	/// </summary>
	public static bool operator ==(SpanComparable comparable, string? str) => comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> and a <see cref="string"/> for inequality.
	/// </summary>
	public static bool operator !=(SpanComparable comparable, string? str) => !comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="string"/> and a <see cref="SpanComparable"/> for equality.
	/// </summary>
	public static bool operator ==(string? str, SpanComparable comparable) => comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="string"/> and a <see cref="SpanComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(string? str, SpanComparable comparable) => !comparable.Equals(str);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> with a span for equality.
	/// </summary>
	public static bool operator ==(SpanComparable comparable, ReadOnlySpan<char> segment) => comparable.Equals(segment);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> with a span for inequality.
	/// </summary>
	public static bool operator !=(SpanComparable comparable, ReadOnlySpan<char> segment) => !comparable.Equals(segment);

	/// <summary>
	/// Compares a span with a <see cref="SpanComparable"/> for equality.
	/// </summary>
	public static bool operator ==(ReadOnlySpan<char> segment, SpanComparable comparable) => comparable.Equals(segment);

	/// <summary>
	/// Compares a span with a <see cref="SpanComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(ReadOnlySpan<char> segment, SpanComparable comparable) => !comparable.Equals(segment);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> with a <see cref="StringComparable"/> for equality.
	/// </summary>
	public static bool operator ==(SpanComparable span, StringComparable str) => span.Equals(str);

	/// <summary>
	/// Compares a <see cref="SpanComparable"/> with a <see cref="StringComparable"/> for inequality.
	/// </summary>
	public static bool operator !=(SpanComparable span, StringComparable str) => !span.Equals(str);
}

/// <summary>Extensions for <see cref="SpanComparable"/>.</summary>
[ExcludeFromCodeCoverage]
public static class SpanComparableExtensions
{
	/// <summary>
	/// Prepares a span for a specific <see cref="StringComparison"/> operation.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(ReadOnlySpan{char})"/>
	public static SpanComparable AsComparable(this ReadOnlySpan<char> source, StringComparison type)
		=> new(source, type);

	/// <summary>
	/// Prepares a span to be case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="StringComparableExtensions.AsCaseInsensitive(string)"/>
	public static SpanComparable AsCaseInsensitive(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Prepares a span to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(ReadOnlySpan{char})"/>
	public static SpanComparable AsCaseInsensitiveInvariantCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Prepares a span to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(ReadOnlySpan{char})"/>
	public static SpanComparable AsCaseInsensitiveCurrentCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// Prepares a span to be current culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(ReadOnlySpan{char})"/>
	public static SpanComparable AsCurrentCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.CurrentCulture);

	/// <summary>
	/// Prepares a span to be invariant culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(ReadOnlySpan{char})"/>
	public static SpanComparable AsInvariantCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.InvariantCulture);
}
