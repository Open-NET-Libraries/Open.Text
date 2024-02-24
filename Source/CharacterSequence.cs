using Microsoft.Extensions.Primitives;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Open.Text;

/// <summary>
/// A read-only struct that represents a sequence of characters.
/// </summary>
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
public readonly record struct CharacterSequence : IEnumerable<char>
{
	private readonly int _length;
	private readonly Func<int>? _getLength;
	private readonly Func<string>? _toString;

	/// <summary>
	/// Indicates whether the sequence is empty.
	/// </summary>
	/// <remarks>
	/// Can still return <see langword="false"/> if the sequence could have a length that potentially changes.
	/// </remarks>
	public bool IsEmpty => _length == 0;

	/// <summary>
	/// The number of characters in the sequence.
	/// </summary>
	public readonly int Length
		=> _length == -1 ? _getLength!() : _length;

	/// <summary>
	/// The characters in the sequence.
	/// </summary>
	public readonly IEnumerable<char> Characters { get; }

	/// <summary>
	/// Constructs a CharacterSequence.
	/// </summary>
	private CharacterSequence(int length, IEnumerable<char> characters, Func<string>? toString = null)
	{
		Debug.Assert(length >= 0);

		_length = length;
		_toString = toString;
		Characters = characters;
	}

	private CharacterSequence(Func<int> getLength, IEnumerable<char> characters, Func<string>? toString = null)
	{
		_length = -1;
		_getLength = getLength;
		_toString = toString;
		Characters = characters;
	}

	/// <summary>
	/// An empty CharacterSequence.
	/// </summary>
	public static CharacterSequence Empty { get; } = new(0, []);

	/// <summary>
	/// Creates a new <see cref="CharacterSequence"/> from the specified characters.
	/// </summary>
	/// <remarks>Returns <see cref="Empty"/> if the <paramref name="length"/> is zero.</remarks>
	internal static CharacterSequence Create(int length, IEnumerable<char> characters)
		=> length == 0 ? Empty : new(length, characters);

	/// <summary>
	/// Creates a new <see cref="CharacterSequence"/> from the specified characters.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CharacterSequence Create(Func<int> getLength, IEnumerable<char> characters)
		=> new(getLength, characters);

	/// <summary>
	/// Implicitly converts a <see cref="StringSegment"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(string characters)
	{
		if (characters is null) return Empty;
		var len = characters.Length;
		return len == 0 ? Empty : new(len, characters, characters.ToString);
	}

	/// <summary>
	/// Implicitly converts a <see cref="char"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(char value)
		=> new(1, Enumerable.Repeat(value, 1));

	/// <summary>
	/// Implicitly converts a <see cref="StringSegment"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(StringSegment characters)
	{
		var len = characters.Length;
		return len == 0 ? Empty : new(len, characters.AsEnumerable(), characters.ToString);
	}

	/// <summary>
	/// Implicitly converts a <see cref="ArraySegment{T}"/> of <see cref="char"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(ArraySegment<char> characters)
		=> Create(characters.Count, characters);

	/// <summary>
	/// Implicitly converts a <see cref="ArraySegment{T}"/> of <see cref="char"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(char[] characters)
	{
		if (characters is null) return Empty;
		var len = characters.Length;
		return len == 0 ? Empty : new(len, characters, () => new string(characters));
	}

	/// <summary>
	/// Implicitly converts a <see cref="ReadOnlyMemory{T}"/> of <see cref="char"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(ReadOnlyMemory<char> characters)
	{
		var len = characters.Length;
		return len == 0 ? Empty : new(len, GetChars(characters), () => characters.Span.ToString());

		static IEnumerable<char> GetChars(ReadOnlyMemory<char> memory)
		{
			var len = memory.Length;
			for (int i = 0; i < len; i++)
				yield return memory.Span[i];
		}
	}

	/// <summary>
	/// Implicitly converts a <see cref="Memory{T}"/> of <see cref="char"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(Memory<char> characters)
		=> (ReadOnlyMemory<char>)characters;

	/// <summary>
	/// Implicitly converts a <see cref="ArraySegment{T}"/> of <see cref="char"/> to a <see cref="CharacterSequence"/>.
	/// </summary>
	public static implicit operator CharacterSequence(StringBuilder characters)
		=> characters is null ? Empty : new(() => characters.Length, characters.AsEnumerable(), characters.ToString);

	/// <summary>
	/// Copies the characters in the sequence to a string.
	/// </summary>
	public override string ToString()
	{
		if (_toString is not null)
			return _toString();

		var len = Length;
		if (len == 0) return string.Empty;

		const int MaxStackSize = 256;
		if (len > MaxStackSize)
		{
			using var lease = MemoryPool<char>.Shared.Rent(len);
			var span = lease.Memory.Span.Slice(0, len);
			CopyTo(span);
			var result = span.ToString();
			span.Clear(); // This is important for security reasons.
			return result;
		}
		else
		{
			Span<char> allocated = stackalloc char[MaxStackSize];
			var span = allocated.Slice(0, len);
			CopyTo(span);
			return span.ToString();
		}
	}

	/// <summary>
	/// Copies the characters in the sequence to a span.
	/// </summary>
	public void CopyTo(Span<char> span)
	{
		var e = Characters.GetEnumerator();
		for (int i = 0; i < span.Length; i++)
		{
			if (!e.MoveNext())
				throw new InvalidOperationException("The sequence is shorter than the specified length.");
			span[i] = e.Current;
		}

		if (e.MoveNext())
			throw new InvalidOperationException("The sequence is longer than the specified length.");
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator<char> GetEnumerator() => Characters.GetEnumerator();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
