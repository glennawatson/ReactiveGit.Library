// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveGITLibrary.Core.Model
{
    /// <summary>
    /// Represents a item in the GIT ref log.
    /// </summary>
    public class GitRefLog : IGitIdObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitRefLog" /> class.
        /// </summary>
        /// <param name="sha">The full length SHA.</param>
        /// <param name="shortSha">The short SHA id of the ref log item.</param>
        /// <param name="action">The action performed by the ref log item.</param>
        /// <param name="messageShort">The short message of the ref log item.</param>
        /// <param name="dateTime">The date time the ref log item happened.</param>
        public GitRefLog(string sha, string shortSha, string action, string messageShort, DateTime dateTime)
        {
            ShaShort = shortSha;
            Action = action;
            MessageShort = messageShort;
            DateTime = dateTime;
            Sha = sha;
        }

        /// <summary>
        /// Gets the action of the ref log item.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Gets the date time when the ref log item happened.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Gets the short message of the ref log item.
        /// </summary>
        public string MessageShort { get; }

        /// <inheritdoc />
        public string Sha { get; }

        /// <summary>
        /// Gets the short SHA id of the ref log item.
        /// </summary>
        public string ShaShort { get; }
    }
}
