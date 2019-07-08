// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using LibGit2Sharp;
using ReactiveGit.Library.Core.Exceptions;
using ReactiveGit.Library.Core.Model;
using ReactiveGit.Library.RunProcess.Managers;
using Xunit;

namespace ReactiveGit.Library.UnitTests
{
    /// <summary>
    /// Tests the git framework.
    /// </summary>
    public class GitUnitTest
    {
        /// <summary>
        /// Test creating several branches and making sure that the full history comes back.
        /// </summary>
        [Fact]
        public void TestFullHistory()
        {
            var tempDirectory = GenerateGitRepository(out var local);

            var numberCommits = 10;
            GenerateCommits(numberCommits, tempDirectory, local, "master");
            var branchManager = new BranchManager(local);

            var commits =
                branchManager.GetCommitsForBranch(
                    new GitBranch("master", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ToList().Wait();

            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            commits.Should().BeInDescendingOrder(x => x.DateTime);
            local.RunGit(new[] { "branch test1" }).FirstOrDefaultAsync().Wait();
            local.RunGit(new[] { "checkout test1" }).FirstOrDefaultAsync().Wait();

            GenerateCommits(numberCommits, tempDirectory, local, "master");

            commits =
                branchManager.GetCommitsForBranch(new GitBranch("test1", false, false), 0, 0, GitLogOptions.None).ToList().Wait();

            commits.Count.Should().Be(numberCommits * 2, $"We have done {numberCommits + 1} commits");
        }

        /// <summary>
        /// Test creating several commits and making sure the get commit message routine
        /// returns all the commits from the selected parent.
        /// </summary>
        [Fact]
        public void TestGetAllCommitMessages()
        {
            var tempDirectory = GenerateGitRepository(out var local);

            IList<string> commitNames = new List<string>();
            var numberCommits = 10;
            GenerateCommits(numberCommits, tempDirectory, local, "master", commitNames);

            var branchManager = new BranchManager(local);
            var commits =
                branchManager.GetCommitsForBranch(
                    new GitBranch("test1", false, false),
                    0,
                    0,
                    GitLogOptions.TopologicalOrder).ToList().Wait();

            commits.Select(x => x.MessageLong).Should().BeEquivalentTo(commitNames.Reverse());
        }

        /// <summary>
        /// Test getting the history from a freshly formed GIT repository and
        /// getting the history for the current branch only.
        /// </summary>
        [Fact]
        public void TestGitHistoryBranchOnly()
        {
            var tempDirectory = GenerateGitRepository(out var local);

            var numberCommits = 10;
            GenerateCommits(numberCommits, tempDirectory, local, "master");

            var branchManager = new BranchManager(local);

            var commits =
                branchManager.GetCommitsForBranch(
                    new GitBranch("master", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ToList().Wait();
            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            using (var repository = new Repository(tempDirectory))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "master");
                branch.Should().NotBeNull();

                if (branch != null)
                {
                    CheckCommits(branch.Commits.ToList(), commits);
                }
            }

            commits.Should().BeInDescendingOrder(x => x.DateTime);

            GenerateCommits(numberCommits, tempDirectory, local, "test1");

            commits =
                branchManager.GetCommitsForBranch(
                    new GitBranch("test1", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ToList().Wait();

            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            using (var repository = new Repository(tempDirectory))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "test1");
                branch.Should().NotBeNull();

                if (branch != null)
                {
                    CheckCommits(branch.Commits.Take(10).ToList(), commits);
                }
            }
        }

        /// <summary>
        /// Checks to make sure that the selected GIT commits are equal to each other.
        /// </summary>
        /// <param name="repoCommits">The commits coming from LibGit2.</param>
        /// <param name="commits">The commits coming from the GIT command line BranchManager.</param>
        private static void CheckCommits(IList<Commit> repoCommits, IList<GitCommit> commits)
        {
            repoCommits.Select(x => x.Sha).Should().BeEquivalentTo(commits.Select(x => x.Sha));
            repoCommits.Select(x => x.MessageShort).Should().BeEquivalentTo(commits.Select(x => x.MessageShort));
        }

        /// <summary>
        /// Generates a GIT repository with the specified process manager.
        /// </summary>
        /// <param name="local">The process manager to use.</param>
        /// <returns>The location of the GIT repository.</returns>
        private static string GenerateGitRepository(out IGitProcessManager local)
        {
            var tempDirectory = Path.Combine(
                Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            Directory.CreateDirectory(tempDirectory);

            local = new GitProcessManager(tempDirectory);

            local.RunGit(new[] { "init" }).Wait();
            local.RunGit(new[] { "config --local commit.gpgsign false" }).FirstOrDefaultAsync().Wait();
            return tempDirectory;
        }

        /// <summary>
        /// Generates a series of commits.
        /// </summary>
        /// <param name="numberCommits">The number of commits to generate.</param>
        /// <param name="directory">The directory of the repository.</param>
        /// <param name="local">The repository manager for the repository.</param>
        /// <param name="branchName">The branch name to add the commits into.</param>
        /// <param name="commitMessages">A optional output list which is populated with the commit messages.</param>
        private static void GenerateCommits(
            int numberCommits,
            string directory,
            IGitProcessManager local,
            string branchName,
            IList<string> commitMessages = null)
        {
            if (branchName != "master")
            {
                local.RunGit(new[] { $"branch {branchName}" }).FirstOrDefaultAsync().Wait();
            }

            try
            {
                local.RunGit(new[] { $"checkout {branchName}" }).Wait();
            }
            catch (GitProcessException)
            {
                // Ignored
            }

            for (var i = 0; i < numberCommits; ++i)
            {
                File.WriteAllText(Path.Combine(directory, Path.GetRandomFileName()), @"Hello World" + i);
                local.RunGit(new[] { "add -A" }).FirstOrDefaultAsync().Wait();
                commitMessages?.Add($"Commit {branchName}-{i}");
                local.RunGit(new[] { $"commit -m \"Commit {branchName}-{i}\"" }).FirstOrDefaultAsync().Wait();
            }
        }
    }
}