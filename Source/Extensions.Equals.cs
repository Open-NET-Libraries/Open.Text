using System;

namespace Open.Text;

public static partial class Extensions
{

	/// <summary>
	/// Optimized equals for comparing as span vs a string.
	/// </summary>
	/// <param name="source">The source span.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this in ReadOnlySpan<char> source, string? other, StringComparison stringComparison)
	{
		if (other is null) return false;
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => source.Equals(other.AsSpan(), stringComparison),
        };
	}

	/// <inheritdoc cref="Equals(in ReadOnlySpan{char}, string?, StringComparison)"/>
	public static bool Equals(this in Span<char> source, string? other, StringComparison stringComparison)
	{
		if (other is null) return false;
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => MemoryExtensions.Equals(source, other.AsSpan(), stringComparison)
        };
	}

	/// <summary>
	/// Optimized equals for comparing spans.
	/// </summary>
	/// <param name="source">The source span.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this in Span<char> source, in ReadOnlySpan<char> other, StringComparison stringComparison)
	{
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => MemoryExtensions.Equals(source, other, stringComparison)
        };
	}

	/// <inheritdoc cref="Equals(in Span{char}, in ReadOnlySpan{char}, StringComparison)" />
	public static bool Equals(this in Span<char> source, in Span<char> other, StringComparison stringComparison)
	{
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => MemoryExtensions.Equals(source, other, stringComparison)
        };
	}

	/// <summary>
	/// Optimized equals for comparing as string to a span.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this string? source, in ReadOnlySpan<char> other, StringComparison stringComparison)
	{
		if (source is null) return false;
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => source.AsSpan().Equals(other, stringComparison),
        };
	}

	/// <inheritdoc cref="Equals(string, in ReadOnlySpan{char}, StringComparison)" />
	public static bool Equals(this string? source, in Span<char> other, StringComparison stringComparison)
	{
		if (source is null) return false;
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => source.AsSpan().Equals(other, stringComparison),
        };
	}

	/// <summary>
	/// Optimized equals for comparing trimmed string with span.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool TrimmedEquals(this string? source, in ReadOnlySpan<char> other, StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source is null) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim();
		slen = span.Length;
		return slen == olen && slen switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => span[0] == other[0],
            _ => span.Equals(other, stringComparison),
        };
	}

	/// <summary>
	/// Optimized equals for comparing a trimmed string with another string.
	/// </summary>
	/// <param name="other">The string to compare to.</param>
	/// <inheritdoc cref="TrimmedEquals(string, in ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, string? other, StringComparison stringComparison = StringComparison.Ordinal)
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
            1 when stringComparison == StringComparison.Ordinal => span[0] == other[0],
            _ => span.Equals(other.AsSpan(), stringComparison),
        };
	}

	/// <param name="trimChar">The character to trim.</param>
	/// <inheritdoc cref="TrimmedEquals(string, in ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, in ReadOnlySpan<char> other, char trimChar, StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source is null) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim(trimChar);
		slen = span.Length;
		return slen == olen && slen switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => span[0] == other[0],
            _ => span.Equals(other, stringComparison),
        };
	}

	/// <param name="trimChar">The character to trim.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, string?, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, string? other, char trimChar, StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source is null) return other is null;
		if (other is null) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim(trimChar);
		slen = span.Length;
		return slen == olen && slen switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => span[0] == other[0],
            _ => span.Equals(other.AsSpan(), stringComparison),
        };
	}

	/// <param name="trimChars">The characters to trim.</param>
	/// <inheritdoc cref="TrimmedEquals(string, in ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, in ReadOnlySpan<char> other, in ReadOnlySpan<char> trimChars, StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source is null) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim(trimChars);
		slen = span.Length;
		return slen == olen && slen switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => span[0] == other[0],
            _ => span.Equals(other, stringComparison),
        };
	}

	/// <param name="trimChars">The characters to trim.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, string?, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, string? other, in ReadOnlySpan<char> trimChars, StringComparison stringComparison = StringComparison.Ordinal)
	{
		if (source is null) return other is null;
		if (other is null) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		var span = source.AsSpan().Trim(trimChars);
		slen = span.Length;
		return slen == olen && slen switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => span[0] == other[0],
            _ => span.Equals(other.AsSpan(), stringComparison),
        };
	}
}
