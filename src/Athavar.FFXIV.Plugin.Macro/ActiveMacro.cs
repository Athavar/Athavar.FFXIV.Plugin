// <copyright file="ActiveMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro;

using System.Reflection;
using System.Text;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar;
using Athavar.FFXIV.Plugin.Macro.Grammar.Commands;
using Athavar.FFXIV.Plugin.Macro.Managers;
using Microsoft.Extensions.DependencyInjection;
using NLua;

/// <summary>
///     Represent a active running macro.
/// </summary>
internal partial class ActiveMacro : IDisposable
{
    private static MacroConfiguration? configuration;
    private static ICommandInterface? commandInterface;
    private Lua? lua;
    private LuaFunction? luaGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActiveMacro" /> class.
    /// </summary>
    /// <param name="node">The node containing the macro code.</param>
    public ActiveMacro(MacroNode node)
    {
        this.Node = node;

        if (node.IsLua)
        {
            this.Steps = new List<MacroCommand>();
            return;
        }

        var contents = ModifyMacroForCraftLoop(node.Contents, node.CraftingLoop, node.CraftLoopCount);
        this.Steps = MacroParser.Parse(contents).ToList();
    }

    /// <summary>
    ///     Gets the <see cref="MacroNode" /> containing the macro code.
    /// </summary>
    public MacroNode Node { get; }

    /// <summary>
    ///     Gets all steps of the <see cref="ActiveMacro" />.
    /// </summary>
    public List<MacroCommand> Steps { get; }

    /// <summary>
    ///     Gets or sets index of current executing step.
    /// </summary>
    public int StepIndex { get; set; }

    /// <summary>
    ///     Gets the <see cref="MacroManager" />.
    /// </summary>
    private static MacroConfiguration Configuration => configuration ?? throw new NullReferenceException("MacroManager is not set");

    /// <summary>
    ///     Gets the <see cref="MacroManager" />.
    /// </summary>
    private static ICommandInterface CommandInterface => commandInterface ?? throw new NullReferenceException("CommandInterface is not set");

    /// <summary>
    ///     Modify a macro for craft looping.
    /// </summary>
    /// <param name="contents">Contents of a macroNode.</param>
    /// <param name="craftLoop">A value indicating whether craftLooping is enabled.</param>
    /// <param name="craftCount">Amount to craftLoop.</param>
    /// <returns>The modified macro.</returns>
    public static string ModifyMacroForCraftLoop(string contents, bool craftLoop, int craftCount)
    {
        if (!craftLoop)
        {
            return contents;
        }

        if (Configuration.UseCraftLoopTemplate)
        {
            var template = Configuration.CraftLoopTemplate;

            if (craftCount == 0)
            {
                return contents;
            }

            if (craftCount == -1)
            {
                craftCount = 999_999;
            }

            if (!template.Contains("{{macro}}"))
            {
                throw new MacroCommandError("CraftLoop template does not contain the {{macro}} placeholder");
            }

            return template
               .Replace("{{macro}}", contents)
               .Replace("{{count}}", craftCount.ToString());
        }

        var maxwait = Configuration.CraftLoopMaxWait;
        var maxwaitMod = maxwait > 0 ? $" <maxwait.{maxwait}>" : string.Empty;

        var echo = Configuration.CraftLoopEcho;
        var echoMod = echo ? " <echo>" : string.Empty;

        var craftGateStep = Configuration.CraftLoopFromRecipeNote
            ? $"/craft {craftCount}{echoMod}"
            : $"/gate {craftCount - 1}{echoMod}";

        var clickSteps = string.Join("\n", $@"/waitaddon ""RecipeNote""{maxwaitMod}", @"/click ""synthesize""", $@"/waitaddon ""Synthesis""{maxwaitMod}");

        var loopStep = $"/loop{echoMod}";

        var sb = new StringBuilder();

        if (Configuration.CraftLoopFromRecipeNote)
        {
            if (craftCount == -1)
            {
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
                sb.AppendLine(loopStep);
            }
            else if (craftCount == 0)
            {
                sb.AppendLine(contents);
            }
            else if (craftCount == 1)
            {
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
            }
            else
            {
                sb.AppendLine(craftGateStep);
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
                sb.AppendLine(loopStep);
            }
        }
        else
        {
            if (craftCount == -1)
            {
                sb.AppendLine(contents);
                sb.AppendLine(clickSteps);
                sb.AppendLine(loopStep);
            }
            else if (craftCount == 0 || craftCount == 1)
            {
                sb.AppendLine(contents);
            }
            else
            {
                sb.AppendLine(contents);
                sb.AppendLine(craftGateStep);
                sb.AppendLine(clickSteps);
                sb.AppendLine(loopStep);
            }
        }

        return sb.ToString().Trim();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.luaGenerator?.Dispose();
        this.lua?.Dispose();
    }

    /// <summary>
    ///     Increase the step count.
    /// </summary>
    public void NextStep() => this.StepIndex++;

    /// <summary>
    ///     Loop.
    /// </summary>
    public void Loop()
    {
        if (this.Node.IsLua)
        {
            throw new MacroCommandError("Loop is not supported for Lua scripts");
        }

        this.StepIndex = -1;
    }

    /// <summary>
    ///     Get the current step.
    /// </summary>
    /// <returns>A command.</returns>
    public MacroCommand? GetCurrentStep()
    {
        if (this.Node.IsLua)
        {
            if (this.lua == null)
            {
                this.InitLuaScript();
            }

            var results = this.luaGenerator!.Call();
            if (results.Length == 0)
            {
                return null;
            }

            if (results[0] is not string text)
            {
                throw new MacroCommandError("Lua macro yielded a non-string");
            }

            var command = MacroParser.ParseLine(text);

            if (command is not null)
            {
                this.Steps.Add(command);
            }

            return command;
        }

        if (this.StepIndex < 0 || this.StepIndex >= this.Steps.Count)
        {
            return null;
        }

        return this.Steps[this.StepIndex];
    }

    /// <summary>
    ///     Setup the <see cref="IServiceProvider" /> for all commands.
    /// </summary>
    /// <param name="sp">The <see cref="IServiceProvider" />.</param>
    internal static void SetServiceProvider(IServiceProvider sp)
    {
        configuration = sp.GetRequiredService<Configuration>().Macro!;
        commandInterface = sp.GetRequiredService<ICommandInterface>();
    }

    private void InitLuaScript()
    {
        var script = this.Node.Contents
           .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
           .Select(line => $"  {line}")
           .Join('\n');

        static void RegisterClass<T>(Lua lua, T instance)
            where T : class
            => instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).ToList()
               .ForEach(method => lua.RegisterFunction(method.Name, instance, method));

        this.lua = new Lua();
        this.lua.State.Encoding = Encoding.UTF8;
        this.lua.LoadCLRPackage();
        RegisterClass(this.lua, CommandInterface);

        script = string.Format(EntrypointTemplate, script);

        var results = this.lua.DoString(script);

        if (results.Length == 0 || results[0] is not LuaFunction coro)
        {
            throw new MacroCommandError("Could not get Lua entrypoint.");
        }

        this.luaGenerator = coro;
    }
}

/// <summary>
///     Lua code snippets.
/// </summary>
internal partial class ActiveMacro
{
    private const string EntrypointTemplate = @"
yield = coroutine.yield
--
function entrypoint()
{0}
end
--
return coroutine.wrap(entrypoint)";

    private const string FStringSnippet = @"
function f(str)
   local outer_env = _ENV
   return (str:gsub(""%b{}"", function(block)
      local code = block:match(""{(.*)}"")
      local exp_env = {}
      setmetatable(exp_env, { __index = function(_, k)
         local stack_level = 5
         while debug.getinfo(stack_level, """") ~= nil do
            local i = 1
            repeat
               local name, value = debug.getlocal(stack_level, i)
               if name == k then
                  return value
               end
               i = i + 1
            until name == nil
            stack_level = stack_level + 1
         end
         return rawget(outer_env, k)
      end })
      local fn, err = load(""return ""..code, ""expression `""..code..""`"", ""t"", exp_env)
      if fn then
         return tostring(fn())
      else
         error(err, 0)
      end
   end))
end";
}