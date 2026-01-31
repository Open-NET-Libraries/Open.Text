# Contributing to Open.Text Analyzers

Thank you for your interest in contributing to the Open.Text Analyzers project! This guide will help you understand how to add new analyzers or improve existing ones.

## Architecture Overview

The analyzer project consists of three main components:

1. **Analyzers** - Detect patterns in code
2. **Code Fix Providers** - Suggest and apply fixes
3. **Tests** - Verify analyzer behavior

## Project Structure

```
Analyzers/
├── DiagnosticDescriptors.cs      # Diagnostic IDs and messages
├── SubstringAnalyzer.cs          # Individual analyzer
├── SplitAnalyzer.cs              # Individual analyzer
├── SplitCodeFixProvider.cs       # Code fix for Split analyzer
├── StringConcatenationAnalyzer.cs
├── IndexOfSubstringAnalyzer.cs
├── TrimEqualsAnalyzer.cs
└── README.md

Analyzers.Tests/
├── Verifiers.cs                  # Test helpers
├── SubstringAnalyzerTests.cs     # Tests for Substring analyzer
├── SplitAnalyzerTests.cs
└── ...
```

## Adding a New Analyzer

### Step 1: Define the Diagnostic

Add a new diagnostic descriptor to `DiagnosticDescriptors.cs`:

```csharp
public static readonly DiagnosticDescriptor UseMyOptimization = new(
    id: "OPENTXT009",  // Use next available ID
    title: "Short title for the issue",
    messageFormat: "Message shown to user with {0} placeholders",
    category: Category,  // Usually "Performance"
    defaultSeverity: DiagnosticSeverity.Info,  // or Warning
    isEnabledByDefault: true,
    description: "Detailed description of the optimization.",
    helpLinkUri: HelpLinkUriBase + "OPENTXT009.md");
```

### Step 2: Create the Analyzer

Create a new file `MyPatternAnalyzer.cs`:

```csharp
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Open.Text.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MyPatternAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.UseMyOptimization);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        // Register for specific syntax node types
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Your analysis logic here
        // 1. Check if this is the pattern you're looking for
        // 2. Use semantic model to verify types
        // 3. Report diagnostic if pattern is found

        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "MethodName")
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol &&
                methodSymbol.ContainingType?.SpecialType == SpecialType.System_String)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseMyOptimization,
                    invocation.GetLocation(),
                    "arg1", "arg2");  // Format arguments
                
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
```

### Step 3: Create the Code Fix Provider (Optional)

Create `MyPatternCodeFixProvider.cs`:

```csharp
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MyPatternCodeFixProvider)), Shared]
public class MyPatternCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.UseMyOptimization.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();

        if (node != null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use optimized pattern",
                    createChangedDocument: c => ApplyFixAsync(context.Document, node, c),
                    equivalenceKey: "UseOptimizedPattern"),
                diagnostic);
        }
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // Create the new syntax node
        var newNode = CreateReplacementNode(invocation);
        
        var newRoot = root.ReplaceNode(invocation, newNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static SyntaxNode CreateReplacementNode(InvocationExpressionSyntax invocation)
    {
        // Build your replacement syntax tree
        // Use SyntaxFactory methods to create nodes
        return invocation;  // Replace with actual logic
    }
}
```

### Step 4: Write Tests

Create `MyPatternAnalyzerTests.cs` in the test project:

```csharp
using Xunit;
using VerifyCS = Open.Text.Analyzers.Tests.CSharpAnalyzerVerifier<
    Open.Text.Analyzers.MyPatternAnalyzer>;

namespace Open.Text.Analyzers.Tests;

public class MyPatternAnalyzerTests
{
    [Fact]
    public async Task DetectsPattern_ShouldReportDiagnostic()
    {
        var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""example"";
        string result = text.{|OPENTXT009:MethodName()|};
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task GoodPattern_NoDiagnostic()
    {
        var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""example"";
        var result = text.OptimizedMethodName();
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
```

If you have a code fix provider:

```csharp
using VerifyCS = Open.Text.Analyzers.Tests.CSharpCodeFixVerifier<
    Open.Text.Analyzers.MyPatternAnalyzer,
    Open.Text.Analyzers.MyPatternCodeFixProvider>;

public class MyPatternCodeFixTests
{
    [Fact]
    public async Task AppliesFix_Correctly()
    {
        var test = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""example"";
        string result = text.{|OPENTXT009:MethodName()|};
    }
}";

        var fixedCode = @"
class TestClass
{
    void TestMethod()
    {
        string text = ""example"";
        var result = text.OptimizedMethodName();
    }
}";

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode);
    }
}
```

## Tips for Writing Analyzers

### 1. Use the Semantic Model

Always verify types using the semantic model, not just syntax:

```csharp
// ❌ Bad - only checks syntax
if (memberAccess.Name.Identifier.Text == "Substring")

// ✅ Good - verifies it's actually string.Substring
var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
if (symbolInfo.Symbol is IMethodSymbol methodSymbol &&
    methodSymbol.ContainingType?.SpecialType == SpecialType.System_String)
```

### 2. Handle Edge Cases

- Null checks
- Empty strings
- Multiple overloads
- Custom types with same method names

### 3. Consider Context

Some optimizations only make sense in certain contexts:
- Return type compatibility (string vs span)
- Whether the result is used
- Whether we're in a hot path

### 4. Write Comprehensive Tests

Test:
- ✅ Pattern is detected
- ✅ Pattern is not detected in valid cases
- ✅ Different syntax variations
- ✅ Edge cases
- ✅ Code fix produces valid code
- ✅ Code fix preserves semantics

## Common Roslyn Patterns

### Finding Method Invocations

```csharp
context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);

private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
{
    var invocation = (InvocationExpressionSyntax)context.Node;
    
    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
    {
        // Check method name
        if (memberAccess.Name.Identifier.Text == "MethodName")
        {
            // Verify it's the right type
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
            // ...
        }
    }
}
```

### Checking if Code is in a Loop

```csharp
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
```

### Creating Syntax Nodes

```csharp
// Create an identifier: myVar
var identifier = SyntaxFactory.IdentifierName("myVar");

// Create a member access: obj.Method
var memberAccess = SyntaxFactory.MemberAccessExpression(
    SyntaxKind.SimpleMemberAccessExpression,
    SyntaxFactory.IdentifierName("obj"),
    SyntaxFactory.IdentifierName("Method"));

// Create an invocation: obj.Method(arg)
var invocation = SyntaxFactory.InvocationExpression(
    memberAccess,
    SyntaxFactory.ArgumentList(
        SyntaxFactory.SingletonSeparatedList(
            SyntaxFactory.Argument(
                SyntaxFactory.IdentifierName("arg")))));
```

## Testing Your Analyzer

### Run Tests

```bash
cd Analyzers.Tests
dotnet test
```

### Debug Analyzer

1. Set breakpoint in your analyzer
2. Set the test project as startup
3. Press F5 in Visual Studio
4. The analyzer will be loaded when tests run

### Test in Real Projects

1. Build the analyzer project
2. Add a project reference to the analyzer
3. Open code files to see warnings in the IDE

## Documentation Requirements

For each new analyzer, provide:

1. **Entry in DiagnosticDescriptors.cs** with clear title and message
2. **Section in README.md** explaining the problem and solution
3. **Examples in EXAMPLES.md** showing before/after
4. **Comprehensive tests** covering all cases

## Code Style

- Follow existing code patterns
- Use meaningful variable names
- Add XML documentation comments
- Keep methods focused and small
- Handle null and edge cases

## Questions?

Open an issue on GitHub or reach out to the maintainers!

Happy coding! 🚀
