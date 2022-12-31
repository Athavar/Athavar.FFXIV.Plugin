// <copyright file="AutoSpear.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils.SeFunctions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

internal partial class AutoSpear : Window
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
    private readonly Dictionary<uint, FishingSpot> spearfishingSpots;

    private readonly string useSpearSkillName;

    private unsafe SpearfishWindow* addon = null;
    private float uiScale = 1;
    private Vector2 uiPos = Vector2.Zero;
    private Vector2 uiSize = Vector2.Zero;

    private FishingSpot? currentSpot;
    private bool isOpen;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoSpear" /> class.
    /// </summary>
    /// <param name="windowSystem"><see cref="WindowSystem" /> added by DI.</param>
    /// <param name="configuration"><see cref="configuration" /> added by Di.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by Di.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    public AutoSpear(WindowSystem windowSystem, Configuration configuration, IDalamudServices dalamudServices, IChatManager chatManager)
        : base("AutoSpear", WindowFlags, true)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.configuration = configuration.AutoSpear!;
        windowSystem.AddWindow(this);
        this.IsOpen = true;

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
           .Where(fs => fs.Spearfishing)
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
    }

    public override unsafe bool DrawConditions()
    {
        var oldOpen = this.isOpen;
        this.addon = (SpearfishWindow*)this.dalamudServices.GameGui.GetAddonByName("SpearFishing", 1);
        this.isOpen = this.addon != null && this.addon->Base.WindowNode != null;
        if (!this.isOpen)
        {
            return false;
        }

        if (this.isOpen != oldOpen)
        {
            this.currentSpot = this.GetTargetFishingSpot();
        }

        return true;
    }

    public override unsafe void PreDraw()
    {
        this.uiScale = this.addon->Base.Scale;
        this.uiPos = new Vector2(this.addon->Base.X, this.addon->Base.Y);
        this.uiSize = new Vector2(this.addon->Base.WindowNode->AtkResNode.Width * this.uiScale, this.addon->Base.WindowNode->AtkResNode.Height * this.uiScale);
    }
}