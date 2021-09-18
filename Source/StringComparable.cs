using System;
using System.Collections.Generic;

namespace Open.Text
{
	public struct StringComparable : IEquatable<StringComparable>, IEquatable<string>
	{
		public StringComparable(string source, StringComparison type)
		{
			Source = source ?? throw new ArgumentNullException(nameof(source));
			Type = type;
		}

		public string Source { get; }
		public StringComparison Type { get; }

		public int Length => Source.Length;

		public bool Equals(string? other)
			=> Source.Equals(other, Type);

		public bool Equals(ReadOnlySpan<char> other)
			=> Source.Equals(other, Type);

		public bool Equals(StringComparable other)
			=> Source.Equals(other.Source, Type);

		public override bool Equals(object obj)
		{
			return obj switch
			{
				StringComparable sc => Equals(sc),
				string s => Equals(s),
				_ => false
			};
		}

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
		/// <param name="type">The type of StrinComparison to perform.</param>
		public static StringComparable AsComparable(this string source, StringComparison type)
			=> new(source, type);

		/// <summary>
		/// Prepares a string to be case insensitive when comparing equality.
		/// </summary>
		/// <param name="source">The source string.</param>
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
