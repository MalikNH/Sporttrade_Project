using Noggog;
using Noggog.Utility;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synthesis.Bethesda.Execution
{
    public static class DotNetCommands
    {
        public const int MinVersion = 5;

        public static bool TryParseLibraryLine(
            string line, 
            [MaybeNullWhen(false)] out string package,
            [MaybeNullWhen(false)] out string requested,
            [MaybeNullWhen(false)] out string resolved,
            [MaybeNullWhen(false)] out string latest)
        {
            var startIndex = line.IndexOf("> ");
            if (startIndex == -1)
            {
                package = default;
                requested = default;
                resolved = default;
                latest = default;
                return false;
            }
            var split = line
                .Substring(startIndex + 2)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .WithIndex()
                .Where(x => x.Index == 0 || x.Item != "(D)")
                .Select(x => x.Item)
                .ToArray();
            package = split[0];
            requested = split[1];
            resolved = split[2];
            latest = split[3];
            return true;
        }

        public static bool TryGetExecutablePathFromOutput(IEnumerable<string> lines, [MaybeNullWhen(false)] out string output)
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!trimmed.EndsWith(".dll")) continue;
                const string delimiter = " -> ";
                var index = trimmed.IndexOf(delimiter, StringComparison.Ordinal);
                if (index == -1) continue;
                output = trimmed.Substring(index + delimiter.Length).Trim();
                return true;
            }
            output = null;
            return false;
        }

        public static bool IsApplicableErrorLine(ReadOnlySpan<char> str)
        {
            return str.Contains(": error ", StringComparison.Ordinal);
        }

        public static void PrintErrorMessage(ReadOnlySpan<char> message, ReadOnlySpan<char> relativePathTrim, ReadOnlySpanAction<char, object?> toDo)
        {
            foreach (var item in message.SplitLines())
            {
                if (!IsApplicableErrorLine(item.Line)) continue;
                toDo(TrimErrorMessage(item.Line, relativePathTrim), null);
            }
        }

        public static ReadOnlySpan<char> TrimErrorMessage(ReadOnlySpan<char> message, ReadOnlySpan<char> relativePathTrim)
        {
            if (message.StartsWith(relativePathTrim))
            {
                message = message.Slice(relativePathTrim.Length);
            }

            int index = 0;
            while (index < message.Length)
            {
                var slice = message.Slice(index);
                var actualIndex = slice.IndexOf('[');
                if (actualIndex == -1) break;
                index = actualIndex + index;
                if (index == message.Length) break;
                if (message.Slice(index + 1).StartsWith(relativePathTrim))
                {
                    message = message.Slice(0, index);
                }
                index++;
            }

            return message.Trim();
        }
    }
}