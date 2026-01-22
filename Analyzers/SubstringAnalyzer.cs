using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Open.Text.Analyzers;

/// <summary>
/// Analyzer that detects string.Substring() calls that could be replaced with span slicing.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SubstringAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.UseSpanInsteadOfSubstring);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		// Check if this is a member access (e.g., str.Substring(...))
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return;

		// Check if the method name is "Substring"
		if (memberAccess.Name.Identifier.Text != "Substring")
			return;

		// Get the symbol to verify it's actually string.Substring
		var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
		if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
			return;

		// Verify it's the string.Substring method
		if (methodSymbol.ContainingType?.SpecialType != SpecialType.System_String)
			return;

		// Check if this is in a context where we can use spans
		// For now, we'll suggest the optimization in most cases
		// Future enhancement: Check if the result is used in a way compatible with spans

		// Build the diagnostic message with the arguments
		var arguments = invocation.ArgumentList.Arguments;
		string argsString = arguments.Count switch
		{
			1 => arguments[0].ToString(),
			2 => $"{arguments[0]}, {arguments[1]}",
			_ => ""
		};

		var diagnostic = Diagnostic.Create(
			DiagnosticDescriptors.UseSpanInsteadOfSubstring,
			invocation.GetLocation(),
			argsString);

		context.ReportDiagnostic(diagnostic);
	}
}
