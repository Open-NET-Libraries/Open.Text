namespace Open.Text;
public static partial class TextExtensions
{
	/// <inheritdoc cref="string.IndexOf(char)"/>
	public static int IndexOf(this ReadOnlySpan<char> source, char search, StringComparison comparisonType)
	{
		Func<char, char> toUpper;
		switch (comparisonType)
		{
			case StringComparison.Ordinal:
			case StringComparison.CurrentCulture:
			case StringComparison.InvariantCulture:
				return source.IndexOf(search);
			case StringComparison.CurrentCultureIgnoreCase:
				toUpper = static c => char.ToUpper(c, CultureInfo.CurrentCulture);
				break;
			case StringComparison.InvariantCultureIgnoreCase:
				toUpper = char.ToUpperInvariant;
				break;
			case StringComparison.OrdinalIgnoreCase:
				toUpper = char.ToUpper;
				break;
			default:
				throw new ArgumentException("Invalid comparison type.", nameof(comparisonType));
		}

		var searchUpper = toUpper(search);
		if (searchUpper == search)
			return source.IndexOf(search);

		for (var i = 0; i < source.Length; i++)
		{
			var c = source[i];
			if (c == search) return i;
			if (toUpper(c) == searchUpper)
				return i;
		}

		return -1;
	}

	/// <inheritdoc cref="string.IndexOf(char, int)"/>
	public static int IndexOf(this ReadOnlySpan<char> source, char value, int startIndex, StringComparison comparisonType)
	{
		if (startIndex >= source.Length)
			throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Must be less than the length of the source.");
		var i = source.Slice(startIndex).IndexOf(value, comparisonType);
		return i == -1 ? -1 : i + startIndex;
	}

	/// <inheritdoc cref="string.LastIndexOf(char)"/>
	public static int LastIndexOf(this ReadOnlySpan<char> source, char value, StringComparison comparisonType)
	{
		Func<char, char> toUpper;
		switch (comparisonType)
		{
			case StringComparison.Ordinal:
			case StringComparison.CurrentCulture:
			case StringComparison.InvariantCulture:
				return source.LastIndexOf(value);
			case StringComparison.CurrentCultureIgnoreCase:
				toUpper = static c => char.ToUpper(c, CultureInfo.CurrentCulture);
				break;
			case StringComparison.InvariantCultureIgnoreCase:
				toUpper = char.ToUpperInvariant;
				break;
			case StringComparison.OrdinalIgnoreCase:
				toUpper = char.ToUpper;
				break;
			default:
				throw new ArgumentException("Invalid comparison type.", nameof(comparisonType));
		}

		var searchUpper = toUpper(value);
		if (searchUpper == value)
			return source.LastIndexOf(value);

		for (var i = source.Length - 1; i >= 0; i--)
		{
			var c = source[i];
			if (c == value) return i;
			if (toUpper(c) == searchUpper)
				return i;
		}

		return -1;
	}

	/// <inheritdoc cref="string.LastIndexOf(char)"/>
	public static int LastIndexOf(this string source, char value, StringComparison comparisonType)
		=> source.AsSpan().LastIndexOf(value, comparisonType);

	/// <inheritdoc cref="string.LastIndexOf(char)"/>
	public static int LastIndexOf(this StringSegment source, char value, StringComparison comparisonType)
		=> source.AsSpan().LastIndexOf(value, comparisonType);

	/// <inheritdoc cref="StringSegment.IndexOf(char)"/>
	public static int IndexOf(this StringSegment source, char value, StringComparison comparisonType)
		=> source.HasValue ? source.AsSpan().IndexOf(value, comparisonType) : -1;

	/// <inheritdoc cref="StringSegment.IndexOf(char)"/>
	public static int IndexOf(this StringSegment source, char value, int startIndex, StringComparison comparisonType)
		=> source.HasValue ? source.AsSpan().IndexOf(value, startIndex, comparisonType) : -1;

#if NETSTANDARD2_0
	/// <inheritdoc cref="StringSegment.IndexOf(char)"/>
	public static int IndexOf(this string source, char value, StringComparison comparisonType)
		=> source.AsSpan().IndexOf(value, comparisonType);

	/// <inheritdoc cref="StringSegment.IndexOf(char)"/>
	public static int LastIndexOf(this string source, char value)
		=> source.AsSpan().LastIndexOf(value);
#endif

	/// <summary>
	/// Note: The NET Standard 2.0 version of this extension allocates strings to call the string.IndexOf method.
	/// This non-extension version will (if possible) avoid allocating strings to find a sequence.
	/// The one improved case is for <see cref="StringComparison.OrdinalIgnoreCase"/>.
	/// </summary>
#if NETSTANDARD2_0
	public static int IndexOf(
		ReadOnlySpan<char> source,
		ReadOnlySpan<char> sequence,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		// This is a weird decision, but must be this way to maintain parity with other methods.
		if (sequence.IsEmpty) return 0;

		switch (comparisonType)
		{
			case StringComparison.OrdinalIgnoreCase:
				break;

			case StringComparison.Ordinal:
				return source.IndexOf(sequence);

			// Use the existing extension as either it will work fine, or no way to avoid allocation.
			case StringComparison.CurrentCulture:
			case StringComparison.InvariantCulture:
			case StringComparison.CurrentCultureIgnoreCase:
			case StringComparison.InvariantCultureIgnoreCase:
				return source.IndexOf(sequence, comparisonType);

			default:
				throw new ArgumentException("Invalid comparison type.", nameof(comparisonType));
		}

		int sequenceLength = sequence.Length;
		int sourceLength = source.Length;
		if (sourceLength == 0) return -1;
		if (sequenceLength > sourceLength) return -1;

		// Note: This is the only case where we can potentially avoid allocating a string.
		int max = sourceLength - sequenceLength;
		int i = -1;
		while (max > i++)
		{
			var span = source.Slice(i, sequenceLength);
			// Note: in rare cases, this will still convert to strings for comparison and could allocate.
			if (span.Equals(sequence, StringComparison.OrdinalIgnoreCase))
				return i;
		}

		return -1;
	}
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf(
		ReadOnlySpan<char> source,
		ReadOnlySpan<char> sequence,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.IndexOf(sequence, comparisonType);
#endif

#if NET6_0_OR_GREATER
#else
	/// <summary>
	/// Reports the zero-based index of the first occurrence
	/// of the specified <paramref name="sequence"/>.
	/// </summary>
	/// <param name="source">The source sequence to seek through.</param>
	/// <param name="sequence">The sequence to look for.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
	/// <exception cref="ArgumentOutOfRangeException">If start index is less than zero.</exception>
	/// <remarks>
	/// Note: The NET Standard 2.x version of this extension allocates strings to call the string.LastIndexOf method.
	/// This non-extension version will (if possible) avoid allocating strings to find a sequence.
	/// The one improved case is for <see cref="StringComparison.OrdinalIgnoreCase"/>.
	/// </remarks>
	public static int LastIndexOf(
		this ReadOnlySpan<char> source,
		ReadOnlySpan<char> sequence,
		StringComparison comparisonType)
	{
		// This is a weird decision, but must be this way to maintain parity with other methods.
		if (sequence.IsEmpty)
			return source.Length;

		switch (comparisonType)
		{
			case StringComparison.OrdinalIgnoreCase:
				break;

			case StringComparison.Ordinal:
				return source.LastIndexOf(sequence);

			// These cases are not supported by the NET Standard 2.x version and will have to allocate.
			case StringComparison.CurrentCulture:
			case StringComparison.InvariantCulture:
			case StringComparison.CurrentCultureIgnoreCase:
			case StringComparison.InvariantCultureIgnoreCase:
				return source.ToString().LastIndexOf(sequence.ToString(), StringComparison.CurrentCulture);

			default:
				throw new ArgumentException("Invalid comparison type.", nameof(comparisonType));
		}

		int sequenceLength = sequence.Length;
		int sourceLength = source.Length;
		if (sourceLength == 0) return -1;
		if (sequenceLength > sourceLength) return -1;

		// Note: This is the only case where we can potentially avoid allocating a string.
		int max = sourceLength - sequenceLength;
		do
		{
			var span = source.Slice(max, sequenceLength);
			// Note: in rare cases, this will still convert to strings for comparison and could allocate.
			if (span.Equals(sequence, StringComparison.OrdinalIgnoreCase))
				return max;
		}
		while (max-- > 0);

		return -1;
	}
#endif

#pragma warning disable CS1587 // XML comment is not placed on a valid language element
	/// <summary>
	/// Reports the zero-based index of the first occurrence
	/// of the specified <paramref name="sequence"/>.
	/// </summary>
	/// <param name="source">The source sequence to seek through.</param>
	/// <param name="sequence">The sequence to look for.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
	/// <exception cref="ArgumentOutOfRangeException">If start index is less than zero.</exception>
#if NET6_0_OR_GREATER
#else
	/// <inheritdoc cref="LastIndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison)"/>
#endif
	public static int LastIndexOf(this StringSegment source, ReadOnlySpan<char> sequence, StringComparison comparisonType)
		=> source.AsSpan().LastIndexOf(sequence, comparisonType);
#pragma warning restore CS1587 // XML comment is not placed on a valid language element

	/// <inheritdoc cref="LastIndexOf(StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static int LastIndexOf(this string source, StringSegment sequence, StringComparison comparisonType)
		=> sequence.HasValue ? source.AsSpan().LastIndexOf(sequence.AsSpan(), comparisonType) : -1;

	/// <inheritdoc cref="LastIndexOf(StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static int LastIndexOf(this string source, ReadOnlySpan<char> sequence, StringComparison comparisonType)
		=> source.AsSpan().LastIndexOf(sequence, comparisonType);

	/// <inheritdoc cref="LastIndexOf(StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static int LastIndexOf(this ReadOnlySpan<char> source, StringSegment sequence, StringComparison comparisonType)
		=> sequence.HasValue ? source.LastIndexOf(sequence.AsSpan(), comparisonType) : -1;

	/// <inheritdoc cref="LastIndexOf(StringSegment, ReadOnlySpan{char}, StringComparison)"/>
	public static int LastIndexOf(this StringSegment source, StringSegment sequence, StringComparison comparisonType)
		=> sequence.HasValue ? source.AsSpan().LastIndexOf(sequence.AsSpan(), comparisonType) : -1;

	/// <summary>
	/// Reports the zero-based index of the first occurrence
	/// of the specified <paramref name="sequence"/>
	/// starting from the <paramref name="startIndex"/>.
	/// </summary>
	/// <param name="source">The source sequence to seek through.</param>
	/// <param name="sequence">The sequence to look for.</param>
	/// <param name="startIndex">The search starting position.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
	/// <exception cref="ArgumentOutOfRangeException">If start index is less than zero.</exception>
	public static int IndexOf(
		this ReadOnlySpan<char> source,
		ReadOnlySpan<char> sequence,
		int startIndex,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (startIndex < 0)
			throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Must be at least zero.");

		if (sequence.IsEmpty)
			return 0;

		if (startIndex == 0)
			return IndexOf(source, sequence, comparisonType);

		var span = source.Slice(startIndex);
		var index = span.IndexOf(sequence, comparisonType);
		return index == -1 ? -1 : index + startIndex;
	}

	/// <inheritdoc cref="IndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, int, StringComparison)"/>
	public static int IndexOf(
		this ReadOnlySpan<char> source,
		StringSegment sequence,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> sequence.HasValue ? source.IndexOf(sequence.AsSpan(), startIndex, comparisonType) : -1;

	/// <inheritdoc cref="IndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, int, StringComparison)"/>
	public static int IndexOf(
		this StringSegment source,
		StringSegment sequence,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.AsSpan().IndexOf(sequence, startIndex, comparisonType);

	/// <inheritdoc cref="IndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, int, StringComparison)"/>
	public static int IndexOf(
		this string source,
		ReadOnlySpan<char> sequence,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.AsSpan().IndexOf(sequence, startIndex, comparisonType);

	/// <inheritdoc cref="IndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, int, StringComparison)"/>
	public static int IndexOf(
		this string source,
		StringSegment sequence,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.AsSpan().IndexOf(sequence, startIndex, comparisonType);

	/// <inheritdoc cref="IndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, int, StringComparison)"/>
	public static int IndexOf(
		this StringSegment source,
		ReadOnlySpan<char> sequence,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.AsSpan().IndexOf(sequence, startIndex, comparisonType);

	/// <summary>
	/// Reports the zero-based index of the first occurrence
	/// of the specified <paramref name="sequence"/>.
	/// </summary>
	/// <inheritdoc cref="IndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, int, StringComparison)"/>
	public static int IndexOf(
		this StringSegment source,
		StringSegment sequence,
		StringComparison comparisonType)
		=> sequence.HasValue ? IndexOf(source.AsSpan(), sequence.AsSpan(), comparisonType) : -1;

	/// <inheritdoc cref="IndexOf(StringSegment, StringSegment, StringComparison)"/>
	public static int IndexOf(
		this string source,
		ReadOnlySpan<char> sequence,
		StringComparison comparisonType)
		=> IndexOf(source.AsSpan(), sequence, comparisonType);

	/// <inheritdoc cref="IndexOf(StringSegment, StringSegment, StringComparison)"/>
	public static int IndexOf(
		this StringSegment source,
		ReadOnlySpan<char> sequence,
		StringComparison comparisonType)
		=> IndexOf(source.AsSpan(), sequence, comparisonType);

	/// <inheritdoc cref="IndexOf(StringSegment, StringSegment, StringComparison)"/>
	public static int IndexOf(
		this ReadOnlySpan<char> source,
		StringSegment sequence,
		StringComparison comparisonType)
		=> sequence.HasValue ? IndexOf(source, sequence.AsSpan(), comparisonType) : -1;

	/// <inheritdoc cref="IndexOf(StringSegment, StringSegment, StringComparison)"/>
	public static int IndexOf(
		this string source,
		StringSegment sequence,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> sequence.HasValue ? IndexOf(source.AsSpan(), sequence.AsSpan(), comparisonType) : -1;

	/// <summary>
	/// Checks if the <paramref name="value"/> is contained
	/// within the <paramref name="source"/> using the <paramref name="comparisonType"/>.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the <paramref name="value"/> is contained;
	/// otherwise <see langword="false"/>.
	/// </returns>
	public static bool Contains(
		this ReadOnlySpan<char> source,
		char value, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, value, comparisonType) != -1;

	/// <inheritdoc cref="Contains(ReadOnlySpan{char}, char, StringComparison)"/>
	public static bool Contains(
		this StringSegment source,
		char value, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, value, comparisonType) != -1;

#if NETSTANDARD2_0
	/// <inheritdoc cref="Contains(ReadOnlySpan{char}, char, StringComparison)"/>
	public static bool Contains(this string source, char value, StringComparison comparisonType)
		=> source.IndexOf(value, comparisonType) != -1;
#endif

	/// <summary>
	/// Checks if the <paramref name="sequence"/> is contained
	/// within the <paramref name="source"/> using the <paramref name="comparisonType"/>.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the <paramref name="sequence"/> is contained;
	/// otherwise <see langword="false"/>.
	/// </returns>
	public static bool Contains(
		this StringSegment source,
		StringSegment sequence, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, sequence, comparisonType) != -1;

	/// <inheritdoc cref="Contains(StringSegment, StringSegment, StringComparison)"/>
	public static bool Contains(
	this StringSegment source,
		ReadOnlySpan<char> sequence, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, sequence, comparisonType) != -1;

	/// <inheritdoc cref="Contains(StringSegment, StringSegment, StringComparison)"/>
	public static bool Contains(
		this ReadOnlySpan<char> source,
		StringSegment sequence, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, sequence, comparisonType) != -1;

	/// <inheritdoc cref="Contains(StringSegment, StringSegment, StringComparison)"/>
	public static bool Contains(
		this string source,
		StringSegment sequence, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, sequence, comparisonType) != -1;

	/// <inheritdoc cref="Contains(StringSegment, StringSegment, StringComparison)"/>
	public static bool Contains(
		this string source,
		ReadOnlySpan<char> sequence, StringComparison comparisonType = StringComparison.Ordinal)
		=> IndexOf(source, sequence, comparisonType) != -1;
}
