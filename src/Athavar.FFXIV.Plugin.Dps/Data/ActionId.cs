// <copyright file="ActionId.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using FFXIVClientStructs.FFXIV.Client.Game;

internal struct ActionId
{
    public static ActionId Clemency = new(ActionType.Action, 3541);
    public readonly uint Raw; // high byte is type, low 3 bytes is ID
    internal const uint ItemDelta = 0x2000000;
    internal const uint MountDelta = 0xD000000;

    public ActionId(uint raw = 0) => this.Raw = raw;

    internal ActionId(ActionType type, uint id) => this.Raw = ((uint)type << 24) | id;

    public static uint Attack => 7;

    public static uint Shot => 8;

    public static uint Bane => 174;

    public static uint Fester => 181;

    public static uint Adloquium => 185;

    public static uint Succor => 186;

    public static uint Bootshine => 53;

    public static uint ShadowOfTheDestroyer => 25767;

    public static uint InnerChaos => 16465;

    public static uint ChaoticCyclone => 16463;

    public static uint PrimalRend => 25673;

    public static uint Assassinate => 2246;

    public static uint DeploymentTactics => 3585;

    public static uint Broil => 3584;

    public static uint RuinScholar => 17870;

    public static uint Outburst => 16511;

    public static uint OmegaJammer => 12911;

    public static uint Exhaust => 18463;

    public static uint Exhaust2 => 18462;

    public static uint HeatShock1 => 22823;

    public static uint ColdShock1 => 22824;

    public static uint ColdShock2 => 22879;

    public static uint HeatShock2 => 22878;

    public static uint TrueThrust => 75;

    public static uint VorpalThrust => 78;

    public static uint Disembowel => 87;

    public static uint KaeshiHiganbana => 16484;

    public static uint CollectiveUnconcious => 3613;

    public static uint Rekindle => 25830;

    public static uint HeartOfCorundrum => 25758;

    public static uint BrutalShell => 16139;

    public static uint Pneuma => 24318;

    public static uint Dosis => 24283;

    public static uint Phlegma => 24289;

    public static uint EukrasianDosis => 24293;

    public static uint Dyskrasia => 24297;

    public static uint Toxikon => 24304;

    public static uint DosisII => 24306;

    public static uint PhlegmaII => 24307;

    public static uint EukrasianDosisII => 21492;

    public static uint DosisIII => 24312;

    public static uint PhlegmaIII => 24313;

    public static uint EukrasianDosisIII => 24314;

    public static uint DyskrasiaII => 24315;

    public static uint ToxikonII => 24316;

    public static uint ArcaneCrest => 24404;

    public static uint AspectedBenefic => 3595;

    public static uint AspectedHelios => 3601;

    public static uint IronJaws => 3560;

    public static uint KaeshiSetsugekka => 16486;

    public static uint OgiNamikiri => 25781;

    public static uint KaeshiNamikiri => 25782;

    public ActionType Type => (ActionType)(this.Raw >> 24);

    public uint Id => this.Raw & 0x00FFFFFFu;

    public static implicit operator bool(ActionId x) => x.Raw != 0;

    public static bool operator ==(ActionId l, ActionId r) => l.Raw == r.Raw;

    public static bool operator !=(ActionId l, ActionId r) => l.Raw != r.Raw;

    public override bool Equals(object? obj) => obj is ActionId && this == (ActionId)obj;

    public override int GetHashCode() => this.Raw.GetHashCode();

    public override string ToString() => $"{this.Type} {this.Id}";
}