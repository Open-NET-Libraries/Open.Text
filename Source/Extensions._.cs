[assembly: CLSCompliant(true)]
namespace Open.Text;

/// <summary>
/// Extensions for comparing strings and manipulating text.
/// </summary>
public static partial class TextExtensions
{
	private const uint BYTE_RED = 1024;

	private static IEnumerable<string> ByteLabels
	{
		get
		{
			yield return "KB";
			yield return "MB";
			yield return "GB";
			yield return "TB";
			yield return "PB";
		}
	}

	private static IEnumerable<string> NumberLabels
	{
		get
		{
			yield return "K";
			yield return "M";
			yield return "G";
		}
	}

	/// <summary>
	/// Converts a string to title-case.
	/// </summary>
	/// <param name="source">The string to apply title-casing to.</param>
	/// <param name="cultureInfo">The optional culture info.  Default is invariant.</param>
	/// <returns>The new title-cased string.</returns>
	[ExcludeFromCodeCoverage] // Would only text .NET TextInfo.ToTitleCase method.
	public static string ToTitleCase(this string source, CultureInfo? cultureInfo = default)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return (cultureInfo ?? CultureInfo.InvariantCulture).TextInfo.ToTitleCase(source);
	}

	/// <summary>
	/// Returns true if any string is null, empty or white-space only.
	/// </summary>
	/// <param name="values">The set of values to validate.</param>
	/// <returns>True if any of the provided values is null, empty or white-space only. Otherwise false.</returns>
	[ExcludeFromCodeCoverage]
	public static bool IsAnyNullOrWhiteSpace(params string[] values)
		=> values != null
		&& values.Length != 0
		&& values.Any(v => string.IsNullOrWhiteSpace(v));

	/// <summary>
	/// Throws if null, empty or white-space only.
	/// </summary>
	/// <exception cref="NullReferenceException">If the source is null.</exception>
	/// <exception cref="ArgumentException">If the source is empty or white-space.</exception>
	/// <param name="source">The source string to validate.</param>
	/// <returns>The original string.</returns>
	[ExcludeFromCodeCoverage]
	[SuppressMessage("Roslynator", "RCS1256:Invalid argument null check.", Justification = "Prefer unique exception for null.")]
	public static string AssertIsNotNullOrWhiteSpace(this string? source)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Cannot be empty or white-space.", nameof(source));
		Contract.EndContractBlock();

		return source;
	}

	/// <summary>
	/// Shortcut for String.IsNullOrWhiteSpace(source).
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static bool IsNullOrWhiteSpace(this string? source)
		=> string.IsNullOrWhiteSpace(source);

	/// <summary>
	/// Shortcut for returning a null if the source string is null, white space or empty.
	/// </summary>
	/// <param name="value">The value to be trimmed.</param>
	/// <param name="trim">True will trim whitespace from valid response.</param>
	[ExcludeFromCodeCoverage]
	public static string? ToNullIfWhiteSpace(this string? value, bool trim = false)
		=> string.IsNullOrWhiteSpace(value) ? null
			: (trim ? value!.Trim() : value);

	/// <summary>
	/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
	/// </summary>
	/// <param name="value">The value to be formatted.</param>
	/// <param name="format">The format string.</param>
	[ExcludeFromCodeCoverage]
	public static string ToFormat(this string value, string? format = null)
		=> string.IsNullOrWhiteSpace(value) ? string.Empty
			: (format is null ? value : string.Format(CultureInfo.InvariantCulture, format, value));

	/// <summary>
	/// Shortcut for returning a formatted a value if non-null.
	/// </summary>
	/// <param name="value">The value to be formatted.</param>
	/// <param name="format">The format string.</param>
	/// <param name="cultureInfo">The optional culture info.  Default is invariant.</param>
	/// <returns>The formatted string, or empty string if the value is null.</returns>
	[ExcludeFromCodeCoverage]
	public static string ToFormat<T>(this T? value, string? format = null, CultureInfo? cultureInfo = default)
		where T : struct
		=> value.HasValue
		? string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format ?? "{0}", value.Value)
		: string.Empty;

	/// <summary>
	/// Returns true if only contains alphanumeric characters. Regex: (^\w+$).
	/// </summary>
	/// <param name="source">The value to be formatted.</param>
	/// <param name="trim">Will be trimmed if true.</param>
	[ExcludeFromCodeCoverage]
	public static bool IsAlphaNumeric(this string source, bool trim = false)
		=> !string.IsNullOrWhiteSpace(source)
		&& (trim ? RegexPatterns.ValidAlphaNumericOnlyUntrimmedPattern : RegexPatterns.ValidAlphaNumericOnlyPattern).IsMatch(source);

	#region Numeric string formatting.
	/// <summary>
	/// Shortcut for formating Nullable&lt;T&gt;.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToString<T>(this T? value, string format, T defaultValue = default, CultureInfo? cultureInfo = default)
		where T : struct, IFormattable
	{
		if (format is null) throw new ArgumentNullException(nameof(format));
		Contract.EndContractBlock();

		return (value ?? defaultValue).ToString(format, cultureInfo ?? CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Shortcut for formating Nullable&lt;double&gt;.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToString(this double? value, string format, double defaultValue = double.NaN, CultureInfo? cultureInfo = default)
		=> ToString<double>(value, format, defaultValue, cultureInfo);

	/// <summary>
	/// Shortcut for formating Nullable&lt;float&gt;.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToString(this float? value, string format, float defaultValue = float.NaN, CultureInfo? cultureInfo = default)
		=> ToString<float>(value, format, defaultValue, cultureInfo);

	/// <summary>
	/// Shortcut for formating to a percent.
	/// </summary>
	public static string ToPercentString(this int value, int range, int decimals = 0, CultureInfo? cultureInfo = default)
		=> ((double)value / range).ToString($"p{decimals}", cultureInfo);

	/// <summary>
	/// Returns an abbreviated metric representation of a quantity of bytes.
	/// </summary>
	public static string ToByteString(this double bytes, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
	{
		const string BYTE = "{0:N0} byte";
		const string BYTES = BYTE + "s";

		if (Math.Abs(bytes) < BYTE_RED)
			return string.Format(cultureInfo, bytes == 1 ? BYTE : BYTES, bytes);

		string label = string.Empty;
		foreach (string s in ByteLabels)
		{
			label = s;
			bytes /= BYTE_RED;
			if (Math.Abs(bytes) < BYTE_RED) break;
		}

		return bytes.ToString(decimalFormat, cultureInfo) + ' ' + label;
	}

	/// <summary>
	/// Returns an abbreviated metric representation of a quantity of bytes.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToByteString(this int bytes, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
		=> ToByteString((double)bytes, decimalFormat, cultureInfo);

	/// <summary>
	/// Returns an abbreviated metric representation of a quantity of bytes.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToByteString(this long bytes, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
		=> ToByteString((double)bytes, decimalFormat, cultureInfo);

	/// <summary>
	/// Returns an abbreviated metric representation of a number.
	/// </summary>
	public static string ToMetricString(this double number, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
	{
		if (Math.Abs(number) < 1000)
			return number.ToString(decimalFormat, cultureInfo ?? CultureInfo.InvariantCulture);

		string label = string.Empty;
		foreach (string s in NumberLabels)
		{
			label = s;
			number /= 1000;
			if (Math.Abs(number) < 1000) break;
		}

		return number.ToString(decimalFormat, cultureInfo ?? CultureInfo.InvariantCulture) + label;
	}

	/// <summary>
	/// Returns an abbreviated metric representation of a number.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToMetricString(this long number, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
		=> ToMetricString((double)number, decimalFormat, cultureInfo);

	/// <summary>
	/// Returns an abbreviated metric representation of a number.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static string ToMetricString(this int number, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
		=> ToMetricString((double)number, decimalFormat, cultureInfo);
	#endregion

	/// <summary>
	/// Replaces any white-space with the specified string.
	/// Collapses multiple white-space characters to a single space if no replacement specified.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="replace">The optional pattern to replace with.</param>
	/// <returns>The resultant string.</returns>
	[ExcludeFromCodeCoverage]
	public static string ReplaceWhiteSpace(this string source, string replace = " ")
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (replace is null) throw new ArgumentNullException(nameof(replace));
		Contract.EndContractBlock();

		return RegexPatterns.WhiteSpacePattern.Replace(source, replace);
	}

	/// <summary>
	/// String constant for carriage return and then newline.
	/// </summary>
	[Obsolete("Use Environment.NewLine instead.")]
	public const string Newline = "\r\n";

	/// <summary>
	/// Shortcut for WriteLineNoTabs on a TextWriter. Mimics similar classes.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static void WriteLineNoTabs(this TextWriter writer, string? s = null)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		Contract.EndContractBlock();

		if (s is not null) writer.Write(s);
		writer.Write(Environment.NewLine);
	}

	/// <summary>
	/// An alternative to String.Format that takes an array of values.
	/// </summary>
	/// <typeparam name="T">The generic type of the values provided.</typeparam>
	/// <param name="format">The format string.</param>
	/// <param name="values">The values to inject.</param>
	/// <param name="cultureInfo">The optional culture info.  Default is invariant.</param>
	/// <returns>The resultant string.</returns>
	[ExcludeFromCodeCoverage]
	public static string Supplant<T>(this string format, T[]? values, CultureInfo? cultureInfo = default)
		=> values is null ? format : values.Length switch
		{
			0 => format,
			1 => string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format, values[0]),
			_ => string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format, values as object[] ?? [.. values.Cast<object>()]),
		};

	/// <summary>
	/// A hashing algorithm for a span of characters.
	/// </summary>
	/// <remarks>
	/// Setting the <paramref name="maxChars"/> parameter to a low number
	/// will dramatically increase the speed as more characters requires more iterations
	/// at the expense of accuracy and possible collisions.
	/// </remarks>
	public static int GetHashCodeFromChars(this ReadOnlySpan<char> chars, StringComparison comparisonType = StringComparison.Ordinal, int maxChars = int.MaxValue)
	{
		int length = chars.Length > maxChars ? maxChars : chars.Length;

		int hash = 17;
#pragma warning disable IDE0010 // Add missing cases
		switch (comparisonType)
		{
			case StringComparison.Ordinal:
			case StringComparison.CurrentCulture:
			case StringComparison.InvariantCulture:
			{
				for (int i = 0; i < length; i++)
				{
					ref readonly char c = ref chars[i];
					hash = (hash * 31) ^ c;
				}

				break;
			}

			case StringComparison.OrdinalIgnoreCase:
			case StringComparison.CurrentCultureIgnoreCase:
			{
				for (int i = 0; i < length; i++)
				{
					ref readonly char c = ref chars[i];
					hash = (hash * 31) ^ char.ToLower(c, CultureInfo.CurrentCulture);
				}

				break;
			}

			case StringComparison.InvariantCultureIgnoreCase:
			{
				for (int i = 0; i < length; i++)
				{
					ref readonly char c = ref chars[i];
					hash = (hash * 31) ^ char.ToLowerInvariant(c);
				}

				break;
			}
		}
#pragma warning restore IDE0010 // Add missing cases

		return hash + length;
	}

	/// <inheritdoc cref="GetHashCodeFromChars(ReadOnlySpan{char}, StringComparison, int)"/>
	public static int GetHashCodeFromChars(this StringSegment chars, StringComparison comparisonType = StringComparison.Ordinal, int maxChars = int.MaxValue)
		=> chars.HasValue
		? chars.AsSpan().GetHashCodeFromChars(comparisonType, maxChars)
		: throw new ArgumentNullException(nameof(chars), "The buffer must not be null.");

	/// <inheritdoc cref="GetHashCodeFromChars(ReadOnlySpan{char}, StringComparison, int)"/>
	public static int GetHashCodeFromChars(this string chars, StringComparison comparisonType = StringComparison.Ordinal, int maxChars = int.MaxValue)
		=> chars.AsSpan().GetHashCodeFromChars(comparisonType, maxChars);
}
