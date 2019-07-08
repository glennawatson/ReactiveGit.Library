// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveGit.Library.Core.ExtensionMethods
{
    /// <summary>
    /// Extension methods related to strings.
    /// </summary>
    public static class StringExtensionMethods
    {
        /// <summary>
        /// Separates a string into an array of the lines within the string.
        /// </summary>
        /// <param name="str">The string to convert into an array.</param>
        /// <returns>The array of lines.</returns>
        public static string[] ToArrayOfLines(this string str)
        {
            if (str == null)
            {
                return Array.Empty<string>();
            }

            return str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Trims any empty lines out of the text.
        /// </summary>
        /// <param name="input">The string with potential empty lines.</param>
        /// <returns>The string minus any new lines.</returns>
        public static string TrimEmptyLines(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            input = input.Trim('\r', '\n').Trim();

            return input;
        }
    }
}
