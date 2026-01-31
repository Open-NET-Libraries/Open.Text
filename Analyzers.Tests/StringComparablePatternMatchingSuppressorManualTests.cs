using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Open.Text.Analyzers.Tests;

/// <summary>
/// <para>MANUAL VERIFICATION TESTS for StringComparablePatternMatchingSuppressor</para>
/// <para>To verify the suppressor works:</para>
/// <para>
/// 1. Create a new test console app:
///    dotnet new console -n SuppressorTest
///    cd SuppressorTest
/// </para>
/// <para>
/// 2. Add Open.Text package:
///    dotnet add package Open.Text
/// </para>
/// <para>3. Add this code to Program.cs:</para>
/// <para>   using Open.Text;</para>
/// <para>   string text = "HELLO";</para>
/// <para>
///    // TEST 1: Regular string - SHOULD show IDE0078 suggestion
///    bool test1 = text == "hello" || text == "world";
///    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
///    //           You SHOULD see: "Use pattern matching" suggestion
/// </para>
/// <para>
///    // TEST 2: StringComparable - should NOT show suggestion (initially)
///    var comparable = text.AsCaseInsensitive();
///    bool test2 = comparable == "hello" || comparable == "world";
///    //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
///    //           You WILL see: "Use pattern matching" suggestion (without analyzer)
/// </para>
/// <para>4. Verify the IDE shows suggestion for BOTH (this is the problem!)</para>
/// <para>
/// 5. Now install the analyzer:
///    dotnet add package Open.Text.Analyzers
/// </para>
/// <para>
/// 6. Rebuild and check:
///    - TEST 1: Still shows suggestion ? (correct)
///    - TEST 2: NO suggestion anymore ? (suppressed!)
/// </para>
/// <para>
/// 7. Try to apply the suggestion on TEST 2 (it won't compile):
///    bool test2 = comparable is "hello" or "world";
///    //           Error CS0029: Cannot implicitly convert type 'string' to 'Open.Text.StringComparable'
/// </para>
/// <para>
/// EXPECTED RESULTS:
/// - Without analyzer: Both show IDE0078
/// - With analyzer: Only test1 shows IDE0078, test2 is suppressed
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped")]
public class StringComparablePatternMatchingSuppressorManualTests
{
	[Fact(Skip = "Manual verification required - see class documentation")]
	public void ManualVerification_Instructions()
	{
		// This test is skipped - it documents how to manually verify the suppressor works
		// Follow the instructions in the class XML documentation above
	}

	/// <summary>
	/// Automated test to verify the suppressor is properly configured.
	/// This doesn't test if it WORKS, but verifies it's set up correctly.
	/// </summary>
	[Fact]
	public void Suppressor_IsProperlyConfigured()
	{
		var suppressor = new StringComparablePatternMatchingSuppressor();

		// Verify it has the expected suppressions
		var suppressions = suppressor.SupportedSuppressions;
		Assert.Equal(4, suppressions.Length);

		// Verify it suppresses IDE0078
		Assert.Contains(suppressions, s => s.SuppressedDiagnosticId == "IDE0078");
		Assert.Contains(suppressions, s => s.SuppressedDiagnosticId == "IDE0083");
		Assert.Contains(suppressions, s => s.SuppressedDiagnosticId == "IDE0260");
		Assert.Contains(suppressions, s => s.SuppressedDiagnosticId == "RCS1246");
	}
}
