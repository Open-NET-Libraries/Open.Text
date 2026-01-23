using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit;

namespace Open.Text.Analyzers.Tests;

public static class CSharpAnalyzerVerifier<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	public static DiagnosticResult Diagnostic(string diagnosticId)
		=> CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

	public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
	{
		var test = new Test { TestCode = source };
		test.ExpectedDiagnostics.AddRange(expected);
		return test.RunAsync();
	}

	private class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
	{
		public Test()
		{
			// Use a recent C# version to support modern syntax
			TestState.AnalyzerConfigFiles.Add(
				("/.editorconfig", """
					is_global = true
					build_property.TargetFramework = net8.0
					"""));
			
			// Add reference to Open.Text library for extension methods
			TestState.AdditionalReferences.Add(typeof(Open.Text.TextExtensions).Assembly);
		}
	}
}

public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new()
{
	public static DiagnosticResult Diagnostic(string diagnosticId)
		=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

	public static Task VerifyCodeFixAsync(string source, string fixedSource)
		=> VerifyCodeFixAsync(source, Array.Empty<DiagnosticResult>(), fixedSource);

	public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
		=> VerifyCodeFixAsync(source, new[] { expected }, fixedSource);

	public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
	{
		var test = new Test
		{
			TestCode = source,
			FixedCode = fixedSource,
		};
		test.ExpectedDiagnostics.AddRange(expected);
		return test.RunAsync();
	}

	private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
	{
		public Test()
		{
			TestState.AnalyzerConfigFiles.Add(
				("/.editorconfig", """
					is_global = true
					build_property.TargetFramework = net8.0
					"""));
			
			// Add reference to Open.Text library for extension methods
			TestState.AdditionalReferences.Add(typeof(Open.Text.TextExtensions).Assembly);
			FixedState.AdditionalReferences.Add(typeof(Open.Text.TextExtensions).Assembly);
		}
	}
}
