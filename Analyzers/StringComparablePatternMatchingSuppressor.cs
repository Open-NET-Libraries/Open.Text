using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Open.Text.Analyzers;

/// <summary>
/// <para>
/// Suppresses IDE0078 (Use pattern matching) and IDE0083 (Use pattern matching for not equals)
/// when the left operand is a StringComparable or SpanComparable type.
/// </para>
/// <para>
/// Pattern matching with constant patterns does not work with these types because
/// they require custom equality logic via operator overloads, not type conversion.
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StringComparablePatternMatchingSuppressor : DiagnosticSuppressor
{
	private const string StringComparableTypeName = "Open.Text.StringComparable";
	private const string SpanComparableTypeName = "Open.Text.SpanComparable";

	// IDE inspections that suggest pattern matching
	private static readonly SuppressionDescriptor SuppressIDE0078 = new(
		id: "OPENTXTSPR001",
		suppressedDiagnosticId: "IDE0078",  // Use pattern matching
		justification: "Pattern matching does not work with StringComparable/SpanComparable types as they use custom equality operators without implicit type conversion");

	private static readonly SuppressionDescriptor SuppressIDE0083 = new(
		id: "OPENTXTSPR002",
		suppressedDiagnosticId: "IDE0083",  // Use pattern matching (not pattern)
		justification: "Pattern matching does not work with StringComparable/SpanComparable types as they use custom equality operators without implicit type conversion");

	private static readonly SuppressionDescriptor SuppressIDE0260 = new(
		id: "OPENTXTSPR003",
		suppressedDiagnosticId: "IDE0260",  // Use pattern matching
		justification: "Pattern matching does not work with StringComparable/SpanComparable types as they use custom equality operators without implicit type conversion");

	// ReSharper equivalent
	private static readonly SuppressionDescriptor SuppressRCS1246 = new(
		id: "OPENTXTSPR004",
		suppressedDiagnosticId: "RCS1246",  // Use pattern matching
		justification: "Pattern matching does not work with StringComparable/SpanComparable types as they use custom equality operators without implicit type conversion");

	public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
		=> [SuppressIDE0078, SuppressIDE0083, SuppressIDE0260, SuppressRCS1246];

	public override void ReportSuppressions(SuppressionAnalysisContext context)
	{
		foreach (var diagnostic in context.ReportedDiagnostics)
		{
			// Only process pattern matching diagnostics
			if (!IsSupportedDiagnostic(diagnostic.Id))
				continue;

			var syntaxTree = diagnostic.Location.SourceTree;
			if (syntaxTree == null)
				continue;

			var root = syntaxTree.GetRoot(context.CancellationToken);
			var node = root.FindNode(diagnostic.Location.SourceSpan);

			// The diagnostic might be on various nodes - check all binary expressions in the tree
			bool shouldSuppress = false;

			// Check if this node or any parent is a binary expression with StringComparable
			var currentNode = node;
			while (currentNode != null && !shouldSuppress)
			{
				if (currentNode is BinaryExpressionSyntax binaryExpression)
				{
					if (ShouldSuppressForBinaryExpression(binaryExpression, context))
					{
						shouldSuppress = true;
						break;
					}
				}
				currentNode = currentNode.Parent;
			}

			if (shouldSuppress)
			{
				var suppressionDescriptor = GetSuppressionDescriptor(diagnostic.Id);
				if (suppressionDescriptor != null)
				{
					context.ReportSuppression(Suppression.Create(suppressionDescriptor, diagnostic));
				}
			}
		}
	}

	private static bool IsSupportedDiagnostic(string diagnosticId)
		=> diagnosticId is "IDE0078" or "IDE0083" or "IDE0260" or "RCS1246";

	private static SuppressionDescriptor? GetSuppressionDescriptor(string diagnosticId) => diagnosticId switch
	{
		"IDE0078" => SuppressIDE0078,
		"IDE0083" => SuppressIDE0083,
		"IDE0260" => SuppressIDE0260,
		"RCS1246" => SuppressRCS1246,
		_ => null
	};

	private static bool ShouldSuppressForBinaryExpression(BinaryExpressionSyntax binaryExpression, SuppressionAnalysisContext context)
	{
		var semanticModel = context.GetSemanticModel(binaryExpression.SyntaxTree);
		if (semanticModel == null)
			return false;

		// For || or && expressions, check both sides
		if (binaryExpression.IsKind(SyntaxKind.LogicalOrExpression)
			|| binaryExpression.IsKind(SyntaxKind.LogicalAndExpression))
		{
			// Check if any child expression involves StringComparable
			return ContainsStringComparableComparison(binaryExpression, semanticModel, context);
		}

		// For == or != expressions, check the left operand type
		var leftType = semanticModel.GetTypeInfo(binaryExpression.Left, context.CancellationToken).Type;
		if (leftType == null)
			return false;

		var fullTypeName = leftType.ToDisplayString();
		return fullTypeName is StringComparableTypeName or SpanComparableTypeName;
	}

	private static bool ContainsStringComparableComparison(SyntaxNode node, SemanticModel semanticModel, SuppressionAnalysisContext context)
	{
		// Recursively check all binary expressions
		foreach (var descendant in node.DescendantNodesAndSelf())
		{
			if (descendant is BinaryExpressionSyntax binaryExpr
				&& (binaryExpr.IsKind(SyntaxKind.EqualsExpression)
					|| binaryExpr.IsKind(SyntaxKind.NotEqualsExpression)))
			{
				var leftType = semanticModel.GetTypeInfo(binaryExpr.Left, context.CancellationToken).Type;
				if (leftType != null)
				{
					var fullTypeName = leftType.ToDisplayString();
					if (fullTypeName is StringComparableTypeName or SpanComparableTypeName)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
