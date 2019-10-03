// <copyright file="StringExtensions.cs" company="Matt Lacey Limited">
// Copyright (c) Matt Lacey Limited. All rights reserved.
// </copyright>

using System;
using System.Linq;

namespace AlwaysUse
{
    public static class StringExtensions
    {
        public static string RationalizeUsingDirective(this string source)
        {
            return source.Split(new[] { "using" }, StringSplitOptions.RemoveEmptyEntries).Last().TrimEnd(';').Trim();
        }
    }
}
