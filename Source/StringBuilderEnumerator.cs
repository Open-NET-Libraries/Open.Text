using System.Collections;
using System.Text;

namespace Open.Text;

/// <summary>
/// A struct for enumerating the characters in a <see cref="StringBuilder"/>.
/// </summary>
/// <remarks>
/// Constructs a new <see cref="StringBuilderEnumerator"/> for the specified <paramref name="builder"/>.
/// </remarks>
public struct StringBuilderEnumerator(StringBuilder builder) : IEnumerator<char>
{
	private readonly StringBuilder _builder = builder;
	private int _position = -1;

	/// <inheritdoc />
	public readonly char Current
		=> _position < 0 || _position >= _builder.Length
		? throw new InvalidOperationException("Enumeration has either not started or has already finished.")
		: _builder[_position];

	readonly object IEnumerator.Current
		=> Current;

	/// <inheritdoc/>
	public bool MoveNext()
	{
		if (_position < _builder.Length - 1)
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
		=> _position = _builder.Length;
}

/// <summary>
/// A container for enumerating the characters in a <see cref="StringBuilder"/>.
/// </summary>
public readonly record struct StringBuilderEnumerable : IEnumerable<char>
{
	readonly StringBuilder _builder;

	/// <summary>
	/// Constructs a new <see cref="StringBuilderEnumerable"/> for the specified <paramref name="builder"/>.
	/// </summary>
	public StringBuilderEnumerable(StringBuilder builder) => _builder = builder;

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public StringBuilderEnumerator GetEnumerator() => new(_builder);

	IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}