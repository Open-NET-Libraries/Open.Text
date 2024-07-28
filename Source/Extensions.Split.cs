namespace Open.Text;

internal static class SingleEmpty
{
	public static readonly IReadOnlyList<string> Instance = Array.AsReadOnly(new[] { string.Empty });
}

public static partial class TextExtensions
{
	static ReadOnlySpan<char> FirstSplitSpan(StringSegment source, int start, int i, int n, out int nextIndex)
	{
		Debug.Assert(start >= 0);
		if (i == -1)
		{
			nextIndex = -1;
			return start == 0 ? source.AsSpan() : source.AsSpan(start);
		}

		nextIndex = i + n;
		var segmentLen = i - start;
		return segmentLen == 0 ? [] : source.AsSpan(start, segmentLen);
	}

	static ReadOnlySpan<char> FirstSplitSpan(ReadOnlySpan<char> rest, int i, int n, out int nextIndex)
	{
		if (i == -1)
		{
			nextIndex = -1;
			return rest;
		}

		nextIndex = i + n;
		return i == 0
			? []
			: rest.Slice(0, i);
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
		return segmentLen == 0
			? ReadOnlyMemory<char>.Empty
			: source.AsMemory(start, segmentLen);
	}

	/// <summary>
	/// Finds the first instance of a character and returns the set of characters up to that character.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitCharacter">The character to find.</param>
	/// <param name="nextIndex">The next possible index following the current one.</param>
	/// <param name="startIndex">The index to start the split.</param>
	/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		char splitCharacter,
		out int nextIndex,
		int startIndex = 0)
	{
		if (!source.HasValue)
			throw new ArgumentException("Cannot split a null string or sequence that has no value.", nameof(source));
		var len = source.Length;
		if (startIndex > len) throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Is greater than the length of the provided string.");
		Contract.EndContractBlock();

		if (startIndex == len)
		{
			nextIndex = -1;
			return [];
		}

		return FirstSplitSpan(source, startIndex, source.IndexOf(splitCharacter, startIndex), 1, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(StringSegment, char, out int, int)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		char splitCharacter,
		out int nextIndex,
		int startIndex = 0)
		=> FirstSplit(source.AsSegment(), splitCharacter, out nextIndex, startIndex);

	/// <summary>
	/// Finds the first instance of a character sequence and returns the set of characters up to that sequence.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitSequence">The sequence to find.</param>
	/// <param name="nextIndex">The next possible index following the current one.</param>
	/// <param name="startIndex">The index to start the split.</param>
	/// <param name="comparisonType">The string comparison type to use.</param>
	/// <returns>The portion of the source up to and excluding the sequence searched for.</returns>
	public static ReadOnlySpan<char> FirstSplit(this StringSegment source,
		StringSegment splitSequence,
		out int nextIndex,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!source.HasValue)
			throw new ArgumentException("Cannot split a null string or sequence that has no value.", nameof(source));
		if (splitSequence.Length == 0)
			throw new ArgumentException("Cannot split using empty sequence.", nameof(splitSequence));
		var len = source.Length;
		if (startIndex > source.Length)
			throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Is greater than the length of the provided string.");
		Contract.EndContractBlock();

		if (startIndex == len)
		{
			nextIndex = -1;
			return [];
		}

		return FirstSplitSpan(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), splitSequence.Length, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(StringSegment, StringSegment, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this string source,
		StringSegment splitSequence,
		out int nextIndex,
		int startIndex = 0,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.AsSegment().FirstSplit(splitSequence, out nextIndex, startIndex, comparisonType);

	/// <inheritdoc cref="FirstSplit(string, char, out int, int)"/>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		char splitCharacter,
		out int nextIndex)
	{
		if (source.Length == 0)
		{
			nextIndex = -1;
			return [];
		}

		var i = source.IndexOf(splitCharacter);
		return FirstSplitSpan(source, i, 1, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(StringSegment, StringSegment, out int, int, StringComparison)"/>
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
			return [];
		}

		var i = source.IndexOf(splitSequence, comparisonType);
		return FirstSplitSpan(source, i, splitSequence.Length, out nextIndex);
	}

	/// <inheritdoc cref="FirstSplit(StringSegment, StringSegment, out int, int, StringComparison)"/>
	public static ReadOnlySpan<char> FirstSplit(this ReadOnlySpan<char> source,
		StringSegment splitSequence,
		out int nextIndex,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.FirstSplit(splitSequence.AsSpan(), out nextIndex, comparisonType);

	/// <summary>
	/// Enumerates a string by segments that are separated by the split character.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitCharacter">The character to find.</param>
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
			_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
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
	/// Enumerates a string by segments that are separated by the split sequence.
	/// </summary>
	/// <param name="source">The source characters to look through.</param>
	/// <param name="splitSequence">The sequence to find.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <param name="comparisonType">The string comparison type to use.</param>
	/// <returns>An IEnumerable&lt;string&gt; of the segments.</returns>
	public static IEnumerable<string> SplitToEnumerable(
		this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
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
				: SplitAsEnumerableCore(source, splitSequence, comparisonType),
			StringSplitOptions.RemoveEmptyEntries => source.Length == 0
				? Enumerable.Empty<string>()
				: SplitAsEnumerableCoreOmitEmpty(source, splitSequence, comparisonType),
			_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
		};

		static IEnumerable<string> SplitAsEnumerableCore(string source, string splitSequence, StringComparison comparisonType)
		{
			var startIndex = 0;
			do
			{
				yield return source.FirstSplit(splitSequence, out var nextIndex, startIndex, comparisonType).ToString();
				startIndex = nextIndex;
			}
			while (startIndex != -1);
		}

		static IEnumerable<string> SplitAsEnumerableCoreOmitEmpty(string source, string splitSequence, StringComparison comparisonType)
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

	/// <returns>An IEnumerable&lt;ReadOnlyMemory&lt;char&gt;&gt; of the segments.</returns>
	/// <inheritdoc cref="SplitToEnumerable(string, char, StringSplitOptions)" />
	public static IEnumerable<ReadOnlyMemory<char>> SplitAsMemory(
		this string source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return options switch
		{
			StringSplitOptions.None => source.Length == 0
				? Enumerable.Repeat(ReadOnlyMemory<char>.Empty, 1)
				: SplitAsMemoryCore(source, splitCharacter),
			StringSplitOptions.RemoveEmptyEntries => source.Length == 0
				? Enumerable.Empty<ReadOnlyMemory<char>>()
				: SplitAsMemoryOmitEmpty(source, splitCharacter),
			_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
		};

		static IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryCore(string source, char splitCharacter)
		{
			var startIndex = 0;
			do
			{
				yield return FirstSplitMemory(source, startIndex, source.IndexOf(splitCharacter, startIndex), 1, out var nextIndex);
				startIndex = nextIndex;
			}
			while (startIndex != -1);
		}

		static IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryOmitEmpty(string source, char splitCharacter)
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

	/// <returns>An IEnumerable&lt;ReadOnlyMemory&lt;char&gt;&gt; of the segments.</returns>
	/// <inheritdoc cref="SplitToEnumerable(string, string, StringSplitOptions, StringComparison)"/>
	public static IEnumerable<ReadOnlyMemory<char>> SplitAsMemory(this string source,
		string splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
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
				: SplitAsMemoryCore(source, splitSequence, comparisonType),
			StringSplitOptions.RemoveEmptyEntries => source.Length == 0
				? Enumerable.Empty<ReadOnlyMemory<char>>()
				: SplitAsMemoryOmitEmpty(source, splitSequence, comparisonType),
			_ => throw new System.ComponentModel.InvalidEnumArgumentException(),
		};

		static IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryOmitEmpty(string source, string splitSequence, StringComparison comparisonType)
		{
			var startIndex = 0;
			var splitLen = splitSequence.Length;
			do
			{
				var result = FirstSplitMemory(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), splitLen, out var nextIndex);
				if (result.Length != 0) yield return result;
				startIndex = nextIndex;
			}
			while (startIndex != -1);
		}

		static IEnumerable<ReadOnlyMemory<char>> SplitAsMemoryCore(string source, string splitSequence, StringComparison comparisonType)
		{
			var startIndex = 0;
			var splitLen = splitSequence.Length;
			do
			{
				yield return FirstSplitMemory(source, startIndex, source.IndexOf(splitSequence, startIndex, comparisonType), splitLen, out var nextIndex);
				startIndex = nextIndex;
			}
			while (startIndex != -1);
		}
	}

	/// <summary>
	/// Splits a sequence of characters into strings using the character provided.
	/// </summary>
	/// <param name="source">The source string to split up.</param>
	/// <param name="splitCharacter">The character to split by.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <returns>The resultant list of string segments.</returns>
	public static IReadOnlyList<string> Split(this ReadOnlySpan<char> source,
		char splitCharacter,
		StringSplitOptions options = StringSplitOptions.None)
	{
		switch (options)
		{
			case StringSplitOptions.None when source.Length == 0:
				return SingleEmpty.Instance;

			case StringSplitOptions.RemoveEmptyEntries when source.Length == 0:
				return Array.Empty<string>();

			case StringSplitOptions.RemoveEmptyEntries:
			{
				Debug.Assert(!source.IsEmpty);
				var list = new List<string>();

			loop:
				var result = source.FirstSplit(splitCharacter, out var nextIndex);
				if (!result.IsEmpty) list.Add(result.ToString());
				if (nextIndex == -1) return list;
				source = source.Slice(nextIndex);
				goto loop;
			}

			default:
			{
				Debug.Assert(!source.IsEmpty);
				var list = new List<string>();
			loop:
				var result = source.FirstSplit(splitCharacter, out var nextIndex);
				list.Add(result.IsEmpty ? string.Empty : result.ToString());
				if (nextIndex == -1) return list;
				source = source.Slice(nextIndex);
				goto loop;
			}
		}
	}

	/// <summary>
	/// Splits a sequence of characters into strings using the character sequence provided.
	/// </summary>
	/// <param name="source">The source string to split up.</param>
	/// <param name="splitSequence">The sequence to split by.</param>
	/// <param name="options">Can specify to omit empty entries.</param>
	/// <param name="comparisonType">The optional comparison type.</param>
	/// <returns>The resultant list of string segments.</returns>
	public static IReadOnlyList<string> Split(this ReadOnlySpan<char> source,
		ReadOnlySpan<char> splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
	{
		switch (options)
		{
			case StringSplitOptions.None when source.IsEmpty:
				return SingleEmpty.Instance;

			case StringSplitOptions.RemoveEmptyEntries when source.IsEmpty:
				return Array.Empty<string>();

			case StringSplitOptions.RemoveEmptyEntries:
			{
				Debug.Assert(!source.IsEmpty);
				var list = new List<string>();

			loop:
				var result = source.FirstSplit(splitSequence, out var nextIndex, comparisonType);
				if (!result.IsEmpty) list.Add(result.ToString());
				if (nextIndex == -1) return list;
				source = source.Slice(nextIndex);
				goto loop;
			}

			default:
			{
				Debug.Assert(!source.IsEmpty);
				var list = new List<string>();
			loop:
				var result = source.FirstSplit(splitSequence, out var nextIndex, comparisonType);
				list.Add(result.IsEmpty ? string.Empty : result.ToString());
				if (nextIndex == -1) return list;
				source = source.Slice(nextIndex);
				goto loop;
			}
		}
	}

	/// <inheritdoc cref="Split(ReadOnlySpan{char}, ReadOnlySpan{char}, StringSplitOptions, StringComparison)"/>
	public static IReadOnlyList<string> Split(this ReadOnlySpan<char> source,
		StringSegment splitSequence,
		StringSplitOptions options = StringSplitOptions.None,
		StringComparison comparisonType = StringComparison.Ordinal)
		=> source.Split(splitSequence.AsSpan(), options, comparisonType);
}
