// Ignore Spelling: Deconstruct

using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using static System.Linq.Expressions.Expression;
//using static FastExpressionCompiler.LightExpression.Expression;
namespace Open.Text;

/// <summary>
/// A case struct representing an enum value that can be implicitly coerced from a string.
/// </summary>
/// <remarks>String parsing or coercion is case sensitive and must be exact.</remarks>
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Already exposes via a property.")]
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Intentional")]
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public readonly struct EnumValue<TEnum>
	: IEquatable<EnumValue<TEnum>>, IEquatable<EnumValueCaseIgnored<TEnum>>, IEquatable<TEnum>
	where TEnum : Enum
{
	private static Type? _underlyingType;

	/// <summary>
	/// The underlying type of <typeparamref name="TEnum"/>.
	/// </summary>
	[SuppressMessage("Roslynator", "RCS1158:Static member in generic type should use a type parameter.")]
	public static Type UnderlyingType => _underlyingType ??= Enum.GetUnderlyingType(typeof(TEnum));

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
	public EnumValue(StringSegment value)
	{
		Value = EnumValue.Parse<TEnum>(value);
	}

	/// <summary>
	/// The enum value that this represents.
	/// </summary>
	public readonly TEnum Value { get; }

	/// <summary>
	/// Returns the string representation of the enum value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => NameLookup(Value);

	private static IReadOnlyList<TEnum>? _values;
	/// <summary>
	/// Precompiled typed list of all enum values.
	/// </summary>
	public static IReadOnlyList<TEnum> Values
		=> LazyInitializer.EnsureInitialized(ref _values,
			() => Array.AsReadOnly(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray()))!;

	private static ConcurrentDictionary<TEnum, IReadOnlyList<Attribute>>? _attributes;
	internal static ConcurrentDictionary<TEnum, IReadOnlyList<Attribute>> Attributes
		=> LazyInitializer.EnsureInitialized(ref _attributes)!;

	static Func<TEnum, string> GetEnumNameDelegate()
	{
		var tResult = typeof(string);
		var eValue = Parameter(typeof(TEnum), "value"); // (TEnum value)

		return Lambda<Func<TEnum, string>>(
			Switch(tResult, eValue,
				Block(
					Throw(New(typeof(ArgumentException).GetConstructor(Type.EmptyTypes))),
					Default(tResult)
				),
				null,
				Values.OrderBy(v => string.Intern(v.ToString())).Select(v => SwitchCase(
					Constant(string.Intern(v.ToString())),
					Constant(v)
				)).ToArray()
			), eValue).Compile();
	}

	private static Func<TEnum, string>? _nameLookup;
	internal static Func<TEnum, string> NameLookup
		=> LazyInitializer.EnsureInitialized(ref _nameLookup,
			GetEnumNameDelegate)!;
	internal static Func<string, ValueLookupResult> GetEnumTryParseDelegate()
	{
		var valueParam = Parameter(typeof(string), "value");
		var lengthParam = Variable(typeof(int), "length");
		var defaultExpression = CreateNewTuple(false, default!);
		var enumValues = Values.Select(v => (value: v, name: string.Intern(v.ToString())));
		var nameGroups = enumValues.GroupBy(v => v.name.Length).OrderBy(g => g.Key);
		var lengthCheckCases = nameGroups.Select(group =>
			SwitchCase(
				Switch(
					valueParam,
					defaultExpression,
					null,
					group.OrderBy(v => v.name).Select(v => SwitchCase(
						CreateNewTuple(true, v.value),
						Constant(v.name)
					))
					.ToArray()
				),
				Constant(group.Key)
			)
		).ToArray();

		return Lambda<Func<string, ValueLookupResult>>(
			Block(new[] { lengthParam },
				Assign(lengthParam, Property(valueParam, nameof(string.Length))),
				Switch(
					lengthParam,
					defaultExpression,
					null,
					lengthCheckCases
				)
			),
			valueParam
		).Compile();

		static Expression CreateNewTuple(bool success, TEnum value)
			=> New(
				typeof(ValueLookupResult).GetConstructor(new[] { typeof(bool), typeof(TEnum) })!,
				Constant(success),
				Constant(value)
			);
	}

	internal readonly struct ValueLookupResult
	{
		public ValueLookupResult(bool success, TEnum value)
		{
			Success = success;
			Value = value;
		}

		public bool Success { get; }
		public TEnum Value { get; }

		public void Deconstruct(out bool success, out TEnum value)
		{
			success = Success;
			value = Value;
		}
	}

	private static Func<string, ValueLookupResult>? _valueLookup;
	internal static Func<string, ValueLookupResult> ValueLookup
		=> LazyInitializer.EnsureInitialized(ref _valueLookup, GetEnumTryParseDelegate)!;

	private static IReadOnlyDictionary<string, TEnum>? _ignoreCaseLookup;
	internal static IReadOnlyDictionary<string, TEnum> IgnoreCaseLookup
		=> LazyInitializer.EnsureInitialized(ref _ignoreCaseLookup,
			() => Values.ToDictionary(e => Enum.GetName(typeof(TEnum), e)!, e => e, StringComparer.OrdinalIgnoreCase))!;

	static Entry[]?[] CreateLookup()
	{
		var longest = 0;
		var d = new Dictionary<int, List<Entry>>();

		foreach (var e in Values)
		{
			var n = string.Intern(e.ToString());
			var len = n.Length;
			if (len > longest) longest = len;
			if (!d.TryGetValue(len, out var v)) d.Add(len, v = new());
			v.Add(new(n, e));
		}

		var result = new Entry[]?[longest + 1];
		foreach (var i in d.Keys)
			result[i] = d[i].OrderBy(v => v.Name).ToArray();

		return result;
	}

	internal readonly struct Entry
	{
		public Entry(string name, TEnum value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public TEnum Value { get; }

		public void Deconstruct(out string name, out TEnum value)
		{
			name = Name;
			value = Value;
		}

		public static int Find(Span<Entry> span, ReadOnlySpan<char> name, StringComparison sc)
		{
			// Small enough? Just brute force the index.
			var len = span.Length;
			if (len < 12)
			{
				for (var i = 0; i < len; i++)
				{
					ref Entry e = ref span[i];
					if (name.Equals(e.Name, sc))
						return i;
				}

				return -1;
			}

			// Use binary search for larger.
			int left = 0;
			int right = span.Length - 1;

			while (left <= right)
			{
				int middle = left + (right - left) / 2;
				var middleKey = span[middle].Name.AsSpan();

				if (right - left < 4 && name.Equals(middleKey, sc))
					return middle;

				var comparison = name.CompareTo(middleKey, sc);
				if (comparison < 0)
					right = middle - 1;
				else if (comparison > 0)
					left = middle + 1;
				else
					return middle;
			}


			return -1;
		}
	}

	private static Entry[]?[]? _lookup;
	internal static Entry[]?[] Lookup
		=> LazyInitializer.EnsureInitialized(ref _lookup,
			() => CreateLookup())!;

	/// <summary>
	/// Indicates whether this instance matches the enum value of <paramref name="other"/>.
	/// </summary>
	/// <returns>true if <paramref name="other"/>'s enum value and this instance's enum value are the same; otherwise false.</returns>
	public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);

	/// <summary>
	/// Compares an EnumValue and EnumValueCaseIgnored for enum equality.
	/// </summary>
	public static bool operator ==(EnumValue<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);

	/// <summary>
	/// Compares an EnumValue and EnumValueCaseIgnored for enum inequality.
	/// </summary>
	public static bool operator !=(EnumValue<TEnum> left, EnumValue<TEnum> right) => !left.Value.Equals(right.Value);

	/// <inheritdoc cref="Equals(EnumValue{TEnum})"/>
	public bool Equals(EnumValueCaseIgnored<TEnum> other) => Value.Equals(other.Value);

	/// <summary>
	/// Compares two EnumValue for enum equality.
	/// </summary>
	public static bool operator ==(EnumValue<TEnum> left, EnumValueCaseIgnored<TEnum> right) => left.Value.Equals(right.Value);

	/// <summary>
	/// Compares two EnumValue for enum inequality.
	/// </summary>
	public static bool operator !=(EnumValue<TEnum> left, EnumValueCaseIgnored<TEnum> right) => !left.Value.Equals(right.Value);

	/// <summary>
	/// Indicates whether this instance matches the provided enum <paramref name="other"/>.
	/// </summary>
	/// <returns>true if <paramref name="other"/> and this instance's enum value are the same; otherwise false.</returns>
	public bool Equals(TEnum other) => Value.Equals(other);

	/// <summary>
	/// Compares an EnumValue and an enum value for enum equality.
	/// </summary>
	public static bool operator ==(EnumValue<TEnum> left, TEnum right) => left.Value.Equals(right);

	/// <summary>
	/// Compares an EnumValue and an enum value for enum inequality.
	/// </summary>
	public static bool operator !=(EnumValue<TEnum> left, TEnum right) => !left.Value.Equals(right);

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is TEnum e && Value.Equals(e)
		|| obj is EnumValue<TEnum> v1 && Value.Equals(v1.Value)
		|| obj is EnumValueCaseIgnored<TEnum> v2 && Value.Equals(v2.Value);

	/// <inheritdoc />
	public override int GetHashCode() => Value.GetHashCode();

	/// <summary>
	/// Implicitly converts an EnumValueCaseInsensitive to an EnumValue.
	/// Before conversion they are already equivalent.
	/// </summary>
	public static implicit operator EnumValue<TEnum>(EnumValueCaseIgnored<TEnum> value) => new(value.Value);

	/// <summary>
	/// Implicitly returns the actual enum contained by the EnumValue.
	/// </summary>
	public static implicit operator TEnum(EnumValue<TEnum> value) => value.Value;

	/// <summary>
	/// Implicitly converts an string to an EnumValue of enum type TEnum.
	/// </summary>
	public static implicit operator EnumValue<TEnum>(StringSegment value) => new(value);

	/// <summary>
	/// Implicitly converts an string to an EnumValue of enum type TEnum.
	/// </summary>
	public static implicit operator EnumValue<TEnum>(string value) => new(value);

	static class Underlying<T>
	{
		public static readonly Dictionary<T, TEnum> Map = new();

		[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider")]
		static Underlying()
		{
			if (typeof(T) != UnderlyingType) return;

			foreach (var e in Values)
			{
				var i = (T)System.Convert.ChangeType(e, typeof(T));
				Map.Add(i, e);
			}
		}
	}

	/// <summary>
	/// Returns true if the <paramref name="value"/> matches a value of <typeparamref name="TEnum"/>.
	/// </summary>
	[SuppressMessage("Roslynator", "RCS1158:Static member in generic type should use a type parameter.")]
	public static bool IsDefined<T>(T value)
		=> Underlying<T>.Map.ContainsKey(value);

	/// <summary>
	/// Returns the <typeparamref name="TEnum"/> from the <paramref name="value"/> provided if it maps directly to the underlying value.
	/// </summary>
	public static bool TryGetValue<T>(T value, out TEnum e)
		=> Underlying<T>.Map.TryGetValue(value, out e);

	private string GetDebuggerDisplay()
	{
		var eType = typeof(TEnum);
		return $"{eType.Name}.{Value} [EnumValue<{eType.FullName}>]";
	}
}

/// <summary>
/// A case struct representing an enum value that when parsing or coercing from a string ignores case differences.
/// </summary>
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Already exposes via a property.")]
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public readonly struct EnumValueCaseIgnored<TEnum>
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
	public EnumValueCaseIgnored(StringSegment value)
	{
		Value = EnumValue.Parse<TEnum>(value, true);
	}

	/// <inheritdoc cref="EnumValue{TEnum}.Value"/>
	public readonly TEnum Value { get; }

	/// <inheritdoc cref="EnumValue{TEnum}.ToString"/>
	public override string ToString() => EnumValue<TEnum>.NameLookup(Value);

	/// <inheritdoc cref="EnumValue{TEnum}.Equals(EnumValue{TEnum})"/>
	public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);

	/// <summary>
	/// Compares an EnumValueCaseIgnored and EnumValue for enum equality.
	/// </summary>
	public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);

	/// <summary>
	/// Compares an EnumValueCaseIgnored and EnumValue for enum inequality.
	/// </summary>
	public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right)
		=> !left.Value.Equals(right.Value);

	/// <inheritdoc cref="EnumValue{TEnum}.Equals(EnumValue{TEnum})"/>
	public bool Equals(EnumValueCaseIgnored<TEnum> other)
		=> Value.Equals(other.Value);

	/// <summary>
	/// Compares two EnumValueCaseIgnored for enum equality.
	/// </summary>
	public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right)
		=> left.Value.Equals(right.Value);

	/// <summary>
	/// Compares two EnumValueCaseIgnored for enum inequality.
	/// </summary>
	public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right)
		=> !left.Value.Equals(right.Value);

	/// <inheritdoc cref="EnumValue{TEnum}.Equals(TEnum)"/>
	public bool Equals(TEnum other)
		=> Value.Equals(other);

	/// <summary>
	/// Compares an EnumValueCaseIgnored and an enum value for enum equality.
	/// </summary>
	public static bool operator ==(EnumValueCaseIgnored<TEnum> left, TEnum right)
		=> left.Value.Equals(right);

	/// <summary>
	/// Compares an EnumValueCaseIgnored and an enum value for enum inequality.
	/// </summary>
	public static bool operator !=(EnumValueCaseIgnored<TEnum> left, TEnum right)
		=> !left.Value.Equals(right);

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is TEnum e && Value.Equals(e)
		|| obj is EnumValueCaseIgnored<TEnum> v1 && Value.Equals(v1.Value)
		|| obj is EnumValue<TEnum> v2 && Value.Equals(v2.Value);

	/// <inheritdoc />
	public override int GetHashCode()
		=> Value.GetHashCode();

	/// <summary>
	/// Implicitly converts an EnumValue to an EnumValueCaseInsensitive.
	/// Before conversion they are already equivalent.
	/// </summary>
	public static implicit operator EnumValueCaseIgnored<TEnum>(EnumValue<TEnum> value)
		=> new(value.Value);

	/// <summary>
	/// Implicitly returns the actual enum contained by the EnumValueCaseIgnored.
	/// </summary>
	public static implicit operator TEnum(EnumValueCaseIgnored<TEnum> value)
		=> value.Value;

	/// <summary>
	/// Implicitly converts an string to an EnumValueCaseIgnored of enum type TEnum.
	/// </summary>
	public static implicit operator EnumValueCaseIgnored<TEnum>(StringSegment value)
		=> new(value);

	/// <summary>
	/// Implicitly converts an string to an EnumValueCaseIgnored of enum type TEnum.
	/// </summary>
	public static implicit operator EnumValueCaseIgnored<TEnum>(string value)
		=> new(value);

	private string GetDebuggerDisplay()
	{
		var eType = typeof(TEnum);
		return $"{eType.Name}.{Value} [EnumValueCaseIgnored<{eType.FullName}>]";
	}
}

/// <summary>
/// Fast utilities and extensions for parsing enums and retrieving the name of an enum value.
/// </summary>
[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider")]
public static class EnumValue
{
	private const string NotFoundMessage = "Requested value '{0}' was not found.";

	/// <returns>The enum that represents the string <paramref name="value"/> provided.</returns>
	/// <exception cref="ArgumentException">Requested <paramref name="value"/> was not found.</exception>
	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum Parse<TEnum>(string value)
		where TEnum : Enum
	{
		var (success, result) = EnumValue<TEnum>.ValueLookup(value);
		return success ? result
			: throw new ArgumentException(string.Format(NotFoundMessage, value), nameof(value));
	}

	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse<TEnum>(string value, out TEnum e)
		where TEnum : Enum
	{
		var result = EnumValue<TEnum>.ValueLookup(value);
		e = result.Value;
		return result.Success;
	}

	/// <returns>The enum that represents the string <paramref name="value"/> provided.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Requested <paramref name="value"/> was not found.</exception>
	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum Parse<TEnum>(StringSegment value)
		where TEnum : Enum
		=> TryParse<TEnum>(value, false, out var e) ? e
		: throw new ArgumentException(string.Format(NotFoundMessage, value), nameof(value));

	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum Parse<TEnum>(string value, bool ignoreCase)
		where TEnum : Enum
		=> ignoreCase
			? EnumValue<TEnum>.IgnoreCaseLookup.TryGetValue(value, out var e) ? e
				: throw new ArgumentException(string.Format(NotFoundMessage, value), nameof(value))
			: Parse<TEnum>(value);

	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum Parse<TEnum>(StringSegment value, bool ignoreCase)
		where TEnum : Enum
	{
		var buffer = value.Buffer ?? throw new ArgumentNullException(nameof(value));
		return value.Length == buffer.Length
			? Parse<TEnum>(value.Buffer, ignoreCase)
			: TryParse<TEnum>(value, ignoreCase, out var e)
			? e : throw new ArgumentException(string.Format(NotFoundMessage, value), nameof(value));
	}

	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse<TEnum>(StringSegment value, out TEnum e)
		where TEnum : Enum
		=> TryParse(value, false, out e);

	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse<TEnum>(string name, bool ignoreCase, out TEnum e)
		where TEnum : Enum
		=> ignoreCase
		? EnumValue<TEnum>.IgnoreCaseLookup.TryGetValue(name, out e)
		: TryParse(name, out e);

	/// <inheritdoc cref="TryParse{TEnum}(StringSegment, bool, out TEnum)"/>
	/// <remarks>Can be slightly faster than other ignore-case methods.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParseIgnoreCase<TEnum>(string name, out TEnum e)
	where TEnum : Enum
		=> EnumValue<TEnum>.IgnoreCaseLookup.TryGetValue(name, out e);

	/// <summary>
	/// Converts the string representation of the name of one or more enumerated constants to an equivalent enumerated object.
	/// </summary>
	/// <param name="name">The string representing the enum name to search for.</param>
	/// <param name="ignoreCase">If true, will ignore case differences when looking for a match.</param>
	/// <param name="e">The enum that represents the string <paramref name="name"/> provided.</param>
	/// <returns>true if the value was found; otherwise false.</returns>
	public static bool TryParse<TEnum>(StringSegment name, bool ignoreCase, out TEnum e)
		where TEnum : Enum
	{
		var len = name.Length;
		if (len == 0) goto notFound;

		// If this is a string, use the optimized version.
		string buffer = name.Buffer!;
		if (buffer.Length == len)
			return TryParse(buffer, ignoreCase, out e);

		var lookup = EnumValue<TEnum>.Lookup;
		if (len >= lookup.Length) goto notFound;

		var r = lookup[len];
		if (r is null) goto notFound;
		Debug.Assert(r.Length != 0);

		var sc = ignoreCase
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

		var span = r.AsSpan();
		var index = EnumValue<TEnum>.Entry.Find(span, name, sc);
		if (index != -1)
		{
			e = span[index].Value;
			return true;
		}

	notFound:
		e = default!;
		return false;
	}

	/// <summary>
	/// Returns the <typeparamref name="TEnum"/> from the <paramref name="value"/> provided if it maps directly to the underlying value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetValue<TEnum, T>(T value, out TEnum e) where TEnum : Enum
		=> EnumValue<TEnum>.TryGetValue(value, out e);

	/// <summary>
	/// Uses an expression tree to do an fast lookup the name of the enum value.
	/// </summary>
	/// <remarks>Is faster than calling .ToString() on a value.</remarks>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	/// <param name="value">The enum value to get the name for.</param>
	/// <returns>The name of the enum.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetName<TEnum>(this TEnum value) where TEnum : Enum
		=> EnumValue<TEnum>.NameLookup(value);

	/// <summary>
	/// Retrieves the attributes for a given enum value.
	/// </summary>
	public static IReadOnlyList<Attribute> GetAttributes<TEnum>(this TEnum value) where TEnum : Enum
	{
		return EnumValue<TEnum>.Attributes.GetOrAdd(value, GetAttributesCore);

		static IReadOnlyList<Attribute> GetAttributesCore(TEnum value)
		{
			var memInfo = typeof(TEnum).GetMember(value.GetName());
			var attributes = memInfo[0].GetCustomAttributes(false);
			return attributes.Length is 0
				? Array.Empty<Attribute>()
				: Array.AsReadOnly(attributes.OfType<Attribute>().ToArray());
		}
	}
}
