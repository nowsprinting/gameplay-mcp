// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;

namespace GameplayMcp.Internals
{
    /// <summary>
    /// Parses command-line arguments for the gameplay-mcp package.
    /// </summary>
    internal static class CommandLineArgs
    {
        private const string ListenPrefixKey = "-gameplayMcpListenPrefix";

        /// <summary>
        /// Default HTTP listener prefix used when <c>-gameplayMcpListenPrefix</c> is not specified.
        /// </summary>
        internal const string DefaultListenPrefix = "http://+:8010/";

        /// <summary>
        /// Parses command-line arguments into a key-value dictionary.
        /// Keys start with "-"; the next element (if it does not start with "-") becomes the value.
        /// </summary>
        // Duplicated from TestHelper.RuntimeInternals.CommandLineArgs rather than referencing it,
        // because that class is internal to test-helper's RuntimeInternals assembly and depending
        // on internal implementation details across package boundaries would be fragile.
        internal static Dictionary<string, string> DictionaryFromCommandLineArgs(string[] args)
        {
            var result = new Dictionary<string, string>();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    var key = args[i];
                    var value = string.Empty;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        value = args[i + 1];
                        i++;
                    }

                    result[key] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the HTTP listener prefix specified by <c>-gameplayMcpListenPrefix</c>.
        /// Returns <see cref="DefaultListenPrefix"/> if the argument is not specified.
        /// </summary>
        /// <param name="args">Command-line arguments. Uses <see cref="Environment.GetCommandLineArgs"/> if null.</param>
        internal static string GetListenPrefix(string[] args = null)
        {
            args ??= Environment.GetCommandLineArgs();
            var dict = DictionaryFromCommandLineArgs(args);
            return dict.TryGetValue(ListenPrefixKey, out var value) ? value : DefaultListenPrefix;
        }
    }
}
