using System.Globalization;

namespace Open.Text.Tests;

[ExcludeFromCodeCoverage]
public static class CasingTests
{
	#region HasUpperCase Tests

	[Theory]
	[InlineData("HELLO", true)]
	[InlineData("Hello", true)]
	[InlineData("hEllo", true)]
	[InlineData("hello", false)]
	[InlineData("12345", false)]
	[InlineData("hello123", false)]
	[InlineData("HELLO123", true)]
	[InlineData("", false)]
	public static void HasUpperCase_String(string input, bool expected)
		=> Assert.Equal(expected, input.HasUpperCase());

	[Fact]
	public static void HasUpperCase_NullString_ReturnsFalse()
		=> Assert.False(((string?)null).HasUpperCase());

	[Theory]
	[InlineData("HELLO", true)]
	[InlineData("Hello", true)]
	[InlineData("hello", false)]
	[InlineData("", false)]
	public static void HasUpperCase_ReadOnlySpan(string input, bool expected)
		=> Assert.Equal(expected, input.AsSpan().HasUpperCase());

	[Theory]
	[InlineData("HELLO", true)]
	[InlineData("Hello", true)]
	[InlineData("hello", false)]
	public static void HasUpperCase_Span(string input, bool expected)
	{
		Span<char> span = input.ToCharArray();
		Assert.Equal(expected, span.HasUpperCase());
	}

	[Theory]
	[InlineData("HELLO", true)]
	[InlineData("Hello", true)]
	[InlineData("hello", false)]
	[InlineData("", false)]
	public static void HasUpperCase_StringSegment(string input, bool expected)
		=> Assert.Equal(expected, input.AsSegment().HasUpperCase());

	[Fact]
	public static void HasUpperCase_StringSegment_Subsegment()
	{
		var segment = "xxHELLOxx".AsSegment(2, 5); // "HELLO"
		Assert.True(segment.HasUpperCase());

		var lowerSegment = "xxhelloxx".AsSegment(2, 5); // "hello"
		Assert.False(lowerSegment.HasUpperCase());
	}

	[Fact]
	public static void HasUpperCase_DefaultStringSegment_ReturnsFalse()
		=> Assert.False(default(StringSegment).HasUpperCase());

	#endregion

	#region HasLowerCase Tests

	[Theory]
	[InlineData("hello", true)]
	[InlineData("Hello", true)]
	[InlineData("HELLo", true)]
	[InlineData("HELLO", false)]
	[InlineData("12345", false)]
	[InlineData("HELLO123", false)]
	[InlineData("hello123", true)]
	[InlineData("", false)]
	public static void HasLowerCase_String(string input, bool expected)
		=> Assert.Equal(expected, input.HasLowerCase());

	[Fact]
	public static void HasLowerCase_NullString_ReturnsFalse()
		=> Assert.False(((string?)null).HasLowerCase());

	[Theory]
	[InlineData("hello", true)]
	[InlineData("Hello", true)]
	[InlineData("HELLO", false)]
	[InlineData("", false)]
	public static void HasLowerCase_ReadOnlySpan(string input, bool expected)
		=> Assert.Equal(expected, input.AsSpan().HasLowerCase());

	[Theory]
	[InlineData("hello", true)]
	[InlineData("Hello", true)]
	[InlineData("HELLO", false)]
	public static void HasLowerCase_Span(string input, bool expected)
	{
		Span<char> span = input.ToCharArray();
		Assert.Equal(expected, span.HasLowerCase());
	}

	[Theory]
	[InlineData("hello", true)]
	[InlineData("Hello", true)]
	[InlineData("HELLO", false)]
	[InlineData("", false)]
	public static void HasLowerCase_StringSegment(string input, bool expected)
		=> Assert.Equal(expected, input.AsSegment().HasLowerCase());

	[Fact]
	public static void HasLowerCase_DefaultStringSegment_ReturnsFalse()
		=> Assert.False(default(StringSegment).HasLowerCase());

	#endregion

	#region ToUpperInvariant Tests

	[Theory]
	[InlineData("hello", "HELLO")]
	[InlineData("Hello", "HELLO")]
	[InlineData("HELLO", "HELLO")]
	[InlineData("hello123", "HELLO123")]
	[InlineData("", "")]
	public static void ToUpperInvariant_StringSegment(string input, string expected)
	{
		var result = input.AsSegment().ToUpperInvariant();
		Assert.Equal(expected, result.ToString());
	}

	[Fact]
	public static void ToUpperInvariant_Subsegment()
	{
		var segment = "xxhelloxx".AsSegment(2, 5); // "hello"
		var result = segment.ToUpperInvariant();
		Assert.Equal("HELLO", result.ToString());
	}

	[Fact]
	public static void ToUpperInvariant_AlreadyUppercase_ReturnsSameSegment()
	{
		var segment = "HELLO".AsSegment();
		var result = segment.ToUpperInvariant();
		// Should return the same segment when no conversion needed
		Assert.Equal("HELLO", result.ToString());
	}

	[Fact]
	public static void ToUpperInvariant_BufferMatchesSegment_ReturnsStringResult()
	{
		const string input = "hello";
		var segment = input.AsSegment();
		var result = segment.ToUpperInvariant();
		Assert.Equal("HELLO", result.ToString());
	}

	[Fact]
	public static void ToUpperInvariant_EmptySegment_ReturnsSameSegment()
	{
		var segment = "hello".AsSegment(2, 0); // empty segment
		var result = segment.ToUpperInvariant();
		Assert.Equal(0, result.Length);
		Assert.Same(segment.Buffer, result.Buffer);
	}

	[Fact]
	public static void ToUpperInvariant_DefaultSegment_Throws()
		=> Assert.Throws<ArgumentNullException>(() => default(StringSegment).ToUpperInvariant());

	#endregion

	#region ToLowerInvariant Tests

	[Theory]
	[InlineData("HELLO", "hello")]
	[InlineData("Hello", "hello")]
	[InlineData("hello", "hello")]
	[InlineData("HELLO123", "hello123")]
	[InlineData("", "")]
	public static void ToLowerInvariant_StringSegment(string input, string expected)
	{
		var result = input.AsSegment().ToLowerInvariant();
		Assert.Equal(expected, result.ToString());
	}

	[Fact]
	public static void ToLowerInvariant_Subsegment()
	{
		var segment = "xxHELLOxx".AsSegment(2, 5); // "HELLO"
		var result = segment.ToLowerInvariant();
		Assert.Equal("hello", result.ToString());
	}

	[Fact]
	public static void ToLowerInvariant_AlreadyLowercase_ReturnsSameSegment()
	{
		var segment = "hello".AsSegment();
		var result = segment.ToLowerInvariant();
		Assert.Equal("hello", result.ToString());
	}

	[Fact]
	public static void ToLowerInvariant_EmptySegment_ReturnsSameSegment()
	{
		var segment = "HELLO".AsSegment(2, 0); // empty segment
		var result = segment.ToLowerInvariant();
		Assert.Equal(0, result.Length);
		Assert.Same(segment.Buffer, result.Buffer);
	}

	[Fact]
	public static void ToLowerInvariant_DefaultSegment_Throws()
		=> Assert.Throws<ArgumentNullException>(() => default(StringSegment).ToLowerInvariant());

	#endregion

	#region ToUpper Tests

	[Theory]
	[InlineData("hello", "HELLO")]
	[InlineData("Hello", "HELLO")]
	[InlineData("HELLO", "HELLO")]
	[InlineData("", "")]
	public static void ToUpper_StringSegment(string input, string expected)
	{
		var result = input.AsSegment().ToUpper();
		Assert.Equal(expected, result.ToString());
	}

	[Fact]
	public static void ToUpper_WithCulture()
	{
		var segment = "hello".AsSegment();
		var result = segment.ToUpper(CultureInfo.InvariantCulture);
		Assert.Equal("HELLO", result.ToString());
	}

	[Fact]
	public static void ToUpper_Subsegment()
	{
		var segment = "xxhelloxx".AsSegment(2, 5); // "hello"
		var result = segment.ToUpper();
		Assert.Equal("HELLO", result.ToString());
	}

	[Fact]
	public static void ToUpper_AlreadyUppercase_ReturnsSameSegment()
	{
		var segment = "HELLO".AsSegment();
		var result = segment.ToUpper();
		Assert.Equal("HELLO", result.ToString());
	}

	[Fact]
	public static void ToUpper_EmptySegment_ReturnsSameSegment()
	{
		var segment = "hello".AsSegment(2, 0);
		var result = segment.ToUpper();
		Assert.Equal(0, result.Length);
		Assert.Same(segment.Buffer, result.Buffer);
	}

	[Fact]
	public static void ToUpper_DefaultSegment_Throws()
		=> Assert.Throws<ArgumentNullException>(() => default(StringSegment).ToUpper());

	#endregion

	#region ToLower Tests

	[Theory]
	[InlineData("HELLO", "hello")]
	[InlineData("Hello", "hello")]
	[InlineData("hello", "hello")]
	[InlineData("", "")]
	public static void ToLower_StringSegment(string input, string expected)
	{
		var result = input.AsSegment().ToLower();
		Assert.Equal(expected, result.ToString());
	}

	[Fact]
	public static void ToLower_WithCulture()
	{
		var segment = "HELLO".AsSegment();
		var result = segment.ToLower(CultureInfo.InvariantCulture);
		Assert.Equal("hello", result.ToString());
	}

	[Fact]
	public static void ToLower_Subsegment()
	{
		var segment = "xxHELLOxx".AsSegment(2, 5); // "HELLO"
		var result = segment.ToLower();
		Assert.Equal("hello", result.ToString());
	}

	[Fact]
	public static void ToLower_AlreadyLowercase_ReturnsSameSegment()
	{
		var segment = "hello".AsSegment();
		var result = segment.ToLower();
		Assert.Equal("hello", result.ToString());
	}

	[Fact]
	public static void ToLower_EmptySegment_ReturnsSameSegment()
	{
		var segment = "HELLO".AsSegment(2, 0);
		var result = segment.ToLower();
		Assert.Equal(0, result.Length);
		Assert.Same(segment.Buffer, result.Buffer);
	}

	[Fact]
	public static void ToLower_DefaultSegment_Throws()
		=> Assert.Throws<ArgumentNullException>(() => default(StringSegment).ToLower());

	#endregion

	#region Edge Cases

	[Fact]
	public static void CaseConversion_MixedContent()
	{
		var segment = "HeLLo WoRLd 123!@#".AsSegment();

		var upper = segment.ToUpperInvariant();
		Assert.Equal("HELLO WORLD 123!@#", upper.ToString());

		var lower = segment.ToLowerInvariant();
		Assert.Equal("hello world 123!@#", lower.ToString());
	}

	[Fact]
	public static void CaseConversion_UnicodeCharacters()
	{
		// Test with accented characters
		var segment = "Héllo Wörld".AsSegment();

		var upper = segment.ToUpperInvariant();
		Assert.Equal("HÉLLO WÖRLD", upper.ToString());

		var lower = segment.ToLowerInvariant();
		Assert.Equal("héllo wörld", lower.ToString());
	}

	[Fact]
	public static void CaseConversion_NumbersAndSymbolsOnly()
	{
		var segment = "12345!@#$%".AsSegment();

		// No case to convert, should return same segment
		var upper = segment.ToUpperInvariant();
		Assert.Equal("12345!@#$%", upper.ToString());

		var lower = segment.ToLowerInvariant();
		Assert.Equal("12345!@#$%", lower.ToString());
	}

	[Fact]
	public static void HasCase_SubsegmentBoundaries()
	{
		// Test that case detection respects segment boundaries
		const string fullString = "ABCdefGHI";

		var lowerSegment = fullString.AsSegment(3, 3); // "def"
		Assert.False(lowerSegment.HasUpperCase());
		Assert.True(lowerSegment.HasLowerCase());

		var upperSegment = fullString.AsSegment(0, 3); // "ABC"
		Assert.True(upperSegment.HasUpperCase());
		Assert.False(upperSegment.HasLowerCase());

		var mixedSegment = fullString.AsSegment(2, 4); // "Cdef"
		Assert.True(mixedSegment.HasUpperCase());
		Assert.True(mixedSegment.HasLowerCase());
	}

	#endregion
}
