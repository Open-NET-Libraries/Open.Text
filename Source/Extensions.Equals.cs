using System;

namespace Open.Text
{
	public static partial class Extensions
	{

		/// <summary>
		/// Optimized equals for comparing as span vs a string.
		/// </summary>
		/// <param name="source">The source span.</param>
		/// <param name="other">The string to compare to.</param>
		/// <param name="stringComparison">The string comparison type.</param>
		/// <returns>True if the are contents equal.</returns>
		public static bool Equals(this in ReadOnlySpan<char> source, string? other, StringComparison stringComparison)
		{
			if (other is null) return false;
			var len = source.Length;
			if (len != other.Length) return false;
			return len switch
			{
				0 => true,
				1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
				_ => source.Equals(other.AsSpan(), stringComparison),
			};
		}

		/// <summary>
		/// Optimized equals for comparing as span vs a string.
		/// </summary>
		/// <param name="source">The source span.</param>
		/// <param name="other">The string to compare to.</param>
		/// <param name="stringComparison">The string comparison type.</param>
		/// <returns>True if the are contents equal.</returns>
		public static bool Equals(this in Span<char> source, string? other, StringComparison stringComparison)
		{
			if (other is null) return false;
			var len = source.Length;
			if (len != other.Length) return false;
			return len switch
			{
				0 => true,
				1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
				_ => MemoryExtensions.Equals(source, other.AsSpan(), stringComparison)
			};
		}

		/// <summary>
		/// Optimized equals for comparing spans.
		/// </summary>
		/// <param name="source">The source span.</param>
		/// <param name="other">The span to compare to.</param>
		/// <param name="stringComparison">The string comparison type.</param>
		/// <returns>True if the are contents equal.</returns>
		public static bool Equals(this in Span<char> source, in Span<char> other, StringComparison stringComparison)
		{
			var len = source.Length;
			if (len != other.Length) return false;
			return len switch
			{
				0 => true,
				1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
				_ => MemoryExtensions.Equals(source, other, stringComparison)
			};
		}

		/// <summary>
		/// Optimized equals for comparing spans.
		/// </summary>
		/// <param name="source">The source span.</param>
		/// <param name="other">The span to compare to.</param>
		/// <param name="stringComparison">The string comparison type.</param>
		/// <returns>True if the are contents equal.</returns>
		public static bool Equals(this in Span<char> source, in ReadOnlySpan<char> other, StringComparison stringComparison)
		{
			var len = source.Length;
			if (len != other.Length) return false;
			return len switch
			{
				0 => true,
				1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
				_ => MemoryExtensions.Equals(source, other, stringComparison)
			};
		}

		/// <summary>
		/// Optimized equals for comparing as string to a span.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="other">The span to compare to.</param>
		/// <param name="stringComparison">The string comparison type.</param>
		/// <returns>True if the are contents equal.</returns>
		public static bool Equals(this string source, in ReadOnlySpan<char> other, StringComparison stringComparison)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			var len = source.Length;
			if (len != other.Length) return false;
			return len switch
			{
				0 => true,
				1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
				_ => source.AsSpan().Equals(other, stringComparison),
			};
		}

		/// <summary>
		/// Optimized equals for comparing as string to a span.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="other">The span to compare to.</param>
		/// <param name="stringComparison">The string comparison type.</param>
		/// <returns>True if the are contents equal.</returns>
		public static bool Equals(this string source, in Span<char> other, StringComparison stringComparison)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			var len = source.Length;
			if (len != other.Length) return false;
			return len switch
			{
				0 => true,
				1 when stringComparison == StringComparison.Ordinal => source[0] == other[0],
				_ => source.AsSpan().Equals(other, stringComparison),
			};
		}
	}
}
