﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveGit.Core.Exceptions
{
    /// <summary>
    /// A exception that occurs when a GIT operation didn't occurr as expected.
    /// </summary>
    public sealed class GitProcessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitProcessException" /> class.
        /// </summary>
        public GitProcessException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitProcessException" /> class.
        /// </summary>
        /// <param name="message">The message about the exception.</param>
        public GitProcessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitProcessException" /> class.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="inner">An inner exception.</param>
        public GitProcessException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitProcessException" /> class.
        /// </summary>
        /// <param name="processOutput">The output from the process.</param>
        /// <param name="commandName">The git command run.</param>
        public GitProcessException(string processOutput, string commandName)
            : base($"{commandName} failed.\r\n{processOutput}")
        {
            ProcessOutput = processOutput;
            CommandName = commandName;
        }

        /// <summary>
        /// Gets the command name specified for git.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Gets the process output from git.
        /// </summary>
        public string ProcessOutput { get; }
    }
}
