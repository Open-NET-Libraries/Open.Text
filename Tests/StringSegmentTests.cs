using System.Text.RegularExpressions;
using ZLinq;

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

	// Unit tests for Replace/ReplaceNoAlloc
	[Theory]
	[InlineData("Hello world", "world", "universe", "Hello universe")]
	[InlineData("Hello world world", "world", "universe", "Hello universe universe")]
	[InlineData("aaa", "a", "bb", "bbbbbb")]
	public static void Replace(string input, string search, string replace, string expected)
	{
		var segment = input.AsSegment();
		// Original Replace
		var result = segment.Replace(search, replace).JoinToString("");
		Assert.Equal(expected, result);
		// NoAlloc variant should produce identical result
		var resultNoAlloc = segment.ReplaceNoAlloc(search, replace).Select(s => s.ToString()).ToArray();
		Assert.Equal(expected, string.Concat(resultNoAlloc));
	}

	// Unit tests for ReplaceAsSegments/ReplaceAsSegmentsNoAlloc
	[Theory]
	[InlineData("Hello world", "world", "universe", "Hello universe")]
	[InlineData("abc123def456ghi", @"\d+", "NUM", "abcNUMdefNUMghi")]
	public static void ReplaceAsSegments(string input, string pattern, string replace, string expected)
	{
		if (pattern.StartsWith(@"\"))
		{
			// Regex pattern
			var regex = new Regex(pattern);
			var result = input.ReplaceAsSegments(regex, replace).Select(s => s.ToString()).ToArray();
			Assert.Equal(expected, string.Concat(result));
			// NoAlloc variant
			var resultNoAlloc = input.ReplaceAsSegmentsNoAlloc(regex, replace).Select(s => s.ToString()).ToArray();
			Assert.Equal(expected, string.Concat(resultNoAlloc));
		}
		else
		{
			// String pattern
			var result = input.ReplaceAsSegments(pattern, replace).Select(s => s.ToString()).ToArray();
			Assert.Equal(expected, string.Concat(result));
			// NoAlloc variant
			var resultNoAlloc = input.ReplaceAsSegmentsNoAlloc(pattern, replace).Select(s => s.ToString()).ToArray();
			Assert.Equal(expected, string.Concat(resultNoAlloc));
		}
	}

	// Unit tests for Join/JoinNoAlloc
	[Theory]
	[InlineData("Hello,there,I,am,Joe", ",", "-", "Hello-there-I-am-Joe")]
	[InlineData("one::two::three", "::", " | ", "one | two | three")]
	public static void Join(string input, string splitBy, string joinBy, string expected)
	{
		var segments = input.SplitAsSegments(splitBy).ToArray().AsEnumerable();
		// Original Join
		var result = segments.Join(joinBy).JoinToString("");
		Assert.Equal(expected, result);
		// NoAlloc variant
		var resultNoAlloc = segments.JoinNoAlloc(joinBy).Select(s => s.ToString()).ToArray();
		Assert.Equal(expected, string.Concat(resultNoAlloc));
	}

	// Unit tests for JoinToStringBuilder nullable behavior (Open.Text base library)
	[Fact]
	public static void JoinToStringBuilder_ReturnsNull_ForEmptySegments()
	{
		// Empty source with RemoveEmptyEntries should return null
		var emptySegments = "".SplitAsSegments(',', StringSplitOptions.RemoveEmptyEntries);
		var result = emptySegments.JoinToStringBuilder("-");
		Assert.Null(result);

		// JoinToString should return empty string for empty source
		var stringResult = emptySegments.JoinToString("-");
		Assert.Equal(string.Empty, stringResult);
	}

	[Fact]
	public static void JoinToStringBuilder_ReturnsStringBuilder_ForNonEmptySegments()
	{
		var segments = "a,b,c".SplitAsSegments(',');
		var result = segments.JoinToStringBuilder("-");
		Assert.NotNull(result);
		Assert.Equal("a-b-c", result!.ToString());
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

	// Unit tests for JoinToString on ZLinq ValueEnumerable
	[Theory]
	[InlineData("Hello,there,I,am,Joe", ",", "", "HellothereIamJoe")]
	[InlineData("Hello,there,I,am,Joe", ",", "-", "Hello-there-I-am-Joe")]
	[InlineData("Hello,there,I,am,Joe", ",", " | ", "Hello | there | I | am | Joe")]
	[InlineData("one::two::three", "::", "", "onetwothree")]
	[InlineData("one::two::three", "::", "-", "one-two-three")]
	[InlineData("a,b", ",", "---", "a---b")]
	[InlineData("single", ",", "-", "single")]
	[InlineData("", ",", "-", "")]
	public static void JoinToString_NoAlloc(string input, string splitBy, string joinBy, string expected)
	{
		// Test JoinToString on character split
		if (splitBy.Length == 1)
		{
			var result = input.SplitAsSegmentsNoAlloc(splitBy[0]).JoinToString(joinBy);
			Assert.Equal(expected, result);
		}

		// Test JoinToString on sequence split
		var resultSeq = input.SplitAsSegmentsNoAlloc(splitBy).JoinToString(joinBy);
		Assert.Equal(expected, resultSeq);
	}

	[Theory]
	[InlineData("Hello,there,I,am,Joe", ",", "", "HellothereIamJoe")]
	[InlineData("Hello,there,I,am,Joe", ",", "-", "Hello-there-I-am-Joe")]
	[InlineData("one::two::three", "::", " | ", "one | two | three")]
	[InlineData("single", ",", "-", "single")]
	public static void JoinToStringBuilder_NoAlloc(string input, string splitBy, string joinBy, string expected)
	{
		// Test JoinToStringBuilder with null StringBuilder (creates new)
		var sbResult = input.SplitAsSegmentsNoAlloc(splitBy).JoinToStringBuilder(joinBy);
		Assert.NotNull(sbResult);
		Assert.Equal(expected, sbResult!.ToString());

		// Test JoinToStringBuilder with existing StringBuilder
		var existingSb = new System.Text.StringBuilder("PREFIX:");
		var sbResult2 = input.SplitAsSegmentsNoAlloc(splitBy).JoinToStringBuilder(existingSb, joinBy);
		Assert.Same(existingSb, sbResult2);
		Assert.Equal("PREFIX:" + expected, sbResult2!.ToString());
	}

	[Fact]
	public static void JoinToStringBuilder_EmptySource_ReturnsNullOrExisting()
	{
		// Empty string split produces one empty segment, so result is not null
		var result = "".SplitAsSegmentsNoAlloc(",").JoinToStringBuilder(null, "-");
		Assert.NotNull(result);
		Assert.Equal("", result!.ToString());

		// With existing StringBuilder, appends the empty segment
		var existingSb = new System.Text.StringBuilder("EXISTING");
		var result2 = "".SplitAsSegmentsNoAlloc(",").JoinToStringBuilder(existingSb, "-");
		Assert.Same(existingSb, result2);
		Assert.Equal("EXISTING", result2!.ToString());

		// Test with RemoveEmptyEntries option - should yield no segments and return null for new StringBuilder
		var result3 = "".SplitAsSegmentsNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).JoinToStringBuilder(null, "-");
		Assert.Null(result3);

		// With RemoveEmptyEntries and existing StringBuilder, returns unchanged
		var existingSb2 = new System.Text.StringBuilder("UNCHANGED");
		var result4 = "".SplitAsSegmentsNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).JoinToStringBuilder(existingSb2, "-");
		Assert.Same(existingSb2, result4);
		Assert.Equal("UNCHANGED", result4!.ToString());
	}

	[Fact]
	public static void JoinToString_WithNoSeparator()
	{
		var result = "Hello,there,world".SplitAsSegmentsNoAlloc(',').JoinToString();
		Assert.Equal("Hellothereworld", result);
	}

	[Fact]
	public static void JoinToString_SingleElement()
	{
		var result = "Hello".SplitAsSegmentsNoAlloc(',').JoinToString("-");
		Assert.Equal("Hello", result);
	}

	[Fact]
	public static void JoinToString_RegexSplit()
	{
		var regex = new Regex(@"\d+");
		var result = "abc123def456ghi".SplitAsSegmentsNoAlloc(regex).JoinToString("-");
		Assert.Equal("abc-def-ghi", result);
	}

	[Theory]
	[InlineData("a,b,c", ",", "X", "aXbXc")]
	[InlineData("hello world hello", " ", "-", "hello-world-hello")]
	public static void JoinToString_CharSplit(string input, string splitChar, string joinBy, string expected)
	{
		var result = input.SplitAsSegmentsNoAlloc(splitChar[0]).JoinToString(joinBy);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("Hello,there,,world", ",", StringSplitOptions.None, "-", "Hello-there--world")]
	[InlineData("Hello,there,,world", ",", StringSplitOptions.RemoveEmptyEntries, "-", "Hello-there-world")]
	public static void JoinToString_WithOptions(string input, string splitBy, StringSplitOptions options, string joinBy, string expected)
	{
		var result = input.SplitAsSegmentsNoAlloc(splitBy[0], options).JoinToString(joinBy);
		Assert.Equal(expected, result);
	}
}
