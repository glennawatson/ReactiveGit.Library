﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveGITLibrary.Core.Model
{
    /// <summary>
    /// A git object that is represented by a SHA id.
    /// </summary>
    public interface IGitIdObject
    {
        /// <summary>
        /// Gets the full length SHA id.
        /// </summary>
        string Sha { get; }

        /// <summary>
        /// Gets the shortened abbreviated SHA id.
        /// </summary>
        string ShaShort { get; }
    }
}