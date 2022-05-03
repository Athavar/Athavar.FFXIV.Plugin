// <copyright file="RecipeCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;

/// <summary>
///     The /recipe command.
/// </summary>
internal class RecipeCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/recipe\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string recipeName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecipeCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="recipeName">Recipe name.</param>
    /// <param name="wait">Wait value.</param>
    private RecipeCommand(string text, string recipeName, WaitModifier wait)
        : base(text, wait)
        => this.recipeName = recipeName.ToLowerInvariant();

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RecipeCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = ExtractAndUnquote(match, "name");

        return new RecipeCommand(text, nameValue, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        if (this.AddonSynthesisIsOpen())
        {
            throw new MacroCommandError("/recipe cannot be used while the Synthesis window is open.");
        }

        var recipeId = this.SearchRecipeId(this.recipeName);
        if (recipeId == 0)
        {
            throw new MacroCommandError("Recipe not found");
        }

        PluginLog.Debug($"RecipeId found : {recipeId}");
        this.OpenRecipeNote(recipeId);

        await this.PerformWait(token);
    }

    private bool AddonSynthesisIsOpen()
    {
        var addon = DalamudServices.GameGui.GetAddonByName("Synthesis", 1);
        return addon != IntPtr.Zero;
    }

    private unsafe void OpenRecipeNote(uint recipeId)
    {
        var agent = AgentRecipeNote.Instance();
        if (agent == null)
        {
            throw new MacroCommandError("RecipeNoteAgent not found");
        }

        agent->OpenRecipeByRecipeId(recipeId);
    }

    private uint SearchRecipeId(string recipeName)
    {
        var recipes = DalamudServices.DataManager.GetExcelSheet<Recipe>()!;
        var founds = recipes.Where(r => r.ItemResult.Value?.Name.ToString() == recipeName).ToList();
        switch (founds.Count)
        {
            case 0: throw new MacroCommandError("Recipe not found");
            case 1: return founds.First().RowId;
            default:
                var jobId = DalamudServices.ClientState.LocalPlayer?.ClassJob.Id;

                var recipe = recipes.FirstOrDefault(r => this.GetClassJobId(r) == jobId);
                if (recipe == default)
                {
                    return recipes.First().RowId;
                }

                return recipe.RowId;
        }
    }

    private uint GetClassJobId(Recipe recipe)
        =>
            /* Name           CraftType ClassJob
               Carpenter      0         8
               Blacksmith     1         9
               Armorer        2         10
               Goldsmith      3         11
               Leatherworker  4         12
               Weaver         5         13
               Alchemist      6         14
               Culinarian     7         15
            */
            recipe.CraftType.Value!.RowId + 8;
}