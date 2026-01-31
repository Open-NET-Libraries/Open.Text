using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Open.Text.Analyzers;

/// <summary>
/// Analyzer that detects string.Split() calls that could use SplitAsSegments for better performance.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SplitAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
		DiagnosticDescriptors.UseSplitAsSegments,
		DiagnosticDescriptors.UseFirstSplitInsteadOfSplitFirst,
		DiagnosticDescriptors.UseSplitToEnumerable,
		DiagnosticDescriptors.UseSplitAsSegmentsNoAlloc,
	];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
		context.RegisterSyntaxNodeAction(AnalyzeElementAccess, SyntaxKind.ElementAccessExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
		{
			return;
		}

		// Check for .Split().First() or .Split().FirstOrDefault()
		if (memberAccess.Name.Identifier.Text is "First" or "FirstOrDefault"
			&& memberAccess.Expression is InvocationExpressionSyntax innerInvocation
			&& innerInvocation.Expression is MemberAccessExpressionSyntax innerMemberAccess
			&& innerMemberAccess.Name.Identifier.Text == "Split")
		{
			var symbolInfo = context.SemanticModel.GetSymbolInfo(innerMemberAccess, context.CancellationToken);
			if (symbolInfo.Symbol is IMethodSymbol innerMethod
				&& innerMethod.ContainingType?.SpecialType == SpecialType.System_String)
			{
				var argsString = GetArgumentsString(innerInvocation.ArgumentList.Arguments);
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.UseFirstSplitInsteadOfSplitFirst,
					invocation.GetLocation(),
					argsString);
				context.ReportDiagnostic(diagnostic);
				return;
			}
		}

		// Check for .Split() that should use SplitAsSegments or SplitToEnumerable
		if (memberAccess.Name.Identifier.Text == "Split")
		{
			var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
			if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
				return;

			if (methodSymbol.ContainingType?.SpecialType != SpecialType.System_String)
				return;

			// Check how the result is used
			var isUsedInLoop = IsUsedInForeachLoop(invocation);
			var hasZLinqUsing = HasZLinqUsing(context);
			var argsString = GetArgumentsString(invocation.ArgumentList.Arguments);

			if (isUsedInLoop)
			{
				if (hasZLinqUsing)
				{
					// If ZLinq is available, suggest zero-allocation NoAlloc variant
					var diagnostic = Diagnostic.Create(
						DiagnosticDescriptors.UseSplitAsSegmentsNoAlloc,
						invocation.GetLocation(),
						argsString);
					context.ReportDiagnostic(diagnostic);
				}
				else
				{
					// Suggest SplitToEnumerable for foreach loops without ZLinq
					var diagnostic = Diagnostic.Create(
						DiagnosticDescriptors.UseSplitToEnumerable,
						invocation.GetLocation(),
						argsString);
					context.ReportDiagnostic(diagnostic);
				}
			}
			else
			{
				// General suggestion for SplitAsSegments
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.UseSplitAsSegments,
					invocation.GetLocation(),
					argsString);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}

	private static bool HasZLinqUsing(SyntaxNodeAnalysisContext context)
	{
		var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);
		if (root is not CompilationUnitSyntax compilationUnit)
			return false;

		// Check for 'using ZLinq;' directive
		return compilationUnit.Usings.Any(u => u.Name?.ToString() == "ZLinq");
	}

	private static void AnalyzeElementAccess(SyntaxNodeAnalysisContext context)
	{
		var elementAccess = (ElementAccessExpressionSyntax)context.Node;

		// Check for .Split()[0] or .Split()[index]
		if (elementAccess.Expression is not InvocationExpressionSyntax invocation
			|| invocation.Expression is not MemberAccessExpressionSyntax memberAccess
			|| memberAccess.Name.Identifier.Text != "Split")
		{
			return;
		}

		var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
		if (symbolInfo.Symbol is not IMethodSymbol methodSymbol
			|| methodSymbol.ContainingType?.SpecialType != SpecialType.System_String)
		{
			return;
		}

		// Check if accessing index 0
		if (elementAccess.ArgumentList.Arguments.Count != 1)
		{
			return;
		}

		var argument = elementAccess.ArgumentList.Arguments[0];
		if (argument.Expression is not LiteralExpressionSyntax literal
			|| literal.Token.ValueText != "0")
		{
			return;
		}

		var argsString = GetArgumentsString(invocation.ArgumentList.Arguments);
		var diagnostic = Diagnostic.Create(
			DiagnosticDescriptors.UseFirstSplitInsteadOfSplitFirst,
			elementAccess.GetLocation(),
			argsString);
		context.ReportDiagnostic(diagnostic);
	}

	private static bool IsUsedInForeachLoop(SyntaxNode node)
	{
		var parent = node.Parent;
		while (parent != null)
		{
			if (parent is ForEachStatementSyntax foreachStatement
				&& foreachStatement.Expression.Contains(node))
			{
				return true;
			}

			parent = parent.Parent;
		}

		return false;
	}

	private static string GetArgumentsString(SeparatedSyntaxList<ArgumentSyntax> arguments)
		=> arguments.Count == 0 ? "" : string.Join(", ", arguments.Select(arg => arg.ToString()));
}
