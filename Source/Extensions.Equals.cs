namespace Open.Text;

public static partial class TextExtensions
{
	/// <summary>
	/// Compares a sequence of characters with another.
	/// </summary>
	/// <param name="source">The source sequence.</param>
	/// <param name="other">The other to compare to.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <returns><see langword="true"/> if the are contents equal; otherwise <see langword="false"/></returns>
	public static bool Equals(this ReadOnlySpan<char> source, StringSegment other, StringComparison comparisonType)
	{
		if (!other.HasValue) return false;
		int len = source.Length;
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
		int len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => MemoryExtensions.Equals(source, other.AsSpan(), comparisonType)
		};
	}

	// Cover the edge case of string null:

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool Equals(this Span<char> source, string? other, StringComparison comparisonType)
		=> other is not null && source.Equals(other.AsSpan(), comparisonType);

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool Equals(this ReadOnlySpan<char> source, string? other, StringComparison comparisonType)
		=> other is not null && source.Equals(other.AsSpan(), comparisonType);

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool Equals(this Span<char> source, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		int len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => MemoryExtensions.Equals(source, other, comparisonType)
		};
	}

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool Equals(this string? source, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		if (source is null) return false;
		int len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => source.AsSpan().Equals(other, comparisonType),
		};
	}

	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool Equals(this StringSegment source, ReadOnlySpan<char> other, StringComparison comparisonType)
	{
		if (!source.HasValue) return false;
		int len = source.Length;
		return len == other.Length && len switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => source[0] == other[0],
			_ => source.AsSpan().Equals(other, comparisonType),
		};
	}

	/// <remarks>Use <see cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool SequenceEqual(this ReadOnlySpan<char> source, StringSegment other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(Span{char}, StringSegment, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool SequenceEqual(this Span<char> source, StringSegment other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(Span{char}, ReadOnlySpan{char}, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool SequenceEqual(this Span<char> source, Span<char> other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(string?, ReadOnlySpan{char}, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool SequenceEqual(this string? source, ReadOnlySpan<char> other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <remarks>Use <see cref="Equals(StringSegment, ReadOnlySpan{char}, StringComparison)"/> for varying case sensitivity.</remarks>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
	public static bool SequenceEqual(this StringSegment source, ReadOnlySpan<char> other)
		=> Equals(source, other, StringComparison.Ordinal);

	/// <inheritdoc cref="TrimmedEquals(string?, ReadOnlySpan{char}, char, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, ReadOnlySpan<char> other, StringComparison comparisonType = StringComparison.Ordinal)
		=> source?.AsSpan().Trim().Equals(other, comparisonType) == true;

	/// <inheritdoc cref="TrimmedEquals(string?, StringSegment, char, StringComparison)"/>
	public static bool TrimmedEquals(this string? source, StringSegment other, StringComparison comparisonType = StringComparison.Ordinal)
		=> source is null ? !other.HasValue : other.HasValue && source.AsSpan().Trim().Equals(other, comparisonType);

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
		bool otherHasValue = !other.HasValue;
		if (source is null) return otherHasValue;
		if (otherHasValue) return false;
		int slen = source.Length, olen = other.Length;
		if (slen < olen) return false;
		ReadOnlySpan<char> span = source.AsSpan().Trim(trimChar);
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
	/// Compares a sequence of characters with leading and trailing whitespace removed with another.
	/// </summary>
	/// <remarks>Only "trims" the source and not the other used to compare agains.</remarks>
	/// <param name="source">The source sequence.</param>
	/// <param name="other">The other to compare to.</param>
	/// <param name="trimChars">The characters to trim.</param>
	/// <param name="comparisonType">The string comparison type.</param>
	/// <inheritdoc cref="Equals(ReadOnlySpan{char}, StringSegment, StringComparison)"/>
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
		ReadOnlySpan<char> trimmed = source.AsSpan().Trim(trimChars);
		slen = trimmed.Length;
		return slen == olen && slen switch
		{
			0 => true,
			1 when comparisonType == StringComparison.Ordinal => trimmed[0] == other[0],
			_ => trimmed.Equals(other.AsSpan(), comparisonType),
		};
	}
}
