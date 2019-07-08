// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveGit.Core.Managers;

namespace ReactiveGit.Core.Model
{
    /// <summary>
    /// Represents a git tag.
    /// </summary>
    public class GitTag : IGitIdObject
    {
        private readonly Lazy<string> _message;

        private readonly ITagManager _tagManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitTag"/> class.
        /// </summary>
        /// <param name="tagManager">The tag manager to get tag information.</param>
        /// <param name="name">The name of the tag.</param>
        /// <param name="shaShort">A abbreviated SHA id.</param>
        /// <param name="sha">The SHA id.</param>
        /// <param name="dateTime">The date time the tag was created.</param>
        public GitTag(ITagManager tagManager, string name, string shaShort, string sha, DateTime dateTime)
        {
            Name = name;
            _tagManager = tagManager;
            Sha = sha;
            ShaShort = shaShort;
            DateTime = dateTime;

            _message = new Lazy<string>(() => _tagManager.GetMessage(this));
        }

        /// <inheritdoc />
        public string Sha { get; }

        /// <inheritdoc />
        public string ShaShort { get; }

        /// <summary>
        /// Gets the date time the tag was created.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Gets a message about the tag.
        /// </summary>
        public string Message => _message.Value;

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        public string Name { get; }
    }
}
