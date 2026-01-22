using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Open.Text.Analyzers;

/// <summary>
/// Code fix provider for string.Split() calls that suggests using SplitAsSegments.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SplitCodeFixProvider)), Shared]
public class SplitCodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			DiagnosticDescriptors.UseSplitAsSegments.Id,
			DiagnosticDescriptors.UseFirstSplitInsteadOfSplitFirst.Id,
			DiagnosticDescriptors.UseSplitToEnumerable.Id);

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) return;

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		foreach (var diagnosticId in context.Diagnostics.Select(d => d.Id).Distinct())
		{
			switch (diagnosticId)
			{
				case "OPENTXT002": // UseSplitAsSegments
					{
						var invocation = root.FindToken(diagnosticSpan.Start).Parent?
							.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
						if (invocation != null)
						{
							context.RegisterCodeFix(
								CodeAction.Create(
									title: "Use SplitAsSegments",
									createChangedDocument: c => ReplaceMethodNameAsync(context.Document, invocation, "SplitAsSegments", c),
									equivalenceKey: "UseSplitAsSegments"),
								diagnostic);
						}
						break;
					}

				case "OPENTXT007": // UseFirstSplitInsteadOfSplitFirst
					{
						var node = root.FindToken(diagnosticSpan.Start).Parent?
							.AncestorsAndSelf().FirstOrDefault(n => 
								n is InvocationExpressionSyntax || 
								n is ElementAccessExpressionSyntax);

						if (node is InvocationExpressionSyntax invocation &&
							invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
							memberAccess.Expression is InvocationExpressionSyntax splitInvocation)
						{
							context.RegisterCodeFix(
								CodeAction.Create(
									title: "Use FirstSplit",
									createChangedDocument: c => ReplaceWithFirstSplitAsync(context.Document, invocation, splitInvocation, c),
									equivalenceKey: "UseFirstSplit"),
								diagnostic);
						}
						else if (node is ElementAccessExpressionSyntax elementAccess &&
								 elementAccess.Expression is InvocationExpressionSyntax splitInv)
						{
							context.RegisterCodeFix(
								CodeAction.Create(
									title: "Use FirstSplit",
									createChangedDocument: c => ReplaceElementAccessWithFirstSplitAsync(context.Document, elementAccess, splitInv, c),
									equivalenceKey: "UseFirstSplit"),
								diagnostic);
						}
						break;
					}

				case "OPENTXT008": // UseSplitToEnumerable
					{
						var invocation = root.FindToken(diagnosticSpan.Start).Parent?
							.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
						if (invocation != null)
						{
							context.RegisterCodeFix(
								CodeAction.Create(
									title: "Use SplitToEnumerable",
									createChangedDocument: c => ReplaceMethodNameAsync(context.Document, invocation, "SplitToEnumerable", c),
									equivalenceKey: "UseSplitToEnumerable"),
								diagnostic);
						}
						break;
					}
			}
		}
	}

	private static async Task<Document> ReplaceMethodNameAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		string newMethodName,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) return document;

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			var newMemberAccess = memberAccess.WithName(
				SyntaxFactory.IdentifierName(newMethodName));
			var newInvocation = invocation.WithExpression(newMemberAccess);
			var newRoot = root.ReplaceNode(invocation, newInvocation);
			return document.WithSyntaxRoot(newRoot);
		}

		return document;
	}

	private static async Task<Document> ReplaceWithFirstSplitAsync(
		Document document,
		InvocationExpressionSyntax outerInvocation,
		InvocationExpressionSyntax splitInvocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) return document;

		if (splitInvocation.Expression is MemberAccessExpressionSyntax splitMemberAccess)
		{
			// Create: source.FirstSplit(args, out int nextIndex)
			var newMemberAccess = splitMemberAccess.WithName(SyntaxFactory.IdentifierName("FirstSplit"));
			
			// Add the "out int nextIndex" parameter
			var newArguments = splitInvocation.ArgumentList.Arguments.Add(
				SyntaxFactory.Argument(
					SyntaxFactory.DeclarationExpression(
						SyntaxFactory.IdentifierName("int"),
						SyntaxFactory.SingleVariableDesignation(
							SyntaxFactory.Identifier("nextIndex"))))
				.WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword)));

			var newInvocation = SyntaxFactory.InvocationExpression(
				newMemberAccess,
				SyntaxFactory.ArgumentList(newArguments));

			// The result is a ReadOnlySpan<char>, so we need to call .ToString() on it
			var newExpression = SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					newInvocation,
					SyntaxFactory.IdentifierName("ToString")),
				SyntaxFactory.ArgumentList());

			var newRoot = root.ReplaceNode(outerInvocation, newExpression);
			return document.WithSyntaxRoot(newRoot);
		}

		return document;
	}

	private static async Task<Document> ReplaceElementAccessWithFirstSplitAsync(
		Document document,
		ElementAccessExpressionSyntax elementAccess,
		InvocationExpressionSyntax splitInvocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) return document;

		if (splitInvocation.Expression is MemberAccessExpressionSyntax splitMemberAccess)
		{
			var newMemberAccess = splitMemberAccess.WithName(SyntaxFactory.IdentifierName("FirstSplit"));
			
			var newArguments = splitInvocation.ArgumentList.Arguments.Add(
				SyntaxFactory.Argument(
					SyntaxFactory.DeclarationExpression(
						SyntaxFactory.IdentifierName("int"),
						SyntaxFactory.SingleVariableDesignation(
							SyntaxFactory.Identifier("nextIndex"))))
				.WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword)));

			var newInvocation = SyntaxFactory.InvocationExpression(
				newMemberAccess,
				SyntaxFactory.ArgumentList(newArguments));

			var newExpression = SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					newInvocation,
					SyntaxFactory.IdentifierName("ToString")),
				SyntaxFactory.ArgumentList());

			var newRoot = root.ReplaceNode(elementAccess, newExpression);
			return document.WithSyntaxRoot(newRoot);
		}

		return document;
	}
}
