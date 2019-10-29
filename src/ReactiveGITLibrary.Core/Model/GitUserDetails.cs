// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveGITLibrary.Core.Model
{
    /// <summary>
    /// Details about a user of a commit.
    /// </summary>
    public class GitUserDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitUserDetails"/> class.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="email">The email of the user.</param>
        public GitUserDetails(string name, string email)
        {
            Name = name;
            Email = email;
        }

        /// <summary>
        /// Gets the author of the commit.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the author's email.
        /// </summary>
        public string Email { get; }
    }
}