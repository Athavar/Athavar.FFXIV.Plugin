// <copyright file="ActionEffectDisplayType.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;

public enum ActionEffectDisplayType : byte
{
    HideActionName = 0,
    ShowActionName = 1,
    ShowItemName = 2,
    MountName = 13,
}