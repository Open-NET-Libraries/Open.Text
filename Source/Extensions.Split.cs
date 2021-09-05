using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Open.Text
{
	public static partial class Extensions
	{
		static ReadOnlySpan<char> FirstSplitSpan(string source, int start, int i, int n, out int nextIndex)
		{
			Debug.Assert(start >= 0);
			if (i == -1)
			{
				nextIndex = -1;
				return start == 0 ? source.AsSpan() : source.AsSpan(start);
			}

			nextIndex = i + n;
			var segmentLen = i - start;
			if (segmentLen == 0) return ReadOnlySpan<char>.Empty;
			return source.AsSpan(start, segmentLen);
		}

		static ReadOnlySpan<char> FirstSplitSpan(ReadOnlySpan<char> rest, int i, int n, out int nextIndex)
		{
			if (i == -1)
			{
				nextIndex = -1;
				return rest;
			}

			nextIndex = i + n;
			if (i == 0) return ReadOnlySpan<char>.Empty;
			return rest.Slice(0, i);
		}

		static ReadOnlyMemory<char> FirstSplitMemory(string source, int start, int i, int n, out int nextIndex)
		{
			Debug.Assert(start >= 0);
			if (i == -1)
			{
				nextIndex = -1;
				return start == 0 ? source.AsMemory() : source.AsMemory(start);
			}

			nextIndex = i + n;
			var segmentLen = i - start;
			if (segmentLen == 0) return ReadOnlyMemory<char>.Empty;
			return source.AsMemory(start, segmentLen);
		}


		/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, char, out int)"/>
		/// <param name="startIndex">The index to start the split.</param>
		public static ReadOnlySpan<char> FirstSplit(this string source,
			char splitCharacter,
			out int nextIndex,
			int startIndex = 0)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			var len = source.Length;
			if (startIndex > len) throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Is greater than the length of the provided string.");
			Contract.EndContractBlock();

			if (startIndex == len)
			{
				nextIndex = -1;
				return ReadOnlySpan<char>.Empty;
			}

			return FirstSplitSpan(source, startIndex, source.IndexOf(splitCharacter, startIndex), 1, out nextIndex);
		}

		/// <inheritdoc cref="FirstSplit(ReadOnlySpan{char}, ReadOnlySpan{char}, out int, StringComparison)"/>
		/// <param name="startIndex">The index to start the split.</param>
		public static ReadOnlySpan<char> FirstSplit(this string source,
			string splitSequence,
			out int nextIndex,
			int startIndex = 0,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
			if (splitSequence.Length == 0)
				throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
			var len = source.Length;
			if (startIndex > source.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Is greater than the length of the provided string.");
			Contract.EndContractBlock();

			if (startIndex == len)
			{
				nextIndex = -1;
				return ReadOnlySpan<char>.Empty;
			}

			return FirstSplitSpan(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), splitSequence.Length, out nextIndex);
		}

		/// <summary>
		/// Finds the first instance of a character and returns the set of characters up to that character.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitCharacter">The charcter to find.</param>
		/// <param name="nextIndex">The next possible index following the the current one.</param>
		/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
		public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
			char splitCharacter,
			out int nextIndex)
		{
			if (source.Length == 0)
			{
				nextIndex = -1;
				return ReadOnlySpan<char>.Empty;
			}

			var i = source.IndexOf(splitCharacter);
			return FirstSplitSpan(source, i, 1, out nextIndex);
		}

		/// <summary>
		/// Finds the first instance of a character sequence and returns the set of characters up to that sequence.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitSequence">The sequence to find.</param>
		/// <param name="nextIndex">The next possible index following the the current one.</param>
		/// <param name="comparisonType">The string comparison type to use.</param>
		/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
		public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
			ReadOnlySpan<char> splitSequence,
			out int nextIndex,
			StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (splitSequence.Length == 0)
				throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
			Contract.EndContractBlock();

			if (source.Length == 0)
			{
				nextIndex = -1;
				return ReadOnlySpan<char>.Empty;
			}

			var i = source.IndexOf(splitSequence, comparisonType);
			return FirstSplitSpan(source, i, 1, out nextIndex);
		}

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

		/// <summary>
		/// Enumerates a string by segments that are separated by the split character.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitCharacter">The charcter to find.</param>
		/// <param name="options">Can specify to omit empty entries.</param>
		/// <returns>An enumerable of the segments.</returns>
		public static IEnumerable<string> SplitToEnumerable(this string source,
			char splitCharacter,
			StringSplitOptions options = StringSplitOptions.None)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			return options switch
			{
				StringSplitOptions.None => source.Length == 0
					? Enumerable.Repeat(string.Empty, 1)
					: SplitAsEnumerableCore(),
				StringSplitOptions.RemoveEmptyEntries => source.Length == 0
					? Enumerable.Empty<string>()
					: SplitAsEnumerableCoreOmitEmpty(),
				// _ => throw new System.ComponentModel.InvalidEnumArgumentException(),
			};

			IEnumerable<string> SplitAsEnumerableCore()
			{
				var startIndex = 0;
				do
				{
					yield return source.FirstSplit(splitCharacter, out var nextIndex, startIndex).ToString();
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}

			IEnumerable<string> SplitAsEnumerableCoreOmitEmpty()
			{
				var startIndex = 0;
				do
				{
					var result = source.FirstSplit(splitCharacter, out var nextIndex, startIndex);
					if (result.Length != 0) yield return result.ToString();
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}
		}


		/// <summary>
		/// Enumerates a string by segments that are separated by the split character.
		/// </summary>
		/// <param name="source">The source characters to look through.</param>
		/// <param name="splitSequence">The sequence to find.</param>
		/// <param name="comparisonType">The string comparison type to use.</param>
		/// <param name="options">Can specify to omit empty entries.</param>
		/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
		public static IEnumerable<string> SplitToEnumerable(this string source,
			string splitSequence,
			StringComparison comparisonType = StringComparison.Ordinal,
			StringSplitOptions options = StringSplitOptions.None)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
			if (splitSequence.Length == 0)
				throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
			Contract.EndContractBlock();

			return options switch
			{
				StringSplitOptions.None => source.Length == 0
					? Enumerable.Repeat(string.Empty, 1)
					: SplitAsEnumerableCore(),
				StringSplitOptions.RemoveEmptyEntries => source.Length == 0
					? Enumerable.Empty<string>()
					: SplitAsEnumerableCoreOmitEmpty(),
				// _ => throw new System.ComponentModel.InvalidEnumArgumentException(),
			};

			IEnumerable<string> SplitAsEnumerableCore()
			{
				var startIndex = 0;
				do
				{
					yield return source.FirstSplit(splitSequence, out var nextIndex, startIndex, comparisonType).ToString();
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}

			IEnumerable<string> SplitAsEnumerableCoreOmitEmpty()
			{
				var startIndex = 0;
				do
				{
					var result = source.FirstSplit(splitSequence, out var nextIndex, startIndex, comparisonType);
					if (result.Length != 0) yield return result.ToString();
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}
		}


		/// <inheritdoc cref="SplitToEnumerable(string, char, StringSplitOptions)">
		public static IEnumerable<ReadOnlyMemory<char>> SplitAsMemory(this string source,
			char splitCharacter,
			StringSplitOptions options = StringSplitOptions.None)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			return options switch
			{
				StringSplitOptions.None => source.Length == 0
					? Enumerable.Repeat(ReadOnlyMemory<char>.Empty, 1)
					: SplitAsMemoryCore(),
				StringSplitOptions.RemoveEmptyEntries => source.Length == 0
					? Enumerable.Empty<ReadOnlyMemory<char>>()
					: SplitAsMemoryOmitEmpty(),
				// _ => throw new System.ComponentModel.InvalidEnumArgumentException(),
			};

			IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryCore()
			{
				var startIndex = 0;
				do
				{
					yield return FirstSplitMemory(source, startIndex, source.IndexOf(splitCharacter, startIndex), 1, out var nextIndex);
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}

			IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryOmitEmpty()
			{
				var startIndex = 0;
				do
				{
					var result = FirstSplitMemory(source, startIndex, source.IndexOf(splitCharacter, startIndex), 1, out var nextIndex);
					if (result.Length != 0) yield return result;
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}
		}


		/// <inheritdoc cref="SplitToEnumerable(string, string, StringComparison, StringSplitOptions)"/>
		public static IEnumerable<ReadOnlyMemory<char>> SplitAsMemory(this string source,
			string splitSequence,
			StringComparison comparisonType = StringComparison.Ordinal,
			StringSplitOptions options = StringSplitOptions.None)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (splitSequence is null) throw new ArgumentNullException(nameof(splitSequence));
			if (splitSequence.Length == 0)
				throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
			Contract.EndContractBlock();

			return options switch
			{
				StringSplitOptions.None => source.Length == 0
					? Enumerable.Repeat(ReadOnlyMemory<char>.Empty, 1)
					: SplitAsMemoryCore(),
				StringSplitOptions.RemoveEmptyEntries => source.Length == 0
					? Enumerable.Empty<ReadOnlyMemory<char>>()
					: SplitAsMemoryOmitEmpty(),
				//_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
			};

			IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryCore()
			{
				var startIndex = 0;
				do
				{
					yield return FirstSplitMemory(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), 1, out var nextIndex);
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}

			IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryOmitEmpty()
			{
				var startIndex = 0;
				do
				{
					var result = FirstSplitMemory(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), 1, out var nextIndex);
					if (result.Length != 0) yield return result;
					startIndex = nextIndex;
				}
				while (startIndex != -1);
			}
		}
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

	}
}
