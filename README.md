# Open.Text

A set of useful extensions for working with strings, string-segments, spans, enums, and value formatting.

[![NuGet](https://img.shields.io/nuget/v/Open.Text.svg)](https://www.nuget.org/packages/Open.Text/)

## Features

* Avoids allocation wherever possible.
* v3.x is a major overhaul with much improved methods and expanded tests and coverage.
* v4.x favored use of `Microsoft.Extensions.Primitives.StringSegments` for string manipulation.

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
* Extensions for adding spans without creating a string first.
* Extensions for converting enumerables to a `StringBuilder`.

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

### ... And more

Various formatting and `Regex` extensions including `Capture.AsSpan()` for getting a `ReadOnlySpan<char>` instead of allocating a string.

