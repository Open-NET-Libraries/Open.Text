using System;

namespace Open.Text;

public static partial class TextExtensions
{

	/// <summary>
	/// Optimized equals for comparing as span vs a string.
	/// </summary>
	/// <param name="source">The source span.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool Equals(this ReadOnlySpan<char> source, string? other, StringComparison stringComparison)
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

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, string?, StringComparison)"/>
	public static bool Equals(this Span<char> source, string? other, StringComparison stringComparison)
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
	public static bool Equals(this Span<char> source, ReadOnlySpan<char> other, StringComparison stringComparison)
	{
		var len = source.Length;
		return len == other.Length && len switch
        {
            0 => true,
            1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
            _ => MemoryExtensions.Equals(source, other, stringComparison)
        };
	}

	/// <inheritdoc cref="Equals(Span{char}, ReadOnlySpan{char}, StringComparison)" />
	public static bool Equals(this Span<char> source, Span<char> other, StringComparison stringComparison)
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
	public static bool Equals(this string? source, ReadOnlySpan<char> other, StringComparison stringComparison)
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

	/// <inheritdoc cref="Equals(string, ReadOnlySpan{char}, StringComparison)" />
	public static bool Equals(this string? source, Span<char> other, StringComparison stringComparison)
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

	/// <inheritdoc cref="TrimmedEquals(string?, ReadOnlySpan{char}, char, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, StringComparison stringComparison = StringComparison.Ordinal)
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

	/// <inheritdoc cref="TrimmedEquals(string?, string?, char, StringComparison)"/>
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

	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="trimChar">The character to trim.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, string?, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, char trimChar, StringComparison stringComparison = StringComparison.Ordinal)
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

	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="trimChar">The character to trim.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, string?, ReadOnlySpan{char}, StringComparison)"/>
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

	/// <summary>
	/// Optimized equals for comparing trimmed string with span.
	/// </summary>
	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The span to compare to.</param>
	/// <param name="trimChars">The characters to trim.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <inheritdoc cref="TrimmedEquals(string?, string?, ReadOnlySpan{char}, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, ReadOnlySpan<char> trimChars, StringComparison stringComparison = StringComparison.Ordinal)
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

	/// <summary>
	/// Optimized equals for comparing a trimmed string with another string.
	/// </summary>
	/// <param name="source">The source string to virtually trim.</param>
	/// <param name="other">The string to compare to.</param>
	/// <param name="trimChars">The characters to trim.</param>
	/// <param name="stringComparison">The string comparison type.</param>
	/// <returns>True if the are contents equal.</returns>
	public static bool TrimmedEquals(this string? source, string? other, ReadOnlySpan<char> trimChars, StringComparison stringComparison = StringComparison.Ordinal)
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
