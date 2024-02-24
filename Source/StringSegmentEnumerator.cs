using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Open.Text;

/// <summary>
/// A struct for enumerating the characters in a <see cref="StringSegment"/>.
/// </summary>
/// <remarks>
/// Constructs a new <see cref="StringSegmentEnumerator"/> for the specified <paramref name="segment"/>.
/// </remarks>
public struct StringSegmentEnumerator(StringSegment segment) : IEnumerator<char>
{
	private readonly StringSegment _segment = segment;
	private int _position = -1;

	/// <inheritdoc />
	public readonly char Current
		=> _position < 0 || _position >= _segment.Length
		? throw new InvalidOperationException("Enumeration has either not started or has already finished.")
		: _segment[_position];

	readonly object IEnumerator.Current
		=> Current;

	/// <inheritdoc/>
	public bool MoveNext()
	{
		if (_position < _segment.Length - 1)
		{
			_position++;
			return true;
		}

		return false;
	}

	/// <inheritdoc/>
	public void Reset()
		=> _position = -1;

	/// <inheritdoc/>
	public void Dispose()
		=> _position = _segment.Length;
}

/// <summary>
/// A container for enumerating the characters in a <see cref="StringSegment"/>.
/// </summary>
public readonly record struct StringSegmentEnumerable : IEnumerable<char>
{
	readonly StringSegment _segment;

	/// <summary>
	/// Constructs a new <see cref="StringSegmentEnumerable"/> for the specified <see cref="StringSegment"/>.
	/// </summary>
	public StringSegmentEnumerable(StringSegment segment)
		=> _segment = segment;

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public StringSegmentEnumerator GetEnumerator() => new(_segment);

	IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}