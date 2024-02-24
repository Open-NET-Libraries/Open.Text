using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace Open.Text;

/// <summary>
/// A class for concatenating sequences of characters similar to a StringBuilder but with simple operators and specialized methods for optimizing memory use.
/// </summary>
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
public sealed class StringConcat(int initialCapacity = -1) : IEnumerable<CharacterSequence>
{
	private readonly List<CharacterSequence> _segments = initialCapacity < 1 ? [] : new(initialCapacity);

	/// <inheritdoc />
	public IEnumerator<CharacterSequence> GetEnumerator() => _segments.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc />
	public int Count => _segments.Count;

	/// <inheritdoc />
	public CharacterSequence this[int index] { get => _segments[index]; set => _segments[index] = value; }

	/// <summary>
	/// The total number of characters in the StringConcat instance.
	/// </summary>
	public int GetLength() => _segments.Sum(s => s.Length);

	/// <summary>
	/// Appends a <paramref name="sequence"/> of characters to this <see cref="StringConcat"/> instance.
	/// </summary>
	public StringConcat Append(CharacterSequence sequence)
	{
		_segments.Add(sequence);
		return this;
	}

	/// <summary>
	/// Appends the <paramref name="sequences"/> to this <see cref="StringConcat"/> instance.
	/// </summary>
	public StringConcat Append(IEnumerable<CharacterSequence> sequences)
	{
		if (sequences is null) return this;
		_segments.AddRange(sequences);
		return this;
	}

	/// <summary>
	/// Appends the <paramref name="sequences"/> to this <see cref="StringConcat"/> instance.
	/// </summary>
	public StringConcat Append(IEnumerable<string> sequences)
	{
		if (sequences is null) return this;
		foreach (var segment in sequences)
			_segments.Add(segment);
		return this;
	}

	/// <summary>
	/// Appends the <paramref name="sequences"/> to this <see cref="StringConcat"/> instance.
	/// </summary>
	public StringConcat Append(IEnumerable<StringSegment> sequences)
	{
		if (sequences is null) return this;
		foreach(var segment in sequences)
			_segments.Add(segment);
		return this;
	}

	/// <summary>
	/// Appends a new line to the StringConcat instance.
	/// </summary>
	public StringConcat AppendLine()
		=> Append(Environment.NewLine);

	/// <summary>
	/// Appends a sequence of characters to the StringConcat instance and then appends a new line.
	/// </summary>
	public StringConcat AppendLine(CharacterSequence value)
		=> Append(value).AppendLine();

	/// <summary>
	/// Empties the StringConcat instance.
	/// </summary>
	public void Clear()
		=> _segments.Clear();

	/// <summary>
	/// Copies the characters in the StringConcat instance to a new string.
	/// </summary>
	public override string ToString()
	{
		if (_segments.Count == 0)
			return string.Empty;

		if (_segments.Count == 1)
			return _segments[0].ToString();

		var sb = new StringBuilder();
		foreach (var segment in _segments)
		{
			foreach (var c in segment)
				sb.Append(c);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Inserts a sequence of characters into the <see cref="StringConcat"/> instance at the specified index.
	/// </summary>
	public void Insert(int index, CharacterSequence item) => _segments.Insert(index, item);

	/// <summary>
	/// Removes the sequence of characters at the specified <paramref name="index"/>.
	/// </summary>
	public void RemoveAt(int index) => _segments.RemoveAt(index);

	/// <summary>
	/// Creates a new <see cref="StringConcat"/> instance with the same content as this instance.
	/// </summary>
	public StringConcat Clone()
		=> new StringConcat(_segments.Count).Append(_segments);

	/// <summary>
	/// Appends the <paramref name="right"/> to the <paramref name="left"/>.
	/// </summary>
	/// <remarks>
	/// The result is the <paramref name="left"/> instance with the <paramref name="right"/> appended.
	/// </remarks>
	internal static StringConcat Add(StringConcat left, CharacterSequence right)
		=> left is null
		? throw new ArgumentNullException(nameof(left))
		: left.Append(right);

	/// <inheritdoc cref="Add"/>
	public static StringConcat operator +(StringConcat left, CharacterSequence right)
		=> Add(left, right);

	/// <inheritdoc cref="Add"/>
	public static StringConcat operator +(StringConcat left, IEnumerable<CharacterSequence> right)
	{
		if (left is null) throw new ArgumentNullException(nameof(left));
		if (right is null) throw new ArgumentNullException(nameof(right));
		// Append all the segments from the right to the left.
		return left.Append(right);
	}

	/// <summary>
	/// Creates a new <see cref="StringConcat"/> instance beginning with the specified <paramref name="sequence"/>.
	/// </summary>
	[SuppressMessage("Style", "IDE0028:Simplify collection initialization")]
	internal static StringConcat NewFrom(CharacterSequence sequence)
		=> sequence.IsEmpty ? new() : new StringConcat().Append(sequence);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(CharacterSequence sequence)
		=> NewFrom(sequence);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(string value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(char value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(StringSegment value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(ArraySegment<char> value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(char[] value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(ReadOnlyMemory<char> value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(Memory<char> value)
		=> NewFrom(value);

	/// <inheritdoc cref="NewFrom(CharacterSequence)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringConcat(StringBuilder value)
		=> NewFrom(value);

	/// <summary>
	/// Converts the <see cref="StringConcat"/> instance to a string.
	/// </summary>
#if NETSTANDARD2_0
#else
	[return:NotNullIfNotNull(nameof(value))]
#endif
	public static implicit operator string?(StringConcat? value)
		=> value?.ToString()!;
}
