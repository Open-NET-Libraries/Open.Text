namespace Open.Text;

/// <summary>
/// A set of commonly used regular expression patterns.
/// </summary>
public static class RegexPatterns
{
	/// <summary>
	/// Compiled pattern for finding alpha-numeric sequences.
	/// </summary>
	public static readonly Regex ValidAlphaNumericOnlyPattern
		= new(@"^\w+$", RegexOptions.Compiled);

	/// <summary>
	/// Compiled pattern for finding alpha-numeric sequences and possible surrounding white-space.
	/// </summary>
	public static readonly Regex ValidAlphaNumericOnlyUntrimmedPattern
		= new(@"^\s*\w+\s*$", RegexOptions.Compiled);

	/// <summary>
	/// Compiled Regex for finding white-space.
	/// </summary>
	public static readonly Regex WhiteSpacePattern = new(@"\s+", RegexOptions.Compiled);
}
