# Open.Text Analyzers

Roslyn analyzers for detecting inefficient string patterns and suggesting modern alternatives using `Span<char>`, `ReadOnlySpan<char>`, and `StringSegment` from the Open.Text library.

## Installation

Install the `Open.Text.Analyzers` NuGet package:

```bash
dotnet add package Open.Text.Analyzers
```

The analyzers will automatically be added to your project and will highlight inefficient string patterns during development.

## Analyzers

### OPENTXT001: Use span slicing instead of Substring

**Problem:** Using `.Substring()` allocates a new string on the heap.

```csharp
// ❌ Inefficient
string text = "Hello World";
string sub = text.Substring(6);  // Allocates new string

// ✅ Better - Zero allocation
ReadOnlySpan<char> span = text.AsSpan(6);
// or
ReadOnlySpan<char> span = text.AsSpan()[6..];
```

### OPENTXT002: Use SplitAsSegments to reduce string allocations

**Problem:** `.Split()` allocates an array and individual strings for each segment upfront.

```csharp
// ❌ Allocates array + all strings immediately
string[] parts = text.Split(',');

// ✅ Better - Defers string allocation
IEnumerable<StringSegment> segments = text.SplitAsSegments(',');
foreach (var segment in segments)
{
    // segment is a StringSegment - strings only created when .ToString() is called
}

// ✅ Alternative - Lazy string evaluation if you need strings
IEnumerable<string> parts = text.SplitToEnumerable(',');
foreach (var part in parts)
{
    // Strings created on-demand during iteration, not upfront
}
```

### OPENTXT003: Combine IndexOf with span slicing

**Problem:** Using `IndexOf` followed by `Substring` allocates unnecessary strings.

```csharp
// ❌ Inefficient
int index = text.IndexOf(',');
string part = text.Substring(0, index);  // Allocates new string

// ✅ Better - Use span slicing
int index = text.IndexOf(',');
ReadOnlySpan<char> part = text.AsSpan(0, index);
```

### OPENTXT004: Avoid string concatenation in loops

**Problem:** String concatenation in loops creates multiple intermediate string allocations.

```csharp
// ❌ Very inefficient - Creates n string allocations
string result = "";
for (int i = 0; i < 100; i++)
{
    result += "item" + i;  // Each += allocates a new string
}

// ✅ Better - Single allocation at the end
var sb = new StringBuilder();
for (int i = 0; i < 100; i++)
{
    sb.Append("item").Append(i);
}
string result = sb.ToString();
```

### OPENTXT005: Use TrimEquals extension method

**Problem:** Calling `.Trim()` followed by `.Equals()` allocates an intermediate trimmed string.

```csharp
// ❌ Inefficient
bool isEqual = text.Trim().Equals("hello");  // Allocates trimmed string

// ✅ Better - No intermediate allocation
bool isEqual = text.TrimEquals("hello");  // From Open.Text
```

### OPENTXT006: Use AsSegment for string manipulation

**Problem:** Multiple string operations can be chained more efficiently with `StringSegment`.

```csharp
// ❌ Less efficient
string result = text.Trim().Substring(5, 10).ToUpper();

// ✅ Better
StringSegment segment = text.AsSegment().Trim().Subsegment(5, 10);
string result = segment.ToString().ToUpper();
```

### OPENTXT007: Use FirstSplit to avoid allocating full split array

**Problem:** Using `.Split()[0]` or `.Split().First()` allocates an entire array when you only need the first element.

```csharp
// ❌ Inefficient - Allocates entire array
string first = text.Split(',')[0];
string first = text.Split(',').First();

// ✅ Better - Only processes first segment
ReadOnlySpan<char> first = text.FirstSplit(',', out int nextIndex);
```

### OPENTXT008: Use SplitToEnumerable for deferred execution

**Problem:** `.Split()` allocates the full array upfront, even if you only iterate part of it.

```csharp
// ❌ Inefficient - Allocates full array
foreach (var part in text.Split(','))
{
    if (part == "target") break;  // May not need all parts
}

// ✅ Better - Lazy evaluation
foreach (var part in text.SplitToEnumerable(','))
{
    if (part == "target") break;  // Stops processing early
}
```

## Benefits

- **Performance:** Reduce memory allocations and improve throughput
- **Memory:** Lower GC pressure by avoiding unnecessary string allocations
- **Modern .NET:** Take advantage of `Span<T>` and `Memory<T>` introduced in modern .NET
- **Best Practices:** Learn and adopt efficient string manipulation patterns

## Severity Levels

- **Info (💡):** Suggestions for potential improvements (OPENTXT001, 002, 003, 005, 006, 007, 008)
- **Warning (⚠️):** Issues that should be addressed (OPENTXT004 - string concatenation in loops)

## Configuration

You can configure the severity of each analyzer in your `.editorconfig`:

```ini
# Disable a specific analyzer
dotnet_diagnostic.OPENTXT001.severity = none

# Change severity to warning
dotnet_diagnostic.OPENTXT002.severity = warning

# Change severity to error
dotnet_diagnostic.OPENTXT004.severity = error
```

## Requirements

- .NET SDK 5.0 or later (for full span support)
- Visual Studio 2022 or later, or VS Code with C# extension
- Open.Text library (for runtime support)

## Contributing

Found a bug or have a suggestion? Please open an issue at:
https://github.com/electricessence/Open.Text/issues

## License

MIT License - See LICENSE.md for details
