// <copyright file="Comment.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.Models;

internal sealed class Comment
{
    public string Text { get; set; } = string.Empty;

    public static implicit operator string(Comment c) => c.Text;
}