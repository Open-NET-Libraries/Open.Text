using System;
using System.Collections.Generic;

namespace Open.Text
{
	public struct StringComparable : IEquatable<StringComparable>, IEquatable<string>
	{
		/// <summary>
		/// Constructs a StringComparable using the provided string and comparison type.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
		public StringComparable(string source, StringComparison type)
		{
			Source = source ?? throw new ArgumentNullException(nameof(source));
			Type = type;
		}

		/// <summary>
		/// The string to use for comparison.
		/// </summary>
		public string Source { get; }

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
			=> Source.Equals(other.Source, Type);

		/// <summary>
		/// Compares <paramref name="obj"/> if it is a string or StringComparable and if the value matches this instance.
		/// </summary>
		/// <returns>true if the value of <paramref name="obj"/> matches; otherwise false. </returns>
		public override bool Equals(object obj)
		{
			return obj switch
			{
				StringComparable sc => Equals(sc),
				string s => Equals(s),
				_ => false
			};
		}

		/// <inheritdoc />
#if NETSTANDARD2_1_OR_GREATER
		public override int GetHashCode()
			=> HashCode.Combine(Source, Type);
#else
		public override int GetHashCode()
		{
			int hashCode = 141257509;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Source);
			hashCode = hashCode * -1521134295 + Type.GetHashCode();
			return hashCode;
		}
#endif

		public static bool operator ==(StringComparable a, StringComparable b) => a.Equals(b);
		public static bool operator !=(StringComparable a, StringComparable b) => !a.Equals(b);

		public static bool operator ==(StringComparable a, string? b) => a.Equals(b);
		public static bool operator !=(StringComparable a, string? b) => !a.Equals(b);

		public static bool operator ==(StringComparable a, ReadOnlySpan<char> b) => a.Equals(b);
		public static bool operator !=(StringComparable a, ReadOnlySpan<char> b) => !a.Equals(b);

	}

	public static class StringComparableExtensions
	{
		/// <inheritdoc cref="AsCaseInsensitive(string)"/>
		/// <summary>
		/// Prepares a string for a specific StringComparison.
		/// </summary>
		public static StringComparable AsComparable(this string source, StringComparison type)
			=> new(source, type);

		/// <summary>
		/// Prepares a string to be case insensitive when comparing equality.
		/// </summary>
		/// <returns>A StringComparable that can be compared (== or !=) against other StringComparables, strings, and ReadOnlySpan&lt;char&gt;.</returns>
		public static StringComparable AsCaseInsensitive(this string source)
			=> new(source, StringComparison.OrdinalIgnoreCase);

		/// <inheritdoc cref="AsCaseInsensitive(string)"/>
		/// <summary>
		/// Prepares a string to be invariant culture and case insensitive when comparing equality.
		/// </summary>
		public static StringComparable AsCaseInsensitiveInvariantCulture(this string source)
			=> new(source, StringComparison.InvariantCultureIgnoreCase);

		/// <inheritdoc cref="AsCaseInsensitive(string)"/>
		/// <summary>
		/// Prepares a string to be invariant culture and case insensitive when comparing equality.
		/// </summary>
		public static StringComparable AsCaseInsensitiveCurrentCulture(this string source)
			=> new(source, StringComparison.CurrentCultureIgnoreCase);

		/// <inheritdoc cref="AsCaseInsensitive(string)"/>
		/// <summary>
		/// Prepares a string to be current culture and case sensitive when comparing equality.
		/// </summary>
		public static StringComparable AsCurrentCulture(this string source)
			=> new(source, StringComparison.CurrentCulture);

		/// <inheritdoc cref="AsCaseInsensitive(string)"/>
		/// <summary>
		/// Prepares a string to be invariant culture and case sensitive when comparing equality.
		/// </summary>
		public static StringComparable AsInvariantCulture(this string source)
			=> new(source, StringComparison.InvariantCulture);

	}
}
