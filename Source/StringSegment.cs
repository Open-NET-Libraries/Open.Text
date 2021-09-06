﻿using System;
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
		public StringSegment Preceding()
			=> IsValid ? Create(Source, 0, Index) : default;

		/// <summary>
		/// Gets the string segment that follows this one.
		/// </summary>
		public StringSegment Following()
			=> IsValid ? Create(Source, Index + Length) : default;

		/// <inheritdoc cref="Preceding"/>
		/// <param name="maxCharacters">The max number of characters to get.</param>
		public StringSegment Preceding(int maxCharacters)
		{
			if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
			if (!IsValid) return default;
			if (maxCharacters == 0) return new(Source, Index, 0);
			var start = Math.Max(0, Index - maxCharacters);
			return new(Source, start, Index - start);
		}

		/// <inheritdoc cref="Following"/>
		/// <param name="maxCharacters">The max number of characters to get.</param>
		public StringSegment Following(int maxCharacters)
		{
			if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters), maxCharacters, "Must be at least zero.");
			if (!IsValid) return default;
			var start = Index + Length;
			if (maxCharacters == 0) return new(Source, start, 0);
			var len = Source.Length;
			return new(Source, start, Math.Min(maxCharacters, len - start));
		}

		/// <summary>
		/// Returns true if the other string segment values match.
		/// </summary>
		public bool Equals(StringSegment other)
			=> Index == other.Index & Length == other.Length && Source == other.Source;
		
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