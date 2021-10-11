/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Open.Text;

public static class StringBuilderExtensions
{
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

	/// <inheritdoc cref="ToStringBuilder{T}(in ReadOnlySpan{T})">
	public static StringBuilder ToStringBuilder<T>(this in Span<T> source)
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
		if (source is null) throw new ArgumentNullException(nameof(source));
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
	public static StringBuilder ToStringBuilder<T>(this in ReadOnlySpan<T> source, string? separator)
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


	/// <inheritdoc cref="ToStringBuilder{T}(in ReadOnlySpan{T}, string)">
	public static StringBuilder ToStringBuilder<T>(this in Span<T> source, string? separator)
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
	public static StringBuilder ToStringBuilder<T>(this in ReadOnlySpan<T> source, char separator)
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

	/// <inheritdoc cref="ToStringBuilder{T}(in ReadOnlySpan{T}, char)">
	public static StringBuilder ToStringBuilder<T>(this in Span<T> source, char separator)
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
	public static StringBuilder ToStringBuilder<T>(this IEnumerable<T> source, string? separator)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return new StringBuilder().AppendAll(source, separator);
	}

	/// <summary>
	/// Adds every entry to a StringBuilder separated by the specified character.
	/// </summary>
	/// <typeparam name="T">The type of the source.</typeparam>
	/// <param name="source">The source enumerable.</param>
	/// <param name="separator">The separator character.</param>
	/// <returns>The resultant StringBuilder.</returns>
	public static StringBuilder ToStringBuilder<T>(this IEnumerable<T> source, char separator)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return new StringBuilder().AppendAll(source, separator);
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T>? values)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (values == null) return target;
		foreach (var value in values)
			target.Append(value);
		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T>? values, string? separator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (values == null) return target;

		if (string.IsNullOrEmpty(separator))
			return target.AppendAll(values);

		using var e = values.GetEnumerator();
		if (!e.MoveNext()) return target;
		if (target.Length != 0) target.Append(separator);
		target.Append(e.Current);
		while (e.MoveNext()) target.Append(separator).Append(e.Current);

		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, IEnumerable<T>? values, char separator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (values == null) return target;

		using var e = values.GetEnumerator();
		if (!e.MoveNext()) return target;
		if (target.Length != 0) target.Append(separator);
		target.Append(e.Current);
		while (e.MoveNext()) target.Append(separator).Append(e.Current);

		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, in ReadOnlySpan<T> values)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		foreach (var value in values)
			target.Append(value);
		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, in ReadOnlySpan<T> values, string? separator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (string.IsNullOrEmpty(separator))
			return target.AppendAll(values);

		var e = values.GetEnumerator();
		if (!e.MoveNext()) return target;
		if (target.Length != 0) target.Append(separator);
		target.Append(e.Current);
		while (e.MoveNext()) target.Append(separator).Append(e.Current);

		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, in ReadOnlySpan<T> values, char separator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		var e = values.GetEnumerator();
		if (!e.MoveNext()) return target;
		if (target.Length != 0) target.Append(separator);
		target.Append(e.Current);
		while (e.MoveNext()) target.Append(separator).Append(e.Current);

		return target;
	}

	/// <summary>
	/// Appends values to StringBuilder prefixing the provided separator.
	/// </summary>
	public static StringBuilder AppendWithSeparator(this StringBuilder target, string? separator, object value, params object[] values)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (string.IsNullOrEmpty(separator))
		{
			target.Append(value);
			foreach (var v in values)
				target.Append(v);
		}
		else
		{

			if (target.Length != 0)
				target.Append(separator);
			target.Append(value);
			foreach (var v in values)
				target.Append(separator).Append(v);
		}

		return target;
	}

	/// <summary>
	/// Appends values to StringBuilder prefixing the provided separator.
	/// </summary>
	public static StringBuilder AppendWithSeparator(this StringBuilder target, char separator, object value, params object[] values)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (target.Length != 0)
			target.Append(separator);
		target.Append(value);
		foreach (var v in values)
			target.Append(separator).Append(v);

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
