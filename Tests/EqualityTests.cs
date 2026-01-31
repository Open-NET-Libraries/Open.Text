namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public class EqualityTests
{
	static readonly ReadOnlyMemory<char> Chars = new[] { ' ', '\t' };
	[Fact]
	public void NullReference()
	{
		Assert.False(default(string).Equals(ReadOnlySpan<char>.Empty, StringComparison.Ordinal));
		Assert.False(default(string).Equals(ReadOnlySpan<char>.Empty.ToArray().AsSpan(), StringComparison.Ordinal));
		Assert.False(default(string).TrimmedEquals(ReadOnlySpan<char>.Empty, StringComparison.Ordinal));
		Assert.False(default(string).TrimmedEquals(ReadOnlySpan<char>.Empty, ' ', StringComparison.Ordinal));
		Assert.False(default(string).TrimmedEquals(ReadOnlySpan<char>.Empty, Chars.Span, StringComparison.Ordinal));
		Assert.True(default(string).TrimmedEquals(default, StringComparison.Ordinal));
		Assert.True(default(string).TrimmedEquals(default, ' ', StringComparison.Ordinal));
		Assert.True(default(string).TrimmedEquals(default, Chars.Span, StringComparison.Ordinal));
		Assert.False(string.Empty.TrimmedEquals(default, StringComparison.Ordinal));
		Assert.False(string.Empty.TrimmedEquals(default, ' ', StringComparison.Ordinal));
		Assert.False(string.Empty.TrimmedEquals(default, Chars.Span, StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("")]
	[InlineData("y")]
	[InlineData("y", StringComparison.OrdinalIgnoreCase)]
	[InlineData("hello")]
	public void EqualsSpan(string value, StringComparison comparison = StringComparison.Ordinal)
	{
		var roSpan = value.AsSpan();
		var span = roSpan.ToArray().AsSpan();
		Assert.True(value.AsComparable(comparison) == roSpan);
		Assert.True(roSpan.Equals(value, comparison));
		Assert.True(value.TrimmedEquals(value, comparison));
		Assert.True(value.TrimmedEquals(roSpan, comparison));
		Assert.True(value.TrimmedEquals(value, ' ', comparison));
		Assert.True(value.TrimmedEquals(roSpan, ' ', comparison));
		Assert.True(value.TrimmedEquals(value, Chars.Span, comparison));
		Assert.True(value.TrimmedEquals(roSpan, Chars.Span, comparison));
		Assert.True(value.Equals(roSpan, comparison));
		Assert.True(value.Equals(roSpan.ToArray().AsSpan(), comparison));
		Assert.True(span.Equals(value, comparison));
		Assert.True(span.Equals(roSpan, comparison));
		Assert.True(roSpan.Equals(span, comparison));
		Assert.True(span.Equals(span, comparison));
	}

	[Theory]
	[InlineData("", null)]
	[InlineData("", "n")]
	[InlineData("y", "n")]
	[InlineData("y", "n", StringComparison.OrdinalIgnoreCase)]
	[InlineData("y", " y")]
	[InlineData("y", "Y")]
	[InlineData("hello", "hell")]
	[InlineData("hell", "hello")]
	public void NotEqualsSpan(string value, string? other, StringComparison comparison = StringComparison.Ordinal)
	{
		var roSpan = value.AsSpan();
		var span = roSpan.ToArray().AsSpan();
		Assert.False(roSpan.Equals(other, comparison));
		Assert.True(value.AsComparable(comparison) != other);
		if (other is not null)
		{
			var oRoSpan = other.AsSpan();
			Assert.False(roSpan.Equals(oRoSpan, comparison));
			Assert.False(value.Equals(oRoSpan, comparison));
			Assert.False(span.Equals(oRoSpan, comparison));
			var oSpan = oRoSpan.ToArray().AsSpan();
			Assert.False(value.Equals(oSpan, comparison));
			Assert.False(span.Equals(oSpan, comparison));
			Assert.False(value.TrimmedEquals(other, comparison));
			Assert.False(value.TrimmedEquals(other, ' ', comparison));
			Assert.False(value.TrimmedEquals(other, Chars.Span, comparison));
			Assert.False(value.TrimmedEquals(oRoSpan, comparison));
			Assert.False(value.TrimmedEquals(oRoSpan, ' ', comparison));
			Assert.False(value.TrimmedEquals(oRoSpan, Chars.Span, comparison));
		}

		Assert.False(span.Equals(other, comparison));
	}

	[Theory]
	[InlineData(" ", "")]
	[InlineData("y ", "y")]
	[InlineData(" y ", "Y", StringComparison.OrdinalIgnoreCase)]
	[InlineData(" hello   ", "hello")]
	[InlineData(" hello   ", "Hello", StringComparison.OrdinalIgnoreCase)]
	public void TrimmedEquals(string value, string? other, StringComparison comparison = StringComparison.Ordinal)
	{
		Assert.True(value.TrimmedEquals(other, comparison));
		Assert.True(value.TrimmedEquals(other!.AsSegment(), comparison));
		Assert.True(value.TrimmedEquals(other.AsSpan(), comparison));
	}

	[Theory]
	[InlineData(" ", "")]
	[InlineData("y ", "y")]
	[InlineData(" y ", "Y", StringComparison.OrdinalIgnoreCase)]
	[InlineData(" hello   ", "hello")]
	[InlineData(" hello   ", "Hello", StringComparison.OrdinalIgnoreCase)]
	public void TrimmedEqualsChar(string value, string? other, StringComparison comparison = StringComparison.Ordinal)
	{
		Assert.True(value.TrimmedEquals(other, ' ', comparison));
		Assert.True(value.TrimmedEquals(other!.AsSegment(), ' ', comparison));
		Assert.True(value.TrimmedEquals(other.AsSpan(), ' ', comparison));
	}

	[Theory]
	[InlineData(" ", "")]
	[InlineData("y ", "y")]
	[InlineData("	 y ", "Y", StringComparison.OrdinalIgnoreCase)]
	[InlineData("	 hello   ", "hello")]
	[InlineData("	hello   ", "Hello", StringComparison.OrdinalIgnoreCase)]
	public void TrimmedEqualsChars(string? value, string? other, StringComparison comparison = StringComparison.Ordinal)
	{
		Assert.True(value.TrimmedEquals(other, Chars.Span, comparison));
		Assert.True(value.TrimmedEquals(other!.AsSegment(), Chars.Span, comparison));
		Assert.True(value.TrimmedEquals(other.AsSpan(), Chars.Span, comparison));
	}

	[Fact]
	public void TrimmedEqualsCharNegativeCases()
	{
		Assert.False(" hello ".TrimmedEquals("world", ' '));
		Assert.False(" hello ".TrimmedEquals("world".AsSegment(), ' '));
		Assert.False(" hello ".TrimmedEquals("hell", ' '));
		Assert.False(" hello ".TrimmedEquals("hell".AsSegment(), ' '));
		Assert.False("x".TrimmedEquals("y".AsSegment(), ' '));
		Assert.False("  x  ".TrimmedEquals("y".AsSegment(), ' '));
	}

	[Fact]
	public void TrimmedEqualsCharsNegativeCases()
	{
		Assert.False("	 hello ".TrimmedEquals("world", Chars.Span));
		Assert.False("	 hello ".TrimmedEquals("world".AsSegment(), Chars.Span));
		Assert.False("	 hello ".TrimmedEquals("hell", Chars.Span));
		Assert.False("	 hello ".TrimmedEquals("hell".AsSegment(), Chars.Span));
		Assert.False("	x	".TrimmedEquals("y".AsSegment(), Chars.Span));
	}
}
