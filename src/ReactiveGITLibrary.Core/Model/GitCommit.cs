// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using ReactiveGit.Library.Core.Managers;
using Splat;

namespace ReactiveGit.Library.Core.Model
{
    /// <summary>
    /// A commit in GIT.
    /// </summary>
    [DebuggerDisplay("Id = {Sha}")]
    public class GitCommit : IEquatable<GitCommit>, IGitIdObject, IEnableLogger
    {
        private readonly IBranchManager _branchManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommit" /> class.
        /// </summary>
        /// <param name="branchManager">The branch manager that owns the commit.</param>
        /// <param name="sha">The Sha Id for this commit.</param>
        /// <param name="messageShort">A short message about the commit.</param>
        /// <param name="dateTime">The date time of the commit.</param>
        /// <param name="author">The author of the commit.</param>
        /// <param name="authorEmail">The email of the author.</param>
        /// <param name="committer">The committer of the commit.</param>
        /// <param name="committerEmail">The email of the committer.</param>
        /// <param name="shaShort">The shorten version of the Sha hash.</param>
        /// <param name="parents">The parents of the commit.</param>
        public GitCommit(
            IBranchManager branchManager,
            string sha,
            string messageShort,
            DateTime dateTime,
            string author,
            string authorEmail,
            string committer,
            string committerEmail,
            string shaShort,
            IReadOnlyList<string> parents)
        {
            Sha = sha;
            MessageShort = messageShort;
            DateTime = dateTime;
            Author = author;
            Committer = committer;
            ShaShort = shaShort;
            Parents = parents;
            CommitterEmail = committerEmail;
            AuthorEmail = authorEmail;

            _branchManager = branchManager ?? throw new ArgumentNullException(nameof(branchManager));
            MessageLong = _branchManager.GetCommitMessageLong(this).Do(x => this.Log().Debug("Got a long commit message with " + x));
        }

        /// <summary>
        /// Gets the author of the commit.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Gets the author's email.
        /// </summary>
        public string AuthorEmail { get; }

        /// <summary>
        /// Gets the committer of the commit.
        /// </summary>
        public string Committer { get; }

        /// <summary>
        /// Gets the committer's email.
        /// </summary>
        public string CommitterEmail { get; }

        /// <summary>
        /// Gets the date time of the commit.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Gets the full commit message.
        /// </summary>
        public IObservable<string> MessageLong { get; }

        /// <summary>
        /// Gets the description of the commit.
        /// </summary>
        public string MessageShort { get; }

        /// <summary>
        /// Gets a read only list of the parents of the commit.
        /// </summary>
        public IReadOnlyList<string> Parents { get; }

        /// <summary>
        /// Gets the Sha Id code.
        /// </summary>
        public string Sha { get; }

        /// <summary>
        /// Gets the short SHA value.
        /// </summary>
        public string ShaShort { get; }

        /// <summary>
        /// Determines if two commits are equal to each other.
        /// </summary>
        /// <param name="left">The left side to compare.</param>
        /// <param name="right">The right side to compare.</param>
        /// <returns>If the commits are equal to each other.</returns>
        public static bool operator ==(GitCommit left, GitCommit right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if two commits are not equal to each other.
        /// </summary>
        /// <param name="left">The left side to compare.</param>
        /// <param name="right">The right side to compare.</param>
        /// <returns>If the commits are not equal to each other.</returns>
        public static bool operator !=(GitCommit left, GitCommit right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Determines if another instance of a commit is logically equal.
        /// </summary>
        /// <param name="other">The other commit.</param>
        /// <returns>If they are logically equal or not.</returns>
        public bool Equals(GitCommit other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || string.Equals(Sha, other.Sha, StringComparison.OrdinalIgnoreCase);
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

            return (obj.GetType() == GetType()) && Equals((GitCommit)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Sha?.GetHashCode() ?? 0;
        }
    }
}
