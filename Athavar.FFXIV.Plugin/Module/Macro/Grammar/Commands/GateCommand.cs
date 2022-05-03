namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;

/// <summary>
///     The /craft command.
/// </summary>
internal class GateCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/(craft|gate)(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly EchoModifier echoMod;
    private readonly int startingCrafts;
    private int craftsRemaining;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GateCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="craftCount">Craft count.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="echo">Echo value.</param>
    private GateCommand(string text, int craftCount, WaitModifier wait, EchoModifier echo)
        : base(text, wait)
    {
        this.startingCrafts = this.craftsRemaining = craftCount;
        this.echoMod = echo;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static GateCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = EchoModifier.TryParse(ref text, out var echoModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var countGroup = match.Groups["count"];
        var countValue = countGroup.Success
            ? int.Parse(countGroup.Value, CultureInfo.InvariantCulture)
            : int.MaxValue;

        return new GateCommand(text, countValue, waitModifier, echoModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        if (this.echoMod.PerformEcho || Configuration.LoopEcho)
        {
            if (this.craftsRemaining == 0)
            {
                ChatManager.PrintChat("No crafts remaining");
            }
            else
            {
                var noun = this.craftsRemaining == 1 ? "craft" : "crafts";
                ChatManager.PrintChat($"{this.craftsRemaining} {noun} remaining");
            }
        }

        this.craftsRemaining--;

        await this.PerformWait(token);

        if (this.craftsRemaining < 0)
        {
            this.craftsRemaining = this.startingCrafts;
            throw new GateComplete();
        }
    }
}