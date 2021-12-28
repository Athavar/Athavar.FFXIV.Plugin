namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;

using System.Text.RegularExpressions;

/// <summary>
///     The &lt;condition&gt; modifier.
/// </summary>
internal class ConditionModifier : MacroModifier
{
    private static readonly Regex Regex = new(@"(?<modifier><condition\.(?<not>(not\.|\!))?(?<name>[a-zA-Z]+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private ConditionModifier(string condition, bool negated) => (this.Condition, this.Negated) = (condition, negated);

    /// <summary>
    ///     Gets the required condition name.
    /// </summary>
    public string Condition { get; }

    /// <summary>
    /// Gets a value indicating whether the condition check should be negated.
    /// </summary>
    public bool Negated { get; }

    /// <summary>
    ///     Parse the text as a modifier.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="command">A parsed modifier.</param>
    /// <returns>A value indicating whether the modifier matched.</returns>
    public static bool TryParse(ref string text, out ConditionModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);

            var conditionName = match.Groups["name"].Value.ToLowerInvariant();
            var negated = match.Groups["not"].Success;

            command = new ConditionModifier(conditionName, negated);
            return true;
        }

        command = new ConditionModifier(string.Empty, false);
        return false;
    }
}