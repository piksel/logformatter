using System.Globalization;
using System.Linq;

namespace LogFormatter.Parser
{
    public readonly struct Value
    {
        private readonly ValueType _type;
        private readonly object _data;

        private Value(ValueType type, object data)
        {
            _type = type;
            _data = data;
        }

        public override string ToString() 
            => _type switch
            {
                ValueType.Integer when _data is long intValue => intValue.ToString(),
                ValueType.Float when _data is double floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
                _ => _data?.ToString() ?? string.Empty,
            };

        public static Value True => Bool(true);
        public static Value Bool(bool value) => new Value(ValueType.Bool, value);
        public static Value String(string value) => new Value(ValueType.String, value);

        public static Value Parse(string valueString)
            => valueString switch
            {
                { } when valueString.All(char.IsDigit) => Integer(valueString),
                { } when double.TryParse(valueString, out var floatValue) => Float(floatValue),
                { } when bool.TryParse(valueString, out var boolValue) => Bool(boolValue),
                null => String(string.Empty),
                _ => String(valueString),
            };

        private static Value Float(in double value) => new Value(ValueType.Float, value);
        private static Value Integer(long value) => new Value(ValueType.Integer, value);
        private static Value Integer(string valueString) => Integer(long.Parse(valueString));
    }

    public enum ValueType
    {
        String,
        Integer,
        Float,
        Bool,
    }
}