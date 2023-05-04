using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

[assembly: CLSCompliant(true)]
namespace Open.Text;

/// <summary>
/// Extensions for comparing strings and manipulating text.
/// </summary>
public static partial class TextExtensions
{
	private const uint BYTE_RED = 1024;
	private static readonly string[] _byte_labels = new[] { "KB", "MB", "GB", "TB", "PB" };
	private static readonly string[] _number_labels = new[] { "K", "M", "B" };

	/// <summary>
	/// Compiled pattern for finding alpha-numeric sequences.
	/// </summary>
	public static readonly Regex ValidAlphaNumericOnlyPattern = new(@"^\w+$", RegexOptions.Compiled);

	/// <summary>
	/// Compiled pattern for finding alpha-numeric sequences and possible surrounding white-space.
	/// </summary>
	public static readonly Regex ValidAlphaNumericOnlyUntrimmedPattern = new(@"^\s*\w+\s*$", RegexOptions.Compiled);

	/// <summary>
	/// Reports the zero-based index position of the last occurrence of a specified string
	/// within this instance. The search starts at a specified character position and
	/// proceeds backward toward the beginning of the string.
	/// </summary>
	/// <param name="source">The source string to search in.</param>
	/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
	/// <param name="comparisonType">The comparison type to use when searching.</param>
	/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
	public static int LastIndexOf(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> search, StringComparison comparisonType)
	{
		if (search.Length > source.Length)
			return -1;

		if (comparisonType == StringComparison.Ordinal)
			return source.LastIndexOf(search);

		// Nothing found?
		var i = source.IndexOf(search, comparisonType);
		if (i == -1)
			return i;

		// Next possible can't fit?
		var n = i + search.Length;
		if (n + search.Length > source.Length)
			return i;

		// Recurse to get the last one.
		var next = source
			.Slice(n)
			.LastIndexOf(search, comparisonType);

		return next == -1 ? i : (n + next);
	}

	/// <inheritdoc cref="LastIndexOf(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison)"/>
	public static int LastIndexOf(this StringSegment source,
		ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (search.Length > source.Length)
			return -1;

		if (comparisonType == StringComparison.Ordinal)
			return source.AsSpan().LastIndexOf(search);

		// Nothing found?
		var i = source.IndexOf(search, comparisonType);
		if (i == -1)
			return i;

		// Next possible can't fit?
		var n = i + search.Length;
		if (n + search.Length > source.Length)
			return i;

		// Recurse to get the last one.
		var next = source
			.AsSpan()
			.Slice(n)
			.LastIndexOf(search, comparisonType);

		return next == -1 ? i : (n + next);
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
		&& (trim ? ValidAlphaNumericOnlyUntrimmedPattern : ValidAlphaNumericOnlyPattern).IsMatch(source);

	#region Regex helper methods.
	private static readonly Func<Capture, string> _textDelegate = (Func<Capture, string>)
		typeof(Capture).GetProperty("Text", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetGetMethod(nonPublic: true)
			.CreateDelegate(typeof(Func<Capture, string>));

	/// <summary>
	/// Returns a ReadOnlySpan of the capture without creating a new string.
	/// </summary>
	/// <remarks>This is a stop-gap until .NET 6 releases the .ValueSpan property.</remarks>
	/// <param name="capture">The capture to get the span from.</param>
	public static ReadOnlySpan<char> AsSpan(this Capture capture)
		=> capture is null
		? throw new ArgumentNullException(nameof(capture))
		: _textDelegate.Invoke(capture).AsSpan(capture.Index, capture.Length);

	/// <summary>
	/// Gets a group by name.
	/// </summary>
	/// <param name="groups">The group collection to get the group from.</param>
	/// <param name="groupName">The declared name of the group.</param>
	/// <returns>The value of the requested group or null if not found.</returns>
	/// <exception cref="ArgumentNullException">Groups or groupName is null.</exception>
	public static string? GetValue(this GroupCollection groups, string groupName)
	{
		if (groups is null)
			throw new ArgumentNullException(nameof(groups));
		if (groupName is null)
			throw new ArgumentNullException(nameof(groupName));
		Contract.EndContractBlock();

		var group = groups[groupName];
		return group.Success
			? group.Value
			: null;
	}

	/// <returns>The value of the requested group or an empty span if not found.</returns>
	/// <inheritdoc cref="GetValue(GroupCollection, string)" />
	public static ReadOnlySpan<char> GetValueSpan(this GroupCollection groups, string groupName)
	{
		if (groups is null)
			throw new ArgumentNullException(nameof(groups));
		if (groupName is null)
			throw new ArgumentNullException(nameof(groupName));
		Contract.EndContractBlock();

		var group = groups[groupName];
		return group.Success
			? group.AsSpan()
			: ReadOnlySpan<char>.Empty;
	}

	/// <summary>
	/// Returns the available matches as StringSegments.
	/// </summary>
	/// <param name="pattern">The pattern to search with.</param>
	/// <param name="input">The string to search.</param>
	/// <returns>An enumerable containing the found segments.</returns>
	/// <exception cref="ArgumentNullException">If the pattern or input is null.</exception>
	public static IEnumerable<StringSegment> AsSegments(this Regex pattern, string input)
	{
		return pattern is null
			? throw new ArgumentNullException(nameof(pattern))
			: input is null
			? throw new ArgumentNullException(nameof(input))
			: input.Length == 0
			? Enumerable.Empty<StringSegment>()
			: AsSegmentsCore(pattern, input);

		static IEnumerable<StringSegment> AsSegmentsCore(Regex pattern, string input)
		{
			var match = pattern.Match(input);
			while (match.Success)
			{
				yield return new(input, match.Index, match.Length);
				match = match.NextMatch();
			}
		}
	}
	#endregion

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

		var label = string.Empty;
		foreach (var s in _byte_labels)
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

		var label = string.Empty;
		foreach (var s in _number_labels)
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
	/// Compiled Regex for finding white-space.
	/// </summary>
	public static readonly Regex WhiteSpacePattern = new(@"\s+", RegexOptions.Compiled);

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

		return WhiteSpacePattern.Replace(source, replace);
	}

	/// <summary>
	/// String constant for carriage return and then newline.
	/// </summary>
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
		writer.Write(Newline);
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
			_ => string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format, values as object[] ?? values.Cast<object>().ToArray()),
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
		switch(comparisonType)
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
