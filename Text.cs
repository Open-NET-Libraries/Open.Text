/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using Open.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Open.Text
{
	public static partial class Extensions
	{
		private const uint BYTE_RED = 1024;
		private static readonly string[] _byte_labels = new[] { "KB", "MB", "GB", "TB" };
		private static readonly string[] _number_labels = new[] { "K", "M", "B" };
		public static readonly Regex VALID_ALPHA_NUMERIC_ONLY = new Regex(@"^\w+$");

		/// <summary>
		/// Finds the first instance of a character and returns the set of characters up to that sequence.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitCharacter">The charcter to find.</param>
		/// <param name="nextIndex">The next possible index following the the current one.</param>
		/// <param name="startIndex">The index to start the split.</param>
		/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
		public static ReadOnlySpan<char> FirstSplit(this string source,
			char splitCharacter,
			out int nextIndex,
			int startIndex = 0)
		{
			if (source is null) throw new NullReferenceException();
			Contract.EndContractBlock();

			var i = source.Length == 0 ? -1 : source.IndexOf(splitCharacter, startIndex);
			if (i == -1)
			{
				nextIndex = -1;
				return source.AsSpan();
			}

			nextIndex = i + 1;
			if (nextIndex >= source.Length) nextIndex = -1;
			return source.AsSpan().Slice(startIndex, i);
		}

		/// <summary>
		/// Finds the first instance of a character sequence and returns the set of characters up to that sequence.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitSequence">The sequence to find.</param>
		/// <param name="nextIndex">The next possible index following the the current one.</param>
		/// <param name="startIndex">The index to start the split.</param>
		/// <param name="comparisonType">The string comparison type to use.</param>
		/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
		public static ReadOnlySpan<char> FirstSplit(this string source,
			string splitSequence,
			out int nextIndex,
			int startIndex = 0,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source is null) throw new NullReferenceException();
			if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
			if (splitSequence.Length == 0)
				throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
			Contract.EndContractBlock();

			var i = source.Length == 0 ? -1 : source.IndexOf(splitSequence, startIndex, comparisonType);
			if (i == -1)
			{
				nextIndex = -1;
				return source.AsSpan();
			}

			nextIndex = i + splitSequence.Length;
			if (nextIndex >= source.Length) nextIndex = -1;
			return source.AsSpan().Slice(startIndex, i);
		}

		/// <summary>
		/// Finds the first instance of a character sequence and returns the set of characters up to that sequence.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitCharacter">The charcter to find.</param>
		/// <returns>The portion of the source up to and excluding the character searched for.</returns>
		public static IEnumerable<string> SplitAsEnumerable(this string source,
			char splitCharacter)
		{
			if (source is null) throw new NullReferenceException();
			Contract.EndContractBlock();

			return source.Length == 0
				? Enumerable.Empty<string>()
				: SplitAsEnumerableCore();

			IEnumerable<string> SplitAsEnumerableCore()
			{
				var startIndex = 0;
				do
				{
					yield return source.FirstSplit(splitCharacter, out var nextIndex, startIndex).ToString();
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}
		}


		/// <summary>
		/// Finds the first instance of a character sequence and returns the set of characters up to that sequence.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitSequence">The sequence to find.</param>
		/// <param name="comparisonType">The string comparison type to use.</param>
		/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
		public static IEnumerable<string> SplitAsEnumerable(this string source,
			string splitSequence,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source is null) throw new NullReferenceException();
			if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
			if (splitSequence.Length == 0)
				throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
			Contract.EndContractBlock();

			return source.Length == 0
				? Enumerable.Empty<string>()
				: SplitAsEnumerableCore();

			IEnumerable<string> SplitAsEnumerableCore()
			{
				var startIndex = 0;
				do
				{
					yield return source.FirstSplit(splitSequence, out var nextIndex, startIndex, comparisonType).ToString();
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}
		}

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static string BeforeFirst(this string source, string search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source is null)
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
			if (source is null)
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
			if (source is null)
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
			if (source is null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (string.IsNullOrEmpty(search))
				return null;

			var i = source.LastIndexOf(search, comparisonType);
			return i == -1 ? null : source.Substring(i + search.Length);
		}

		/// <summary>
		/// Splits a sequence of characters into strings using the character sequence provided.
		/// </summary>
		/// <param name="source">The source string to split up.</param>
		/// <param name="characters">The sequence to split by.</param>
		/// <param name="comparisonType">The optional comparsion type.</param>
		/// <returns>The resultant list of string segments.</returns>
		public static List<string> Split(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> characters, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var result = new List<string>();
			var item = source;
			while (item.Length != 0)
				result.Add(item.FirstSplit(in characters, out item, comparisonType).ToString());
			return result;
		}

		/// <summary>
		/// Splits a sequence of characters into strings using the character provided.
		/// </summary>
		/// <param name="source">The source string to split up.</param>
		/// <param name="characters">The character to split by.</param>
		/// <returns>The resultant list of string segments.</returns>
		public static List<string> Split(this in ReadOnlySpan<char> source,
			in char character)
		{
			var result = new List<string>();
			var item = source;
			while (item.Length != 0)
				result.Add(item.FirstSplit(in character, out item).ToString());
			return result;
		}

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static int LastIndexOf(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			// TODO: This all could be improved or replaced later...
			if (search.Length > source.Length)
				return -1;

			// Nothing found?
			var i = search.IndexOf(search, comparisonType);
			if (i == -1)
				return i;

			// Next possible can't fit?
			var n = i + search.Length;
			if (n + search.Length > source.Length)
				return i;

			var next = source
				.Slice(n)
				.LastIndexOf(search, comparisonType);

			return next == -1 ? i : (i + next);
		}

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static ReadOnlySpan<char> BeforeFirst(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var i = source.IndexOf(search, comparisonType);
			return i > 0
				? source.Slice(0, i)
				: ReadOnlySpan<char>.Empty;
		}

		/// <summary>
		/// Provides the substring after the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string after the search string.  Returns null if search string is not found.</returns>
		public static ReadOnlySpan<char> AfterFirst(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var i = source.IndexOf(search, comparisonType);
			var p = i + search.Length;
			return (i == -1 || p >= source.Length)
				? ReadOnlySpan<char>.Empty
				: source.Slice(p);
		}

		/// <summary>
		/// Provides the substring before the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string before the search string.  Returns null if search string is not found.</returns>
		public static ReadOnlySpan<char> BeforeLast(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var i = source.LastIndexOf(search, comparisonType);
			return i > 0
				? source.Slice(0, i)
				: ReadOnlySpan<char>.Empty;
		}

		/// <summary>
		/// Provides the substring after the search string.
		/// </summary>
		/// <param name="source">The source string to search in.</param>
		/// <param name="search">The search string to look for.  If the search is null or empty this method returns null.</param>
		/// <param name="comparisonType">The comparison type to use when searching.</param>
		/// <returns>The source of the string after the search string.  Returns null if search string is not found.</returns>
		public static ReadOnlySpan<char> AfterLast(this in ReadOnlySpan<char> source,
			in ReadOnlySpan<char> search, StringComparison comparisonType = StringComparison.Ordinal)
		{
			var i = source.LastIndexOf(search, comparisonType);
			var p = i + search.Length;
			return (i == -1 || p >= source.Length)
				? ReadOnlySpan<char>.Empty
				: source.Slice(p);
		}

		static readonly Regex ToTitleCaseRegex = new Regex(@"\b(\[a-z]})");

		/// <summary>
		/// Shortcut for rendering the current culture's title case.
		/// </summary>
		public static string ToTitleCase(this string source)
		{
			if (source is null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			return ToTitleCaseRegex.Replace(
				source.ToLowerInvariant(),
				match => match.Value.ToUpperInvariant());

			//return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(source);
		}

		public static bool IsAnyNullOrWhiteSpace(params string[] values)
		{
			if (values is null || values.Length == 0)
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
			=> string.IsNullOrWhiteSpace(source);

		/// <summary>
		/// Shortcut for returning a null addValue if the source string is null, white space or empty.
		/// </summary>
		/// <param name="value">The value to be trimmed.</param>
		/// <param name="trim">True will trim whitespace from valid response.</param>
		public static string ToNullIfWhiteSpace(this string value, bool trim = false)
			=> string.IsNullOrWhiteSpace(value) ? null
				: (trim ? value.Trim() : value);


		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
		public static string ToFormat(this string value, string format = null)
			=> string.IsNullOrWhiteSpace(value) ? string.Empty
				: (format is null ? value : string.Format(format, value));


		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
		public static string ToFormat(this int? value, string format = null)
		{
			if (format is null) format = "{0}";
			return value is null ? string.Empty : string.Format(format, value.Value);
		}

		/// <summary>
		/// Shortcut for returning a formatted string if non-null, non-whitespace action exists.
		/// </summary>
		/// <param name="value">The value to be formatted.</param>
		/// <param name="format">The format string.</param>
		public static string ToFormat(this short? value, string format = null)
		{
			if (format is null) format = "{0}";
			return value is null ? string.Empty : string.Format(format, value.Value);
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
			if (groups is null)
				throw new NullReferenceException();
			if (groupName is null)
				throw new ArgumentNullException(nameof(groupName));
			Contract.EndContractBlock();

			var group = groups[groupName];
			if (group is null)
			{
				if (throwIfInvalid)
					throw new ArgumentException();
				return null;
			}

			return group.Success ? group.Value : null;
		}
		#endregion


		#region Numeric string formatting.
		/// <summary>
		/// Shortcut for formating Nullable&lt;double&gt;.
		/// </summary>
		public static string ToString(this double? value, string format)
		{
			if (format is null)
				throw new ArgumentNullException(nameof(format));
			Contract.EndContractBlock();

			return value.HasValue ? value.Value.ToString(format) : double.NaN.ToString(format);
		}


		/// <summary>
		/// Shortcut for formating Nullable&lt;float&gt;.
		/// </summary>
		public static string ToString(this float? value, string format)
		{
			if (format is null)
				throw new ArgumentNullException(nameof(format));
			Contract.EndContractBlock();

			return value.HasValue ? value.Value.ToString(format) : float.NaN.ToString(format);
		}

		/// <summary>
		/// Shortcut for formating Nullable&lt;int&gt;.
		/// </summary>
		public static string ToString(this int? value, string format)
		{
			if (format is null)
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
			if (source is null)
				throw new NullReferenceException();
			if (replace is null)
				throw new ArgumentNullException(nameof(replace));
			Contract.EndContractBlock();

			return WHITESPACE.Replace(source, replace);
		}

		public static string TrimStart(this string source, string pattern)
		{
			if (source is null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (pattern is null)
				return source.TrimStart();

			if (pattern == string.Empty)
				return source;

			return source.IndexOf(pattern, StringComparison.Ordinal) == 0
				? source.Substring(pattern.Length)
				: source;
		}

		public static string TrimEnd(this string source, string pattern)
		{
			if (source is null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (pattern is null)
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
			if (writer is null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (s != null)
				writer.Write(s);
			writer.Write(NEWLINE);
		}

		/// <summary>
		/// An alternative to String.Format that takes an array of values.
		/// </summary>
		/// <typeparam name="T">The generic type of the values provided.</typeparam>
		/// <param name="format">The format string.</param>
		/// <param name="values">The values to inject.</param>
		/// <returns>The resultant string.</returns>
		public static string Supplant<T>(this string format, T[] values)
		{
			if (values is null)
				return format;

			switch (values.Length)
			{
				case 0:
					return format;
				case 1:
					return string.Format(format, values[0]);
				default:
					return string.Format(format,
						values as object[] ?? values.Cast<object>().ToArray());
			}
		}

	}
}
