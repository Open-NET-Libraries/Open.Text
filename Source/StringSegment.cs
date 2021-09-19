using System;
using System.Collections.Generic;

namespace Open.Text
{
	/// <summary>
	/// Similar to an ArraySegment but specifically for strings.
	/// </summary>
	public struct StringSegment
	{
		private StringSegment(string source, int start, int length)
		{
			Source = source;
			if (start > source.Length) throw new ArgumentOutOfRangeException(nameof(start), start, "Cannot be greater than the length of the string.");
			if (start + length > source.Length) throw new ArgumentOutOfRangeException(nameof(length), length, "Exceeds the number of characters available.");
			Index = start;
			Length = length;
		}

		public static StringSegment Create(string source, int start, int length)
		{
			return new(source ?? throw new ArgumentNullException(nameof(source)), start, length);
		}

		public static StringSegment Create(string source, int start)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			return new(source, start, source.Length - start);
		}

		public static StringSegment Create(string source)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			return new(source, 0, source.Length);
		}

		public StringSegment SourceSegment
		{
			get
			{
				if (!IsValid || Index == 0 && Length == Source.Length) return this;
				return Create(Source);
			}
		}

		public bool IsValid => Source is not null;

		public string Source { get; }
		public int Index { get; }
		public int Length { get; }

		public ReadOnlyMemory<char> AsMemory()
			=> IsValid ? Source.AsMemory(Index, Length) : ReadOnlyMemory<char>.Empty;

		public ReadOnlySpan<char> AsSpan()
			=> IsValid ? Source.AsSpan(Index, Length) : ReadOnlySpan<char>.Empty;

		public override string ToString()
			=> IsValid ? Source.Substring(Index, Length) : base.ToString();

		/// <summary>
		/// Gets the string segment that precedes this one.
		/// </summary>
		/// <param name="includeSegment">When true, will include this segment.</param>
		public StringSegment Preceding(bool includeSegment = false)
			=> IsValid ? Create(Source, 0, includeSegment ? (Index + Length) : Index) : default;

		/// <summary>
		/// Gets the string segment that follows this one.
		/// </summary>
		/// <param name="includeSegment">When true, will include this segment.</param>
		public StringSegment Following(bool includeSegment = false)
			=> IsValid ? Create(Source, includeSegment ? Index : (Index + Length)) : default;

		/// <param name="maxCharacters">The max number of characters to get.</param>
		/// <inheritdoc cref="Preceding"/>
		public StringSegment Preceding(int maxCharacters, bool includeSegment = false)
		{
			if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
			if (!IsValid) return default;
			if (maxCharacters == 0) return includeSegment ? this : new(Source, Index, 0);
			var start = Math.Max(0, Index - maxCharacters);
			return new(Source, start, includeSegment ? (Index - start + Length) : (Index - start));
		}

		/// <param name="maxCharacters">The max number of characters to get.</param>
		/// <inheritdoc cref="Following"/>
		public StringSegment Following(int maxCharacters, bool includeSegment = false)
		{
			if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
			if (!IsValid) return default;
			var start = includeSegment ? Index : (Index + Length);
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
			if (newIndex < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index less than the start of the string.");
			if (offset > Length) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot index greater than the length of the segment.");
			return new(Source, newIndex, Length - offset);
		}

		/// <summary>
		/// Creates a StringSegment modifies the length by the value provided.
		/// </summary>
		/// <param name="offset">The value (positive or negative) to adjust the length.</param>
		public StringSegment OffsetLength(int offset)
		{
			if (!IsValid) return default;
			var newLength = Length + offset;
			if (newLength < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot shrink less than the start of the string.");
			if (Index + newLength > Source.Length) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Cannot expand greater than the length of the source.");
			return new(Source, Index, newLength);
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
				if (offset == 0 && length == 0) return this;
				throw new InvalidOperationException("Cannot slice a null value.");
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
				var end = Index + Length;
				if (newEnd > end)
				{
					if (newIndex > end) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Index is greater than the length of the segment.");
					throw new ArgumentOutOfRangeException(nameof(length), length, "Desired length will extend greater than the length of the segment.");
				}
				return new(Source, newIndex, length);
			}

		}

		/// <summary>
		/// Returns true if the other string segment values match.
		/// </summary>
		public bool Equals(StringSegment other)
			=> Index == other.Index & Length == other.Length && Source == other.Source;


		/// <inheritdoc cref="string.Equals(string, StringComparison)">
		public bool Equals(string? other, StringComparison stringComparison = StringComparison.Ordinal)
		{
			if (other is null) return !IsValid;
			if (other.Length != Length) return false;
			return AsSpan().Equals(other, stringComparison);
		}

		public bool Equals(in ReadOnlySpan<char> other, StringComparison stringComparison = StringComparison.Ordinal)
		{
			if (other.Length != Length) return false;
			return AsSpan().Equals(other, stringComparison);
		}

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

		public static bool operator ==(StringSegment left, StringSegment right)
			=> left.Equals(right);

		public static bool operator !=(StringSegment left, StringSegment right)
			=> !(left == right);

		public static implicit operator ReadOnlySpan<char>(StringSegment segment) => segment.AsSpan();

		public static implicit operator ReadOnlyMemory<char>(StringSegment segment) => segment.AsMemory();
	}
}
