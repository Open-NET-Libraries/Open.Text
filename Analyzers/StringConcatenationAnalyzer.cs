using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Open.Text.Analyzers;

/// <summary>
/// Analyzer that detects string concatenation in loops.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StringConcatenationAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticDescriptors.UseStringBuilderInLoop];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeAssignment,
			SyntaxKind.AddAssignmentExpression,
			SyntaxKind.SimpleAssignmentExpression);
	}

	private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
	{
		var assignment = (AssignmentExpressionSyntax)context.Node;

		// Check if we're in a loop
		if (!IsInLoop(assignment))
			return;

		// For += operator
		if (assignment.IsKind(SyntaxKind.AddAssignmentExpression))
		{
			// Check if left side is a string
			var typeInfo = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken);
			if (typeInfo.Type?.SpecialType == SpecialType.System_String)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.UseStringBuilderInLoop,
					assignment.GetLocation());
				context.ReportDiagnostic(diagnostic);
			}
		}
		// For = operator with string concatenation on right side
		else if (assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
		{
			// Check if left side is a string
			var leftTypeInfo = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken);
			if (leftTypeInfo.Type?.SpecialType != SpecialType.System_String)
				return;

			// Check if right side involves the same variable (e.g., str = str + "...")
			if (assignment.Right is not BinaryExpressionSyntax binaryExpr
				|| !binaryExpr.IsKind(SyntaxKind.AddExpression)
				|| !ReferencesVariable(binaryExpr.Left, assignment.Left, context)
					&& !ReferencesVariable(binaryExpr.Right, assignment.Left, context))
			{
				return;
			}

			var diagnostic = Diagnostic.Create(
				DiagnosticDescriptors.UseStringBuilderInLoop,
				assignment.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsInLoop(SyntaxNode node)
	{
		var parent = node.Parent;
		while (parent != null)
		{
			switch (parent.Kind())
			{
				case SyntaxKind.ForStatement:
				case SyntaxKind.ForEachStatement:
				case SyntaxKind.WhileStatement:
				case SyntaxKind.DoStatement:
					return true;
			}

			parent = parent.Parent;
		}

		return false;
	}

	private static bool ReferencesVariable(SyntaxNode expression, SyntaxNode variable, SyntaxNodeAnalysisContext context)
	{
		if (expression is not IdentifierNameSyntax identifier
			|| variable is not IdentifierNameSyntax varIdentifier)
		{
			return false;
		}

		var exprSymbol = context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol;
		var varSymbol = context.SemanticModel.GetSymbolInfo(varIdentifier, context.CancellationToken).Symbol;
		return SymbolEqualityComparer.Default.Equals(exprSymbol, varSymbol);
	}
}
