using System;
using System.IO;
using System.Linq;
using Piksel.LogFormatter.Parser;

namespace Piksel.LogFormatter.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            using var stdIn = Console.OpenStandardInput();
            
            var streamReader = new StreamReader(stdIn);
            var parser = new LogFmtParser(streamReader.ReadToEnd());

            if (parser.TryParse())
            {
                var items = parser.GetItems();

                Console.Error.WriteLine($"Parsed {items.Count()} items in {parser.Row} row(s)");

                foreach (var item in items)
                {
                    Console.Out.WriteLine(item.FormatRow(LogStringFormatter.DefaultLogFormat, 24));
                }
            }
            else
            {
                Console.Error.WriteLine(parser.ParseError?.ToString() ?? "Unknown error");
            }
        }
    }
}
