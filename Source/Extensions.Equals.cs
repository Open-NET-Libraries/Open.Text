using Microsoft.Extensions.Primitives;
using System;

namespace Open.Text;

public static partial class TextExtensions
{
	/// <summary>
	/// Optimized equals for comparing as span vs a string.
	/// </summary>
	/// <param name="source">The source span.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this ReadOnlySpan<char> source, StringSegment other, StringComparison comparisonType)
	{
		if (!other.HasValue) return false;
		var len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => source.Equals(other.AsSpan(), comparisonType),
		};
	}

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool Equals(this Span<char> source, StringSegment other, StringComparison comparisonType)
	{
		if (!other.HasValue) return false;
		var len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => MemoryExtensions.Equals(source, other.AsSpan(), comparisonType)
		};
	}

	/// <summary>
	/// Optimized equals for comparing spans.
	/// </summary>
	/// <param name="source">The source span.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this Span<char> source, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		var len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => MemoryExtensions.Equals(source, other, comparisonType)
		};
	}

	/// <summary>
	/// Optimized equals for comparing as string to a span.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this string? source, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		if (source is null) return false;
		var len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => source.AsSpan().Equals(other, comparisonType),
		};
	}

	/// <inheritdoc cref="Equals(string, ReadOnlySpan{char}, StringComparison)" />
	public static bool Equals(this StringSegment source, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		if (!source.HasValue) return false;
		var len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => source.AsSpan().Equals(other, comparisonType),
		};
	}

	/// <remarks>Use <see cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)" />
	public static bool SequenceEqual(this ReadOnlySpan<char> source, StringSegment other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(Span{char}, StringSegment, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(Span{char}, StringSegment, StringComparison)" />
	public static bool SequenceEqual(this Span<char> source, StringSegment other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(Span{char}, ReadOnlySpan{char}, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(Span{char}, ReadOnlySpan{char}, StringComparison)" />
	public static bool SequenceEqual(this Span<char> source, Span<char> other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(string?, ReadOnlySpan{char}, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(string, ReadOnlySpan{char}, StringComparison)" />
	public static bool SequenceEqual(this string? source, ReadOnlySpan<char> other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(StringSegment, ReadOnlySpan{char}, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(StringSegment, ReadOnlySpan{char}, StringComparison)" />
	public static bool SequenceEqual(this StringSegment source, ReadOnlySpan<char> other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <inheritdoc cref="TrimmedEquals(string?, ReadOnlySpan{char}, char, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, StringComparison comparisonType = StringComparison.Ordinal)
		=> source is not null
		&& source.Length >= other.Length
		&& source.AsSpan().Trim().Equals(other, comparisonType);

	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, char, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, StringSegment other, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) return !other.HasValue;
		if (!other.HasValue) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.Trim();
		slen = span.Length;
		return slen == olen && slen switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => span[0] == other[0],
			_ => span.Equals(other, comparisonType),
		};
	}

	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, char, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, string? other, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) return other is null;
		if (other is null) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim();
		slen = span.Length;
		return slen == olen && slen switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => span[0] == other[0],
			_ => span.Equals(other.AsSpan(), comparisonType),
		};
	}

	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, char, StringComparison)"/>
	public static bool TrimmedEquals(this StringSegment source, StringSegment other, StringComparison comparisonType = StringComparison.Ordinal)
		=> source.HasValue
		 ? other.HasValue
			&& source.Length >= other.Length
			&& source.Trim().Equals(other, comparisonType)
		 : !other.HasValue;

	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, char, StringComparison)"/>
	public static bool TrimmedEquals(this StringSegment source, ReadOnlySpan<char> other, StringComparison comparisonType = StringComparison.Ordinal)
		=> source.HasValue
		&& source.Length >= other.Length
		&& source.Trim().Equals(other, comparisonType);

	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="trimChar">The character to trim.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, char trimChar, StringComparison comparisonType = StringComparison.Ordinal)
		=> source is not null
		&& source.Length >= other.Length
		&& source.AsSpan().Trim(trimChar).Equals(other, comparisonType);

	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="trimChar">The character to trim.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, StringSegment other, char trimChar, StringComparison comparisonType = StringComparison.Ordinal)
	{
		var otherHasValue = !other.HasValue;
		if (source is null) return otherHasValue;
		if (otherHasValue) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim(trimChar);
		slen = span.Length;
		return slen == olen && slen switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => span[0] == other[0],
			_ => span.Equals(other.AsSpan(), comparisonType),
		};
	}

	/// <summary>
	/// Optimized equals for comparing trimmed string with span.
	/// </summary>
	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="trimChars">The characters to trim.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <inheritdoc cref="TrimmedEquals(StringSegment, StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this StringSegment source, ReadOnlySpan<char> other, ReadOnlySpan<char> trimChars, StringComparison comparisonType = StringComparison.Ordinal)
		=> source.HasValue
		&& source.Length >= other.Length
		&& source.AsSpan().Trim(trimChars).Equals(other, comparisonType);

	/// <inheritdoc cref="TrimmedEquals(StringSegment, ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, ReadOnlySpan<char> trimChars, StringComparison comparisonType = StringComparison.Ordinal)
		=> source is not null
		&& source.Length >= other.Length
		&& source.AsSpan().Trim(trimChars).Equals(other, comparisonType);

	/// <summary>
	/// Optimized equals for comparing a trimmed string with another string.
	/// </summary>
	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="trimChars">The characters to trim.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool TrimmedEquals(this StringSegment source, StringSegment other, ReadOnlySpan<char> trimChars, StringComparison comparisonType = StringComparison.Ordinal)
		=> source.HasValue
		 ? other.HasValue
			&& source.Length >= other.Length
			&& source.Trim(trimChars).Equals(other, comparisonType)
		 : !other.HasValue;

	/// <inheritdoc cref="TrimmedEquals(StringSegment, StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, StringSegment other, ReadOnlySpan<char> trimChars, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (source is null) return !other.HasValue;
		if (!other.HasValue) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var trimmed = source.AsSpan().Trim(trimChars);
		slen = trimmed.Length;
		return slen == olen && slen switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => trimmed[0] == other[0],
			_ => trimmed.Equals(other.AsSpan(), comparisonType),
		};
	}
}
