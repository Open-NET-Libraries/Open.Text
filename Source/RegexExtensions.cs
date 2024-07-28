namespace Open.Text;

/// <summary>
/// A set of regular expression extensions.
/// </summary>
public static class RegexExtensions
{
	private static Func<Capture, string> GetOriginalTextDelegate()
	{
		var textProp = typeof(Capture).GetProperty("Text", BindingFlags.Instance | BindingFlags.NonPublic);
		if (textProp is not null)
		{
			var method = textProp.GetGetMethod(nonPublic: true)
				?? throw new InvalidOperationException("Could not find the Text property getter.");

			return (Func<Capture, string>)method.CreateDelegate(typeof(Func<Capture, string>));
		}

		// Some older versions of .NET use this instead.
		var textField = typeof(Capture).GetField("_text", BindingFlags.Instance | BindingFlags.NonPublic);
		return textField is not null
			? (capture => (string)textField.GetValue(capture)!)
			: throw new NotSupportedException("Capture: could not find the Text property or _text field.");
	}

	private static readonly Func<Capture, string> _textDelegate = GetOriginalTextDelegate();

	/// <summary>
	/// Returns a ReadOnlySpan of the capture without creating a new string.
	/// </summary>
	/// <remarks>This is a stop-gap until .NET 6 releases the .ValueSpan property.</remarks>
	/// <param name="capture">The capture to get the span from.</param>
	public static ReadOnlySpan<char> AsSpan(this Capture capture)
		=> capture is null
		? throw new ArgumentNullException(nameof(capture))
		: _textDelegate(capture).AsSpan(capture.Index, capture.Length);

	/// <summary>
	/// Gets a group by name.
	/// </summary>
	/// <param name="groups">The group collection to get the group from.</param>
	/// <param name="groupName">The declared name of the group.</param>
	/// <returns>The value of the requested group or null if not found.</returns>
	/// <exception cref="ArgumentNullException">Groups or groupName is null.</exception>
	public static string? GetValue(this GroupCollection groups, string groupName)
	{
		if (groups is null)
			throw new ArgumentNullException(nameof(groups));
		if (groupName is null)
			throw new ArgumentNullException(nameof(groupName));
		Contract.EndContractBlock();

		var group = groups[groupName];
		return group.Success
			? group.Value
			: null;
	}

	/// <returns>The value of the requested group or an empty span if not found.</returns>
	/// <inheritdoc cref="GetValue(GroupCollection, string)" />
	public static ReadOnlySpan<char> GetValueSpan(this GroupCollection groups, string groupName)
	{
		if (groups is null)
			throw new ArgumentNullException(nameof(groups));
		if (groupName is null)
			throw new ArgumentNullException(nameof(groupName));
		Contract.EndContractBlock();

		var group = groups[groupName];
		return group.Success
			? group.AsSpan()
			: [];
	}

	/// <summary>
	/// Returns the available matches as StringSegments.
	/// </summary>
	/// <param name="pattern">The pattern to search with.</param>
	/// <param name="input">The string to search.</param>
	/// <returns>An enumerable containing the found segments.</returns>
	/// <exception cref="ArgumentNullException">If the pattern or input is null.</exception>
	public static IEnumerable<StringSegment> AsSegments(this Regex pattern, string input)
	{
		return pattern is null
			? throw new ArgumentNullException(nameof(pattern))
			: input is null
			? throw new ArgumentNullException(nameof(input))
			: input.Length == 0
			? Enumerable.Empty<StringSegment>()
			: AsSegmentsCore(pattern, input);

		static IEnumerable<StringSegment> AsSegmentsCore(Regex pattern, string input)
		{
			var match = pattern.Match(input);
			while (match.Success)
			{
				yield return new(input, match.Index, match.Length);
				match = match.NextMatch();
			}
		}
	}
}
