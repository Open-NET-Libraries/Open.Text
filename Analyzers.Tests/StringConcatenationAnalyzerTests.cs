using System.Threading.Tasks;
using Xunit;
using VerifyCS = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.StringConcatenationAnalyzer>;

namespace Open.Text.Analyzers.Tests;

public class StringConcatenationAnalyzerTests
{
	[Fact]
	public async Task StringConcatInLoop_ShouldReportDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string result = "";
			        for (int i = 0; i < 10; i++)
			        {
			            {|OPENTXT004:result += "a"|};
			        }
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task StringConcatInForeach_ShouldReportDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string result = "";
			        foreach (var item in new[] { "a", "b", "c" })
			        {
			            {|OPENTXT004:result += item|};
			        }
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task StringConcatInWhileLoop_ShouldReportDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string result = "";
			        int i = 0;
			        while (i < 10)
			        {
			            {|OPENTXT004:result = result + "a"|};
			            i++;
			        }
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task StringConcatOutsideLoop_NoDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string result = "";
			        result += "a";
			        result += "b";
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task IntConcatInLoop_NoDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        int result = 0;
			        for (int i = 0; i < 10; i++)
			        {
			            result += i;
			        }
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}
}
