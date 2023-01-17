// <copyright file="SpearfishWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
#pragma warning disable CS1591
namespace Athavar.FFXIV.Plugin.Utils.SeFunctions;

using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Module.AutoSpear.Enum;
using FFXIVClientStructs.FFXIV.Component.GUI;

// Source: https://github.com/Ottermandias/GatherBuddy/blob/main/GatherBuddy/SeFunctions/SpearfishWindow.cs
[StructLayout(LayoutKind.Explicit)]
internal struct SpearfishWindow
{
    [FieldOffset(0)]
    public AtkUnitBase Base;

    // 0
    // + 544 (AtkUnitBase)
    // + 100
    // 644
    [FieldOffset(0x284)]
    public Info Fish1;

    // + 28
    // 672
    [FieldOffset(0x2A0)]
    public Info Fish2;

    // + 28
    // 700
    [FieldOffset(0x2BC)]
    public Info Fish3;

    public unsafe AtkResNode* FishLines => this.Base.UldManager.NodeList[3];

    public unsafe AtkResNode* Fish1Node => this.Base.UldManager.NodeList[15];

    public unsafe AtkResNode* Fish2Node => this.Base.UldManager.NodeList[16];

    public unsafe AtkResNode* Fish3Node => this.Base.UldManager.NodeList[17];

    [StructLayout(LayoutKind.Explicit)]
    public struct Info
    {
        [FieldOffset(0x00)]
        public bool Available;

        [FieldOffset(0x08)]
        public bool InverseDirection;

        [FieldOffset(0x09)]
        public bool GuaranteedLarge;

        [FieldOffset(0x0A)]
        public SpearfishSize Size;

        [FieldOffset(0x0C)]
        public SpearfishSpeed Speed;
    }
}