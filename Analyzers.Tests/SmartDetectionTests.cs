using System;
using System.Threading.Tasks;
using Xunit;
using VerifySubstring = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.SubstringAnalyzer>;
using VerifySplit = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.SplitAnalyzer>;
using VerifyConcat = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.StringConcatenationAnalyzer>;
using VerifyTrim = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.TrimEqualsAnalyzer>;

namespace Open.Text.Analyzers.Tests;

/// <summary>
/// Tests to ensure analyzers are smart and don't produce annoying false positives.
/// These tests verify that analyzers only trigger in appropriate contexts.
/// </summary>
public class SmartDetectionTests
{
	#region Substring Analyzer - Smart Detection

	[Fact]
	public async Task Substring_OnlyOnStringType_NotCustomClass()
	{
		// Should NOT report diagnostic for custom class with Substring method
		const string test = """
			class CustomString
			{
			    public string Substring(int start) => string.Empty;
			}

			class TestClass
			{
			    void TestMethod()
			    {
			        var custom = new CustomString();
			        string result = custom.Substring(5);  // Should NOT warn
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task Substring_OnActualString_ShouldWarn()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "hello";
			        string result = {|OPENTXT001:text.Substring(5)|};  // SHOULD warn
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(test);
	}

	#endregion

	#region Split Analyzer - Smart Detection

	[Fact]
	public async Task Split_OnlyOnStringType_NotCustomClass()
	{
		// Should NOT report diagnostic for custom class with Split method
		const string test = """
			class CustomParser
			{
			    public string[] Split(char delimiter) => new string[0];
			}

			class TestClass
			{
			    void TestMethod()
			    {
			        var parser = new CustomParser();
			        string[] parts = parser.Split(',');  // Should NOT warn
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task Split_WhenResultIsStored_NotJustIterated()
	{
		// This is a gray area - user might need the array for indexing later
		// Currently warns, but this test documents the behavior
		const string test = """
			using System;

			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "a,b,c";
			        string[] parts = {|OPENTXT002:text.Split(',')|};
			        
			        // Using array indexing - might actually need the array
			        Console.WriteLine(parts[0]);
			        Console.WriteLine(parts[1]);
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task Split_InForeachLoop_SuggestsSplitToEnumerable()
	{
		// Smart: Detects foreach usage and suggests lazy evaluation
		const string test = """
			using System;
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "a,b,c";
			        foreach (var item in {|OPENTXT008:text.Split(',')|})
			        {
			            Console.WriteLine(item);
			        }
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task Split_FirstElement_SuggestsFirstSplit()
	{
		// Smart: Detects that only first element is needed
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "a,b,c";
			        string first = {|OPENTXT007:{|OPENTXT002:text.Split(',')|}[0]|};
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task Split_NonFirstElement_NoFirstSplitWarning()
	{
		// Smart: Doesn't suggest FirstSplit when getting other elements
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "a,b,c";
			        string[] parts = {|OPENTXT002:text.Split(',')|};
			        string second = parts[1];  // Not first element - FirstSplit wouldn't help
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	#endregion

	#region String Concatenation - Smart Detection

	[Fact]
	public async Task StringConcat_OnlyInLoops_NotOutside()
	{
		// Should NOT warn about concatenation outside loops
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string a = "hello";
			        string b = a + " world";  // Should NOT warn - not in a loop
			        string c = b + "!";
			    }
			}
			""";

		await VerifyConcat.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task StringConcat_InLoop_ShouldWarn()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string result = "";
			        for (int i = 0; i < 10; i++)
			        {
			            {|OPENTXT004:result += "item"|}; // SHOULD warn
			        }
			    }
			}
			""";

		await VerifyConcat.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task IntConcat_InLoop_NoWarning()
	{
		// Smart: Only warns for strings, not other types
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        int sum = 0;
			        for (int i = 0; i < 10; i++)
			        {
			            sum += i;  // Should NOT warn - this is fine for integers
			        }
			    }
			}
			""";

		await VerifyConcat.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task StringConcat_DifferentVariableInLoop_NoWarning()
	{
		// Smart: Doesn't warn if concatenating different variables
		const string test = """
			using System;

			class TestClass
			{
			    void TestMethod()
			    {
			        string prefix = "Item: ";
			        for (int i = 0; i < 10; i++)
			        {
			            string temp = prefix + i;  // Different variable each iteration - OK
			            Console.WriteLine(temp);
			        }
			    }
			}
			""";

		await VerifyConcat.VerifyAnalyzerAsync(test);
	}

	#endregion

	#region Trim + Equals - Smart Detection

	[Fact]
	public async Task TrimEquals_OnlyWhenCombined_NotSeparate()
	{
		// Should NOT warn when Trim and Equals are separate
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "  hello  ";
			        string trimmed = text.Trim();  // Should NOT warn
			        
			        string other = "world";
			        bool equal = other.Equals("world");  // Should NOT warn
			    }
			}
			""";

		await VerifyTrim.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task TrimEquals_WhenChained_ShouldWarn()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "  hello  ";
			        bool equal = {|OPENTXT005:text.Trim().Equals("hello")|}; // SHOULD warn
			    }
			}
			""";

		await VerifyTrim.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task TrimEquals_OnlyForStrings_NotCustomClass()
	{
		// Should NOT warn for custom class with Trim method
		const string test = """
			class CustomText
			{
			    public CustomText Trim() => this;
			    public bool Equals(string other) => true;
			}

			class TestClass
			{
			    void TestMethod()
			    {
			        var custom = new CustomText();
			        bool equal = custom.Trim().Equals("hello");  // Should NOT warn
			    }
			}
			""";

		await VerifyTrim.VerifyAnalyzerAsync(test);
	}

	#endregion

	#region Context-Aware Tests

	[Fact]
	public async Task MultiplePatterns_OnlyWarnsRelevantOnes()
	{
		// Demonstrates that only appropriate patterns are detected
		// Each analyzer runs independently and only sees its own diagnostics
		const string substringTest = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "a,b,c,d,e";
			        string sub = {|OPENTXT001:text.Substring(5)|};
			    }
			}
			""";

		const string splitTest = """
			using System;
			using System.Linq;

			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "a,b,c,d,e";
			        
			        // Split with FirstOrDefault - double diagnostic
			        string first = {|OPENTXT007:{|OPENTXT002:text.Split(',')|}|}.FirstOrDefault();
			        
			        // Split in foreach
			        foreach (var part in {|OPENTXT008:text.Split(',')|})
			        {
			            Console.WriteLine(part);
			        }
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(substringTest);
		await VerifySplit.VerifyAnalyzerAsync(splitTest);
	}

	[Fact]
	public async Task RealWorldScenario_CsvParsing()
	{
		// Real-world scenario: Should only warn about inefficiencies, not valid patterns
		const string test = """
			using System.Linq;

			class CsvParser
			{
			    public void ParseCsv(string csvContent)
			    {
			        // SHOULD warn - Split on large string
			        string[] lines = {|OPENTXT002:csvContent.Split('\n')|};
			        
			        foreach (var line in lines)
			        {
			            if (string.IsNullOrWhiteSpace(line)) continue;
			            
			            // SHOULD warn - Split again
			            string[] columns = {|OPENTXT002:line.Split(',')|};
			            
			            if (columns.Length < 3) continue;
			            
			            // These are fine - no warnings
			            string name = columns[0].Trim();
			            string email = columns[1].Trim();
			            int age = int.Parse(columns[2]);
			        }
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task EdgeCase_EmptyString()
	{
		// Should still warn even with edge cases
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string empty = "";
			        string sub = {|OPENTXT001:empty.Substring(0)|};  // Still inefficient, even if empty
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task EdgeCase_NullableStrings()
	{
		// Should handle nullable strings appropriately
		const string test = """
			#nullable enable
			class TestClass
			{
			    void TestMethod(string? nullableText)
			    {
			        if (nullableText != null)
			        {
			            string sub = {|OPENTXT001:nullableText.Substring(5)|};  // SHOULD warn
			        }
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(test);
	}

	#endregion

	#region Performance Impact Tests

	[Fact]
	public async Task HighImpact_StringConcatInTightLoop()
	{
		// High-impact scenario: concatenation in tight loop (correctly warns)
		const string test = """
			class TestClass
			{
			    string BuildLargeString(int iterations)
			    {
			        string result = "";
			        for (int i = 0; i < iterations; i++)
			        {
			            {|OPENTXT004:result += "segment" + i + ","|}; // Very inefficient!
			        }
			        return result;
			    }
			}
			""";

		await VerifyConcat.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task LowImpact_SingleSubstring()
	{
		// Lower impact but still worth suggesting
		const string test = """
			class TestClass
			{
			    string GetDomain(string email)
			    {
			        int index = email.IndexOf('@');
			        return index == -1 ? "" : {|OPENTXT001:email.Substring(index + 1)|};
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(test);
	}

	#endregion

	#region False Positive Prevention

	[Fact]
	public async Task NoFalsePositive_LinqMethodsOnNonString()
	{
		// Should NOT warn for LINQ methods on other types
		const string test = """
			using System.Linq;

			class TestClass
			{
			    void TestMethod()
			    {
			        int[] numbers = { 1, 2, 3, 4, 5 };
			        int first = numbers.First();  // Should NOT warn - not string.Split()
			    }
			}
			""";

		await VerifySplit.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task NoFalsePositive_CustomExtensionMethods()
	{
		// Should NOT warn for custom extension methods
		const string test = """
			static class Extensions
			{
			    public static string Substring(this object obj, int index) => "";
			}

			class TestClass
			{
			    void TestMethod()
			    {
			        object obj = new object();
			        string result = obj.Substring(5);  // Should NOT warn - different type
			    }
			}
			""";

		await VerifySubstring.VerifyAnalyzerAsync(test);
	}

	#endregion
}
