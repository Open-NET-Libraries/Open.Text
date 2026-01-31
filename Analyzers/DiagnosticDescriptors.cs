using Microsoft.CodeAnalysis;

namespace Open.Text.Analyzers;

/// <summary>
/// Diagnostic descriptors for Open.Text analyzers.
/// </summary>
internal static class DiagnosticDescriptors
{
	private const string Category = "Performance";
	private const string HelpLinkUriBase = "https://github.com/electricessence/Open.Text/blob/main/docs/analyzers/";

	public static readonly DiagnosticDescriptor UseSpanInsteadOfSubstring = new(
		id: "OPENTXT001",
		title: "Use span slicing instead of Substring",
		messageFormat: "Consider using '.AsSpan({0})' or range indexer '[{0}]' instead of '.Substring({0})' for better performance",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "Using ReadOnlySpan<char> or span slicing avoids string allocations and improves performance.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT001.md");

	public static readonly DiagnosticDescriptor UseSplitAsSegments = new(
		id: "OPENTXT002",
		title: "Use SplitAsSegments to reduce string allocations",
		messageFormat: "Consider using '.SplitAsSegments({0})' instead of '.Split({0})' to avoid allocating an array and individual strings. For zero-allocation enumeration, use '.SplitAsSegmentsNoAlloc({0})' with ZLinq.",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "SplitAsSegments returns IEnumerable<StringSegment> which defers string allocation until ToString() is called. SplitAsSegmentsNoAlloc returns a ValueEnumerable for zero-allocation foreach loops when used with ZLinq.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT002.md");

	public static readonly DiagnosticDescriptor UseSpanForIndexOfSubstring = new(
		id: "OPENTXT003",
		title: "Combine IndexOf with span slicing",
		messageFormat: "Consider using span slicing after IndexOf instead of Substring for better performance",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "When IndexOf is followed by Substring, using span slicing avoids string allocation.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT003.md");

	public static readonly DiagnosticDescriptor UseStringBuilderInLoop = new(
		id: "OPENTXT004",
		title: "Avoid string concatenation in loops",
		messageFormat: "String concatenation in a loop creates multiple string allocations. Consider using StringBuilder.",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "String concatenation in loops is inefficient. Use StringBuilder for better performance.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT004.md");

	public static readonly DiagnosticDescriptor UseTrimEquals = new(
		id: "OPENTXT005",
		title: "Use TrimEquals extension method",
		messageFormat: "Consider using '.TrimEquals({0})' instead of '.Trim().Equals({0})' for better performance",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "The TrimEquals extension method from Open.Text avoids creating an intermediate trimmed string.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT005.md");

	public static readonly DiagnosticDescriptor UseAsSegment = new(
		id: "OPENTXT006",
		title: "Use AsSegment for string manipulation",
		messageFormat: "Consider using '.AsSegment()' for efficient string manipulation without allocations",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "StringSegment provides efficient string manipulation without allocations.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT006.md");

	public static readonly DiagnosticDescriptor UseFirstSplitInsteadOfSplitFirst = new(
		id: "OPENTXT007",
		title: "Use FirstSplit to avoid allocating full split array",
		messageFormat: "Consider using '.FirstSplit({0})' instead of '.Split({0})[0]' or '.Split({0}).First()' to avoid unnecessary allocations",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "FirstSplit returns only the first segment without allocating an entire array.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT007.md");

	public static readonly DiagnosticDescriptor UseSplitToEnumerable = new(
		id: "OPENTXT008",
		title: "Use SplitToEnumerable for lazy string evaluation",
		messageFormat: "Consider using '.SplitToEnumerable({0})' instead of '.Split({0})' when iterating results to avoid allocating the full array upfront. For zero-allocation enumeration, use '.SplitAsSegmentsNoAlloc({0})' with ZLinq, or '.SplitAsSegments({0})' for minimal allocation.",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "SplitToEnumerable returns IEnumerable<string> with lazy evaluation, avoiding the upfront allocation of the full string array. For even better performance, use SplitAsSegmentsNoAlloc which returns a ValueEnumerable for zero-allocation foreach loops.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT008.md");

	public static readonly DiagnosticDescriptor UseSplitAsSegmentsNoAlloc = new(
		id: "OPENTXT009",
		title: "Use SplitAsSegmentsNoAlloc for zero-allocation enumeration",
		messageFormat: "Consider using '.SplitAsSegmentsNoAlloc({0})' for zero-allocation foreach loops. Requires 'using ZLinq;' for LINQ operations.",
		category: Category,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "SplitAsSegmentsNoAlloc returns a ValueEnumerable<StringSegmentSplitEnumerator, StringSegment> that enables zero-allocation enumeration when used with foreach or ZLinq operations.",
		helpLinkUri: HelpLinkUriBase + "OPENTXT009.md");
}
