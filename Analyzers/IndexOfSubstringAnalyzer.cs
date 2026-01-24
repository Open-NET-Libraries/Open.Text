using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Open.Text.Analyzers;

/// <summary>
/// Analyzer that detects patterns where IndexOf is followed by Substring.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IndexOfSubstringAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticDescriptors.UseSpanForIndexOfSubstring];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
		context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
	}

	private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
	{
		var method = (MethodDeclarationSyntax)context.Node;
		if (method.Body != null)
			AnalyzeBlock(context, method.Body.Statements);
		else if (method.ExpressionBody != null)
			AnalyzeExpressionBody(context, method.ExpressionBody);
	}

	private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
	{
		var localFunction = (LocalFunctionStatementSyntax)context.Node;
		if (localFunction.Body != null)
			AnalyzeBlock(context, localFunction.Body.Statements);
		else if (localFunction.ExpressionBody != null)
			AnalyzeExpressionBody(context, localFunction.ExpressionBody);
	}

	private static void AnalyzeBlock(SyntaxNodeAnalysisContext context, SyntaxList<StatementSyntax> statements)
	{
		for (int i = 0; i < statements.Count - 1; i++)
		{
			// Look for variable declaration with IndexOf assignment
			if (statements[i] is LocalDeclarationStatementSyntax localDecl)
			{
				foreach (var variable in localDecl.Declaration.Variables)
				{
					if (variable.Initializer?.Value is InvocationExpressionSyntax indexOfCall)
					{
						if (IsIndexOfCall(indexOfCall, context))
						{
							// Look ahead for Substring call using this variable
							var variableName = variable.Identifier.Text;
							if (FindSubstringUsage(statements.Skip(i + 1), variableName, context, out var substringLocation))
							{
								var diagnostic = Diagnostic.Create(
									DiagnosticDescriptors.UseSpanForIndexOfSubstring,
									substringLocation);
								context.ReportDiagnostic(diagnostic);
							}
						}
					}
				}
			}
			// Look for assignment of IndexOf to existing variable
			else if (statements[i] is ExpressionStatementSyntax exprStmt &&
					 exprStmt.Expression is AssignmentExpressionSyntax assignment &&
					 assignment.Left is IdentifierNameSyntax identifier &&
					 assignment.Right is InvocationExpressionSyntax indexOfCall)
			{
				if (IsIndexOfCall(indexOfCall, context))
				{
					var variableName = identifier.Identifier.Text;
					if (FindSubstringUsage(statements.Skip(i + 1), variableName, context, out var substringLocation))
					{
						var diagnostic = Diagnostic.Create(
							DiagnosticDescriptors.UseSpanForIndexOfSubstring,
							substringLocation);
						context.ReportDiagnostic(diagnostic);
					}
				}
			}
		}
	}

	private static void AnalyzeExpressionBody(SyntaxNodeAnalysisContext context, ArrowExpressionClauseSyntax expressionBody)
	{
		// For expression-bodied members, we can check if there's a chained pattern
		// This is a simplified check for common patterns
	}

	private static bool IsIndexOfCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
	{
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
			(memberAccess.Name.Identifier.Text == "IndexOf" ||
			 memberAccess.Name.Identifier.Text == "LastIndexOf"))
		{
			var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
			if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
			{
				var containingType = methodSymbol.ContainingType;
				return containingType?.SpecialType == SpecialType.System_String ||
					   containingType?.ToString() == "Microsoft.Extensions.Primitives.StringSegment";
			}
		}
		return false;
	}

	private static bool FindSubstringUsage(
		System.Collections.Generic.IEnumerable<StatementSyntax> statements,
		string variableName,
		SyntaxNodeAnalysisContext context,
		out Location? location)
	{
		location = null;

		foreach (var statement in statements)
		{
			var invocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>();
			foreach (var invocation in invocations)
			{
				if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
					memberAccess.Name.Identifier.Text == "Substring")
				{
					// Check if any argument references our variable
					foreach (var arg in invocation.ArgumentList.Arguments)
					{
						if (arg.Expression.DescendantNodesAndSelf()
							.OfType<IdentifierNameSyntax>()
							.Any(id => id.Identifier.Text == variableName))
						{
							location = invocation.GetLocation();
							return true;
						}
					}
				}
			}

			// Stop searching if the variable is reassigned
			var assignments = statement.DescendantNodes().OfType<AssignmentExpressionSyntax>();
			if (assignments.Any(a => a.Left is IdentifierNameSyntax id && id.Identifier.Text == variableName))
			{
				break;
			}
		}

		return false;
	}
}
