using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogFormatter;
using LogFormatter.Parser;

namespace Piksel.LogFormatter.Parser
{
    public class LogItem: Dictionary<string, Value>
    {
        public string FormatRow(string format, int? restIndent = null)
        {
            var formatKeys = Regex.Matches(format, "{([a-z]+)(:\\S+)?}")
                .Select(m => m.Groups[1].Value)
                .ToArray();

            var sbExtra = new StringBuilder();
            for (int i = 0; i < formatKeys.Length; i++)
            {
                format = format.Replace(formatKeys[i], i.ToString());
            }

            foreach(var key in Keys.Where(key => !formatKeys.Contains(key))) 
            {
                if (restIndent is {} i)
                {
                    sbExtra.Append($"{Environment.NewLine}{"".PadRight(i)} {key}: {this[key]}");
                }
            }

            var row = LogStringFormatter.Format(format, formatKeys
                .Select(k => ContainsKey(k) ? this[k] as object : "")
                .ToArray());

            if (sbExtra.Length > 0)
            {
                return row + sbExtra;
            }

            return row;
        }

        public string FormatRowSimple(string format)
        {
            foreach (var (key, value) in this)
            {
                format = format.Replace($"{{{key}}}", value.ToString());
            }

            return format;
        }
    }

    public class LogFmtParser
    {
        private char[] _textBuffer;
        private readonly StringBuilder _readBuilder = new StringBuilder();
        private int _parsePos = 0;
        private const char EOF = char.MaxValue;

        private int _row = 0;
        public int Row => _row;

        private int _col = 0;
        public int Col => _col;

        public Exception? ParseError { get; private set; } = null;

        private char CurrentChar => _textBuffer.Length > _parsePos ? _textBuffer[_parsePos] : EOF;

        private List<LogItem> _logItems = new List<LogItem>();

        public LogFmtParser(ReadOnlySpan<char> source)
        {
            _textBuffer = source.ToArray();
        }

        public bool TryParse()
        {
            try
            {
                DoParse();
                return true;
            }
            catch (Exception x)
            {
                ParseError = new ParserException(_row, _col, x);
                return false;
            }
        }

        private void DoParse()
        {
            while (CurrentChar != EOF)
            {
                _row += EatNewLines();
                var itemRow = new LogItem();
                while (CurrentChar != '\n')
                {

                    EatAll(' ');
                    var key = ReadKey();
                    var value = Value.True;

                    if (CurrentChar == '=')
                    {
                        _parsePos++;
                        value = ReadValue();
                    }
                    else
                    {
                        // CurrentChar is either a space, newline, or EOF
                    }

                    itemRow[key] = value;

                    EatAll(' ');

                    if(CurrentChar == '\r') EatOne();
                    if(CurrentChar == EOF) break;
                }

                _logItems.Add(itemRow);

                _row += EatNewLines();
            }

        }

        private Value ReadValue()
        {
            if (CurrentChar == '"')
            {
                EatOne();
                var value = ReadUntilWithEscape(escape: '\\', '"');
                EatOne();
                return Value.String(value);
            }
            else
            {
                var value = ReadUntil(' ', EOF, '\r', '\n');
                return Value.Parse(value);
            }


        }

        private ActionResult<LogFmtParser> Do(Action action)
        {
            action();
            return ActionResult.For(this);
        }

        private void EatOne() => _parsePos++;

        private int EatAll(params char[] chars)
        {
            int charsConsumed = 0;
            while (chars.Contains(CurrentChar))
            {
                EatOne();
                charsConsumed++;
            }

            return charsConsumed;
        }

        private int EatNewLines()
        {
            var charsConsumed = 0;
            do
            {
                if (CurrentChar == '\r') EatOne();
                if (CurrentChar != '\n') return charsConsumed;

                EatOne();
                charsConsumed++;
            } while (true);
        }

        private string ReadKey() => ReadUntil('=', ' ', EOF);

        private string ReadUntil(params char[] chars)
        {
            _readBuilder.Clear();

            while (!chars.Contains(CurrentChar))
            {
                _readBuilder.Append(CurrentChar);
                EatOne();
            }

            return _readBuilder.ToString();
        }

        private string ReadUntilWithEscape(char escape, params char[] chars)
        {
            var prevEscape = false;
            _readBuilder.Clear();

            while (true)
            {
                if (prevEscape)
                {
                    if (chars.Contains(CurrentChar))
                    {
                        _readBuilder.Append(CurrentChar);
                        EatOne();
                        prevEscape = false;
                    }
                    else
                    {
                        throw new SyntaxErrorException($"Invalid escaped character '{CurrentChar}'");
                    }
                }
                else
                {
                    if (CurrentChar == escape)
                    {
                        prevEscape = true;
                        EatOne();
                        continue;
                    }
                }

                if (chars.Contains(CurrentChar)) break;

                _readBuilder.Append(CurrentChar);
                EatOne();
            }

            return _readBuilder.ToString();
        }

        public IEnumerable<LogItem> GetItems() => _logItems.AsReadOnly();
    }

    public sealed class ParserException: ApplicationException
    {
        public ParserException(in int row, in int col, Exception innerException): base($"Parser error at row {row}, column {col}: {innerException.Message}", innerException)
        {
            if (Data == null) return;

            Data["Row"] = row;
            Data["Column"] = col;
        }
    }
}