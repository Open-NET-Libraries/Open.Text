using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Open.Text;

public readonly ref struct SpanComparable
{
	/// <summary>
	/// Constructs a SpanComparable using the provided string and comparison type.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
	public SpanComparable(ReadOnlySpan<char> source, StringComparison type)
	{
		Source = source;
		Type = type;
	}

	/// <summary>
	/// The span to use for comparison.
	/// </summary>
	public ReadOnlySpan<char> Source { get; }

	/// <summary>
	/// The type of string comparison.
	/// </summary>
	public StringComparison Type { get; }

	/// <summary>
	/// The length of the string.
	/// </summary>
	public int Length => Source.Length;

	/// <summary>
	/// Compares <paramref name="other"/> if its value matches this instance.
	/// </summary>
	/// <returns>true if the value of <paramref name="other"/> matches; otherwise false. </returns>
	public bool Equals(string? other)
		=> Source.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(ReadOnlySpan<char> other)
		=> Source.Equals(other, Type);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(StringComparable other)
		=> Source.Equals(other.Source, Type)
		|| Type != other.Type && other.Equals(Source);

	/// <inheritdoc cref="Equals(string?)"/>
	public bool Equals(SpanComparable other)
		=> Source.Equals(other.Source, Type)
		|| Type != other.Type && other.Equals(Source);


	[Obsolete("Equals() on ReadOnlySpan will always throw an exception. Use == instead.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "<Pending>")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	public override bool Equals(object obj) => throw new NotSupportedException();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0070 // Use 'System.HashCode'
	/// <inheritdoc />
	public override int GetHashCode()
	{
		int hashCode = 1013877538;
		hashCode = hashCode * -1521134295 + Source.GetHashCode();
		hashCode = hashCode * -1521134295 + Type.GetHashCode();
		return hashCode;
	}
#pragma warning restore IDE0070 // Use 'System.HashCode'
#pragma warning restore IDE0079 // Remove unnecessary suppression

	public static bool operator ==(SpanComparable a, SpanComparable b) => a.Equals(b);
	public static bool operator !=(SpanComparable a, SpanComparable b) => !a.Equals(b);

	public static bool operator ==(SpanComparable a, string? b) => a.Equals(b);
	public static bool operator !=(SpanComparable a, string? b) => !a.Equals(b);

	public static bool operator ==(SpanComparable a, ReadOnlySpan<char> b) => a.Equals(b);
	public static bool operator !=(SpanComparable a, ReadOnlySpan<char> b) => !a.Equals(b);

	public static bool operator ==(SpanComparable a, StringComparable b) => a.Equals(b);
	public static bool operator !=(SpanComparable a, StringComparable b) => !a.Equals(b);


}

public static class SpanComparableExtensions
{
	/// <summary>
	/// Prepares a span for a specific StringComparison operation.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static SpanComparable AsComparable(this ReadOnlySpan<char> source, StringComparison type)
		=> new(source, type);

	/// <summary>
	/// Prepares a span to be case insensitive when comparing equality.
	/// </summary>
	/// <returns>A SpanComparable that can be compared (== or !=) against other SpanComparable, StringComparables, strings, and ReadOnlySpan&lt;char&gt;.</returns>
	public static SpanComparable AsCaseInsensitive(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Prepares a span to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static SpanComparable AsCaseInsensitiveInvariantCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.InvariantCultureIgnoreCase);

	/// <summary>
	/// Prepares a span to be invariant culture and case insensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static SpanComparable AsCaseInsensitiveCurrentCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	/// Prepares a span to be current culture and case sensitive when comparing equality.
	/// </summary>
	/// <inheritdoc cref="AsCaseInsensitive(string)"/>
	public static SpanComparable AsCurrentCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.CurrentCulture);

	/// <summary>
	/// Prepares a span to be invariant culture and case sensitive when comparing equality.
	/// </summary>
	public static SpanComparable AsInvariantCulture(this ReadOnlySpan<char> source)
		=> new(source, StringComparison.InvariantCulture);

}
