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
	[ExcludeFromCodeCoverage] // Reason: no need to cover underlying .NET code.
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
			StringSegment s => Append(sb, s),
			ReadOnlyMemory<char> s => sb.Append(s.Span),
			_ => sb.Append(value)
		};

	/// <summary>
	/// Creates a <see cref="StringBuilderEnumerable"/> for enumerating over the characters in the <paramref name="builder"/>.
	/// </summary>
	public static StringBuilderEnumerable AsEnumerable(this StringBuilder builder)
		=> new(builder);

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
	[ExcludeFromCodeCoverage] // Reason: component parts are tested.
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

	/// <summary>
	/// Appends the characters of <paramref name="value"/> to this instance.
	/// </summary>
	/// <param name="target">The <see cref="StringBuilder"/> to append to.</param>
	/// <param name="value">The <see cref="StringSegment"/> to append.</param>
	/// <returns>A reference to this instance after the append operation is completed.</returns>
	/// <exception cref="ArgumentNullException">If the target is null.</exception>
	[ExcludeFromCodeCoverage] // Reason: component parts are tested.
	[SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
	// Need to have a different name than just Append because of the potential collision with string.
	public static StringBuilder AppendSegment(this StringBuilder target, StringSegment value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (!value.HasValue) return target;
		if (value.Length == 0) return target.Append(string.Empty);
		if (value.Length == value.Buffer.Length) return target.Append(value.Buffer);
		return target.Append(value.AsSpan());
	}

#if NETSTANDARD2_0
	/// <summary>
	/// Appends the string representation of a specified read-only character span to this instance.
	/// </summary>
	/// <param name="target">The StringBuilder to append to.</param>
	/// <param name="value">The read-only character span to append.</param>
	/// <returns>A reference to this instance after the append operation is completed.</returns>
	/// <exception cref="ArgumentNullException">If the target is null.</exception>
	[ExcludeFromCodeCoverage] // Reason: component parts are tested.
	public static StringBuilder Append(this StringBuilder target, ReadOnlySpan<char> value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		foreach (var v in value) target.Append(v);
		return target;
	}

	/// <summary>
	/// Appends the characters from another <see cref="StringBuilder"/> this instance.
	/// </summary>
	public static StringBuilder Append(this StringBuilder target, StringBuilder value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (value is null) return target;
		var len = value.Length;
		for (var i = 0; i < len; i++)
			target.Append(value[i]);
		return target;
	}
#endif

	/// <summary>
	/// Trims whitespace from the end of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">If <paramref name="sb"/> is null.</exception>
	public static StringBuilder TrimEnd(this StringBuilder sb)
	{
		if (sb is null) throw new ArgumentNullException(nameof(sb));
		Contract.EndContractBlock();

		var last = sb.Length - 1;
		if (last == -1) return sb;

		var end = last;
		while (end >= 0 && char.IsWhiteSpace(sb[end]))
			end--;

		if (end < last)
			sb.Remove(end + 1, sb.Length - end - 1);

		return sb;
	}

	/// <summary>
	/// Trims the specified characters from the end of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">If <paramref name="sb"/> is null.</exception>
	public static StringBuilder TrimEnd(this StringBuilder sb, ReadOnlySpan<char> characters)
	{
		if (sb is null) throw new ArgumentNullException(nameof(sb));
		Contract.EndContractBlock();

		var last = sb.Length - 1;
		if (last == -1) return sb;

		var end = last;
		while (end >= 0 && characters.IndexOf(sb[end]) != -1)
			end--;

		if (end < last)
			sb.Remove(end + 1, sb.Length - end - 1);

		return sb;
	}

	/// <summary>
	/// Trims whitespace from the beginning of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">If <paramref name="sb"/> is null.</exception>
	public static StringBuilder TrimStart(this StringBuilder sb)
	{
		if (sb is null) throw new ArgumentNullException(nameof(sb));
		Contract.EndContractBlock();

		var start = 0;
		while (start < sb.Length && char.IsWhiteSpace(sb[start]))
			start++;

		if (start > 0)
			sb.Remove(0, start);

		return sb;
	}

	/// <summary>
	/// Trims the specified characters from the beginning of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">If <paramref name="sb"/> is null.</exception>
	public static StringBuilder TrimStart(this StringBuilder sb, ReadOnlySpan<char> characters)
	{
		if (sb is null) throw new ArgumentNullException(nameof(sb));
		Contract.EndContractBlock();

		var start = 0;
		while (start < sb.Length && characters.IndexOf(sb[start]) != -1)
			start++;

		if (start > 0)
			sb.Remove(0, start);

		return sb;
	}

	/// <summary>
	/// Trims whitespace from the beginning and end of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">If <paramref name="sb"/> is null.</exception>
	public static StringBuilder Trim(this StringBuilder sb)
		=> TrimEnd(sb).TrimStart();

	/// <summary>
	/// Trims the specified characters from the beginning and end of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">If <paramref name="sb"/> is null.</exception>
	public static StringBuilder Trim(this StringBuilder sb, ReadOnlySpan<char> characters)
		=> TrimEnd(sb, characters).TrimStart(characters);
}
