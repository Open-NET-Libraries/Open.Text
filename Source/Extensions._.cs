/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT
 */

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


namespace Open.Text
{
	public static partial class Extensions
	{
		private const uint BYTE_RED = 1024;
		private static readonly string[] _byte_labels = new[] { "KB", "MB", "GB", "TB", "PB" };
		private static readonly string[] _number_labels = new[] { "K", "M", "B" };
		public static readonly Regex VALID_ALPHA_NUMERIC_ONLY = new(@"^\w+$", RegexOptions.Compiled);
		public static readonly Regex VALID_ALPHA_NUMERIC_ONLY_TRIM = new(@"^\s*\w+\s*$", RegexOptions.Compiled);

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static int LastIndexOf(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> search, StringComparison comparisonType)
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

		/// <summary>
		/// Converts a string to title-case.
		/// </summary>
		/// <param name="source">The string to apply title-casing to.</param>
		/// <param name="cultureInfo">The optional culture info.  Default is invariant.</param>
		/// <returns>The new title-cased string.</returns>
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
		/// <returns>True if any of the provided values is is null, empty or white-space only. Otherwise false.</returns>
		public static bool IsAnyNullOrWhiteSpace(params string[] values)
			=> values != null && values.Length != 0 && values.Any(v => string.IsNullOrWhiteSpace(v));

		/// <summary>
		/// Throws if null, empty or white-space only.
		/// </summary>
		/// <exception cref="NullReferenceException">If the source is null.</exception>
		/// <exception cref="ArgumentException">If the source is empty or white-space.</exception>
		/// <param name="source">The source string to validate.</param>
		/// <returns>The original string.</returns>
		public static string AssertIsNotNullOrWhiteSpace(this string? source)
		{
			if (source is null) throw new NullReferenceException(nameof(source));
			if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Cannot be empty or white-space.", nameof(source));
			Contract.EndContractBlock();

			return source;
		}

		/// <summary>
		/// Shortcut for String.IsNullOrWhiteSpace(source).
		/// </summary>
		public static bool IsNullOrWhiteSpace(this string? source)
			=> string.IsNullOrWhiteSpace(source);

		/// <summary>
		/// Shortcut for returning a null if the source string is null, white space or empty.
		/// </summary>
		/// <param name="value">The value to be trimmed.</param>
		/// <param name="trim">True will trim whitespace from valid response.</param>
		public static string? ToNullIfWhiteSpace(this string? value, bool trim = false)
			=> string.IsNullOrWhiteSpace(value) ? null
				: (trim ? value!.Trim() : value);

		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
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
		public static string ToFormat<T>(this T? value, string? format = null, CultureInfo? cultureInfo = default)
			where T : struct
		{
			if (!value.HasValue) return string.Empty;
			return string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format ?? "{0}", value.Value);
		}

		/// <summary>
		/// Returns true if only contains alphanumeric characters. Regex: (^\w+$).
		/// </summary>
		/// <param name="source">The value to be formatted.</param>
		/// <param name="trim">Will be trimmed if true.</param>
		public static bool IsAlphaNumeric(this string source, bool trim = false)
			=> !string.IsNullOrWhiteSpace(source)
			&& (trim ? VALID_ALPHA_NUMERIC_ONLY_TRIM : VALID_ALPHA_NUMERIC_ONLY).IsMatch(source);

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
		public static ReadOnlySpan<char> AsSpan(this Capture capture) =>
			_textDelegate.Invoke(capture).AsSpan(capture.Index, capture.Length);

		public static string? GetValue(this GroupCollection groups, string groupName, bool throwIfInvalid = false)
		{
			if (groups is null)
				throw new ArgumentNullException(nameof(groups));
			if (groupName is null)
				throw new ArgumentNullException(nameof(groupName));
			Contract.EndContractBlock();

			var group = groups[groupName];
			if (group is null)
			{
				if (throwIfInvalid)
					throw new ArgumentException("Group not found.", nameof(groupName));
				return null;
			}

			return group.Success ? group.Value : null;
		}

		public static ReadOnlySpan<char> GetValueSpan(this GroupCollection groups, string groupName, bool throwIfInvalid = false)
		{
			if (groups is null)
				throw new ArgumentNullException(nameof(groups));
			if (groupName is null)
				throw new ArgumentNullException(nameof(groupName));
			Contract.EndContractBlock();

			var group = groups[groupName];
			if (group is null)
			{
				if (throwIfInvalid)
					throw new ArgumentException("Group not found.", nameof(groupName));
				return null;
			}

			return group.Success ? group.AsSpan() : ReadOnlySpan<char>.Empty;
		}
		#endregion


		#region Numeric string formatting.
		/// <summary>
		/// Shortcut for formating Nullable&lt;T&gt;.
		/// </summary>
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
		public static string ToString(this double? value, string format, double defaultValue = double.NaN, CultureInfo? cultureInfo = default)
			=> ToString<double>(value, format, defaultValue, cultureInfo);

		/// <summary>
		/// Shortcut for formating Nullable&lt;float&gt;.
		/// </summary>
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
		public static string ToByteString(this int bytes, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
			=> ToByteString((double)bytes, decimalFormat, cultureInfo);

		/// <summary>
		/// Returns an abbreviated metric representation of a quantity of bytes. 
		/// </summary>
		public static string ToByteString(this long bytes, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
			=> ToByteString((double)bytes, decimalFormat, cultureInfo);

		/// <summary>
		/// Returns an abbreviated metric representation of a quantity of bytes. 
		/// </summary>
		public static string ToByteString(this ulong bytes, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
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
		public static string ToMetricString(this ulong number, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
			=> ToMetricString((double)number, decimalFormat, cultureInfo);

		/// <summary>
		/// Returns an abbreviated metric representation of a number. 
		/// </summary>
		public static string ToMetricString(this long number, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
			=> ToMetricString((double)number, decimalFormat, cultureInfo);

		/// <summary>
		/// Returns an abbreviated metric representation of a number. 
		/// </summary>
		public static string ToMetricString(this int number, string decimalFormat = "N1", CultureInfo? cultureInfo = default)
			=> ToMetricString((double)number, decimalFormat, cultureInfo);
		#endregion

		public static readonly Regex WHITESPACE = new(@"\s+", RegexOptions.Compiled);

		/// <summary>
		/// Replaces any white-space with the specified string.
		/// Collapses multiple white-space characters to a single space if no replacement specified.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="replace">The optional pattern to replace with.</param>
		/// <returns>The resultant string.</returns>
		public static string ReplaceWhiteSpace(this string source, string replace = " ")
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (replace is null) throw new ArgumentNullException(nameof(replace));
			Contract.EndContractBlock();

			return WHITESPACE.Replace(source, replace);
		}


		public const string NEWLINE = "\r\n";
		/// <summary>
		/// Shortcut for WriteLineNoTabs on a TextWriter. Mimimcs similar classes.
		/// </summary>
		public static void WriteLineNoTabs(this TextWriter writer, string? s = null)
		{
			if (writer is null) throw new ArgumentNullException(nameof(writer));
			Contract.EndContractBlock();

			if (s is not null) writer.Write(s);
			writer.Write(NEWLINE);
		}

		/// <summary>
		/// An alternative to String.Format that takes an array of values.
		/// </summary>
		/// <typeparam name="T">The generic type of the values provided.</typeparam>
		/// <param name="format">The format string.</param>
		/// <param name="values">The values to inject.</param>
		/// <param name="cultureInfo">The optional culture info.  Default is invariant.</param>
		/// <returns>The resultant string.</returns>
		public static string Supplant<T>(this string format, T[]? values, CultureInfo? cultureInfo = default)
			=> values is null ? format : values.Length switch
			{
				0 => format,
				1 => string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format, values[0]),
				_ => string.Format(cultureInfo ?? CultureInfo.InvariantCulture, format, values as object[] ?? values.Cast<object>().ToArray()),
			};
	}
}
