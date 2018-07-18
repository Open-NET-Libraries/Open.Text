/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Open.Text
{
	public static class Extensions
	{
		private const uint BYTE_RED = 1024;
		private static readonly string[] _byte_labels = new[] { "KB", "MB", "GB", "TB" };
		private static readonly string[] _number_labels = new[] { "K", "M", "B" };
		public static readonly Regex VALID_ALPHA_NUMERIC_ONLY = new Regex(@"^\w+$");

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static string BeforeFirst(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (string.IsNullOrEmpty(search))
				return null;

			var i = source.IndexOf(search, comparisonType);
			return i == -1 ? null : source.Substring(0, i);
		}

		/// <summary>
		/// Provides the substring after the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string after the search string.  Returns null if search string is not found.</returns>
		public static string AfterFirst(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (string.IsNullOrEmpty(search))
				return null;

			var i = source.IndexOf(search, comparisonType);
			return i == -1 ? null : source.Substring(i + search.Length);
		}

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static string BeforeLast(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (string.IsNullOrEmpty(search))
				return null;

			var i = source.LastIndexOf(search, comparisonType);
			return i == -1 ? null : source.Substring(0, i);
		}

		/// <summary>
		/// Provides the substring after the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string after the search string.  Returns null if search string is not found.</returns>
		public static string AfterLast(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (string.IsNullOrEmpty(search))
				return null;

			var i = source.LastIndexOf(search, comparisonType);
			return i == -1 ? null : source.Substring(i + search.Length);
		}

		static readonly Regex ToTitleCaseRegex = new Regex(@"\b(\[a-z]})");

		/// <summary>
		/// Shortcut for rendering the current culture's title case.
		/// </summary>
		public static string ToTitleCase(this string source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			return ToTitleCaseRegex.Replace(
				source.ToLowerInvariant(),
				match => match.Value.ToUpperInvariant());

			//return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(source);
		}

		public static bool IsAnyNullOrWhiteSpace(params string[] values)
		{
			if (values == null || values.Length == 0)
				return false;

			return values.Any(string.IsNullOrWhiteSpace);
		}

		public static string AssertIsNotNullOrWhiteSpace(this string source)
		{
			if (string.IsNullOrWhiteSpace(source))
				throw new ArgumentException();
			Contract.EndContractBlock();

			return source;
		}

		/// <summary>
		/// Shortcut for String.IsNullOrWhiteSpace(source).
		/// </summary>
		public static bool IsNullOrWhiteSpace(this string source)
		{
			return string.IsNullOrWhiteSpace(source);
		}

		/// <summary>
		/// Shortcut for returning a null addValue if the source string is null, white space or empty.
		/// </summary>
		/// <param name="value">The value to be trimmed.</param>
		/// <param name="trim">True will trim whitespace from valid response.</param>
		public static string ToNullIfWhiteSpace(this string value, bool trim = false)
		{
			return string.IsNullOrWhiteSpace(value) ? null : (trim ? value.Trim() : value);
		}

		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
		public static string ToFormat(this string value, string format = null)
		{
			return string.IsNullOrWhiteSpace(value) ? string.Empty : (format == null ? value : string.Format(format, value));
		}

		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
		public static string ToFormat(this int? value, string format = null)
		{
			if (format == null) format = "{0}";
			return value == null ? string.Empty : string.Format(format, value.Value);
		}

		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
		public static string ToFormat(this short? value, string format = null)
		{
			if (format == null) format = "{0}";
			return value == null ? string.Empty : string.Format(format, value.Value);
		}


		/// <summary>
		/// Returns true if only contains alphanumeric characters. Regex: (^\w+$).
		/// </summary>
		/// <param name="source">The value to be formatted.</param>
		/// <param name="trim">Will be trimmed if true.</param>
		public static bool IsAlphaNumeric(this string source, bool trim = false)
		{
			return !string.IsNullOrWhiteSpace(source) && VALID_ALPHA_NUMERIC_ONLY.IsMatch(trim ? source.Trim() : source);
		}

		#region Regex helper methods.
		public static string GetValue(this GroupCollection groups, string groupName, bool throwIfInvalid = false)
		{
			if (groups == null)
				throw new NullReferenceException();
			if (groupName == null)
				throw new ArgumentNullException(nameof(groupName));
			Contract.EndContractBlock();

			var group = groups[groupName];
			if (group == null)
			{
				if (throwIfInvalid)
					throw new ArgumentException();
				return null;
			}

			return group.Success ? group.Value : null;
		}
		#endregion


		#region StringBuilder helper methods.
		/// <summary>
		/// Shortcut for adding an array of values to a StringBuilder.
		/// </summary>
		public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T> values, string separator = null)
		{
			if (target == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (values != null)
			{
				if (string.IsNullOrEmpty(separator))
				{
					foreach (var value in values)
						target.Append(value);
				}
				else
				{
					foreach (var value in values)
						target.AppendWithSeparator(separator, value);
				}
			}
			return target;
		}

		/// <summary>
		/// Shortcut for adding an array of values to a StringBuilder.
		/// </summary>
		public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T> values, char separator)
		{
			if (target == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (values != null)
				foreach (var value in values)
					target.AppendWithSeparator(separator, value);
			return target;
		}


		/// <summary>
		/// Appends values to StringBuilder prefixing the provided separator.
		/// </summary>
		public static StringBuilder AppendWithSeparator<T>(this StringBuilder target, string separator, params T[] values)
		{
			if (target == null)
				throw new NullReferenceException();
			if (values == null || values.Length == 0)
				throw new ArgumentException("Parameters missing.");
			Contract.EndContractBlock();

			if (!string.IsNullOrEmpty(separator) && target.Length != 0)
				target.Append(separator);

			target.AppendAll(values);
			return target;
		}

		/// <summary>
		/// Appends values to StringBuilder prefixing the provided separator.
		/// </summary>
		public static StringBuilder AppendWithSeparator<T>(this StringBuilder target, char separator, params T[] values)
		{
			if (target == null)
				throw new NullReferenceException();
			if (values == null || values.Length == 0)
				throw new ArgumentException("Parameters missing.");
			Contract.EndContractBlock();

			if (target.Length != 0)
				target.Append(separator);
			target.AppendAll(values);

			return target;
		}

		/// <summary>
		/// Appends a key/value pair to StringBuilder using the provided separators.
		/// </summary>
		public static void AppendWithSeparator<T>(this StringBuilder target, IDictionary<string, T> source, string key, string itemSeparator, string keyValueSeparator)
		{
			if (target == null)
				throw new NullReferenceException();
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (itemSeparator == null)
				throw new ArgumentNullException(nameof(itemSeparator));
			if (keyValueSeparator == null)
				throw new ArgumentNullException(nameof(keyValueSeparator));
			Contract.EndContractBlock();

			if (source.TryGetValue(key, out var result))
				target
					.AppendWithSeparator(itemSeparator, key)
					.Append(keyValueSeparator)
					.Append(result);
		}
		#endregion


		#region Numeric string formatting.
		/// <summary>
		/// Shortcut for formating Nullable&lt;double&gt;.
		/// </summary>
		public static string ToString(this double? value, string format)
		{
			if (format == null)
				throw new ArgumentNullException(nameof(format));
			Contract.EndContractBlock();

			return value.HasValue ? value.Value.ToString(format) : double.NaN.ToString(format);
		}


		/// <summary>
		/// Shortcut for formating Nullable&lt;float&gt;.
		/// </summary>
		public static string ToString(this float? value, string format)
		{
			if (format == null)
				throw new ArgumentNullException(nameof(format));
			Contract.EndContractBlock();

			return value.HasValue ? value.Value.ToString(format) : float.NaN.ToString(format);
		}

		/// <summary>
		/// Shortcut for formating Nullable&lt;int&gt;.
		/// </summary>
		public static string ToString(this int? value, string format)
		{
			if (format == null)
				throw new ArgumentNullException(nameof(format));
			Contract.EndContractBlock();

			return value.HasValue ? value.Value.ToString(format) : 0.ToString(format);
		}

		/// <summary>
		/// Shortcut for formating to a percent.
		/// </summary>
		public static string ToPercentString(this int value, int range, int decimals = 0)
		{
			return ((double)value / range).ToString("p" + decimals);
		}


		/// <summary>
		/// Returns an abbreviated metric representation of a quantity of bytes. 
		/// </summary>
		public static string ToByteString(this double bytes)
		{
			if (Math.Abs(bytes) < BYTE_RED)
				return bytes.ToString("0") + " bytes";

			foreach (var s in _byte_labels)
			{
				bytes /= BYTE_RED;
				if (Math.Abs(bytes) < BYTE_RED)
					return bytes.ToString("0.0") + " " + s;
			}

			return bytes.ToString("#,##0.0") + " " + _byte_labels.Last();
		}

		/// <summary>
		/// Returns an abbreviated metric representation of a quantity of bytes. 
		/// </summary>
		public static string ToByteString(this long bytes)
		{
			return ToByteString((double)bytes);
		}

		/// <summary>
		/// Returns an abbreviated metric representation of a quantity of bytes. 
		/// </summary>
		public static string ToByteString(this int bytes)
		{
			return ToByteString((double)bytes);
		}

		/// <summary>
		/// Returns an abbreviated metric representation of a number. 
		/// </summary>
		public static string ToMetricString(this double number)
		{
			if (Math.Abs(number) < 1000)
				return number.ToString("0.0");

			foreach (var s in _number_labels)
			{
				number /= 1000;
				if (Math.Abs(number) < 1000)
					return number.ToString("0.0") + s;
			}

			return number.ToString("#,##0.0") + _number_labels.Last();
		}

		/// <summary>
		/// Returns an abbreviated metric representation of a number. 
		/// </summary>
		public static string ToMetricString(this long number)
		{
			return ToMetricString((double)number);
		}

		/// <summary>
		/// Returns an abbreviated metric representation of a number. 
		/// </summary>
		public static string ToMetricString(this int number)
		{
			return ToMetricString((double)number);
		}
		#endregion



		public static readonly Regex WHITESPACE = new Regex(@"\s+");
		public static string ReplaceWhiteSpace(this string source, string replace = " ")
		{
			if (source == null)
				throw new NullReferenceException();
			if (replace == null)
				throw new ArgumentNullException(nameof(replace));
			Contract.EndContractBlock();

			return WHITESPACE.Replace(source, replace);
		}

		public static string TrimStart(this string source, string pattern)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (pattern == null)
				return source.TrimStart();

			if (pattern == string.Empty)
				return source;

			return source.IndexOf(pattern, StringComparison.Ordinal) == 0
				? source.Substring(pattern.Length)
				: source;
		}

		public static string TrimEnd(this string source, string pattern)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (pattern == null)
				return source.TrimEnd();

			if (pattern == string.Empty)
				return source;

			var expectedIndex = source.Length - pattern.Length;
			var result = source.IndexOf(pattern, StringComparison.Ordinal);
			return result >= 0 && result == expectedIndex
				? source.Substring(0, expectedIndex)
				: source;
		}

		public const string NEWLINE = "\r\n";
		/// <summary>
		/// Shortcut for WriteLineNoTabs on a TextWriter. Mimimcs similar classes.
		/// </summary>
		public static void WriteLineNoTabs(this TextWriter writer, string s = null)
		{
			if (writer == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (s != null)
				writer.Write(s);
			writer.Write(NEWLINE);
		}

		static readonly Regex NumericSupplantRegex = new Regex(@"{(?<index>\d+)}");
		public static string Supplant<T>(this string source, T[] values)
		{
			return NumericSupplantRegex.Replace(source, match =>
				values[int.Parse(match.Groups["index"].Value)].ToString());
		}

	}
}
