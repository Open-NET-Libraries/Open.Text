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

	#region ZLinq Integration Tests

	// These tests prove that our JoinToString extension works correctly downstream of ZLinq operations

	[Fact]
	public static void ZLinq_SplitWhereJoin_FilterByLength()
	{
		// Split, filter segments with length > 3, then join
		const string input = "a,bb,ccc,dddd,eeeee,ff";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Where(s => s.Length > 3)
			.JoinToString("-");

		Assert.Equal("dddd-eeeee", result);
	}

	[Fact]
	public static void ZLinq_SplitWhereJoin_FilterByContent()
	{
		// Split, filter segments containing 'o', then join
		const string input = "hello,world,foo,bar,boo";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Where(s => s.AsSpan().Contains('o', StringComparison.Ordinal))
			.JoinToString(" | ");

		Assert.Equal("hello | world | foo | boo", result);
	}

	[Fact]
	public static void ZLinq_SplitSelectJoin_TransformAndJoin()
	{
		// Split, select first character of each, then join
		const string input = "apple,banana,cherry,date";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Select(s => s.Subsegment(0, 1))
			.JoinToString("");

		Assert.Equal("abcd", result);
	}

	[Fact]
	public static void ZLinq_SplitTakeJoin()
	{
		// Split and take first N segments
		const string input = "one,two,three,four,five";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Take(3)
			.JoinToString("-");

		Assert.Equal("one-two-three", result);
	}

	[Fact]
	public static void ZLinq_SplitSkipJoin()
	{
		// Split and skip first N segments
		const string input = "one,two,three,four,five";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Skip(2)
			.JoinToString("-");

		Assert.Equal("three-four-five", result);
	}

	[Fact]
	public static void ZLinq_SplitSkipTakeJoin()
	{
		// Split, skip, take (pagination pattern)
		const string input = "a,b,c,d,e,f,g,h";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Skip(2)
			.Take(3)
			.JoinToString(",");

		Assert.Equal("c,d,e", result);
	}

	[Fact]
	public static void ZLinq_SplitWhereSelectJoin_ComplexPipeline()
	{
		// Complex pipeline: split, filter, transform, join
		const string input = "Item1:10,Item2:25,Item3:5,Item4:30,Item5:15";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Where(s => s.Length > 7) // Filter items with value >= 10 (longer strings)
			.Select(s => s.Subsegment(0, s.IndexOf(':'))) // Extract just the name
			.JoinToString("; ");

		Assert.Equal("Item1; Item2; Item4; Item5", result);
	}

	[Fact]
	public static void ZLinq_SplitDistinctJoin()
	{
		// Split and get distinct values
		const string input = "apple,banana,apple,cherry,banana,date";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Distinct()
			.JoinToString(",");

		Assert.Equal("apple,banana,cherry,date", result);
	}

	[Fact]
	public static void ZLinq_SplitOrderByJoin()
	{
		// Split and order alphabetically
		const string input = "cherry,apple,banana,date";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.OrderBy(s => s.ToString())
			.JoinToString(",");

		Assert.Equal("apple,banana,cherry,date", result);
	}

	[Fact]
	public static void ZLinq_ChainedSplitOperations()
	{
		// Split by one delimiter, filter, then join with another
		const string input = "key1=value1;key2=value2;key3=value3";
		var result = input
			.SplitAsSegmentsNoAlloc(';')
			.Where(s => !s.AsSpan().Contains('1', StringComparison.Ordinal))
			.JoinToString(" & ");

		Assert.Equal("key2=value2 & key3=value3", result);
	}

	[Fact]
	public static void ZLinq_EmptyAfterFilter()
	{
		// All segments filtered out should produce empty string
		const string input = "a,b,c";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Where(s => s.Length > 10) // Nothing matches
			.JoinToString("-");

		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public static void ZLinq_SingleElementAfterFilter()
	{
		// Single element remaining after filter
		const string input = "short,verylongword,tiny";
		var result = input
			.SplitAsSegmentsNoAlloc(',')
			.Where(s => s.Length > 5)
			.JoinToString("-");

		Assert.Equal("verylongword", result);
	}

	[Theory]
	[InlineData("a::b::c::d", "::", 2, "a-b")]
	[InlineData("one||two||three||four", "||", 3, "one-two-three")]
	public static void ZLinq_SequenceSplit_TakeJoin(string input, string delimiter, int takeCount, string expected)
	{
		var result = input
			.SplitAsSegmentsNoAlloc(delimiter)
			.Take(takeCount)
			.JoinToString("-");

		Assert.Equal(expected, result);
	}

	[Fact]
	public static void ZLinq_RegexSplit_WhereJoin()
	{
		// Split by regex (digits), filter, join
		var regex = new Regex(@"\d+");
		var result = "abc123def456ghi789jkl"
			.SplitAsSegmentsNoAlloc(regex)
			.Where(s => s.Length == 3)
			.JoinToString("-");

		Assert.Equal("abc-def-ghi-jkl", result);
	}

	[Fact]
	public static void ZLinq_JoinToStringBuilder_Direct()
	{
		// Test JoinToStringBuilder works directly on split result (no intermediate operations)
		const string input = "alpha,beta,gamma,delta";
		var sb = input
			.SplitAsSegmentsNoAlloc(',')
			.JoinToStringBuilder("-");

		Assert.NotNull(sb);
		Assert.Equal("alpha-beta-gamma-delta", sb!.ToString());
	}

	[Fact]
	public static void ZLinq_JoinToStringBuilder_AppendToExisting()
	{
		// Append split result to existing StringBuilder (no intermediate operations)
		var existingSb = new System.Text.StringBuilder("Result: ");
		const string input = "x,y,z";
		var sb = input
			.SplitAsSegmentsNoAlloc(',')
			.JoinToStringBuilder(existingSb, "-");

		Assert.Same(existingSb, sb);
		Assert.Equal("Result: x-y-z", sb!.ToString());
	}

	// Test IEnumerable<StringSegment> -> ValueEnumerable -> JoinToString
	[Fact]
	public static void ZLinq_FromIEnumerable_AsValueEnumerable_Join()
	{
		// Start with IEnumerable<StringSegment>, convert to ValueEnumerable, then join
		IEnumerable<StringSegment> segments = "a,b,c,d,e".SplitAsSegments(',');
		var result = segments
			.AsValueEnumerable()
			.Where(s => s.Length == 1)
			.JoinToString("-");

		Assert.Equal("a-b-c-d-e", result);
	}

	[Fact]
	public static void ZLinq_FromIEnumerable_FilterAndJoin()
	{
		// IEnumerable -> ValueEnumerable -> Filter -> Join
		IEnumerable<StringSegment> segments = "apple,banana,apricot,cherry".SplitAsSegments(',');
		var result = segments
			.AsValueEnumerable()
			.Where(s => s.AsSpan().StartsWith("a".AsSpan(), StringComparison.Ordinal))
			.JoinToString(", ");

		Assert.Equal("apple, apricot", result);
	}

	[Fact]
	public static void ZLinq_FromArray_AsValueEnumerable_Join()
	{
		// Array of StringSegment -> ValueEnumerable -> Join
		var segments = new[]
		{
			"first".AsSegment(),
			"second".AsSegment(),
			"third".AsSegment()
		};

		var result = segments
			.AsValueEnumerable()
			.Skip(1)
			.JoinToString(" -> ");

		Assert.Equal("second -> third", result);
	}

	[Fact]
	public static void ZLinq_FromList_ComplexPipeline()
	{
		// List<StringSegment> -> ValueEnumerable -> complex operations -> Join
		var list = new List<StringSegment>
		{
			"Item-A".AsSegment(),
			"Item-B".AsSegment(),
			"Thing-C".AsSegment(),
			"Item-D".AsSegment()
		};

		var result = list
			.AsValueEnumerable()
			.Where(s => s.AsSpan().StartsWith("Item".AsSpan(), StringComparison.Ordinal))
			.Select(s => s.Subsegment(5)) // Get part after "Item-"
			.JoinToString(",");

		Assert.Equal("A,B,D", result);
	}

	[Theory]
	[InlineData("1,2,3,4,5", 2, "3,4,5")]
	[InlineData("a;b;c;d", 1, "b;c;d")]
	public static void ZLinq_FromSplitAsSegments_ToValueEnumerable_SkipJoin(string input, int skipCount, string expected)
	{
		char delimiter = input.Contains(';') ? ';' : ',';
		string joinDelimiter = delimiter.ToString();

		IEnumerable<StringSegment> segments = input.SplitAsSegments(delimiter);
		var result = segments
			.AsValueEnumerable()
			.Skip(skipCount)
			.JoinToString(joinDelimiter);

		Assert.Equal(expected, result);
	}

	[Fact]
	public static void ZLinq_SelectMany_FromStringArray_SplitAndFlatten()
	{
		// Start with string[], split each, flatten all segments, then join
		var lines = new[] { "a,b,c", "d,e,f", "g,h,i" };
		var result = lines
			.AsValueEnumerable()
			.SelectMany(line => line.SplitAsSegmentsNoAlloc(','))
			.JoinToString("-");

		Assert.Equal("a-b-c-d-e-f-g-h-i", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_FilterThenFlatten()
	{
		// Filter lines first, then split and flatten
		var lines = new[] { "keep:a,b,c", "skip:x,y,z", "keep:d,e,f" };
		var result = lines
			.AsValueEnumerable()
			.Where(line => line.StartsWith("keep:"))
			.SelectMany(line => line.AsSegment().Subsegment(5).SplitAsSegmentsNoAlloc(','))
			.JoinToString(" ");

		Assert.Equal("a b c d e f", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_FlattenThenFilter()
	{
		// Split and flatten first, then filter the segments
		var lines = new[] { "apple,apricot,banana", "avocado,blueberry,cherry" };
		var result = lines
			.AsValueEnumerable()
			.SelectMany(line => line.SplitAsSegmentsNoAlloc(','))
			.Where(s => s.AsSpan().StartsWith("a".AsSpan(), StringComparison.Ordinal))
			.JoinToString(", ");

		Assert.Equal("apple, apricot, avocado", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_FromIEnumerableStrings()
	{
		// Start with IEnumerable<string>
		IEnumerable<string> lines = ["1,2,3", "4,5,6"];
		var result = lines
			.AsValueEnumerable()
			.SelectMany(line => line.SplitAsSegmentsNoAlloc(','))
			.JoinToString("+");

		Assert.Equal("1+2+3+4+5+6", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_NestedSplit()
	{
		// Split by one delimiter, then split each result by another
		const string input = "a.b.c|d.e.f|g.h.i";
		var result = input
			.SplitAsSegmentsNoAlloc('|')
			.SelectMany(segment => segment.SplitAsSegmentsNoAlloc('.'))
			.JoinToString("-");

		Assert.Equal("a-b-c-d-e-f-g-h-i", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_WithTake()
	{
		// Flatten then take first N
		var lines = new[] { "a,b,c", "d,e,f", "g,h,i" };
		var result = lines
			.AsValueEnumerable()
			.SelectMany(line => line.SplitAsSegmentsNoAlloc(','))
			.Take(5)
			.JoinToString(",");

		Assert.Equal("a,b,c,d,e", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_EmptyLines()
	{
		// Handle empty strings in the source
		var lines = new[] { "a,b", "", "c,d" };
		var result = lines
			.AsValueEnumerable()
			.Where(line => line.Length > 0) // Filter out empty
			.SelectMany(line => line.SplitAsSegmentsNoAlloc(','))
			.JoinToString("-");

		Assert.Equal("a-b-c-d", result);
	}

	[Fact]
	public static void ZLinq_SelectMany_CSVRows()
	{
		// Realistic scenario: process CSV-like data
		var csvRows = new[]
		{
			"Name,Age,City",
			"Alice,30,NYC",
			"Bob,25,LA"
		};

		// Extract just the names (first column of data rows)
		var names = csvRows
			.AsValueEnumerable()
			.Skip(1) // Skip header
			.SelectMany(row => row.SplitAsSegmentsNoAlloc(',').Take(1))
			.JoinToString(" and ");

		Assert.Equal("Alice and Bob", names);
	}

	[Fact]
	public static void ZLinq_SelectMany_KeyValuePairs()
	{
		// Parse key=value pairs from multiple config lines
		var configLines = new[] { "host=localhost;port=8080", "timeout=30;retries=3" };

		// Extract all keys
		var keys = configLines
			.AsValueEnumerable()
			.SelectMany(line => line.SplitAsSegmentsNoAlloc(';'))
			.Select(pair => pair.Subsegment(0, pair.IndexOf('=')))
			.JoinToString(", ");

		Assert.Equal("host, port, timeout, retries", keys);
	}

	#endregion
}
