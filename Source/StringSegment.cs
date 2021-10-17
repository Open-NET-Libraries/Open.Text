using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Open.Text;

/// <summary>
/// Similar to an ArraySegment but specifically for strings.<br/>
/// Provides a reference to the original string.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "AsSpan and AsMemory methods are available.")]
public readonly struct StringSegment : IEquatable<StringSegment>
{
	private StringSegment(string source, int start, int length)
	{
		Source = source;
		if (start > source.Length) throw new ArgumentOutOfRangeException(nameof(start), start, "Cannot be greater than the length of the string.");
		if (start + length > source.Length) throw new ArgumentOutOfRangeException(nameof(length), length, "Exceeds the number of characters available.");
		Index = start;
		Length = length;
		End = start + length;
	}

	/// <summary>
	/// A StringSegment representing an empty string.
	/// </summary>
	public static readonly StringSegment Empty = new(string.Empty, 0, 0);

	/// <summary>
	/// Creates a StringSegment representing the provided string.
	/// </summary>
	/// <param name="source">The string the segment belongs to.</param>
	/// <param name="start">The starting point of the string to use as the index of the segment.</param>
	/// <param name="length">The length of the segment.</param>
	/// <exception cref="ArgumentNullException">If the source is null.</exception>
	public static StringSegment Create(string source, int start, int length)
		=> new(source ?? throw new ArgumentNullException(nameof(source)), start, length);

	/// <inheritdoc cref="Create(string, int, int)"/>
	public static StringSegment Create(string source, int start)
		=> source is null
		? throw new ArgumentNullException(nameof(source))
		: (new(source, start, source.Length - start));

	/// <inheritdoc cref="Create(string, int, int)"/>
	public static StringSegment Create(string source)
		=> source is null
		? throw new ArgumentNullException(nameof(source))
		: (new(source, 0, source.Length));

	/// <summary>
	/// A segment representing the full length of the source string.
	/// </summary>
	public StringSegment SourceSegment
		=> !IsValid || Index == 0 && Length == Source.Length
		? this
		: Create(Source);

	/// <summary>
	/// True if this has a source string.
	/// False if it is a default segment.
	/// </summary>
	public bool IsValid
		=> Source is not null;

	/// <summary>
	/// The original string this segment is using.
	/// </summary>
	public readonly string Source { get; }

	/// <summary>
	/// The starting point on the original string that this segment begins.
	/// </summary>
	public readonly int Index { get; }

	/// <summary>
	/// The length of the segment.
	/// </summary>
	public readonly int Length { get; }

	/// <summary>
	/// The index just beyond the last character.
	/// </summary>
	public readonly int End { get; }

	/// <summary>
	/// Returns a ReadOnlyMemory representing the segment of the string defined by this.
	/// </summary>
	public ReadOnlyMemory<char> AsMemory()
		=> IsValid
		? Source.AsMemory(Index, Length)
		: ReadOnlyMemory<char>.Empty;

	/// <summary>
	/// Returns a ReadOnlySpan representing the segment of the string defined by this.
	/// </summary>
	public ReadOnlySpan<char> AsSpan()
		=> IsValid
		? Source.AsSpan(Index, Length)
		: ReadOnlySpan<char>.Empty;

	/// <summary>
	/// Returns a string representing the value of this StringSegment.
	/// </summary>
	/// <remarks>Will return the base.ToString() value if this is invalid.</remarks>
	public override string ToString()
		=> IsValid
		? Source.Substring(Index, Length)
		: base.ToString();

	/// <inheritdoc cref="Preceding(int, bool)"/>
	public StringSegment Preceding(bool includeSegment = false)
		=> IsValid
		? Create(Source, 0, includeSegment ? End : Index)
		: default;

	/// <inheritdoc cref="Following(int, bool)"/>
	public StringSegment Following(bool includeSegment = false)
		=> IsValid
		? Create(Source, includeSegment ? Index : End)
		: default;

	/// <summary>
	/// Gets the string segment that precedes this one.
	/// </summary>
	/// <param name="maxCharacters">The max number of characters to get.</param>
	/// <param name="includeSegment">When true, will include this segment.</param>
	public StringSegment Preceding(int maxCharacters, bool includeSegment = false)
	{
		if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
		if (!IsValid) return default;
		if (maxCharacters == 0) return includeSegment ? this : new(Source, Index, 0);
		var start = Math.Max(0, Index - maxCharacters);
		return new(Source, start, includeSegment ? (End - start) : (Index - start));
	}

	/// <summary>
	/// Gets the string segment that follows this one.
	/// </summary>
	/// <param name="maxCharacters">The max number of characters to get.</param>
	/// <param name="includeSegment">When true, will include this segment.</param>
	public StringSegment Following(int maxCharacters, bool includeSegment = false)
	{
		if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
		if (!IsValid) return default;
		var start = includeSegment ? Index : End;
		if (maxCharacters == 0) return includeSegment ? this : new(Source, start, 0);
		var len = Math.Min(includeSegment ? (maxCharacters + Length) : maxCharacters, Source.Length - start);
		return new(Source, start, len);
	}

	/// <summary>
	/// Creates a StringSegment that starts offset by the value provided.
	/// </summary>
	/// <param name="offset">The value (positive or negative) to move the index by and adjust the length.</param>
	public StringSegment OffsetIndex(int offset)
	{
		if (!IsValid) return default;
		var newIndex = Index + offset;
		return newIndex < 0
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index less than the start of the string.")
			: offset > Length
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index greater than the length of the segment.")
			: (new(Source, newIndex, Length - offset));
	}

	/// <summary>
	/// Creates a StringSegment modifies the length by the value provided.
	/// </summary>
	/// <param name="offset">The value (positive or negative) to adjust the length.</param>
	public StringSegment OffsetLength(int offset)
	{
		if (!IsValid) return default;
		var newLength = Length + offset;
		return newLength < 0
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot shrink less than the start of the string.")
			: Index + newLength > Source.Length
			? throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot expand greater than the length of the source.")
			: (new(Source, Index, newLength));
	}

	/// <summary>
	/// Creates a StringSegment that starts offset by the value provided and extends by the length provided.
	/// </summary>
	/// <param name="offset">The value (positive or negative) to move the index by.</param>
	/// <param name="length">The length desired.</param>
	/// <param name="ignoreLengthBoundary">
	///	If true, the length can exceed the segment length but not past the full length of the source string.
	///	If false (default), an ArgumentOutOfRangeException will be thrown if the expected length exeeds the segment.
	///	</param>
	public StringSegment Slice(int offset, int length, bool ignoreLengthBoundary = false)
	{
		if (!IsValid)
		{
			return offset == 0 && length == 0 ? this
				: throw new InvalidOperationException("Cannot slice a null value.");
		}
		var newIndex = Index + offset;
		if (newIndex < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index less than the start of the string.");
		var newEnd = newIndex + length;
		if (ignoreLengthBoundary)
		{
			var end = Source.Length;
			if (newEnd > end)
			{
				if (newIndex > end) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Index is greater than the end of the source string.");
				throw new ArgumentOutOfRangeException(nameof(length), length, "Desired length will extend greater than the end of the source string.");
			}
			return new(Source, newIndex, length);
		}
		else
		{
			var end = End;
			if (newEnd > end)
			{
				if (newIndex > end) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Index is greater than the length of the segment.");
				throw new ArgumentOutOfRangeException(nameof(length), length, "Desired length will extend greater than the length of the segment.");
			}
			return new(Source, newIndex, length);
		}

	}

	private int TrimStartCore(ReadOnlySpan<char> span)
	{
		int i = Index;
		var end = End;
		Debug.Assert(i < end);

		while (end != i)
		{
			if (!char.IsWhiteSpace(span[i])) break;
			++i;
		}

		return i - Index;
	}

	private int TrimStartCore(ReadOnlySpan<char> span, char trim)
	{
		int i = Index;
		var end = End;
		Debug.Assert(i < end);

		while (end != i)
		{
			if (span[i] != trim) break;
			++i;
		}

		return i - Index;
	}

	private int TrimStartCore(ReadOnlySpan<char> span, ReadOnlySpan<char> trim)
	{
		int i = Index;
		var end = End;
		Debug.Assert(i < end);

		while (end != i)
		{
			if (trim.IndexOf(span[i]) == -1) break;
			++i;
		}

		return i - Index;
	}

	private int TrimEndCore(ReadOnlySpan<char> span)
	{
		int i = Index;
		var end = End;
		Debug.Assert(i < end);

		while (end != i)
		{
			if (!char.IsWhiteSpace(span[end - 1])) break;
			--end;
		}

		return End - end;
	}

	private int TrimEndCore(ReadOnlySpan<char> span, char trim)
	{
		int i = Index;
		var end = End;
		Debug.Assert(i < end);

		while (end != i)
		{
			if (span[end - 1] != trim) break;
			--end;
		}

		return End - end;
	}

	private int TrimEndCore(ReadOnlySpan<char> span, ReadOnlySpan<char> trim)
	{
		int i = Index;
		var end = End;
		Debug.Assert(i < end);

		while (end != i)
		{
			if (trim.IndexOf(span[end - 1]) == -1) break;
			--end;
		}

		return End - end;
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have whitespace at the beginning.
	/// </summary>
	public StringSegment TrimStart()
	{
		if (!IsValid || Length == 0) return this;

		var trimmed = TrimStartCore(Source.AsSpan());
		return trimmed == 0 ? this
			: Create(Source, Index + trimmed, Length - trimmed);
	}

	/// <summary>
	/// Returns a StringSegment that does not have whitespace at the end.
	/// </summary>
	public StringSegment TrimEnd()
	{
		if (!IsValid || Length == 0) return this;

		var trimmed = TrimEndCore(Source.AsSpan());
		return trimmed == 0 ? this
			: Create(Source, Index, Length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the beginning.
	/// </summary>
	/// <param name="trim">The character to skip over.</param>
	public StringSegment TrimStart(char trim)
	{
		if (!IsValid || Length == 0) return this;

		var trimmed = TrimStartCore(Source.AsSpan(), trim);
		return trimmed == 0 ? this
			: Create(Source, Index + trimmed, Length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(char)"/>
	public StringSegment TrimEnd(char trim)
	{
		if (!IsValid || Length == 0) return this;

		var trimmed = TrimEndCore(Source.AsSpan(), trim);
		return trimmed == 0 ? this
			: Create(Source, Index, Length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have any of the trim characters at the beginning.
	/// </summary>
	/// <param name="trim">The characters to skip over.</param>
	public StringSegment TrimStart(ReadOnlySpan<char> trim)
	{
		if (!IsValid || Length == 0) return this;

		var trimmed = TrimStartCore(Source.AsSpan(), trim);
		return trimmed == 0 ? this
			: Create(Source, Index + trimmed, Length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have any of the trim characters at the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(ReadOnlySpan{char})"/>
	public StringSegment TrimEnd(ReadOnlySpan<char> trim)
	{
		if (!IsValid || Length == 0) return this;

		var trimmed = TrimEndCore(Source.AsSpan(), trim);
		return trimmed == 0 ? this
			: Create(Source, Index, Length - trimmed);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have whitespace at the beginning nor the end.
	/// </summary>
	public StringSegment Trim()
	{
		if (!IsValid || Length == 0) return this;

		var span = Source.AsSpan();
		var trimmedEnd = TrimEndCore(span);
		if (trimmedEnd == Length) return Create(Source, Index, 0);
		var trimmedStart = TrimStartCore(span);
		return trimmedEnd ==0 && trimmedStart==0 ? this
			: Create(Source, Index + trimmedStart, Length - trimmedEnd - trimmedStart);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the beginning nor the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(char)"/>
	public StringSegment Trim(char trim)
	{
		if (!IsValid || Length == 0) return this;

		var span = Source.AsSpan();
		var trimmedEnd = TrimEndCore(span, trim);
		if (trimmedEnd == Length) return Create(Source, Index, 0);
		var trimmedStart = TrimStartCore(span, trim);
		return trimmedEnd == 0 && trimmedStart == 0 ? this
			: Create(Source, Index + trimmedStart, Length - trimmedEnd - trimmedStart);
	}

	/// <summary>
	/// Returns the StringSegment of this segment that does not have the trim character at the beginning nor the end.
	/// </summary>
	/// <inheritdoc cref="TrimStart(ReadOnlySpan{char})"/>
	public StringSegment Trim(ReadOnlySpan<char> trim)
	{
		if (!IsValid || Length == 0) return this;

		var span = Source.AsSpan();
		var trimmedEnd = TrimEndCore(span, trim);
		if (trimmedEnd == Length) return Create(Source, Index, 0);
		var trimmedStart = TrimStartCore(span, trim);
		return trimmedEnd == 0 && trimmedStart == 0 ? this
			: Create(Source, Index + trimmedStart, Length - trimmedEnd - trimmedStart);
	}

	/// <summary>
	/// Returns true if the other string segment values match.
	/// </summary>
	public bool Equals(StringSegment other)
		=> Index == other.Index & Length == other.Length && Source == other.Source;

	/// <summary>
	/// Determines whether this StringSegment and a specified System.String object have the same characters.
	/// </summary>
	/// <inheritdoc cref="string.Equals(string, StringComparison)"/>
	public bool Equals(string? other, StringComparison stringComparison = StringComparison.Ordinal)
		=> other is null ? !IsValid : other.Length == Length && AsSpan().Equals(other, stringComparison);

	/// <summary>
	/// Determines whether this StringSegment and a specified ReadOnlySpan have the same characters.
	/// </summary>
	/// <inheritdoc cref="string.Equals(string, StringComparison)"/>
	public bool Equals(ReadOnlySpan<char> other, StringComparison stringComparison = StringComparison.Ordinal)
		=> other.Length == Length && AsSpan().Equals(other, stringComparison);

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is StringSegment segment && Equals(segment);

	/// <inheritdoc />
#if NETSTANDARD2_1
	public override int GetHashCode()
		=> HashCode.Combine(Source, Index, Length);
#else
	public override int GetHashCode()
	{
		int hashCode = 1124846000;
		hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Source);
		hashCode = hashCode * -1521134295 + Index.GetHashCode();
		hashCode = hashCode * -1521134295 + Length.GetHashCode();
		return hashCode;
	}
#endif

	/// <summary>
	/// Compares two StringSegments for equality.
	/// </summary>
	public static bool operator ==(StringSegment left, StringSegment right)
		=> left.Equals(right);

	/// <summary>
	/// Compares two StringSegments for inequality.
	/// </summary>
	public static bool operator !=(StringSegment left, StringSegment right)
		=> !left.Equals(right);

	/// <summary>
	/// Implicitly converts a StringSegment to a ReadOnlySpan.
	/// </summary>
	public static implicit operator ReadOnlySpan<char>(StringSegment segment) => segment.AsSpan();

	/// <summary>
	/// Implicitly converts a StringSegment to a ReadOnlyMemory.
	/// </summary>
	public static implicit operator ReadOnlyMemory<char>(StringSegment segment) => segment.AsMemory();
}
