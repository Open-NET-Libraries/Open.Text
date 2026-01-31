namespace Open.Text.Tests;

/// <summary>
/// <para>
/// These tests verify assumptions about how .NET pattern matching works with custom types.
/// DISCOVERY: Pattern matching with 'is' does NOT work with StringComparable!
/// The IDE inspection suggesting pattern matching is a FALSE POSITIVE.
/// </para>
/// <para>
/// SOLUTION: Install the optional Open.Text.Analyzers NuGet package to automatically
/// suppress these incorrect IDE suggestions when using StringComparable/SpanComparable.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
public static class PatternMatchingAssumptionTests
{
	[Fact]
	[SuppressMessage("Style", "IDE0078:Use pattern matching", Justification = "<Pending>")]
	[SuppressMessage("Roslynator", "RCS1118:Mark local variable as const", Justification = "<Pending>")]
	public static void Baseline_PatternMatching_WithRegularString_Works()
	{
		// ? Pattern matching works fine with regular strings
		string value = "hello";
		bool oldStyleOr = value == "hello" || value == "there";
		bool patternStyleOr = value is "hello" or "there";

		Assert.True(oldStyleOr);
		Assert.True(patternStyleOr);
		Assert.Equal(oldStyleOr, patternStyleOr);
	}

	[Fact]
	public static void DISCOVERY_PatternMatching_DoesNotCompile_WithStringComparable()
	{
		// ? CRITICAL: Pattern matching does NOT work with StringComparable
		// The IDE suggests converting == to 'is', but it won't compile!

		var comparable = "hello".AsCaseInsensitive();

		// ? This works - uses operator overload
		bool operatorMatch = comparable == "hello";
		Assert.True(operatorMatch, "Operator == works correctly");

		// ? This DOES NOT COMPILE:
		// bool patternMatch = comparable is "hello";
		// Error: CS0029: Cannot implicitly convert type 'string' to 'Open.Text.StringComparable'

		// CONCLUSION: Stick with == operator. Ignore IDE inspection!
	}

	[Fact]
	public static void CORRECT_Usage_MultipleComparisons()
	{
		// ? Real-world example: checking multiple values case-insensitively
		var userInput = "GOODBYE".AsCaseInsensitive();

		// This is correct - use == operator
		bool isGreeting = userInput == "hello" ||
						  userInput == "hi" ||
						  userInput == "goodbye";

		Assert.True(isGreeting);

		// IDE might suggest pattern matching, but it won't compile!
		// Pattern matching requires type conversion, not just operator overloads
	}

	[Fact]
	public static void Suppress_IDE_Inspection_Example()
	{
		// ?? How to suppress the incorrect IDE inspection
		var text = "HELLO".AsCaseInsensitive();
		bool matches = text == "hello" || text == "there";
		Assert.True(matches);
	}
}

