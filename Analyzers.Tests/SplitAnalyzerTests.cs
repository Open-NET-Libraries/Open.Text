using System.Threading.Tasks;
using Xunit;
using VerifyCS = Open.Text.Analyzers.Tests.CSharpCodeFixVerifier<
	Open.Text.Analyzers.SplitAnalyzer,
	Open.Text.Analyzers.SplitCodeFixProvider>;

namespace Open.Text.Analyzers.Tests;

public class SplitAnalyzerTests
{
	[Fact]
	public async Task SplitCall_ShouldReportDiagnostic()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        string[] parts = text.{|OPENTXT002:Split(',')|};
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SplitInForeach_ShouldReportUseSplitToEnumerable()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        foreach (var part in text.{|OPENTXT008:Split(',')|})
        {
            System.Console.WriteLine(part);
        }
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SplitWithFirstCall_ShouldReportDiagnostic()
	{
		var test = @"
using System.Linq;

class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        string first = text.Split(',').{|OPENTXT007:First()|};
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SplitWithIndexZero_ShouldReportDiagnostic()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        string first = text.Split(','){|OPENTXT007:[0]|};
    }
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task SplitAsSegments_CodeFix()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        string[] parts = text.{|OPENTXT002:Split(',')|};
    }
}";

		var fixedCode = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        string[] parts = text.SplitAsSegments(',');
    }
}";

		await VerifyCS.VerifyCodeFixAsync(test, fixedCode);
	}

	[Fact]
	public async Task SplitToEnumerable_CodeFix()
	{
		var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        foreach (var part in text.{|OPENTXT008:Split(',')|})
        {
            System.Console.WriteLine(part);
        }
    }
}";

		var fixedCode = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""a,b,c"";
        foreach (var part in text.SplitToEnumerable(','))
        {
            System.Console.WriteLine(part);
        }
    }
}";

		await VerifyCS.VerifyCodeFixAsync(test, fixedCode);
	}
}
