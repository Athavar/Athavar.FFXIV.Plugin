namespace Athavar.FFXIV.Plugin.Config;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Config.Interfaces;

public class RegexWrapper : Regex, IRegex
{
    public RegexWrapper(string pattern, RegexOptions options)
        : base(pattern, options)
    {
    }
}