namespace Open.Text;

/// <summary>
/// Zero-allocation regular expression extensions using ZLinq ValueEnumerable patterns.
/// </summary>
public static class RegexExtensions
{
	/// <summary>
	/// Returns the available matches as StringSegments.
	/// </summary>
	/// <param name="pattern">The pattern to search with.</param>
	/// <param name="input">The string to search.</param>
	/// <returns>A ValueEnumerable of the found segments (zero-allocation when used with foreach or ZLinq).</returns>
	/// <exception cref="System.ArgumentNullException">If the pattern or input is null.</exception>
	public static ValueEnumerable<RegexMatchSegmentEnumerator, StringSegment> AsSegmentsNoAlloc(
		this Regex pattern, string input)
	{
		if (pattern is null) throw new ArgumentNullException(nameof(pattern));
		if (input is null) throw new ArgumentNullException(nameof(input));
		Contract.EndContractBlock();

		return new ValueEnumerable<RegexMatchSegmentEnumerator, StringSegment>(
			new RegexMatchSegmentEnumerator(pattern, input));
	}
}
