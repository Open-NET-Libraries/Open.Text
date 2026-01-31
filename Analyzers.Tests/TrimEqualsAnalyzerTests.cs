using System.Threading.Tasks;
using Xunit;
using VerifyCS = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<Open.Text.Analyzers.TrimEqualsAnalyzer>;

namespace Open.Text.Analyzers.Tests;

public class TrimEqualsAnalyzerTests
{
	[Fact]
	public async Task TrimThenEquals_ShouldReportDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "  hello  ";
			        bool isEqual = {|OPENTXT005:text.Trim().Equals("hello")|};
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task TrimThenEqualsOperator_ShouldReportDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "  hello  ";
			        bool isEqual = {|OPENTXT005:text.Trim() == "hello"|};
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task TrimThenNotEquals_ShouldReportDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "  hello  ";
			        bool isNotEqual = {|OPENTXT005:text.Trim() != "hello"|};
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task TrimWithoutEquals_NoDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "  hello  ";
			        string trimmed = text.Trim();
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Fact]
	public async Task EqualsWithoutTrim_NoDiagnostic()
	{
		const string test = """
			class TestClass
			{
			    void TestMethod()
			    {
			        string text = "hello";
			        bool isEqual = text.Equals("hello");
			    }
			}
			""";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}
}
