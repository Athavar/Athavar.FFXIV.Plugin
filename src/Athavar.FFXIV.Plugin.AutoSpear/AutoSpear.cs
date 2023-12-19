// <copyright file="AutoSpear.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.AutoSpear;

using System.Numerics;
using Athavar.FFXIV.Plugin.AutoSpear.SeFunctions;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

internal sealed partial class AutoSpear : Tab
{
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoDecoration
                                               | ImGuiWindowFlags.NoInputs
                                               | ImGuiWindowFlags.AlwaysAutoResize
                                               | ImGuiWindowFlags.NoFocusOnAppearing
                                               | ImGuiWindowFlags.NoNavFocus
                                               | ImGuiWindowFlags.NoBackground;

    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly AutoSpearConfiguration configuration;
    private readonly IFrameworkManager frameworkManager;
    private readonly Dictionary<uint, FishingSpot> spearfishingSpots;

    private readonly string useSpearSkillName;

    private unsafe SpearfishWindow* addon = null;
    private float uiScale = 1;
    private Vector2 uiPos = Vector2.Zero;
    private Vector2 uiSize = Vector2.Zero;

    private FishingSpot? currentSpot;
    private bool isOpen;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoSpear"/> class.
    /// </summary>
    /// <param name="configuration"><see cref="configuration"/> added by Di.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by Di.</param>
    /// <param name="chatManager"><see cref="IChatManager"/> added by DI.</param>
    public AutoSpear(AutoSpearConfiguration configuration, IDalamudServices dalamudServices, IChatManager chatManager, IFrameworkManager frameworkManager)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.configuration = configuration;
        this.frameworkManager = frameworkManager;

        var sheet = dalamudServices.DataManager.GetExcelSheet<Action>()!;
        this.useSpearSkillName = sheet.GetRow(7632)!.Name;

        var dataManager = dalamudServices.DataManager;
        var spearFishes = dalamudServices.DataManager.GetExcelSheet<SpearfishingItem>()!
           .Where(sf => sf.Item.Row != 0 && sf.Item.Row < 1000000)
           .Select(sf => new SpearFish(dataManager, sf)).ToDictionary(f => f.ItemId, f => f);

        this.ApplyData(spearFishes);

        var fishingSpots = dalamudServices.DataManager.GetExcelSheet<SpearfishingNotebook>()!
           .Where(sf => sf.PlaceName.Row != 0 && sf.TerritoryType.Row > 0)
           .Select(f => new FishingSpot(spearFishes, f));

        var points = dalamudServices.DataManager.GetExcelSheet<GatheringPoint>()!;

        // We go through all fishingspots and correspond them to their gathering point base.
        var baseNodes = fishingSpots
           .Where(fs => fs is { Spearfishing: true, SpearfishingSpotData: not null })
           .ToDictionary(fs => fs.SpearfishingSpotData!.GatheringPointBase.Row, fs => fs);

        // Now we correspond all gathering nodes to their associated fishing spot.
        this.spearfishingSpots = new Dictionary<uint, FishingSpot>(baseNodes.Count);
        foreach (var point in points)
        {
            if (!baseNodes.TryGetValue(point.GatheringPointBase.Row, out var node))
            {
                continue;
            }

            this.spearfishingSpots.Add(point.RowId, node);
        }

        this.frameworkManager.Subscribe(this.Tick);
    }

    public override void Dispose()
    {
        this.frameworkManager.Unsubscribe(this.Tick);
        base.Dispose();
    }

    private unsafe void Tick(IFramework framework)
    {
        var oldOpen = this.isOpen;
        this.addon = (SpearfishWindow*)this.dalamudServices.GameGui.GetAddonByName("SpearFishing");
        this.isOpen = this.addon != null && this.addon->Base.WindowNode != null;
        if (!this.isOpen)
        {
            return;
        }

        if (this.isOpen != oldOpen)
        {
            this.currentSpot = this.GetTargetFishingSpot();
        }

        this.uiScale = this.addon->Base.Scale;
        this.uiPos = new Vector2(this.addon->Base.X, this.addon->Base.Y);
        this.uiSize = new Vector2(this.addon->Base.WindowNode->AtkResNode.Width * this.uiScale, this.addon->Base.WindowNode->AtkResNode.Height * this.uiScale);

        this.CheckFish(this.currentSpot, this.addon->Fish1, this.addon->Fish1Node, 0);
        this.CheckFish(this.currentSpot, this.addon->Fish2, this.addon->Fish2Node, 1);
        this.CheckFish(this.currentSpot, this.addon->Fish3, this.addon->Fish3Node, 2);
    }
}