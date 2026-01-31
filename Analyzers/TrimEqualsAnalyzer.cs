using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Open.Text.Analyzers;

/// <summary>
/// Analyzer that detects patterns where Trim is followed by Equals/comparison.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TrimEqualsAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticDescriptors.UseTrimEquals];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
		context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		// Check for .Trim().Equals(...)
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
			|| memberAccess.Name.Identifier.Text != "Equals")
		{
			return;
		}

		// Check if the expression before .Equals() is a .Trim() call
		if (memberAccess.Expression is not InvocationExpressionSyntax trimInvocation
			|| trimInvocation.Expression is not MemberAccessExpressionSyntax trimMemberAccess
			|| trimMemberAccess.Name.Identifier.Text != "Trim")
		{
			return;
		}

		var symbolInfo = context.SemanticModel.GetSymbolInfo(trimMemberAccess, context.CancellationToken);
		if (symbolInfo.Symbol is not IMethodSymbol trimMethod
			|| trimMethod.ContainingType?.SpecialType != SpecialType.System_String)
		{
			return;
		}

		// Get the arguments from the Equals call
		var equalsArgs = invocation.ArgumentList.Arguments;
		string argsString = equalsArgs.Count > 0 ? equalsArgs[0].ToString() : "";

		// Add comparison type if present
		if (equalsArgs.Count > 1)
		{
			argsString += $", {equalsArgs[1]}";
		}

		var diagnostic = Diagnostic.Create(
			DiagnosticDescriptors.UseTrimEquals,
			invocation.GetLocation(),
			argsString);
		context.ReportDiagnostic(diagnostic);
	}

	private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
	{
		var binaryExpr = (BinaryExpressionSyntax)context.Node;

		// Check for .Trim() == "value" or "value" == .Trim()
		InvocationExpressionSyntax? trimCall = null;
		ExpressionSyntax? otherSide = null;

		if (binaryExpr.Left is InvocationExpressionSyntax leftInvocation
			&& IsTrimCall(leftInvocation, context))
		{
			trimCall = leftInvocation;
			otherSide = binaryExpr.Right;
		}
		else if (binaryExpr.Right is InvocationExpressionSyntax rightInvocation
				 && IsTrimCall(rightInvocation, context))
		{
			trimCall = rightInvocation;
			otherSide = binaryExpr.Left;
		}

		if (trimCall != null
			&& otherSide != null)
		{
			// Check if the other side is a string
			var typeInfo = context.SemanticModel.GetTypeInfo(otherSide, context.CancellationToken);
			if (typeInfo.Type?.SpecialType == SpecialType.System_String)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.UseTrimEquals,
					binaryExpr.GetLocation(),
					otherSide.ToString());
				context.ReportDiagnostic(diagnostic);
			}
		}
	}

	private static bool IsTrimCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
			&& memberAccess.Name.Identifier.Text == "Trim")
		{
			var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
			if (symbolInfo.Symbol is IMethodSymbol method
				&& method.ContainingType?.SpecialType == SpecialType.System_String)
			{
				return true;
			}
		}

		return false;
	}
}
