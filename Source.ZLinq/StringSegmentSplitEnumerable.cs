namespace Open.Text;

/// <summary>
/// Zero-allocation enumerator for splitting strings by character.
/// Implements ZLinq's IValueEnumerator for full LINQ compatibility with zero allocations.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct StringSegmentSplitEnumerator : IValueEnumerator<StringSegment>
{
	private readonly StringSegment _source;
	private readonly char _splitCharacter;
	private readonly bool _removeEmpty;
#if NET5_0_OR_GREATER
	private readonly bool _trimEach;
#endif
	private int _startIndex;
	private bool _completed;

	internal StringSegmentSplitEnumerator(StringSegment source, char splitCharacter, StringSplitOptions options)
	{
		_source = source;
		_splitCharacter = splitCharacter;
		_removeEmpty = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
#if NET5_0_OR_GREATER
		_trimEach = options.HasFlag(StringSplitOptions.TrimEntries);
#endif
		_startIndex = 0;
		_completed = false;
	}

	/// <summary>
	/// Advances to the next segment.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_completed)
		{
			current = default;
			return false;
		}

		int len = _source.Length;

		// Handle empty source
		if (len == 0)
		{
			_completed = true;
			if (_removeEmpty)
			{
				current = default;
				return false;
			}

			current = StringSegment.Empty;
			return true;
		}

		while (_startIndex <= len)
		{
			int nextIndex = _source.IndexOf(_splitCharacter, _startIndex);

			if (nextIndex == -1)
			{
				// Last segment
				_completed = true;
				var segment = _source.Subsegment(_startIndex);

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
				}
#endif

				if (_removeEmpty && segment.Length == 0)
				{
					current = default;
					return false;
				}

				current = segment;
				return true;
			}

			if (nextIndex == len)
			{
				// Ends with separator
				_completed = true;
				var segment = _source.Subsegment(nextIndex, 0);

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
				}
#endif

				if (_removeEmpty && segment.Length == 0)
				{
					current = default;
					return false;
				}

				current = segment;
				return true;
			}

			{
				// Found separator
				var segment = _source.Subsegment(_startIndex, nextIndex - _startIndex);
				_startIndex = nextIndex + 1;

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
				}
#endif
				if (_removeEmpty && segment.Length == 0)
				{
					continue; // Skip empty
				}

				current = segment;
				return true;
			}
		}

		_completed = true;
		current = default;
		return false;
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as split segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public readonly void Dispose()
	{
		// Nothing to dispose
	}
}

/// <summary>
/// Zero-allocation enumerator for splitting strings by a sequence (substring).
/// Implements ZLinq's IValueEnumerator for full LINQ compatibility with zero allocations.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct StringSegmentSequenceSplitEnumerator : IValueEnumerator<StringSegment>
{
	private readonly StringSegment _source;
	private readonly StringSegment _splitSequence;
	private readonly StringComparison _comparisonType;
	private readonly int _sequenceLength;
	private readonly bool _removeEmpty;
#if NET5_0_OR_GREATER
	private readonly bool _trimEach;
#endif
	private int _startIndex;
	private bool _completed;

	internal StringSegmentSequenceSplitEnumerator(
		StringSegment source,
		StringSegment splitSequence,
		StringSplitOptions options,
		StringComparison comparisonType)
	{
		_source = source;
		_splitSequence = splitSequence;
		_comparisonType = comparisonType;
		_sequenceLength = splitSequence.Length;
		_removeEmpty = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
#if NET5_0_OR_GREATER
		_trimEach = options.HasFlag(StringSplitOptions.TrimEntries);
#endif
		_startIndex = 0;
		_completed = false;
	}

	/// <summary>
	/// Advances to the next segment.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_completed)
		{
			current = default;
			return false;
		}

		int len = _source.Length;

		// Handle empty source
		if (len == 0)
		{
			_completed = true;
			if (_removeEmpty)
			{
				current = default;
				return false;
			}

			current = StringSegment.Empty;
			return true;
		}

		while (_startIndex <= len)
		{
			int nextIndex = _source.IndexOf(_splitSequence, _startIndex, _comparisonType);

			if (nextIndex == -1)
			{
				// Last segment - no more separators found
				_completed = true;
				var segment = _source.Subsegment(_startIndex);

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
				}
#endif
				if (_removeEmpty && segment.Length == 0)
				{
					current = default;
					return false;
				}

				current = segment;
				return true;
			}
			else if (nextIndex == len)
			{
				// Separator found at end
				_completed = true;
				var segment = _source.Subsegment(nextIndex, 0);

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
				}
#endif
				if (_removeEmpty && segment.Length == 0)
				{
					current = default;
					return false;
				}

				current = segment;
				return true;
			}
			else
			{
				// Found separator in middle
				var segment = _source.Subsegment(_startIndex, nextIndex - _startIndex);
				_startIndex = nextIndex + _sequenceLength;

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
				}
#endif
				if (_removeEmpty && segment.Length == 0)
				{
					continue; // Skip empty
				}

				current = segment;
				return true;
			}
		}

		_completed = true;
		current = default;
		return false;
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as split segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public readonly void Dispose()
	{
		// Nothing to dispose
	}
}

/// <summary>
/// Zero-allocation enumerator for iterating regex matches as StringSegments.
/// Implements ZLinq's IValueEnumerator for full LINQ compatibility with zero allocations.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct RegexMatchSegmentEnumerator : IValueEnumerator<StringSegment>
{
	private readonly string _input;
	private Match? _currentMatch;

	internal RegexMatchSegmentEnumerator(Regex pattern, string input)
	{
		_input = input;
		_currentMatch = pattern.Match(input);
	}

	/// <summary>
	/// Advances to the next match.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_currentMatch?.Success != true)
		{
			current = default;
			return false;
		}

		current = new StringSegment(_input, _currentMatch.Index, _currentMatch.Length);
		_currentMatch = _currentMatch.NextMatch();
		return true;
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as match segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public readonly void Dispose()
	{
		// Nothing to dispose
	}
}

/// <summary>
/// Zero-allocation enumerator for splitting strings by regex pattern.
/// Implements ZLinq's IValueEnumerator for full LINQ compatibility with zero allocations.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct RegexSplitSegmentEnumerator : IValueEnumerator<StringSegment>
{
	private readonly string _source;
	private readonly bool _removeEmpty;
#if NET5_0_OR_GREATER
	private readonly bool _trimEach;
#endif
	private Match? _currentMatch;
	private int _startIndex;
	private bool _completed;
	private bool _finalSegmentPending;

	internal RegexSplitSegmentEnumerator(string source, Regex pattern, StringSplitOptions options)
	{
		_source = source;
		_removeEmpty = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
#if NET5_0_OR_GREATER
		_trimEach = options.HasFlag(StringSplitOptions.TrimEntries);
#endif
		_currentMatch = pattern.Match(source);
		_startIndex = 0;
		_completed = false;
		_finalSegmentPending = false;
	}

	/// <summary>
	/// Advances to the next segment.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_completed)
		{
			current = default;
			return false;
		}

		int sourceLen = _source.Length;

		// Handle empty source
		if (sourceLen == 0)
		{
			_completed = true;
			if (_removeEmpty)
			{
				current = default;
				return false;
			}

			current = StringSegment.Empty;
			return true;
		}

		while (true)
		{
			// Check if we need to yield final segment
			if (_finalSegmentPending)
			{
				_completed = true;
				int len = sourceLen - _startIndex;

				if (_removeEmpty && len == 0)
				{
					current = default;
					return false;
				}

				var segment = new StringSegment(_source, _startIndex, len);
#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
					if (_removeEmpty && segment.Length == 0)
					{
						current = default;
						return false;
					}
				}
#endif
				current = segment;
				return true;
			}

			// Check for more matches
			if (_currentMatch?.Success != true)
			{
				// No more matches - yield final segment
				_finalSegmentPending = true;
				continue;
			}

			// Found a match - yield segment before it
			int matchIndex = _currentMatch.Index;
			int matchLength = _currentMatch.Length;
			int segmentLength = matchIndex - _startIndex;

			var result = new StringSegment(_source, _startIndex, segmentLength);
			_startIndex = matchIndex + matchLength;
			_currentMatch = _currentMatch.NextMatch();

#if NET5_0_OR_GREATER
			if (_trimEach)
			{
				result = result.Trim();
				if (_removeEmpty && result.Length == 0)
				{
					continue; // Skip empty after trim
				}
			}
			else
#endif
			if (_removeEmpty && result.Length == 0)
			{
				continue; // Skip empty
			}

			current = result;
			return true;
		}
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as split segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public readonly void Dispose()
	{
		// Nothing to dispose
	}
}

/// <summary>
/// Zero-allocation enumerator for joining string segments with a separator.
/// Implements ZLinq's IValueEnumerator for full LINQ compatibility with zero allocations.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct StringSegmentJoinEnumerator : IValueEnumerator<StringSegment>
{
	private readonly StringSegment _separator;
	private readonly IEnumerator<StringSegment> _source;
	private bool _isFirst;
	private StringSegment _pendingElement;
	private bool _completed;

	internal StringSegmentJoinEnumerator(IEnumerable<StringSegment> source, StringSegment separator)
	{
		_separator = separator;
		_source = source.GetEnumerator();
		_isFirst = true;
		_pendingElement = default;
		_completed = false;
	}

	/// <summary>
	/// Advances to the next segment.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_completed)
		{
			current = default;
			return false;
		}

		// If we have a pending element to yield (after yielding separator)
		if (_pendingElement.HasValue)
		{
			current = _pendingElement;
			_pendingElement = default;
			return true;
		}

		// Try to get next element from source
		while (_source.MoveNext())
		{
			var element = _source.Current;

			if (_isFirst)
			{
				_isFirst = false;
				// Skip empty first element (don't yield it)
				if (element.Length == 0)
					continue;
				current = element;
				return true;
			}

			// For subsequent elements, yield separator first (if has value)
			if (_separator.HasValue)
			{
				// Only store as pending if non-empty
				if (element.Length != 0)
					_pendingElement = element;
				current = _separator;
				return true;
			}

			// No separator, only yield non-empty elements
			if (element.Length == 0)
				continue;
			current = element;
			return true;
		}

		_completed = true;
		current = default;
		return false;
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as joined segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// Disposes the underlying enumerator.
	/// </summary>
	public readonly void Dispose() => _source.Dispose();
}

/// <summary>
/// Zero-allocation enumerator for joining regex split segments with a separator.
/// Wraps RegexSplitSegmentEnumerator directly without boxing.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct RegexSplitJoinEnumerator : IValueEnumerator<StringSegment>
{
	private readonly StringSegment _separator;
	private RegexSplitSegmentEnumerator _source;
	private bool _isFirst;
	private StringSegment _pendingElement;
	private bool _completed;

	internal RegexSplitJoinEnumerator(RegexSplitSegmentEnumerator source, StringSegment separator)
	{
		_separator = separator;
		_source = source;
		_isFirst = true;
		_pendingElement = default;
		_completed = false;
	}

	/// <summary>
	/// Advances to the next segment.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_completed)
		{
			current = default;
			return false;
		}

		// If we have a pending element to yield (after yielding separator)
		if (_pendingElement.HasValue)
		{
			current = _pendingElement;
			_pendingElement = default;
			return true;
		}

		// Try to get next element from source
		while (_source.TryGetNext(out var element))
		{
			if (_isFirst)
			{
				_isFirst = false;
				// Skip empty first element (don't yield it)
				if (element.Length == 0)
					continue;
				current = element;
				return true;
			}

			// For subsequent elements, yield separator first (if has value)
			if (_separator.HasValue)
			{
				// Only store as pending if non-empty
				if (element.Length != 0)
					_pendingElement = element;
				current = _separator;
				return true;
			}

			// No separator, only yield non-empty elements
			if (element.Length == 0)
				continue;
			current = element;
			return true;
		}

		_completed = true;
		current = default;
		return false;
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as joined segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public readonly void Dispose() => _source.Dispose();
}

/// <summary>
/// Zero-allocation enumerator for joining sequence split segments with a separator.
/// Wraps StringSegmentSequenceSplitEnumerator directly without boxing.
/// </summary>
[SuppressMessage("Design", "CA1815:Override equals and operator equals on value types", Justification = "Enumerators with mutable state should not be compared for equality")]
public struct SequenceSplitJoinEnumerator : IValueEnumerator<StringSegment>
{
	private readonly StringSegment _separator;
	private StringSegmentSequenceSplitEnumerator _source;
	private bool _isFirst;
	private StringSegment _pendingElement;
	private bool _completed;

	internal SequenceSplitJoinEnumerator(StringSegmentSequenceSplitEnumerator source, StringSegment separator)
	{
		_separator = separator;
		_source = source;
		_isFirst = true;
		_pendingElement = default;
		_completed = false;
	}

	/// <summary>
	/// Advances to the next segment.
	/// </summary>
	public bool TryGetNext(out StringSegment current)
	{
		if (_completed)
		{
			current = default;
			return false;
		}

		// If we have a pending element to yield (after yielding separator)
		if (_pendingElement.HasValue)
		{
			current = _pendingElement;
			_pendingElement = default;
			return true;
		}

		// Try to get next element from source
		while (_source.TryGetNext(out var element))
		{
			if (_isFirst)
			{
				_isFirst = false;
				// Skip empty first element (don't yield it)
				if (element.Length == 0)
					continue;
				current = element;
				return true;
			}

			// For subsequent elements, yield separator first (if has value)
			if (_separator.HasValue)
			{
				// Only store as pending if non-empty
				if (element.Length != 0)
					_pendingElement = element;
				current = _separator;
				return true;
			}

			// No separator, only yield non-empty elements
			if (element.Length == 0)
				continue;
			current = element;
			return true;
		}

		_completed = true;
		current = default;
		return false;
	}

	/// <summary>
	/// Returns false as we cannot determine count without enumerating.
	/// </summary>
	public readonly bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as joined segments are not contiguous in memory.
	/// </summary>
	public readonly bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public readonly bool TryCopyTo(scoped Span<StringSegment> destination, Index offset) => false;

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public readonly void Dispose() => _source.Dispose();
}
