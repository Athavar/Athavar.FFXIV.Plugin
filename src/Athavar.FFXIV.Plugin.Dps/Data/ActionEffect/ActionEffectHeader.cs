// <copyright file="ActionEffectHeader.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActionEffectHeader
{
    public uint AnimationTargetId;

    public uint Unknown1;

    public uint ActionId;

    public uint GlobalEffectCounter;

    public float AnimationLockTime;

    public uint Unknown2;

    public ushort HiddenAnimation;

    public ushort Rotation;

    public ushort ActionAnimationId;

    public byte Variation;

    public ActionEffectDisplayType EffectDisplayType;

    public byte Unknown3;

    public byte EffectCount;

    public ushort Unknown4;
}