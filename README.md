# Open.Text

A set of useful extensions for working with strings, string-segments, spans, enums, and value formatting.

[![NuGet](https://img.shields.io/nuget/v/Open.Text.svg)](https://www.nuget.org/packages/Open.Text/)

## Features

* Avoids allocation wherever possible.
* v3.x is a major overhaul with much improved methods and expanded tests and coverage.
* v4.x favored use of `Microsoft.Extensions.Primitives.StringSegments` for string manipulation.
* **NEW:** Zero-allocation `*NoAlloc` methods available in the separate [`Open.Text.ZLinq`](#opentextzlinq---zero-allocation-extension-package) package!
* **NEW:** Roslyn analyzers to detect inefficient string patterns and suggest modern alternatives!

---

## 📊 Benchmark Summary

Comparing `string.Split()` (BCL) vs `SplitAsSegments` (IEnumerable) vs `SplitAsSegmentsNoAlloc` (ValueEnumerable via ZLinq):

| Category | Method | Time | Allocated |
|----------|--------|-----:|----------:|
| **Count** | BCL Split + LINQ Count | 46.9 ns | 256 B |
| | SplitAsSegments + LINQ Count | 61.9 ns | 88 B |
| | **SplitAsSegmentsNoAlloc + ZLinq Count** | 55.3 ns | **0 B** ✅ |
| **LINQ-Chain** | **SplitAsSegmentsNoAlloc + ZLinq** | 50.8 ns | **0 B** ✅ |
| | BCL + System.Linq | 65.5 ns | 304 B |
| | SplitAsSegments + System.Linq | 104.4 ns | 152 B |
| **Large-Foreach** | BCL Split (1000 items) | 7,342 ns | 47,952 B |
| | SplitAsSegments (1000 items) | 11,740 ns | 88 B |
| | **SplitAsSegmentsNoAlloc (1000 items)** | 11,557 ns | **0 B** ✅ |
| **Small-Foreach** | BCL Split | 42.5 ns | 256 B |
| | **SplitAsSegmentsNoAlloc** | 45.6 ns | **0 B** ✅ |
| | SplitAsSegments | 72.4 ns | 88 B |
| **Seq-Split** | **SplitAsSegmentsNoAlloc(string)** | 177.6 ns | **0 B** ✅ |
| | SplitAsSegments(string) | 215.2 ns | 128 B |
| | BCL Split(string) | 227.1 ns | 696 B |

> **Key Takeaway:** The `SplitAsSegmentsNoAlloc` methods achieve **zero heap allocations** when iterating `StringSegment` values—ideal for high-throughput scenarios where GC pressure matters.
>
> **Note:** Regex-based split methods have unavoidable `Match` object allocations.

---

## Open.Text.ZLinq - Zero-Allocation Extension Package

For scenarios requiring **true zero-allocation** string operations, install the companion package:

```bash
dotnet add package Open.Text.ZLinq
```

This package provides `*NoAlloc` extension methods that return ZLinq `ValueEnumerable<TEnumerator, T>` structs instead of heap-allocated enumerables. These methods integrate seamlessly with [ZLinq](https://github.com/Cysharp/ZLinq) for zero-allocation LINQ operations.

### Available Methods

- `SplitAsSegmentsNoAlloc(char)` - Split by character
- `SplitAsSegmentsNoAlloc(string)` - Split by string sequence
- `SplitAsSegmentsNoAlloc(Regex)` - Split by regex pattern
- `JoinNoAlloc(...)` - Join segments with separator
- `ReplaceNoAlloc(...)` / `ReplaceAsSegmentsNoAlloc(...)` - Replace sequences
- `AsSegmentsNoAlloc(Regex)` - Get regex matches as segments

### Example

```cs
using Open.Text;
using ZLinq;

// Zero-allocation split and filter
var count = "a,b,c,d,e"
    .SplitAsSegmentsNoAlloc(',')
    .Where(s => s.Length > 0)
    .Count(); // No heap allocations!
```

---

## 🔍 Roslyn Analyzers

Open.Text now includes **Roslyn analyzers** that help you write more efficient code by detecting common string manipulation anti-patterns and suggesting better alternatives using spans and string segments.

### Installation

```bash
dotnet add package Open.Text.Analyzers
```

### What it does

The analyzers detect patterns like:
- `.Substring()` → suggests `.AsSpan()` or span slicing
- `.Split()` → suggests `.SplitAsSegments()` or `.SplitToEnumerable()` to reduce allocations
- `.Split()[0]` → suggests `.FirstSplit()` to avoid array allocation
- String concatenation in loops → suggests `StringBuilder`
- `.Trim().Equals()` → suggests `.TrimEquals()` to avoid intermediate string

See the [Analyzers README](Analyzers/README.md) for complete documentation.

---

### Regex Extensions

```cs
ReadOnlySpan<char> Capture.AsSpan()
Enumerable<StringSegment> Regex.AsSegments(string input)
string GroupCollection.GetValue(string groupName)
ReadOnlySpan<char> GroupCollection.GetValueSpan(string groupName)
IEnumerable<StringSegment> string.Split(Regex pattern)
```

---

### String vs Span Equality

Optimized `.Equals(...)` extension methods for comparing spans and strings.

---

### String & Span Splitting

#### `SplitToEnumerable`

Returns each string segment of the split through an enumerable instead of all at once in an array.

#### `SplitAsMemory`

Produces an enumerable where each segment is yielded as a `ReadOnlyMemory<char>`.

#### `SplitAsSegment`

Produces an enumerable where each segment is yielded as a `StringSegment`.


---

### Trimming

#### `TrimStartPattern` & `TrimEndPattern`

Similar to their character trimming counterparts, these methods can trim sequences of characters or regular expression patterns.

---

### `StringBuilder` Extensions

Extensions for:

* adding segments with separators.
* adding spans without creating a string first.
* converting enumerables to a `StringBuilder`.

---
### `StringSegment` Extensions

Extensions for:

* `.Trim(char)` and `.Trim(ReadOnlySpan<char>)`.
* finding and pivoting from sequences without allocation.

---

### `StringComparable` & `SpanComparable` Extensions

```cs
if(myString.AsCaseInsensitive()=="HELLO!") { }
```

instead of

```cs
if(myString.Equals("HELLO!", StringComparison.OrdinalIgnoreCase)) { }
```
---

### `EnumValue<TEnum>` & `EnumValueIgnoreCase<TEnum>`

Implicit conversion makes it easy.  Optimized methods make it fast.

Consider the following:

```cs
enum Greek { Alpha, Beta, Gamma }

void DoSomethingWithGreek(Greek value) { }

DoSomethingWithGreek(Greek.Alpha);
```

It's nice that `Greek` is an enum because it won't be null, and it has to be one of the values.
But what if you want to write a single function that will take an `Greek` or a string?
This gets problematic as the string value has to be parsed and you'll likely need an overload.

`EnumValue<TEnum>` solves this problem:

```cs
enum Greek { Alpha, Beta, Gamma }

void DoSomethingWithGreek(EnumValue<Greek> value) { }

// Both work fine.
DoSomethingWithGreek("Alpha");
DoSomethingWithGreek(Greek.Alpha);

// Throws an ArgumentException:
DoSomethingWithGreek("Theta");
```

The implicit conversion between a `string` and `EnumValue<TEnum>` make this possible.

If you need to allow for case-insensitive comparison then simply use `EnumValueCaseIgnored<TEnum>` instead.

The performance is outstanding as it uses the length of the names to build a tree in order to parse values and uses an expression tree instead of calling `.ToString()` on the value.

---

### And more ...

#### `string.Supplant(...)`

An alternative to String.Format that takes an array of values.

#### `string.ReplaceWhiteSpace(...)`

A shortcut for replacing whitespace with a Regex.

#### `string.ToMetricString(...)`

Returns an abbreviated metric representation of a number.

#### `ToByteString(...)`

Returns an abbreviated metric representation of a quantity of bytes.

#### `ToPercentString(...)`

Shortcut for formating to a percent.

#### `ToNullIfWhiteSpace()`

Allows for simple null operators if a string is empty or whitespace.