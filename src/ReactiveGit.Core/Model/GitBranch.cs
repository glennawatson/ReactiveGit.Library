// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace ReactiveGit.Core.Model
{
    /// <summary>
    /// Represents a branch in GIT.
    /// </summary>
    [DebuggerDisplay("{FriendlyName}")]
    public class GitBranch : IEquatable<GitBranch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitBranch" /> class.
        /// </summary>
        /// <param name="friendlyName">A human friendly name of the branch.</param>
        /// <param name="isRemote">If the branch is remote or not.</param>
        /// <param name="isCheckedOut">If the branch is checked out or not.</param>
        public GitBranch(string friendlyName, bool isRemote, bool isCheckedOut)
        {
            FriendlyName = friendlyName;
            IsRemote = isRemote;
            IsCheckedOut = isCheckedOut;
        }

        /// <summary>
        /// Gets the description of the branch.
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// Gets a value indicating whether the branch is checked out.
        /// </summary>
        public bool IsCheckedOut { get; }

        /// <summary>
        /// Gets a value indicating whether the branch is remote.
        /// </summary>
        public bool IsRemote { get; }

        /// <summary>
        /// Operator for equality.
        /// </summary>
        /// <param name="left">The left comparison object.</param>
        /// <param name="right">The right comparison object.</param>
        /// <returns>If the two sides match.</returns>
        public static bool operator ==(GitBranch left, GitBranch right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if the two branch objects are not equal to each other.
        /// </summary>
        /// <param name="left">The left comparison object.</param>
        /// <param name="right">The right comparison object.</param>
        /// <returns>If the two sides do not equal each other.</returns>
        public static bool operator !=(GitBranch left, GitBranch right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Determines if two branches are equal to each other.
        /// </summary>
        /// <param name="other">The other branch to compare.</param>
        /// <returns>If they are equal to each other.</returns>
        public bool Equals(GitBranch other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || string.Equals(FriendlyName, other.FriendlyName, StringComparison.InvariantCulture);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj.GetType() == GetType()) && Equals((GitBranch)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FriendlyName?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FriendlyName;
        }
    }
}
