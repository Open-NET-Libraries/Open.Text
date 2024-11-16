using System.Text;

namespace Open.Text;

/// <summary>
/// Allows for easy conversion from a string to a <see cref="StringBuilder"/> by declaring the type as <see cref="StringBuilderHelper"/>.
/// </summary>
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
public readonly record struct StringBuilderHelper
{
	/// <summary>
	/// The underlying <see cref="Builder"/>.
	/// </summary>
	public StringBuilder Builder { get; }

	/// <inheritdoc cref="StringBuilder.ToString()"/>
	public override string ToString()
		=> Builder.ToString();

	/// <summary>
	/// Constructs a new <see cref="StringBuilderHelper"/> with the specified <see cref="StringBuilder"/>.
	/// </summary>
	public StringBuilderHelper(StringBuilder sb)
		=> Builder = sb ?? throw new ArgumentNullException(nameof(sb));

	/// <summary>
	/// Constructs a new <see cref="StringBuilderHelper"/> with a new <see cref="StringBuilder"/>.
	/// </summary>
	public StringBuilderHelper()
		: this(new StringBuilder()) { }

	/// <summary>
	/// Constructs a new <see cref="StringBuilderHelper"/> with a <see cref="StringBuilder"/> of the <paramref name="initialCapacity"/>.
	/// </summary>
	public StringBuilderHelper(int initialCapacity)
		: this(new StringBuilder(initialCapacity)) { }

	/// <summary>
	/// Appends the characters to the underlying <see cref="StringBuilder"/>.
	/// </summary>
	public static StringBuilderHelper Add(StringBuilderHelper helper, string characters)
	{
		_ = helper.Builder.Append(characters);
		return helper;
	}

	/// <summary>
	/// Appends the <paramref name="character"/> to the underlying <see cref="StringBuilder"/>.
	/// </summary>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, char character)
		=> helper.Builder.Append(character);

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, string characters)
		=> Add(helper, characters);

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, StringSegment characters)
	{
		_ = helper.Builder.AppendSegment(characters);
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, ReadOnlySpan<char> characters)
	{
		_ = helper.Builder.Append(characters);
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, Span<char> characters)
	{
		_ = helper.Builder.Append(characters);
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, char[] characters)
	{
		if (characters is null) return helper;
		_ = helper.Builder.Append(characters.AsSpan());
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, ReadOnlyMemory<char> characters)
	{
		_ = helper.Builder.Append(characters.Span);
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, Memory<char> characters)
	{
		_ = helper.Builder.Append(characters.Span);
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, ArraySegment<char> characters)
	{
		_ = helper.Builder.Append(characters.AsSpan());
		return helper;
	}

	/// <inheritdoc cref="Add(StringBuilderHelper, string)"/>
	public static StringBuilderHelper operator +(StringBuilderHelper helper, IEnumerable<char> characters)
	{
		if (characters is null) return helper;
		StringBuilder sb = helper.Builder;
		foreach (char c in characters)
			sb.Append(c);
		return helper;
	}

	/// <summary>
	/// Creates a new <see cref="StringBuilderHelper"/> instance beginning with the specified <paramref name="sequence"/>.
	/// </summary>
	internal static StringBuilderHelper NewFrom<T>(T sequence)
		=> throw new NotImplementedException();

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	[ExcludeFromCodeCoverage]
	public static implicit operator StringBuilderHelper(string sequence)
		=> string.IsNullOrEmpty(sequence) ? new() : new(new StringBuilder(sequence));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(char value)
		=> new(new StringBuilder(1).Append(value));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(StringSegment value)
		=> new(new StringBuilder(value.Length).AppendSegment(value));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(ReadOnlySpan<char> value)
		=> new(new StringBuilder(value.Length).Append(value));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(ReadOnlyMemory<char> value)
		=> new(new StringBuilder(value.Length).Append(value.Span));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(Memory<char> value)
		=> new(new StringBuilder(value.Length).Append(value.Span));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(ArraySegment<char> value)
		=> new(new StringBuilder(value.Count).Append(value.AsSpan()));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(char[] value)
		=> value is null ? new() : new(new StringBuilder(value.Length).Append(value.AsSpan()));

	/// <inheritdoc cref="NewFrom{T}(T)"/>
	public static implicit operator StringBuilderHelper(StringBuilder value)
		=> value is null ? new() : new(new StringBuilder(value.Length).Append(value));

	/// <summary>
	/// Converts the <see cref="StringBuilderHelper"/> instance to a string.
	/// </summary>
	public static implicit operator string(StringBuilderHelper value)
		=> value.Builder.ToString();
}
