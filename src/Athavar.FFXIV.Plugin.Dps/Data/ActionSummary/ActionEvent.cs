// <copyright file="ActionEvent.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;

internal record struct ActionEvent(DateTime Timestamp, uint Actor, ActionEventModifier Mod, uint Amount, uint OverAmount = 0);