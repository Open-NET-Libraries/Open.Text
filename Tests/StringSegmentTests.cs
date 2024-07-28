using Microsoft.Extensions.Primitives;

namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class StringSegmentTests
{
	[Fact]
	public static void Validity()
	{
		var s = default(StringSegment);
		Assert.False(s.HasValue);
		Assert.Equal(s, s.Buffer.AsSegment());
		Assert.Throws<ArgumentOutOfRangeException>(() => s.Subsegment(0, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => s.Subsegment(1, 1));
		Assert.True(s == s.Buffer.AsSegment());
		Assert.Equal(0, ((ReadOnlySpan<char>)s).Length);
		Assert.Equal(0, ((ReadOnlyMemory<char>)s).Length);
	}

	[Theory]
	[InlineData("Hello well how are you", "o", "Hell")]
	[InlineData("Hello well how are you", "ll", "He")]
	[InlineData("Hello well how are you", "Ll", "He", StringComparison.OrdinalIgnoreCase)]
	[InlineData("Hello well how are you", "Hell", "")]
	[InlineData("Hello well how are you", "xxx", null)]
	public static void BeforeFirst(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
	{
		var first = source.First(pattern, comparisonType);
		if (expected is null)
		{
			Assert.False(first.HasValue);
		}
		else
		{
			var p = first.Preceding();
			var pString = p.ToString();
			pString.Should().Be(expected);

			pString.Equals(expected, comparisonType).Should().BeTrue();
			p.AsSpan().Equals(expected, comparisonType).Should().BeTrue();
			p.AsMemory().Span.Equals(expected, comparisonType).Should().BeTrue();
		}
	}

	[Theory]
	[InlineData("Hello well how are you", "o", " well how are you")]
	[InlineData("Hello well how are you", "ll", "o well how are you")]
	[InlineData("Hello well how are you", "Ll", "o well how are you", StringComparison.OrdinalIgnoreCase)]
	[InlineData("Hello well how are you", "Hell", "o well how are you")]
	[InlineData("Hello well how are you", "xxx", null)]
	public static void AfterFirst(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
	{
		var first = source.First(pattern, comparisonType);
		if (expected is null) Assert.False(first.HasValue);
		else Assert.Equal(expected, first.Following().ToString());
	}

	[Theory]
	[InlineData("Hello well how are you", "o", "Hello well how are y")]
	[InlineData("Hello well how are you", "ll", "Hello we")]
	[InlineData("Hello well how are you", "Ll", "Hello we", StringComparison.OrdinalIgnoreCase)]
	[InlineData("Hello well how are you", "Hell", "")]
	[InlineData("Hello well how are you", "xxx", null)]
	public static void BeforeLast(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
	{
		var last = source.Last(pattern, comparisonType);
		if (expected is null) Assert.False(last.HasValue);
		else Assert.Equal(expected, last.Preceding().ToString());
	}

	[Theory]
	[InlineData("Hello well how are you", "o", "u")]
	[InlineData("Hello well how are you", "ll", " how are you")]
	[InlineData("Hello well how are you", "Ll", " how are you", StringComparison.OrdinalIgnoreCase)]
	[InlineData("Hello well how are you", "Hell", "o well how are you")]
	[InlineData("Hello well how are you", "xxx", null)]
	public static void AfterLast(string source, string pattern, string? expected, StringComparison comparisonType = StringComparison.Ordinal)
	{
		var last = source.Last(pattern, comparisonType);
		if (expected is null) Assert.False(last.HasValue);
		else Assert.Equal(expected, last.Following().ToString());
	}

	[Fact]
	public static void Offsets()
	{
		var segment = "Hello well how are you".First("well how");
		Assert.Equal("Hello ", segment.Preceding(100).ToString());
		Assert.Equal("o ", segment.Preceding(2).ToString());
		Assert.Equal(" are you", segment.Following(100).ToString());
		Assert.Equal(" a", segment.Following(2).ToString());
		Assert.Equal("Hello well how", segment.Preceding(100, true).ToString());
		Assert.Equal("o well how", segment.Preceding(2, true).ToString());
		Assert.Equal("well how are you", segment.Following(100, true).ToString());
		Assert.Equal("well how a", segment.Following(2, true).ToString());
		Assert.Equal("well how", segment.ToString());
		Assert.Equal("well", segment.OffsetEnd(-4).ToString());
		Assert.Equal("how", segment.Subsegment(+5).ToString());
		Assert.Equal("well", segment.OffsetEnd(-4).ToString());
		Assert.Equal("how", segment.OffsetStart(+5).ToString());
		Assert.Equal("well how are", segment.OffsetEnd(+4).ToString());
		Assert.Equal("Hello well how", segment.OffsetStart(-6).ToString());
		Assert.Throws<ArgumentOutOfRangeException>(() => segment.OffsetStart(-7));
		Assert.Throws<ArgumentOutOfRangeException>(() => segment.OffsetEnd(9));
	}

	[Fact]
	public static void Slice()
	{
		var segment = "Hello well how are you".First("well how");
		Assert.Equal("well how", segment.ToString());
		var slice = segment.Subsegment(0, 4);
		Assert.Equal("well", slice.ToString());
		Assert.True(slice.SequenceEqual("well".AsSpan()));
		Assert.Throws<ArgumentException>(() => slice.Subsegment(5, 5));
		Assert.Throws<ArgumentException>(() => slice.Subsegment(0, 5));
		Assert.True(slice.Slice(1, 6, true).Equals("ell ho"));
		Assert.True(slice.Slice(1, 7, true).Equals("ell how"));
		Assert.True(slice.Slice(1, 15, true).Equals("ell how are you"));
		Assert.Throws<ArgumentOutOfRangeException>(() => slice.Slice(1, 16, true));
	}

	[Theory]
	[InlineData("xyz")]
	[InlineData(" xyz")]
	[InlineData("  xyz ")]
	[InlineData(" \t xyz ")]
	public static void Trim(string input)
	{
		var segment = input.AsSegment();
		Assert.Equal(input.TrimEnd(), segment.TrimEnd().Value);
		Assert.Equal(input.TrimStart(), segment.TrimStart().Value);
		Assert.Equal(input.Trim(), segment.Trim().Value);
	}

	[Theory]
	[InlineData("xyz", '*')]
	[InlineData("*xyz", '*')]
	[InlineData("**xyz*", '*')]
	[InlineData("*xyz**", '*')]
	public static void TrimChar(string input, char c)
	{
		var segment = input.AsSegment();
		Assert.Equal(input.TrimEnd(c), segment.TrimEnd(c).Value);
		Assert.Equal(input.TrimStart(c), segment.TrimStart(c).Value);
		Assert.Equal(input.Trim(c), segment.Trim(c).Value);
	}

	[Theory]
	[InlineData("xyz", '*', '$')]
	[InlineData("*xyz", '*', '$')]
	[InlineData("**xyz*", '*', '$')]
	[InlineData("*xyz**", '*', '$')]
	[InlineData("*$*xyz*", '*', '$')]
	[InlineData("*xyz*$", '*', '$')]
	[InlineData("$*xyz*", '*', '$')]
	public static void TrimChars(string input, params char[] c)
	{
		var segment = input.AsSegment();
		Assert.Equal(input.TrimEnd(c), segment.TrimEnd(c).Value);
		Assert.Equal(input.TrimStart(c), segment.TrimStart(c).Value);
		Assert.Equal(input.Trim(c), segment.Trim(c).Value);
	}

	// Unit test for .ReplaceEach
	[Theory]
	// simple cases
	[InlineData("Hello world", "Hello", "Goodbye", "Goodbye world")]
	[InlineData("Hello world", "world", "universe", "Hello universe")]
	[InlineData("Hello world", "Hello world", "Goodbye universe", "Goodbye universe")]
	public static void ReplaceEach(string input, string search, string replace, string expected)
	{
		var segment = new[] { input.AsSegment() };
		var result = segment.ReplaceEach(search, replace).JoinToString("");
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("Hello, world!", "world", 0, StringComparison.Ordinal, 7)]
	[InlineData("Hello, world!", "WORLD", 0, StringComparison.Ordinal, -1)]
	[InlineData("Hello, world!", "WORLD", 0, StringComparison.OrdinalIgnoreCase, 7)]
	[InlineData("abcdefg", "cde", 0, StringComparison.Ordinal, 2)]
	[InlineData("abcdefg", "CDE", 0, StringComparison.Ordinal, -1)]
	[InlineData("abcdefg", "CDE", 0, StringComparison.OrdinalIgnoreCase, 2)]
	[InlineData("Hello, world! world!", "world", 8, StringComparison.Ordinal, 14)]
	[InlineData("Hello, world! world!", "WORLD", 8, StringComparison.Ordinal, -1)]
	[InlineData("Hello, world! world!", "WORLD", 8, StringComparison.OrdinalIgnoreCase, 14)]
	public static void IndexOf_FindsSubstringWithCaseVariations(
		string input,
		string value,
		int startIndex,
		StringComparison comparisonType,
		int expectedResult)
	{
		StringSegment segment = input;
		StringSegment valueSegment = value;

		int result = segment.IndexOf(valueSegment, startIndex, comparisonType);
		Assert.Equal(expectedResult, result);
		result = segment.IndexOf(valueSegment.AsSpan(), startIndex, comparisonType);
		Assert.Equal(expectedResult, result);
	}
}
