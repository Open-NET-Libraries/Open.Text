using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Open.Text
{
    [DebuggerDisplay("{GetDebuggerDisplay()}")]
    public struct EnumValue<TEnum>
		where TEnum : Enum
	{
		public EnumValue(TEnum value)
		{
			Value = value;
		}

        public EnumValue(string value)
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

            Value = Parse(value);
		}

		public TEnum Value { get; }

        public override string ToString() => Value.ToString();

        static readonly Func<string, TEnum> Parser = CreateParseEnumDelegate();
        public static TEnum Parse(string value)
        {
            try
            {
                return Parser(value);
            }
            catch (Exception ex)
			{
                throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value), ex);
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
                      Expression.Throw(Expression.New(typeof(Exception).GetConstructor(Type.EmptyTypes))),
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

        public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);
        public static bool operator ==(EnumValue<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);
        public static bool operator !=(EnumValue<TEnum> left, EnumValue<TEnum> right) => !left.Value.Equals(right.Value);

        public bool Equals(EnumValueCaseIgnored<TEnum> other) => Value.Equals(other.Value);
        public static bool operator ==(EnumValue<TEnum> left, EnumValueCaseIgnored<TEnum> right) => left.Value.Equals(right.Value);
        public static bool operator !=(EnumValue<TEnum> left, EnumValueCaseIgnored<TEnum> right) => !left.Value.Equals(right.Value);

        public bool Equals(TEnum other) => Value.Equals(other);
        public static bool operator ==(EnumValue<TEnum> left, TEnum right) => left.Value.Equals(right);
        public static bool operator !=(EnumValue<TEnum> left, TEnum right) => !left.Value.Equals(right);

        public override bool Equals(object? obj)
		{
			return obj is TEnum e && Value.Equals(e)
                || obj is EnumValue<TEnum> v1 && Value.Equals(v1.Value)
                || obj is EnumValueCaseIgnored<TEnum> v2 && Value.Equals(v2.Value);
		}

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

    [DebuggerDisplay("{GetDebuggerDisplay()}")]
    public struct EnumValueCaseIgnored<TEnum>
        where TEnum : Enum
    {
        public EnumValueCaseIgnored(TEnum value)
        {
            Value = value;
        }

        public EnumValueCaseIgnored(string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            Value = Parse(value);
        }

        public TEnum Value { get; }

        public override string ToString() => Value.ToString();

        internal static readonly ImmutableDictionary<string, TEnum> CaseInsensitiveLookup
            = CreateCaseInsensitiveDictionary();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum Parse(string value)
        {
            try
            {
                return CaseInsensitiveLookup[value];
            }
            catch (KeyNotFoundException ex)
            {
                throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value), ex);
            }
        }

        static ImmutableDictionary<string, TEnum> CreateCaseInsensitiveDictionary()
        => Enum
            .GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .ToImmutableDictionary(v => v.ToString(), v => v, StringComparer.OrdinalIgnoreCase);

        public bool Equals(EnumValue<TEnum> other) => Value.Equals(other.Value);
        public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => left.Value.Equals(right.Value);
        public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValue<TEnum> right) => !left.Value.Equals(right.Value);

        public bool Equals(EnumValueCaseIgnored<TEnum> other) => Value.Equals(other.Value);
        public static bool operator ==(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right) => left.Value.Equals(right.Value);
        public static bool operator !=(EnumValueCaseIgnored<TEnum> left, EnumValueCaseIgnored<TEnum> right) => !left.Value.Equals(right.Value);

        public bool Equals(TEnum other) => Value.Equals(other);
        public static bool operator ==(EnumValueCaseIgnored<TEnum> left, TEnum right) => left.Value.Equals(right);
        public static bool operator !=(EnumValueCaseIgnored<TEnum> left, TEnum right) => !left.Value.Equals(right);

        public override bool Equals(object? obj)
        {
            return obj is TEnum e && Value.Equals(e)
                || obj is EnumValueCaseIgnored<TEnum> v1 && Value.Equals(v1.Value)
                || obj is EnumValue<TEnum> v2 && Value.Equals(v2.Value);
        }

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
}
