namespace Open.Text;

/// <summary>
/// Extension methods for case detection and conversion.
/// </summary>
public static partial class TextExtensions
{
	#region HasUpperCase / HasLowerCase

	/// <summary>
	/// Determines whether the span contains any uppercase characters.
	/// </summary>
	/// <param name="span">The span to check.</param>
	/// <returns><see langword="true"/> if any character is uppercase; otherwise, <see langword="false"/>.</returns>
	public static bool HasUpperCase(this ReadOnlySpan<char> span)
	{
		foreach (char c in span)
		{
			if (char.IsUpper(c)) return true;
		}
		return false;
	}

	/// <summary>
	/// Determines whether the span contains any lowercase characters.
	/// </summary>
	/// <param name="span">The span to check.</param>
	/// <returns><see langword="true"/> if any character is lowercase; otherwise, <see langword="false"/>.</returns>
	public static bool HasLowerCase(this ReadOnlySpan<char> span)
	{
		foreach (char c in span)
		{
			if (char.IsLower(c)) return true;
		}
		return false;
	}

	/// <inheritdoc cref="HasUpperCase(ReadOnlySpan{char})"/>
	public static bool HasUpperCase(this Span<char> span)
		=> HasUpperCase((ReadOnlySpan<char>)span);

	/// <inheritdoc cref="HasLowerCase(ReadOnlySpan{char})"/>
	public static bool HasLowerCase(this Span<char> span)
		=> HasLowerCase((ReadOnlySpan<char>)span);

	/// <summary>
	/// Determines whether the string contains any uppercase characters.
	/// </summary>
	/// <param name="source">The string to check.</param>
	/// <returns><see langword="true"/> if any character is uppercase; otherwise, <see langword="false"/>.</returns>
	public static bool HasUpperCase(this string? source)
		=> source?.AsSpan().HasUpperCase() == true;

	/// <summary>
	/// Determines whether the string contains any lowercase characters.
	/// </summary>
	/// <param name="source">The string to check.</param>
	/// <returns><see langword="true"/> if any character is lowercase; otherwise, <see langword="false"/>.</returns>
	public static bool HasLowerCase(this string? source)
		=> source?.AsSpan().HasLowerCase() == true;

	/// <summary>
	/// Determines whether the segment contains any uppercase characters.
	/// </summary>
	/// <param name="segment">The segment to check.</param>
	/// <returns><see langword="true"/> if any character is uppercase; otherwise, <see langword="false"/>.</returns>
	public static bool HasUpperCase(this StringSegment segment)
		=> segment.HasValue && segment.AsSpan().HasUpperCase();

	/// <summary>
	/// Determines whether the segment contains any lowercase characters.
	/// </summary>
	/// <param name="segment">The segment to check.</param>
	/// <returns><see langword="true"/> if any character is lowercase; otherwise, <see langword="false"/>.</returns>
	public static bool HasLowerCase(this StringSegment segment)
		=> segment.HasValue && segment.AsSpan().HasLowerCase();

	#endregion

	#region ToUpperInvariant / ToLowerInvariant

	/// <summary>
	/// Returns a copy of this segment converted to uppercase using the invariant culture.
	/// </summary>
	/// <remarks>
	/// This method is optimized to avoid allocation if the segment is already uppercase.
	/// </remarks>
	/// <param name="segment">The segment to convert.</param>
	/// <returns>The uppercase equivalent of the segment.</returns>
	/// <exception cref="ArgumentNullException">The segment has no value.</exception>
	public static StringSegment ToUpperInvariant(this StringSegment segment)
	{
		if (!segment.HasValue)
			throw new ArgumentNullException(nameof(segment), MustBeSegmentWithValue);

		int len = segment.Length;
		if (len == 0) return segment;

		// Does the buffer exactly match the segment? Then just call ToUpper on the string.
		if (len == segment.Buffer.Length)
			return segment.Buffer.ToUpperInvariant();

		// Fast path: check if already uppercase
		if (!segment.HasLowerCase()) return segment;

#if NETSTANDARD2_0
		return Core(segment);

		static string Core(StringSegment seg)
		{
			var sb = new System.Text.StringBuilder(seg.Length);
			foreach (char c in seg.AsSpan())
				sb.Append(char.ToUpperInvariant(c));
			return sb.ToString();
		}
#else
		return string.Create(len, segment,
			static (dest, seg) => seg.AsSpan().ToUpperInvariant(dest));
#endif
	}

	/// <summary>
	/// Returns a copy of this segment converted to lowercase using the invariant culture.
	/// </summary>
	/// <remarks>
	/// This method is optimized to avoid allocation if the segment is already lowercase.
	/// </remarks>
	/// <param name="segment">The segment to convert.</param>
	/// <returns>The lowercase equivalent of the segment.</returns>
	/// <exception cref="ArgumentNullException">The segment has no value.</exception>
	public static StringSegment ToLowerInvariant(this StringSegment segment)
	{
		if (!segment.HasValue)
			throw new ArgumentNullException(nameof(segment), MustBeSegmentWithValue);

		int len = segment.Length;
		if (len == 0) return segment;

		// Does the buffer exactly match the segment? Then just call ToLower on the string to avoid copying.
		if (len == segment.Buffer.Length)
#pragma warning disable CA1308 // Normalize strings to uppercase
			return segment.Buffer.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

		// Fast path: check if already lowercase
		if (!segment.HasUpperCase()) return segment;

#if NETSTANDARD2_0
		return Core(segment);

		static string Core(StringSegment seg)
		{
			var sb = new System.Text.StringBuilder(seg.Length);
			foreach (char c in seg.AsSpan())
				sb.Append(char.ToLowerInvariant(c));
			return sb.ToString();
		}
#else
		return string.Create(len, segment,
			static (dest, seg) => seg.AsSpan().ToLowerInvariant(dest));
#endif
	}

	#endregion

	#region ToUpper / ToLower
	/// <summary>
	/// Returns a copy of this segment converted to uppercase using the specified culture.
	/// </summary>
	/// <param name="segment">The segment to convert.</param>
	/// <param name="culture">The culture to use for the conversion.</param>
	/// <returns>The uppercase equivalent of the segment.</returns>
	/// <exception cref="ArgumentNullException">The segment has no value or culture is null.</exception>
	public static StringSegment ToUpper(this StringSegment segment, CultureInfo? culture = null)
	{
		if (!segment.HasValue)
			throw new ArgumentNullException(nameof(segment), MustBeSegmentWithValue);

		int len = segment.Length;
		if (len == 0) return segment;

		culture ??= CultureInfo.CurrentCulture;

		// Does the buffer exactly match the segment? Then just call directly on the string.
		if (len == segment.Buffer.Length)
			return segment.Buffer.ToUpper(culture);

		// Fast path: check if already uppercase
		if (!segment.HasLowerCase()) return segment;

#if NETSTANDARD2_0
		return Core(segment, culture);

		static string Core(StringSegment seg, CultureInfo cult)
		{
			var sb = new System.Text.StringBuilder(seg.Length);
			foreach (char c in seg.AsSpan())
				sb.Append(char.ToUpper(c, cult));
			return sb.ToString();
		}
#else
		return string.Create(len, (segment, culture),
			static (dest, state) => state.segment.AsSpan().ToUpper(dest, state.culture));
#endif
	}

	/// <summary>
	/// Returns a copy of this segment converted to lowercase using the specified culture.
	/// </summary>
	/// <param name="segment">The segment to convert.</param>
	/// <param name="culture">The culture to use for the conversion.</param>
	/// <returns>The lowercase equivalent of the segment.</returns>
	/// <exception cref="ArgumentNullException">The segment has no value or culture is null.</exception>
	public static StringSegment ToLower(this StringSegment segment, CultureInfo? culture = null)
	{
		if (!segment.HasValue)
			throw new ArgumentNullException(nameof(segment), MustBeSegmentWithValue);

		int len = segment.Length;
		if (len == 0) return segment;

		culture ??= CultureInfo.CurrentCulture;

		// Does the buffer exactly match the segment? Then just call directly on the string.
		if (len == segment.Buffer.Length)
			return segment.Buffer.ToLower(culture);

		// Fast path: check if already lowercase
		if (!segment.HasUpperCase()) return segment;

#if NETSTANDARD2_0
		return Core(segment, culture);

		static string Core(StringSegment seg, CultureInfo cult)
		{
			var sb = new System.Text.StringBuilder(seg.Length);
			foreach (char c in seg.AsSpan())
				sb.Append(char.ToLower(c, cult));
			return sb.ToString();
		}
#else
		return string.Create(len, (segment, culture),
			static (dest, state) => state.segment.AsSpan().ToLower(dest, state.culture));
#endif
	}

	#endregion
}
