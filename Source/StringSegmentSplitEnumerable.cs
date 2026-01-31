using ZLinq;

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
					if (_removeEmpty && segment.Length == 0)
					{
						current = default;
						return false;
					}
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
				// Ends with separator
				_completed = true;
				var segment = _source.Subsegment(nextIndex, 0);

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
				// Found separator
				var segment = _source.Subsegment(_startIndex, nextIndex - _startIndex);
				_startIndex = nextIndex + 1;

#if NET5_0_OR_GREATER
				if (_trimEach)
				{
					segment = segment.Trim();
					if (_removeEmpty && segment.Length == 0)
					{
						continue; // Skip empty after trim
					}
				}
				else
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
	public bool TryGetNonEnumeratedCount(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>
	/// Returns false as split segments are not contiguous in memory.
	/// </summary>
	public bool TryGetSpan(out ReadOnlySpan<StringSegment> span)
	{
		span = default;
		return false;
	}

	/// <summary>
	/// Returns false as we don't support indexed access efficiently.
	/// </summary>
	public bool TryCopyTo(scoped Span<StringSegment> destination, Index offset)
	{
		return false;
	}

	/// <summary>
	/// No resources to dispose.
	/// </summary>
	public void Dispose()
	{
		// Nothing to dispose
	}
}
