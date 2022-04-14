// <copyright file="MacroHelpWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using System;
using System.Collections.Generic;
using System.Numerics;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

/// <summary>
///     Help window for macro creation.
/// </summary>
internal class MacroHelpWindow : Window
{
    private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    private readonly (string Name, string? Alias, string Description, string[] Modifiers, string[] Examples)[] commandData =
    {
        (
            "action", "ac",
            "Execute an action and wait for the server to respond.",
            new[] { "wait", /*"unsafe",*/ "condition" },
            new[]
            {
                "/ac Groundwork",
                "/ac \"Tricks of the Trade\"",
            }),
        (
            "click", null,
            "Click a pre-defined button in an addon or window.",
            new[] { "wait" },
            new[]
            {
                "/click synthesize",
            }),
        (
            "loop", null,
            "Loop the current macro forever, or a certain amount of times.",
            new[] { "wait", "echo" },
            new[]
            {
                "/loop",
                "/loop 5",
            }),
        (
            "require", null,
            "Require a certain effect to be present before continuing.",
            new[] { "wait", "maxwait" },
            new[]
            {
                "/require \"Well Fed\"",
            }),
        (
            "requirestats", null,
            "Require a certain amount of stats effect to be present before continuing. Syntax is Craftsmanship, Control, then CP.",
            new[] { "wait", "maxwait" },
            new[]
            {
                "/requirestats 2700 2600 500",
            }),
        (
            "runmacro", null,
            "Start a macro from within another macro.",
            new[] { "wait" },
            new[]
            {
                "/runmacro \"Sub macro\"",
            }),
        (
            "send", null,
            "Send an arbitrary keystroke.",
            new[] { "wait" },
            new[]
            {
                "/send MULTIPLY",
                "/send NUMPAD0",
            }),
        (
            "target", null,
            "Target anyone and anything that can be selected.",
            new[] { "wait" },
            new[]
            {
                "/target Eirikur",
                "/target Moyce",
            }),
        (
            "waitaddon", null,
            "Wait for an addon, otherwise known as a UI component to be present. You can discover these names by using the \"Addon Inspector\" view inside the \"/xldata\" window.",
            new[] { "wait", "maxwait" },
            new[]
            {
                "/waitaddon RecipeNote",
            }),
        (
            "wait", null,
            "The same as the wait modifier, but as a command.",
            Array.Empty<string>(),
            new[]
            {
                "/wait 1-5",
            }),
    };

    private readonly (string Name, string Description, string[] Examples)[] modifierData =
    {
        (
            "wait",
            "Wait a certain amount of time, or a random time within a range.",
            new[]
            {
                "/ac Groundwork <wait.3>       # Wait 3 seconds",
                "/ac Groundwork <wait.3.5>     # Wait 3.5 seconds",
                "/ac Groundwork <wait.1-5>     # Wait between 1 and 5 seconds",
                "/ac Groundwork <wait.1.5-5.5> # Wait between 1.5 and 5.5 seconds",
            }),
        (
            "maxwait",
            "For certain commands, the maximum time to wait for a certain state to be achieved. By default, this is 5 seconds.",
            new[]
            {
                "/waitaddon RecipeNote <maxwait.10>",
            }),
        (
            "condition",
            "Require a crafting condition to perform the action specified. This is taken from the Synthesis window and may be localized to your client language.",
            new[]
            {
                "/ac Observe <condition.poor>",
                "/ac \"Precise Touch\" <condition.good>",
                "/ac \"Byregot's Blessing\" <condition.not.poor>",
                "/ac \"Byregot's Blessing\" <condition.!poor>",
            }),
        /*(
            "unsafe",
            "Prevent the /action command from waiting for a positive server response and attempting to execute the command anyways.",
            new[]
            {
                "/ac \"Tricks of the Trade\" <unsafe>",
            }),*/
        (
            "echo",
            "Echo the amount of loops remaining after executing a /loop command.",
            new[]
            {
                "/loop 5 <echo>",
            }),
    };

    private readonly (string Name, string Description, string? Example)[] cliData =
    {
        ("help", "Show this window.", null),
        ("run", "Run a macro, the name must be unique.", "/pmacro run MyMacro"),
        ("run loop #", "Run a macro and then loop N times, the name must be unique. Only the last /loop in the macro is replaced", "/pmacro run loop 5 MyMacro"),
        ("pause", "Pause the currently executing macro.", null),
        ("pause loop", "Pause the currently executing macro at the next /loop.", null),
        ("resume", "Resume the currently paused macro.", null),
        ("stop", "Clear the currently executing macro list.", null),
        ("stop loop", "Clear the currently executing macro list at the next /loop.", null),
    };

    private readonly IList<string> clickNames;
    private readonly MacroConfiguration configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroHelpWindow" /> class.
    /// </summary>
    /// <param name="windowSystem"><see cref="WindowSystem" /> added by DI.</param>
    /// <param name="click"><see cref="IClick" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    public MacroHelpWindow(WindowSystem windowSystem, IClick click, Configuration configuration)
        : base($"{Plugin.PluginName} Macro Help")
    {
        this.configuration = configuration.Macro!;
        this.Flags |= ImGuiWindowFlags.NoScrollbar;

        this.Size = new Vector2(400, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.RespectCloseHotkey = false;

        this.clickNames = click.GetClickNames();
        windowSystem.AddWindow(this);
    }

    /// <inheritdoc />
    public override void Draw()
    {
        if (ImGui.BeginTabBar("HelpTab"))
        {
            var tabs = new (string Title, Action Dele)[]
                       {
                           ("Options", this.DrawOptions),
                           ("Commands", this.DrawCommands),
                           ("Modifiers", this.DrawModifiers),
                           ("CLI", this.DrawCli),
                           ("Clicks", this.DrawClicks),
                           ("Sends", this.DrawVirtualKeys),
                       };

            foreach (var (title, dele) in tabs)
            {
                if (ImGui.BeginTabItem(title))
                {
                    ImGui.BeginChild("scrolling", new Vector2(0, -1), false);
                    dele();
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    private void DrawOptions()
    {
        void DisplayOption(params string[] lines)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, this.shadedColor);

            foreach (var line in lines)
            {
                ImGui.TextWrapped(line);
            }

            ImGui.PopStyleColor();
        }

        ImGui.PushFont(UiBuilder.MonoFont);
        {
            var craftSkip = this.configuration.CraftSkip;
            if (ImGui.Checkbox("Craft Skip", ref craftSkip))
            {
                this.configuration.CraftSkip = craftSkip;
                this.configuration.Save();
            }

            DisplayOption("- Skip craft actions when not crafting.");
        }

        {
            var craftWaitSkip = this.configuration.CraftWaitSkip;
            if (ImGui.Checkbox("Craft Wait Skip", ref craftWaitSkip))
            {
                this.configuration.CraftWaitSkip = craftWaitSkip;
                this.configuration.Save();
            }

            DisplayOption("- Ignore <wait> in craft commands and execute next line as soon as possible.");
        }

        {
            var qualitySkip = this.configuration.QualitySkip;
            if (ImGui.Checkbox("Quality Skip", ref qualitySkip))
            {
                this.configuration.QualitySkip = qualitySkip;
                this.configuration.Save();
            }

            DisplayOption("- Skip quality increasing actions when the HQ chance is at 100%. If you depend on durability increases from Manipulation towards the end of your macro, you will likely want to disable this.");
        }

        {
            var loopTotal = this.configuration.LoopTotal;
            if (ImGui.Checkbox("Loop Total", ref loopTotal))
            {
                this.configuration.LoopTotal = loopTotal;
                this.configuration.Save();
            }

            DisplayOption("- The numeric option provided to /loop will be considered as the total number of iterations, rather than the amount of times to loop. Internally, this will just subtract 1 from your /loop <amount> command.");
        }

        {
            var loopEcho = this.configuration.LoopEcho;
            if (ImGui.Checkbox("Loop Echo", ref loopEcho))
            {
                this.configuration.LoopEcho = loopEcho;
                this.configuration.Save();
            }

            DisplayOption("- Loop commands will always have an <echo> tag applied.");
        }

        {
            var disableMonospaced = this.configuration.DisableMonospaced;
            if (ImGui.Checkbox("Disable Monospaced fonts", ref disableMonospaced))
            {
                this.configuration.DisableMonospaced = disableMonospaced;
                this.configuration.Save();
            }

            DisplayOption("- Use the regular font instead of monospaced in the macro window. This may be handy for JP users so as to prevent missing unicode errors.");
        }

        // Crafting Loop
        {
            var craftLoopFromRecipeNote = this.configuration.CraftLoopFromRecipeNote;
            if (ImGui.Checkbox("CraftLoop starts in the Crafting Log", ref craftLoopFromRecipeNote))
            {
                this.configuration.CraftLoopFromRecipeNote = craftLoopFromRecipeNote;
                this.configuration.Save();
            }

            DisplayOption("- When enabled the CraftLoop option will expect the Crafting Log to be visible, otherwise the Synthesis window must be visible.");

            var craftLoopEcho = this.configuration.CraftLoopEcho;
            if (ImGui.Checkbox("CraftLoop echo", ref craftLoopEcho))
            {
                this.configuration.CraftLoopEcho = craftLoopEcho;
                this.configuration.Save();
            }

            DisplayOption("- When enabled the /loop commands supplied by CraftLoop option will an echo modifier.");

            var craftLoopMaxWait = this.configuration.CraftLoopMaxWait;
            ImGui.SetNextItemWidth(50);
            if (ImGui.InputInt("CraftLoop maxwait", ref craftLoopMaxWait, 0))
            {
                if (craftLoopMaxWait < 0)
                {
                    craftLoopMaxWait = 0;
                }

                if (craftLoopMaxWait != this.configuration.CraftLoopMaxWait)
                {
                    this.configuration.CraftLoopMaxWait = craftLoopMaxWait;
                    this.configuration.Save();
                }
            }

            DisplayOption("- The CraftLoop /waitaddon \"...\" <maxwait> modifiers have their maximum wait set to this value.");
        }

        ImGui.PopFont();
    }

    private void DrawCommands()
    {
        ImGui.PushFont(UiBuilder.MonoFont);

        foreach (var (name, alias, desc, modifiers, examples) in this.commandData)
        {
            ImGui.Text($"/{name}");

            ImGui.PushStyleColor(ImGuiCol.Text, this.shadedColor);

            if (alias != null)
            {
                ImGui.Text($"- Alias: /{alias}");
            }

            ImGui.TextWrapped($"- Description: {desc}");

            ImGui.Text("- Modifiers:");
            foreach (var mod in modifiers)
            {
                ImGui.Text($"  - <{mod}>");
            }

            ImGui.Text("- Examples:");
            foreach (var example in examples)
            {
                ImGui.Text($"  - {example}");
            }

            ImGui.PopStyleColor();

            ImGui.Separator();
        }

        ImGui.PopFont();
    }

    private void DrawClicks()
    {
        ImGui.PushFont(UiBuilder.MonoFont);

        foreach (var name in this.clickNames)
        {
            ImGui.Text($"/click {name}");
        }

        ImGui.PopFont();
    }

    private void DrawVirtualKeys()
    {
        ImGui.PushFont(UiBuilder.MonoFont);

        var names = Enum.GetNames<Native.KeyCode>();

        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i];

            ImGui.Text($"/send {name}");
        }

        ImGui.PopFont();
    }

    private void DrawModifiers()
    {
        ImGui.PushFont(UiBuilder.MonoFont);

        foreach (var (name, desc, examples) in this.modifierData)
        {
            ImGui.Text($"<{name}>");

            ImGui.PushStyleColor(ImGuiCol.Text, this.shadedColor);

            ImGui.TextWrapped($"- Description: {desc}");

            ImGui.Text("- Examples:");
            foreach (var example in examples)
            {
                ImGui.Text($"  - {example}");
            }

            ImGui.PopStyleColor();

            ImGui.Separator();
        }

        ImGui.PopFont();
    }

    private void DrawCli()
    {
        ImGui.PushFont(UiBuilder.MonoFont);

        foreach (var (name, desc, example) in this.cliData)
        {
            ImGui.Text($"/pmacro {name}");

            ImGui.PushStyleColor(ImGuiCol.Text, this.shadedColor);

            ImGui.TextWrapped($"- Description: {desc}");

            if (example != null)
            {
                ImGui.Text($"- Example: {example}");
            }

            ImGui.PopStyleColor();

            ImGui.Separator();
        }

        ImGui.PopFont();
    }
}