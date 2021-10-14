using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Open.Text;

/// <summary>
/// A case struct representing an enum value that can be implicitly coerced from a string.
/// </summary>
/// <remarks>String parsing or coercion is case sensitve and must be exact.</remarks>
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public struct EnumValue<TEnum>
	: IEquatable<EnumValue<TEnum>>, IEquatable<EnumValueCaseIgnored<TEnum>>, IEquatable<TEnum>
	where TEnum : Enum
{
	/// <summary>
	/// Constructs an EnumValue&lt;<typeparamref name="TEnum"/>&gt; using the provided enum value.
	/// </summary>
	public EnumValue(TEnum value)
	{
		Value = value;
	}

	/// <summary>
	/// Parses the string value to construct an EnumValue&lt;<typeparamref name="TEnum"/>&gt; instance.
	/// </summary>
	/// <exception cref="ArgumentNullException">value is null.</exception>
	public EnumValue(string value)
	{
		Value = EnumValue.Parse<TEnum>(value);
	}

	/// <summary>
	/// The actual enum value.
	/// </summary>
	public TEnum Value { get; }

	/// <summary>
	/// Returns the string representation of the enum value.
	/// </summary>
	public override string ToString() => NameLookup(Value);

	internal static readonly (string Name, TEnum Value)[]?[] Lookup = CreateLookup();

	internal static readonly Func<TEnum, string> NameLookup = GetEnumNameDelegate();

	static Func<TEnum, string> GetEnumNameDelegate()
	{
		var eValue = Expression.Parameter(typeof(TEnum), "value"); // (TEnum value)
		var tEnum = typeof(TEnum);
		var tResult = typeof(string);

		return
		  Expression.Lambda<Func<TEnum, string>>(
			Expression.Block(tResult,
			  Expression.Switch(tResult, eValue,
				Expression.Block(tResult,
				  Expression.Throw(Expression.New(typeof(Exception).GetConstructor(Type.EmptyTypes))),
				  Expression.Default(tResult)
				),
				null,
				Enum.GetValues(tEnum).Cast<object>().Select(v => Expression.SwitchCase(
				  Expression.Constant(string.Intern(v.ToString())),
				  Expression.Constant(v)
				)).ToArray()
			  )
			), eValue
		  ).Compile();
	}

	static (string Name, TEnum Value)[]?[] CreateLookup()
	{
		var longest = 0;
		var d = new Dictionary<int, List<(string Name, TEnum Value)>>();
		var values = Enum
			.GetValues(typeof(TEnum))
			.Cast<TEnum>();

		foreach (var e in values)
		{
			var n = string.Intern(e.ToString());
			var len = n.Length;
			if (len > longest) longest = len;
			if (!d.TryGetValue(len, out var v)) d.Add(len, v = new());
			v.Add((n, e));
		}

		var result = new (string Name, TEnum Value)[]?[longest + 1];
		foreach (var i in d.Keys)
			result[i] = d[i].ToArray();

		return result;
	}

	/// <summary>
	/// Indicates whether this instance matches the enum value of <paramref name="other"/>.
	/// </summary>
	/// <returns>true if <paramref name="value"/>'s enum value and this instance's enum value are the same; otherwise false.</returns>
	public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);
	public static bool operator ==(EnumValue<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);
	public static bool operator !=(EnumValue<TEnum> left, EnumValue<TEnum> right) => !left.Value.Equals(right.Value);

	/// <inheritdoc cref="Equals(EnumValue{TEnum})"/>
	public bool Equals(EnumValueCaseIgnored<TEnum> other) => Value.Equals(other.Value);
	public static bool operator ==(EnumValue<TEnum> left, EnumValueCaseIgnored<TEnum> right) => left.Value.Equals(right.Value);
	public static bool operator !=(EnumValue<TEnum> left, EnumValueCaseIgnored<TEnum> right) => !left.Value.Equals(right.Value);

	/// <summary>
	/// Indicates whether this instance matches the provided enum <paramref name="other"/>.
	/// </summary>
	/// <returns>true if <paramref name="other"/> and this instance's enum value are the same; otherwise false.</returns>
	public bool Equals(TEnum other) => Value.Equals(other);
	public static bool operator ==(EnumValue<TEnum> left, TEnum right) => left.Value.Equals(right);
	public static bool operator !=(EnumValue<TEnum> left, TEnum right) => !left.Value.Equals(right);

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is TEnum e && Value.Equals(e)
		|| obj is EnumValue<TEnum> v1 && Value.Equals(v1.Value)
		|| obj is EnumValueCaseIgnored<TEnum> v2 && Value.Equals(v2.Value);

	/// <inheritdoc />
	public override int GetHashCode() => Value.GetHashCode();

	public static implicit operator EnumValue<TEnum>(EnumValueCaseIgnored<TEnum> value) => new(value.Value);

	public static implicit operator TEnum(EnumValue<TEnum> value) => value.Value;

	public static implicit operator EnumValue<TEnum>(string value) => new(value);

	private string GetDebuggerDisplay()
	{
		var eType = typeof(TEnum);
		return $"{eType.Name}.{Value} [EnumValue<{eType.FullName}>]";
	}
}

/// <summary>
/// A case struct representing an enum value that when parsing or coercing from a string ignores case differences.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public struct EnumValueCaseIgnored<TEnum>
	: IEquatable<EnumValueCaseIgnored<TEnum>>, IEquatable<EnumValue<TEnum>>, IEquatable<TEnum>
	where TEnum : Enum
{
	/// <summary>
	/// Constructs an EnumValueCaseIgnored&lt;<typeparamref name="TEnum"/>&gt; using the provided enum value.
	/// </summary>
	public EnumValueCaseIgnored(TEnum value)
	{
		Value = value;
	}

	/// <summary>
	/// Parses the string value to construct an EnumValueCaseIgnored&lt;<typeparamref name="TEnum"/>&gt; instance.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	public EnumValueCaseIgnored(string value)
	{
		Value = EnumValue.Parse<TEnum>(value, true);
	}

	public TEnum Value { get; }

	/// <inheritdoc cref="EnumValue{TEnum}.ToString"/>
	public override string ToString() => EnumValue<TEnum>.NameLookup(Value);

	/// <inheritdoc cref="EnumValue{TEnum}.Equals(EnumValue{TEnum})"/>
	public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);
	public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);
	public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => !left.Value.Equals(right.Value);

	/// <inheritdoc cref="EnumValue{TEnum}.Equals(EnumValue{TEnum})"/>
	public bool Equals(EnumValueCaseIgnored<TEnum> other) => Value.Equals(other.Value);
	public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right) => left.Value.Equals(right.Value);
	public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right) => !left.Value.Equals(right.Value);

	/// <inheritdoc cref="EnumValue{TEnum}.Equals(TEnum)"/>
	public bool Equals(TEnum other) => Value.Equals(other);
	public static bool operator ==(EnumValueCaseIgnored<TEnum> left, TEnum right) => left.Value.Equals(right);
	public static bool operator !=(EnumValueCaseIgnored<TEnum> left, TEnum right) => !left.Value.Equals(right);

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is TEnum e && Value.Equals(e)
		|| obj is EnumValueCaseIgnored<TEnum> v1 && Value.Equals(v1.Value)
		|| obj is EnumValue<TEnum> v2 && Value.Equals(v2.Value);

	/// <inheritdoc />
	public override int GetHashCode() => Value.GetHashCode();

	public static implicit operator EnumValueCaseIgnored<TEnum>(EnumValue<TEnum> value) => new(value.Value);

	public static implicit operator TEnum(EnumValueCaseIgnored<TEnum> value) => value.Value;

	public static implicit operator EnumValueCaseIgnored<TEnum>(string value) => new(value);

	private string GetDebuggerDisplay()
	{
		var eType = typeof(TEnum);
		return $"{eType.Name}.{Value} [EnumValueCaseIgnored<{eType.FullName}>]";
	}
}

public static class EnumValue
{
	/// <summary>
	/// Converts the string representation of the name of one or more enumerated constants to an equivalent enumerated object.
	/// A parameter specifies whether the operation is case-insensitive.
	/// </summary>
	/// <param name="value">The string representing the enum value to search for.</param>
	/// <returns>The enum that represents the string <paramref name="value"/> provided.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Requested <paramref name="value"/> was not found.</exception>
	public static TEnum Parse<TEnum>(string value)
		where TEnum : Enum
		=> TryParse<TEnum>(value, false, out var e) ? e
		: throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value));

	/// <summary>
	/// Converts the string representation of the name of one or more enumerated constants to an equivalent enumerated object.
	/// A parameter specifies whether the operation is case-insensitive.
	/// </summary>
	/// <param name="ignoreCase">If true, will ignore case differences when looking for a match.</param>
	/// <inheritdoc cref="Parse{TEnum}(string)"/>
	public static TEnum Parse<TEnum>(string value, bool ignoreCase)
		where TEnum : Enum
		=> TryParse<TEnum>(value, ignoreCase, out var e) ? e
		: throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value));

	/// <summary>
	/// Converts the string representation of the name of one or more enumerated constants to an equivalent enumerated object.
	/// A parameter specifies whether the operation is case-insensitive.
	/// </summary>
	/// <param name="value">The string representing the enum value to search for.</param>
	/// <param name="e">The enum that represents the string <paramref name="value"/> provided.</param>
	/// <returns>true if the value was found; otherwise false.</returns>
	public static bool TryParse<TEnum>(string value, out TEnum e)
		where TEnum : Enum
		=> TryParse(value, false, out e);

	/// <param name="ignoreCase">If true, will ignore case differences when looking for a match.</param>
	/// <inheritdoc cref="TryParse{TEnum}(string, out TEnum)"/>
	public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum e)
		where TEnum : Enum
	{
		if (value is null) goto notFound;
		var len = value.Length;
		var lookup = EnumValue<TEnum>.Lookup;
		if (len >= lookup.Length) goto notFound;
		var r = lookup[len];
		if (r is null) goto notFound;

		var sc = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		foreach (var (Name, Value) in r)
		{
			if (Name.Equals(value, sc))
			{
				e = Value;
				return true;
			}
		}

	notFound:
		e = default!;
		return false;
	}

	/// <summary>
	/// Uses a dictionary to lookup the name of the enum value.
	/// </summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	/// <param name="value">The enum value to get the name for.</param>
	/// <returns>The name of the enum.</returns>
	public static string GetName<TEnum>(TEnum value) where TEnum : Enum
		=> EnumValue<TEnum>.NameLookup(value);
}
