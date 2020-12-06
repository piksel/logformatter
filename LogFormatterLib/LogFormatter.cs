using System;
using System.Collections.Generic;

namespace Piksel.LogFormatter
{
    public class LogStringFormatter : IFormatProvider, ICustomFormatter
    {
        public static readonly string DefaultLogFormat = "{time:C22} [{level:U1}] {msg}";

        public static string Format(string format, params object[] args) 
            => string.Format(new LogStringFormatter(), format, args);

        private DateTimeOffset prevTime;

        public object? GetFormat(Type? formatType) => formatType == typeof(ICustomFormatter) ? this : null;

        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if (!Equals(formatProvider)) return null!;

            var result = arg?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(format)) return result;

            if (format[0] == 'T')
            {
                
            }

            if (int.TryParse(format[1..], out var maxLen) && maxLen != 0 && Math.Abs(maxLen) < result.Length)
            {
                result = maxLen > 0 ? result[..maxLen] : result[maxLen..];
            }

            return format.ToUpperInvariant()[0] switch
            {
                'C' => result,
                'L' => result.ToLower(),
                'U' => result.ToUpper(),
                _ => throw new FormatException($"The '{format}' format specifier is not supported.")
            };
        }
    }
}