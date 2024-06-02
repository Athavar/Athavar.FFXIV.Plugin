// <copyright file="RegexWrapper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Config;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Models.Interfaces;

public sealed class RegexWrapper : Regex, IRegex
{
    public RegexWrapper(string pattern, RegexOptions options)
        : base(pattern, options)
    {
    }
}