# Open.Text

A set of useful extensions for working with strings, spans, and value formatting.

[![NuGet](https://img.shields.io/nuget/v/Open.Text.svg)](https://www.nuget.org/packages/Open.Text/)

## Features

* v3.x is a major overhaul with much improved methods and expanded tests and coverage.
* Avoids allocation wherever possible.

### String vs Span Equality

Optimized `.Equals(...)` extension methods for comparing spans and strings.

### String & Span Splitting

#### `SplitToEnumerable`

Returns each string segment of the split through an enumerable instead of all at once in an array.

#### `SplitToMemory`

Produces an enumerable where each segment is yielded as a `ReadOnlyMemory<char>`.

### Trimming

#### `TrimStartPattern` & `TrimEndPattern`

Similar to their character trimming counterparts, these methods can trim sequences of characters or regular expression patterns.

### String Segments

Similar to `ArraySegment`, `StringSegment` offers methods for operating on strings without requiring allocation.

Instead of extensions like `string.BeforeFirst(search)`, now you can call `string.First(search).Preceding()`.

### StringBuilder Extensions

* Extensions for adding segments with separators.
* Extensions for adding spans without creating a string first.
* Extensions for converting enumerables to a `StringBuilder`.

### ... And more

Various formatting and `Regex` extensions including `Capture.AsSpan()` for getting a `ReadOnlySpan<char>` instead of allocating a string.

