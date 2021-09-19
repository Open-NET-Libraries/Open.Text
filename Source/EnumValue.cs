using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Open.Text
{
	/// <summary>
	/// A case struct representing an enum value that can be implicitly coerced from a string.
	/// </summary>
	/// <remarks>String parsing or coercion is case sensitve and must be exact.</remarks>
	[DebuggerDisplay("{GetDebuggerDisplay()}")]
	public struct EnumValue<TEnum>
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
			Value = Parse(value);
		}

		/// <summary>
		/// The actual enum value.
		/// </summary>
		public TEnum Value { get; }

		/// <summary>
		/// Returns the string representation of the enum value.
		/// </summary>
		public override string ToString() => Value.ToString();

		static readonly Func<string, TEnum> Parser = CreateParseEnumDelegate();

		/// <summary>
		/// Uses an expression tree switch to get a matching enum value.
		/// </summary>
		/// <param name="value">The string represnting the enum to search for.</param>
		/// <returns>The enum that represents the string <paramref name="value"/> provided.</returns>
		/// <exception cref="ArgumentNullException">value is null</exception>
		/// <exception cref="ArgumentException">Requested <paramref name="value"/> was not found.</exception>
		public static TEnum Parse(string value)
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			try
			{
				return Parser(value);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value), ex);
			}
		}

		/// <summary>
		/// Uses an expression tree switch to get a matching enum value.
		/// </summary>
		/// <returns>true if the value found; otherwise false.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryParse(string value, out TEnum e)
		{
			try
			{
				e = Parser(value);
				return true;
			}
			catch (ArgumentException)
			{
				e = default!;
				return false;
			}
		}

		// https://stackoverflow.com/questions/26678181/enum-parse-vs-switch-performance
		static Func<string, TEnum> CreateParseEnumDelegate()
		{
			var eValue = Expression.Parameter(typeof(string), "value"); // (string value)
			var tEnum = typeof(TEnum);

			return
			  Expression.Lambda<Func<string, TEnum>>(
				Expression.Block(tEnum,
				  Expression.Switch(tEnum, eValue,
					Expression.Block(tEnum,
					  Expression.Throw(Expression.New(typeof(ArgumentException).GetConstructor(Type.EmptyTypes))),
					  Expression.Default(tEnum)
					),
					null,
					Enum.GetValues(tEnum).Cast<object>().Select(v => Expression.SwitchCase(
					  Expression.Constant(v),
					  Expression.Constant(v.ToString())
					)).ToArray()
				  )
				), eValue
			  ).Compile();
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
		/// Indicates whether this instance matches the provided enum <paramref name="value"/>.
		/// </summary>
		/// <returns>true if <paramref name="value"/> and this instance's enum value are the same; otherwise false.</returns>
		public bool Equals(TEnum value) => Value.Equals(value);
		public static bool operator ==(EnumValue<TEnum> left, TEnum right) => left.Value.Equals(right);
		public static bool operator !=(EnumValue<TEnum> left, TEnum right) => !left.Value.Equals(right);

		/// <inheritdoc />
		public override bool Equals(object? obj)
		{
			return obj is TEnum e && Value.Equals(e)
				|| obj is EnumValue<TEnum> v1 && Value.Equals(v1.Value)
				|| obj is EnumValueCaseIgnored<TEnum> v2 && Value.Equals(v2.Value);
		}

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
			Value = Parse(value);
		}

		public TEnum Value { get; }

		/// <inheritdoc cref="EnumValue{TEnum}.ToString"/>
		public override string ToString() => Value.ToString();

		internal static readonly ImmutableDictionary<string, TEnum> CaseInsensitiveLookup
			= CreateCaseInsensitiveDictionary();

		/// <summary>
		/// Uses a case-insenstive dictionary lookup to get a matching enum value.
		/// </summary>
		/// <inheritdoc cref="EnumValue{TEnum}.Parse(string)" />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TEnum Parse(string value)
			=> TryParse(value, out var e) ? e : throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value));

		/// <summary>
		/// Uses a case-insenstive dictionary lookup to get a matching enum value.
		/// </summary>
		/// <inheritdoc cref="EnumValue{TEnum}.TryParse(string, out TEnum)"/>
		public static bool TryParse(string value, out TEnum e)
		{
			if (CaseInsensitiveLookup.TryGetValue(value, out e!)) return true;
			e = default!;
			return false;
		}

		static ImmutableDictionary<string, TEnum> CreateCaseInsensitiveDictionary()
			=> Enum
				.GetValues(typeof(TEnum))
				.Cast<TEnum>()
				.ToImmutableDictionary(v => v.ToString(), v => v, StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc cref="EnumValue{TEnum}.Equals(EnumValue{TEnum})"/>
		public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);
		public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);
		public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => !left.Value.Equals(right.Value);

		/// <inheritdoc cref="EnumValue{TEnum}.Equals(EnumValue{TEnum})"/>
		public bool Equals(EnumValueCaseIgnored<TEnum> other) => Value.Equals(other.Value);
		public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right) => left.Value.Equals(right.Value);
		public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right) => !left.Value.Equals(right.Value);

		/// <inheritdoc cref="EnumValue{TEnum}.Equals(TEnum)"/>
		public bool Equals(TEnum value) => Value.Equals(value);
		public static bool operator ==(EnumValueCaseIgnored<TEnum> left, TEnum right) => left.Value.Equals(right);
		public static bool operator !=(EnumValueCaseIgnored<TEnum> left, TEnum right) => !left.Value.Equals(right);

		/// <inheritdoc />
		public override bool Equals(object? obj)
		{
			return obj is TEnum e && Value.Equals(e)
				|| obj is EnumValueCaseIgnored<TEnum> v1 && Value.Equals(v1.Value)
				|| obj is EnumValue<TEnum> v2 && Value.Equals(v2.Value);
		}

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
		/// <inheritdoc cref="EnumValue{TEnum}.Parse(string)"/>
		public static TEnum Parse<TEnum>(string value)
			where TEnum : Enum
			=> EnumValue<TEnum>.Parse(value);

		/// <summary>
		/// Converts the string representation of the name of one or more enumerated constants to an equivalent enumerated object.
		/// A parameter specifies whether the operation is case-insensitive.
		/// </summary>
		/// <param name="ignoreCase">If true, will ignore case differences when looking for a match.</param>
		/// <inheritdoc cref="EnumValue{TEnum}.Parse(string)"/>
		public static TEnum Parse<TEnum>(string value, bool ignoreCase)
			where TEnum : Enum
			=> ignoreCase
			? EnumValueCaseIgnored<TEnum>.Parse(value)
			: EnumValue<TEnum>.Parse(value);

		/// <inheritdoc cref="EnumValue{TEnum}.TryParse(string, out TEnum)"/>
		public static bool TryParse<TEnum>(string value, out TEnum e)
			where TEnum : Enum
			=> EnumValue<TEnum>.TryParse(value, out e);

		/// <inheritdoc cref="EnumValue{TEnum}.TryParse(string, out TEnum)"/>
		/// <summary>
		/// Converts the string representation of the name of one or more enumerated constants to an equivalent enumerated object.
		/// A parameter specifies whether the operation is case-insensitive.
		/// </summary>
		/// <param name="ignoreCase">If true, will ignore case differences when looking for a match.</param>
		public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum e)
			where TEnum : Enum
			=> ignoreCase
			? EnumValueCaseIgnored<TEnum>.TryParse(value, out e)
			: EnumValue<TEnum>.TryParse(value, out e);
	}
}
