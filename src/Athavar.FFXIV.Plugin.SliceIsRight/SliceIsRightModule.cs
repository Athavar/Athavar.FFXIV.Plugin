// <copyright file="SliceIsRightModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.SliceIsRight;

using System.Numerics;
using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using ImGuiNET;

[Module(ModuleName, ModuleConfigurationType = typeof(SliceIsRightConfiguration))]
public sealed class SliceIsRightModule : Module<SliceIsRightConfiguration>, IDisposable
{
    internal const string ModuleName = "SliceIsRight";

    private const ushort GoldSaucerTerritoryId = 144;
    private const float MaxDistance = 30f;
    private const float HALF_PI = 1.5707964f;

    private readonly IDictionary<uint, DateTime> objectsAndSpawnTime = new Dictionary<uint, DateTime>();

    private readonly IDalamudServices dalamudServices;
    private readonly uint COLOUR_BLUE = ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 1f, 0.15f)));
    private readonly uint COLOUR_GREEN = ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 1f, 0.0f, 0.15f)));
    private readonly uint COLOUR_RED = ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.0f, 0.0f, 0.4f)));

    private bool enableState;

    public SliceIsRightModule(SliceIsRightConfiguration configuration, IDalamudServices dalamudServices)
        : base(configuration)
    {
        this.dalamudServices = dalamudServices;
        if (this.GetEnableStateAction().Get())
        {
            this.Enable();
        }
    }

    public override string Name => "Slice Is Right";

    private bool IsInGoldSaucer { get; set; }

    public void Dispose() => this.OnDisabled();

    protected override void OnEnabled()
    {
        if (this.enableState)
        {
            return;
        }

        this.enableState = true;
        this.IsInGoldSaucer = this.dalamudServices.ClientState.TerritoryType == GoldSaucerTerritoryId;
        this.dalamudServices.ClientState!.TerritoryChanged += this.TerritoryChanged;
        if (this.IsInGoldSaucer)
        {
            this.dalamudServices.PluginInterface.UiBuilder.Draw += this.DrawUi;
        }
    }

    protected override void OnDisabled()
    {
        if (!this.enableState)
        {
            return;
        }

        this.enableState = false;
        this.dalamudServices.ClientState.TerritoryChanged -= this.TerritoryChanged;
        if (this.IsInGoldSaucer)
        {
            this.dalamudServices.PluginInterface.UiBuilder.Draw -= this.DrawUi;
        }
    }

    private void TerritoryChanged(ushort e)
    {
        var oldState = this.IsInGoldSaucer;
        this.IsInGoldSaucer = e == GoldSaucerTerritoryId;
        if (this.IsInGoldSaucer != oldState)
        {
            if (this.IsInGoldSaucer)
            {
                this.dalamudServices.PluginInterface.UiBuilder.Draw += this.DrawUi;
            }
            else
            {
                this.dalamudServices.PluginInterface.UiBuilder.Draw -= this.DrawUi;
            }
        }
    }

    private void DrawUi()
    {
        if (!this.dalamudServices.ClientState.IsLoggedIn || !this.IsInGoldSaucer)
        {
            return;
        }

        for (var index = 0; index < this.dalamudServices.ObjectTable.Length; ++index)
        {
            var gameObject = this.dalamudServices.ObjectTable[index];
            if (!(gameObject == null) && this.DistanceToPlayer(gameObject.Position) <= 30.0)
            {
                var model = Marshal.ReadInt32(gameObject.Address + 128);
                if (gameObject.ObjectKind == ObjectKind.EventObj && model is >= 2010777 and <= 2010779)
                {
                    this.RenderObject(index, gameObject, model);
                }
                else
                {
                    var objectId1 = this.dalamudServices.ClientState.LocalPlayer?.ObjectId;
                    var objectId2 = gameObject.ObjectId;
                    var num = ((int)objectId1.GetValueOrDefault() == (int)objectId2) & objectId1.HasValue ? 1 : 0;
                }
            }
        }
    }

    private void RenderObject(int index, GameObject obj, int model, float? radius = null)
    {
        if (this.objectsAndSpawnTime.TryGetValue(obj.ObjectId, out var dateTime))
        {
            if (dateTime.AddSeconds(5.0) > DateTime.Now)
            {
                return;
            }

            switch (model)
            {
                case 2010777:
                    this.DrawRectWorld(obj, obj.Rotation + HALF_PI, radius ?? 25f, 5f, this.COLOUR_BLUE);
                    break;
                case 2010778:
                    this.DrawRectWorld(obj, obj.Rotation + HALF_PI, radius ?? 25f, 5f, this.COLOUR_GREEN);
                    this.DrawRectWorld(obj, obj.Rotation - HALF_PI, radius ?? 25f, 5f, this.COLOUR_GREEN);
                    break;
                case 2010779:
                    this.DrawFilledCircleWorld(obj, radius ?? 11f, this.COLOUR_RED);
                    break;
            }
        }
        else
        {
            this.objectsAndSpawnTime.Add(obj.ObjectId, DateTime.Now);
        }
    }

    private void BeginRender(string name)
    {
        ImGui.PushID("sliceWindowI" + name);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero, pivot: Vector2.Zero);
        ImGui.Begin("sliceWindow" + name, ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoSavedSettings);
        ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
    }

    private void EndRender()
    {
        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopID();
    }

    private void DrawFilledCircleWorld(GameObject obj, float radius, uint colour)
    {
        this.BeginRender(obj.Address.ToString());
        var position = obj.Position;
        var num = 100;
        var flag = false;
        for (var index = 0; index <= 2 * num; ++index)
        {
            flag |= this.dalamudServices.GameGui.WorldToScreen(new Vector3(position.X + (radius * (float)Math.Sin((Math.PI / num) * index)), position.Y, position.Z + (radius * (float)Math.Cos((Math.PI / num) * index))), out var screenPos);
            ImGui.GetWindowDrawList().PathLineTo(screenPos);
        }

        if (flag)
        {
            ImGui.GetWindowDrawList().PathFillConvex(colour);
        }
        else
        {
            ImGui.GetWindowDrawList().PathClear();
        }

        this.EndRender();
    }

    private void DrawRectWorld(
        GameObject obj,
        float rotation,
        float length,
        float width,
        uint colour)
    {
        this.BeginRender(obj.Address + obj.Rotation.ToString());
        var position = obj.Position;
        var displaySize = ImGui.GetIO().DisplaySize;
        var near1 = new Vector3(position.X + ((width / 2f) * (float)Math.Sin(HALF_PI + rotation)), position.Y, position.Z + ((width / 2f) * (float)Math.Cos(HALF_PI + rotation)));
        var near2 = new Vector3(position.X + ((width / 2f) * (float)Math.Sin(rotation - HALF_PI)), position.Y, position.Z + ((width / 2f) * (float)Math.Cos(rotation - HALF_PI)));
        var nearCenter = new Vector3(position.X, position.Y, position.Z);
        var rectangleCount = 20;
        var rectangleLength = length / rectangleCount;
        var windowDrawList = ImGui.GetWindowDrawList();
        for (var index = 1; index <= rectangleCount; ++index)
        {
            var far1 = new Vector3(near1.X + (rectangleLength * (float)Math.Sin(rotation)), near1.Y, near1.Z + (rectangleLength * (float)Math.Cos(rotation)));
            var far2 = new Vector3(near2.X + (rectangleLength * (float)Math.Sin(rotation)), near2.Y, near2.Z + (rectangleLength * (float)Math.Cos(rotation)));
            var farCenter = new Vector3(nearCenter.X + (rectangleLength * (float)Math.Sin(rotation)), nearCenter.Y, nearCenter.Z + (rectangleLength * (float)Math.Cos(rotation)));
            var flag = false;
            var vector3Array = new Vector3[6]
            {
                far2,
                farCenter,
                far1,
                near1,
                nearCenter,
                near2,
            };
            foreach (var worldPos in vector3Array)
            {
                Vector2 screenPos;
                flag |= this.dalamudServices.GameGui.WorldToScreen(worldPos, out screenPos);
                if ((screenPos.X > 0.0) & (screenPos.X < (double)displaySize.X) || (screenPos.Y > 0.0) & (screenPos.Y < (double)displaySize.Y))
                {
                    windowDrawList.PathLineTo(screenPos);
                }
            }

            if (flag)
            {
                windowDrawList.PathFillConvex(colour);
            }
            else
            {
                windowDrawList.PathClear();
            }

            near1 = far1;
            near2 = far2;
            nearCenter = farCenter;
        }

        this.EndRender();
    }

    private float DistanceToPlayer(Vector3 center) => Vector3.Distance(this.dalamudServices.ClientState.LocalPlayer?.Position ?? Vector3.Zero, center);
}