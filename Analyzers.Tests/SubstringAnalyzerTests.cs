using System.Threading.Tasks;
using Xunit;
using VerifyCS = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.SubstringAnalyzer>;

namespace Open.Text.Analyzers.Tests;

public class SubstringAnalyzerTests
{
	[Fact]
	public async Task EmptyCode_NoDiagnostic()
	{
		var test = @"";
		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SubstringCall_ShouldReportDiagnostic()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""Hello World"";
        string sub = {|OPENTXT001:text.Substring(6)|};
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SubstringWithTwoParameters_ShouldReportDiagnostic()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""Hello World"";
        string sub = {|OPENTXT001:text.Substring(0, 5)|};
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task NoSubstringCall_NoDiagnostic()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""Hello World"";
        string upper = text.ToUpper();
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SubstringOnNonString_NoDiagnostic()
	{
		var test = @"
class CustomClass
{
    public string Substring(int start) => string.Empty;
}

class TestClass
{
    void TestMethod()
    {
        var obj = new CustomClass();
        string sub = obj.Substring(5);
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}
}
