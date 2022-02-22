using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Open.Text;

/// <summary>
/// Useful extensions for working with StringBuilders.
/// </summary>
public static class StringBuilderExtensions
{
	/// <summary>
	/// Generic variation on the append method that attempts to avoid boxing via a switch.
	/// </summary>
	/// <typeparam name="T">The type being passed.</typeparam>
	/// <param name="sb">The string builder to append to.</param>
	/// <param name="value">The generic value to append.</param>
	/// <exception cref="ArgumentNullException">If the provided StringBuilder is null.</exception>
	public static StringBuilder Append<T>(this StringBuilder sb, T value)
		=> sb is null
		? throw new ArgumentNullException(nameof(sb))
		: value switch
		{
			char c => sb.Append(c),
			byte b => sb.Append(b),
			sbyte b => sb.Append(b),
			bool b => sb.Append(b),
			ulong ul => sb.Append(ul),
			uint l => sb.Append(l),
			ushort u => sb.Append(u),
			long l => sb.Append(l),
			int i => sb.Append(i),
			short s => sb.Append(s),
			float f => sb.Append(f),
			double d => sb.Append(d),
			decimal d => sb.Append(d),
			string s => sb.Append(s),
			char[] c => sb.Append(c),
			_ => sb.Append(value)
		};

	/// <summary>
	/// Adds every entry to a StringBuilder.
	/// </summary>
	/// <typeparam name="T">The type of the source.</typeparam>
	/// <param name="source">The source span.</param>
	/// <returns>The resultant StringBuilder.</returns>
	public static StringBuilder ToStringBuilder<T>(this ReadOnlySpan<T> source)
	{
		var len = source.Length;
		var sb = new StringBuilder(len);

		for (var i = 0; i < len; i++)
			sb.Append<T>(source[i]);

		return sb;
	}

	/// <inheritdoc cref="ToStringBuilder{T}(ReadOnlySpan{T})" />
	public static StringBuilder ToStringBuilder<T>(this Span<T> source)
	{
		var len = source.Length;
		var sb = new StringBuilder(len);

		for (var i = 0; i < len; i++)
			sb.Append<T>(source[i]);

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
			sb.Append<T>(s);

		return sb;
	}

	/// <summary>
	/// Adds every entry to a StringBuilder separated by the specified sequence.
	/// </summary>
	/// <typeparam name="T">The type of the source.</typeparam>
	/// <param name="source">The source span.</param>
	/// <param name="separator">The separator sequence.</param>
	/// <returns>The resultant StringBuilder.</returns>
	public static StringBuilder ToStringBuilder<T>(this ReadOnlySpan<T> source, string? separator)
	{
		var len = source.Length;
		if (len < 2 || string.IsNullOrEmpty(separator))
			return ToStringBuilder(source);

		var sb = new StringBuilder(2 * len - 1);

		sb.Append<T>(source[0]);
		for (var i = 1; i < len; i++)
		{
			sb.Append(separator);
			sb.Append<T>(source[i]);
		}

		return sb;
	}

	/// <inheritdoc cref="ToStringBuilder{T}(ReadOnlySpan{T}, string)" />
	public static StringBuilder ToStringBuilder<T>(this Span<T> source, string? separator)
	{
		var len = source.Length;
		if (len < 2 || string.IsNullOrEmpty(separator))
			return ToStringBuilder(source);

		var sb = new StringBuilder(2 * len - 1);

		sb.Append<T>(source[0]);
		for (var i = 1; i < len; i++)
		{
			sb.Append(separator);
			sb.Append<T>(source[i]);
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
	public static StringBuilder ToStringBuilder<T>(this ReadOnlySpan<T> source, char separator)
	{
		var len = source.Length;
		if (len < 2) return ToStringBuilder(source);

		var sb = new StringBuilder(2 * len - 1);

		sb.Append<T>(source[0]);
		for (var i = 1; i < len; i++)
		{
			sb.Append(separator);
			sb.Append<T>(source[i]);
		}

		return sb;
	}

	/// <inheritdoc cref="ToStringBuilder{T}(ReadOnlySpan{T}, char)" />
	public static StringBuilder ToStringBuilder<T>(this Span<T> source, char separator)
	{
		var len = source.Length;
		if (len < 2) return ToStringBuilder(source);

		var sb = new StringBuilder(2 * len - 1);

		sb.Append<T>(source[0]);
		for (var i = 1; i < len; i++)
		{
			sb.Append(separator);
			sb.Append<T>(source[i]);
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
			target.Append<T>(value);
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
		target.Append<T>(e.Current);
		while (e.MoveNext())
			target.Append(separator).Append<T>(e.Current);

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
		target.Append<T>(e.Current);
		while (e.MoveNext())
			target.Append(separator).Append<T>(e.Current);

		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, ReadOnlySpan<T> values)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		foreach (var value in values)
			target.Append<T>(value);
		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, ReadOnlySpan<T> values, string? separator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		if (string.IsNullOrEmpty(separator))
			return target.AppendAll(values);

		var e = values.GetEnumerator();
		if (!e.MoveNext()) return target;
		if (target.Length != 0) target.Append(separator);
		target.Append<T>(e.Current);
		while (e.MoveNext())
			target.Append(separator).Append<T>(e.Current);

		return target;
	}

	/// <summary>
	/// Shortcut for adding an array of values to a StringBuilder.
	/// </summary>
	public static StringBuilder AppendAll<T>(this StringBuilder target, ReadOnlySpan<T> values, char separator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		var e = values.GetEnumerator();
		if (!e.MoveNext()) return target;
		if (target.Length != 0) target.Append(separator);
		target.Append<T>(e.Current);
		while (e.MoveNext())
			target.Append(separator).Append<T>(e.Current);

		return target;
	}

	/// <summary>
	/// Appends values to StringBuilder prefixing the provided separator.
	/// </summary>
	public static StringBuilder AppendWithSeparator(this StringBuilder target, string? separator, object value, params object[] values)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (values is null) throw new ArgumentNullException(nameof(values));
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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (values is null) throw new ArgumentNullException(nameof(values));
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
	public static StringBuilder AppendWithSeparator<T>(this StringBuilder target, IDictionary<string, T> source, string key, string itemSeparator, string keyValueSeparator)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));
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
		{
			target
				.AppendWithSeparator(itemSeparator, key)
				.Append(keyValueSeparator)
				.Append<T>(result);
		}

		return target;
	}

#if NETSTANDARD2_0
	/// <summary>
	/// Appends the string representation of a specified read-only character span to this instance.
	/// </summary>
	/// <param name="target">The StringBuilder to append to.</param>
	/// <param name="value">The read-only character span to append.</param>
	/// <returns>A reference to this instance after the append operation is completed.</returns>
	/// <exception cref="ArgumentNullException">If the target is null.</exception>
	public static StringBuilder Append(this StringBuilder target, ReadOnlySpan<char> value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		foreach (var v in value) target.Append(v);
		return target;
	}
#endif

}
