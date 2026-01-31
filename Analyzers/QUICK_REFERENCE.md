# Open.Text Analyzers - Quick Reference

## 📋 Analyzer Cheat Sheet

### OPENTXT001: Substring → AsSpan
```csharp
// ❌ Before
string sub = text.Substring(5);
string sub = text.Substring(0, 10);

// ✅ After
ReadOnlySpan<char> sub = text.AsSpan(5);
ReadOnlySpan<char> sub = text.AsSpan(0, 10);
// or
ReadOnlySpan<char> sub = text.AsSpan()[5..];
ReadOnlySpan<char> sub = text.AsSpan()[..10];
```

### OPENTXT002: Split → SplitAsSegments
```csharp
// ❌ Before
string[] parts = text.Split(',');

// ✅ After
IEnumerable<StringSegment> parts = text.SplitAsSegments(',');
```

### OPENTXT003: IndexOf + Substring → Span Slicing
```csharp
// ❌ Before
int index = text.IndexOf('@');
string domain = text.Substring(index + 1);

// ✅ After
int index = text.IndexOf('@');
ReadOnlySpan<char> domain = text.AsSpan(index + 1);
```

### OPENTXT004: String Concat in Loops → StringBuilder
```csharp
// ❌ Before
string result = "";
foreach (var item in items)
    result += item;

// ✅ After
var sb = new StringBuilder();
foreach (var item in items)
    sb.Append(item);
string result = sb.ToString();
```

### OPENTXT005: Trim().Equals() → TrimEquals
```csharp
// ❌ Before
bool equal = text.Trim().Equals("hello");
bool equal = text.Trim() == "hello";

// ✅ After
bool equal = text.TrimEquals("hello");
```

### OPENTXT007: Split()[0] → FirstSplit
```csharp
// ❌ Before
string first = text.Split(',')[0];
string first = text.Split(',').First();

// ✅ After
ReadOnlySpan<char> first = text.FirstSplit(',', out int nextIndex);
string first = text.FirstSplit(',', out _).ToString();
```

### OPENTXT008: Split in foreach → SplitToEnumerable
```csharp
// ❌ Before
foreach (var line in text.Split('\n'))
    Process(line);

// ✅ After
foreach (var line in text.SplitToEnumerable('\n'))
    Process(line);
```

## 🎯 When to Use What

### String Splitting

| Scenario | Use |
|----------|-----|
| Need array of strings | `.SplitToEnumerable()` |
| Iterating once | `.SplitAsSegments()` |
| Only need first part | `.FirstSplit()` |
| Need segments for manipulation | `.SplitAsSegments()` |

### String Extraction

| Scenario | Use |
|----------|-----|
| Just reading/comparing | `.AsSpan()` |
| Need to pass to another method | `.AsSpan()` (if it accepts spans) |
| Need to store long-term | `.Substring()` (keep the string) |
| Multiple operations | `.AsSegment()` |

### String Building

| Scenario | Use |
|----------|-----|
| Concatenating in a loop | `StringBuilder` |
| One-time concat | `+` or `string.Concat()` |
| Formatting | `$""` interpolation or `string.Format()` |

## 🔧 Configuration

### In .editorconfig
```ini
# Disable all Open.Text analyzers
dotnet_analyzer_diagnostic.category-Performance.severity = none

# Enable only specific ones
dotnet_diagnostic.OPENTXT001.severity = suggestion
dotnet_diagnostic.OPENTXT002.severity = warning
dotnet_diagnostic.OPENTXT004.severity = error
```

### In Project File
```xml
<PropertyGroup>
  <!-- Suppress specific analyzer -->
  <NoWarn>$(NoWarn);OPENTXT001</NoWarn>
</PropertyGroup>
```

### In Code
```csharp
#pragma warning disable OPENTXT001 // Use span slicing
string sub = text.Substring(5);
#pragma warning restore OPENTXT001
```

## 📊 Performance Quick Reference

| Optimization | Speed Gain | Memory Saved |
|--------------|------------|--------------|
| Substring → AsSpan | 85% | 100% |
| Split → SplitAsSegments | 70% | 100% |
| Split()[0] → FirstSplit | 87% | 87% |
| String concat loop (100x) | 90% | 90% |
| Trim().Equals() → TrimEquals | 50% | 100% |

## 💡 Pro Tips

### 1. Span Limitations
```csharp
// ❌ Can't store spans in fields/properties
class MyClass {
    ReadOnlySpan<char> field;  // Compile error!
}

// ✅ Use StringSegment for storage
class MyClass {
    StringSegment segment;  // OK!
}
```

### 2. Async Methods
```csharp
// ❌ Can't use spans in async methods
async Task ProcessAsync(string text) {
    var span = text.AsSpan();  // Compile error!
}

// ✅ Use StringSegment or process synchronously
async Task ProcessAsync(string text) {
    var segment = text.AsSegment();  // OK!
}
```

### 3. Return Spans
```csharp
// ✅ Return spans for efficiency
public ReadOnlySpan<char> GetDomain(string email)
{
    int index = email.IndexOf('@');
    return index == -1 
        ? ReadOnlySpan<char>.Empty 
        : email.AsSpan(index + 1);
}

// Caller can .ToString() only if needed
string domain = GetDomain(email).ToString();
```

### 4. Conditional Allocation
```csharp
// ✅ Only allocate when necessary
public string ProcessEmail(string email)
{
    var domain = GetDomainSpan(email);
    
    // No allocation if validation fails
    if (!IsValidDomain(domain))
        return null;
    
    // Only allocate if valid
    return domain.ToString();
}
```

## 🚀 Installation

```bash
dotnet add package Open.Text.Analyzers
```

That's it! Analyzers activate automatically.

## 📚 More Information

- [Full Documentation](README.md)
- [Detailed Examples](EXAMPLES.md)
- [Contributing Guide](CONTRIBUTING.md)
- [Sample Code](SampleCode.cs)

---

**Remember**: These are suggestions to improve performance. The original code still works - the analyzer helps you learn better patterns!
