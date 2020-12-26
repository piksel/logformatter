using System;
using System.IO;
using System.Linq;
using Piksel.LogFormatter.Parser;

namespace Piksel.LogFormatter.CommandLine
{
    record Options(string Format, int RestIndent, string Input, string Output){}

    class Program
    {
        static int Main(string[] args)
        {
            Options opts;
            try
            {
                opts = ParseArgs(args);
            }
            catch (Exception x)
            {
                Console.Error.WriteLine(x.Message);
                Console.Error.WriteLine("Usage: ");
                Console.Error.WriteLine("logformatter [-r REST_INDENTATION] [-f FORMAT] [-i INPUT] [-o OUTPUT]");
                return x is ApplicationException ? 0 : 1;
            }

            var input = opts.Input == null ? Console.In : File.OpenText(opts.Input);
            Console.Error.WriteLine($"Reading from {opts.Input ?? "STDIN"}");

            var output = opts.Output == null ? Console.Out : new StreamWriter(opts.Output);
            Console.Error.WriteLine($"Writing to {opts.Output ?? "STDOUT"}");

            Console.Error.WriteLine($"Using format: {opts.Format}");

            var parser = new LogFmtParser(input.ReadToEnd());

            if (parser.TryParse())
            {
                var items = parser.GetItems();

                Console.Error.WriteLine($"Parsed {items.Count()} items in {parser.Row} row(s)");

                foreach (var item in items)
                {
                    var row = item.FormatRow(opts.Format, opts.RestIndent);
                    if (!string.IsNullOrWhiteSpace(row))
                    {
                        output.WriteLine(row);
                    }
                }
            }
            else
            {
                Console.Error.WriteLine(parser.ParseError?.ToString() ?? "Unknown error");
            }

            return 0;
        }
        static Options ParseArgs(string[] args)
        {
            var opts = new Options(LogStringFormatter.DefaultLogFormat, 24, null, null);
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                    case "--format":
                        if (args.Length <= ++i) throw new ArgumentException($"Missing argument for {args[i - 1]}");
                        opts = opts with { Format = args[i] };
                        break;

                    case "-r":
                    case "--rest":
                        if (args.Length <= ++i) throw new ArgumentException($"Missing argument for {args[i - 1]}");
                        opts = opts with { RestIndent = int.Parse(args[i]) };
                        break;

                    case "-i":
                    case "--input":
                        if (args.Length <= ++i) throw new ArgumentException($"Missing argument for {args[i - 1]}");
                        opts = opts with { Input = args[i] };
                        break;

                    case "-o":
                    case "--output":
                        if (args.Length <= ++i) throw new ArgumentException($"Missing argument for {args[i - 1]}");
                        opts = opts with { Output = args[i] };
                        break;

                    case "-h":
                    case "--help":
                        throw new ApplicationException("");

                    default:
                        throw new ArgumentException($"Invalid argument {args[i]}");
                }
            }

            return opts;
        }
    }
}
