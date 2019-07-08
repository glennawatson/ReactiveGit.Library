// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ReactiveGit.RunProcess.ExtensionMethods
{
    /// <summary>
    /// Extension methods related to Type's.
    /// </summary>
    public static class TypeExtensionMethods
    {
        /// <summary>
        /// Gets the element type of the specified IEnumerable type.
        /// </summary>
        /// <param name="seqType">The type to find the element type of.</param>
        /// <returns>The element type.</returns>
        public static Type GetElementType(this Type seqType)
        {
            var enumerableType = FindIEnumerable(seqType);
            return enumerableType == null ? seqType : enumerableType.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            while (true)
            {
                if ((seqType == null) || (seqType == typeof(string)))
                {
                    return null;
                }

                if (seqType.IsArray)
                {
                    return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
                }

                if (seqType.IsGenericType)
                {
                    foreach (var arg in seqType.GetGenericArguments())
                    {
                        var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                        if (ienum.IsAssignableFrom(seqType))
                        {
                            return ienum;
                        }
                    }
                }

                var interfaces = seqType.GetInterfaces();
                if (interfaces.Length > 0)
                {
                    foreach (var interfaceType in interfaces)
                    {
                        var enumerableType = FindIEnumerable(interfaceType);
                        if (enumerableType != null)
                        {
                            return enumerableType;
                        }
                    }
                }

                if ((seqType.BaseType == null) || (seqType.BaseType == typeof(object)))
                {
                    return null;
                }

                seqType = seqType.BaseType;
            }
        }
    }
}