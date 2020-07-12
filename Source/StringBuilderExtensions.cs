/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Open.Text
{
	public static class StringBuilderExtensions
	{
		private const string ParametersMissing = "Parameters missing.";

		/// <summary>
		/// Adds every entry to a StringBuilder.
		/// </summary>
		/// <typeparam name="T">The type of the source.</typeparam>
		/// <param name="source">The source span.</param>
		/// <returns>The resultant StringBuilder.</returns>
		public static StringBuilder ToStringBuilder<T>(this in ReadOnlySpan<T> source)
		{
			var len = source.Length;
			var sb = new StringBuilder(len);

			for (var i = 0; i < len; i++)
				sb.Append(source[i]);

			return sb;
		}

		/// <summary>
		/// Adds every entry to a StringBuilder.
		/// </summary>
		/// <typeparam name="T">The type of the source.</typeparam>
		/// <param name="source">The source span.</param>
		/// <returns>The resultant StringBuilder.</returns>
		public static StringBuilder ToStringBuilder<T>(this IEnumerable<T> source)
		{
			if (source is null) throw new NullReferenceException();
			var sb = new StringBuilder();
			foreach (var s in source)
				sb.Append(s);

			return sb;
		}

		/// <summary>
		/// Adds every entry to a StringBuilder separated by the specified sequence.
		/// </summary>
		/// <typeparam name="T">The type of the source.</typeparam>
		/// <param name="source">The source span.</param>
		/// <param name="separator">The separator sequence.</param>
		/// <returns>The resultant StringBuilder.</returns>
		public static StringBuilder ToStringBuilder<T>(this in ReadOnlySpan<T> source, string separator)
		{
			var len = source.Length;
			if (len < 2 || string.IsNullOrEmpty(separator))
				return ToStringBuilder(source);

			var sb = new StringBuilder(2 * len - 1);

			sb.Append(source[0]);
			for (var i = 1; i < len; i++)
			{
				sb.Append(separator);
				sb.Append(source[i]);
			}

			return sb;
		}

		/// <summary>
		/// Adds every entry to a StringBuilder separated by the specified character.
		/// </summary>
		/// <typeparam name="T">The type of the source.</typeparam>
		/// <param name="source">The source span.</param>
		/// <param name="separator">The separator character.</param>
		/// <returns>The resultant StringBuilder.</returns>
		public static StringBuilder ToStringBuilder<T>(this in ReadOnlySpan<T> source, in char separator)
		{
			var len = source.Length;
			if (len < 2) return ToStringBuilder(source);

			var sb = new StringBuilder(2 * len - 1);

			sb.Append(source[0]);
			for (var i = 1; i < len; i++)
			{
				sb.Append(separator);
				sb.Append(source[i]);
			}

			return sb;
		}

		/// <summary>
		/// Adds every entry to a StringBuilder separated by the specified sequence.
		/// </summary>
		/// <typeparam name="T">The type of the source.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="separator">The separator sequence.</param>
		/// <returns>The resultant StringBuilder.</returns>
		public static StringBuilder ToStringBuilder<T>(this IEnumerable<T> source, string separator)
		{
			if (source is null) throw new NullReferenceException();
			Contract.EndContractBlock();

			if (string.IsNullOrEmpty(separator))
				return ToStringBuilder(source);

			var first = true;
			var sb = new StringBuilder();
			foreach (var s in source)
			{
				if (first) first = false;
				else sb.Append(separator);
				sb.Append(s);
			}
			return sb;
		}

		/// <summary>
		/// Adds every entry to a StringBuilder separated by the specified character.
		/// </summary>
		/// <typeparam name="T">The type of the source.</typeparam>
		/// <param name="source">The source enumerable.</param>
		/// <param name="separator">The separator character.</param>
		/// <returns>The resultant StringBuilder.</returns>
		public static StringBuilder ToStringBuilder<T>(this IEnumerable<T> source, in char separator)
		{
			if (source is null) throw new NullReferenceException();
			Contract.EndContractBlock();

			var sb = new StringBuilder();
			var first = true;
			foreach (var s in source)
			{
				if (first) first = false;
				else sb.Append(separator);
				sb.Append(s);
			}
			return sb;
		}


		/// <summary>
		/// Shortcut for adding an array of values to a StringBuilder.
		/// </summary>
		public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T> values, string? separator = null)
		{
			if (target is null)
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
						target.AppendWithSeparator(separator!, value);
				}
			}
			return target;
		}

		/// <summary>
		/// Shortcut for adding an array of values to a StringBuilder.
		/// </summary>
		public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T> values, in char separator)
		{
			if (target is null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (values != null)
				foreach (var value in values)
					target.AppendWithSeparator(in separator, value);
			return target;
		}


		/// <summary>
		/// Appends values to StringBuilder prefixing the provided separator.
		/// </summary>
		public static StringBuilder AppendWithSeparator<T>(this StringBuilder target, string separator, params T[] values)
		{
			if (target is null)
				throw new NullReferenceException();
			if (values is null || values.Length == 0)
				throw new ArgumentException(ParametersMissing);
			Contract.EndContractBlock();

			if (!string.IsNullOrEmpty(separator) && target.Length != 0)
				target.Append(separator);

			target.AppendAll(values);
			return target;
		}

		/// <summary>
		/// Appends values to StringBuilder prefixing the provided separator.
		/// </summary>
		public static StringBuilder AppendWithSeparator<T>(this StringBuilder target, in char separator, params T[] values)
		{
			if (target is null)
				throw new NullReferenceException();
			if (values is null || values.Length == 0)
				throw new ArgumentException(ParametersMissing);
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
			if (target is null)
				throw new NullReferenceException();
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (key is null)
				throw new ArgumentNullException(nameof(key));
			if (itemSeparator is null)
				throw new ArgumentNullException(nameof(itemSeparator));
			if (keyValueSeparator is null)
				throw new ArgumentNullException(nameof(keyValueSeparator));
			Contract.EndContractBlock();

			if (source.TryGetValue(key, out var result))
				target
					.AppendWithSeparator(itemSeparator, key)
					.Append(keyValueSeparator)
					.Append(result);
		}
	}
}
