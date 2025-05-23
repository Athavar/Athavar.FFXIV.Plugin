// <copyright file="Constants.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

public static class Constants
{
    public const string PluginName = "Athavar's Toolbox";

    public const int MaxLevel = 100;

    public const uint PlayerId = 0xE0000000;

    public static class Addons
    {
        public const string Materialize = "Materialize";
        public const string RecipeNote = "RecipeNote";
        public const string Repair = "Repair";
        public const string Synthesis = "Synthesis";
        public const string WksRecipeNote = "WKSRecipeNotebook";
    }

    public static class FontsManager
    {
        public const string DalamudFontKey = "Dalamud Font";
    }
}