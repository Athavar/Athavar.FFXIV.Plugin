// <copyright file="BaseCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;

/// <summary>
///     Base implementation for a command.
/// </summary>
internal abstract class BaseCommand
{
    private static readonly Regex WaitModifier = new(@"(?<modifier>\s*<wait\.(?<time>\d+(?:\.\d+)?)(?:-(?<maxtime>\d+(?:\.\d+)?))?>\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex UnsafeModifier = new(@"(?<modifier>\s*<unsafe>\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MaxWaitModifier = new(@"(?<modifier>\s*<maxwait\.(?<time>\d+(?:\.\d+)?)>\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public BaseCommand(MacroManager macroManager) => this.Manager = macroManager;

    /// <summary>
    ///     Gets the command names of the command.
    /// </summary>
    public abstract IEnumerable<string> CommandAliases { get; }

    /// <summary>
    ///     Gets the <see cref="MacroManager" />.
    /// </summary>
    protected MacroManager Manager { get; } = null!;

    /// <summary>
    ///     Execute the command.
    /// </summary>
    /// <param name="step">The command line.</param>
    /// <param name="macro">The current active macro.</param>
    /// <param name="wait">The extracted waittime.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> that can be used to cancel the work.</param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation. The task result contains a
    ///     <see cref="TimeSpan" /> of the time that should be waited after the command.
    /// </returns>
    public abstract Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken);

    /// <summary>
    ///     Extract the waiting time from a command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>The extracted <see cref="TimeSpan" />.</returns>
    internal static TimeSpan ExtractWait(ref string command)
    {
        var match = WaitModifier.Match(command);

        var waitTime = TimeSpan.Zero;

        if (!match.Success)
        {
            return waitTime;
        }

        var modifier = match.Groups["modifier"].Value;
        command = command.Replace(modifier, " ").Trim();

        var waitMatch = match.Groups["time"];
        if (waitMatch.Success && double.TryParse(waitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
        {
            waitTime = TimeSpan.FromSeconds(seconds);
        }

        var maxWaitMatch = match.Groups["maxtime"];
        if (maxWaitMatch.Success && double.TryParse(maxWaitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxSeconds))
        {
            var rand = new Random();

            var maxWaitTime = TimeSpan.FromSeconds(maxSeconds);
            var diff = rand.Next((int)maxWaitTime.TotalMilliseconds - (int)waitTime.TotalMilliseconds);

            waitTime = TimeSpan.FromMilliseconds((int)waitTime.TotalMilliseconds + diff);
        }

        return waitTime;
    }

    /// <summary>
    /// </summary>
    /// <param name="waitInterval"></param>
    /// <param name="maxWait"></param>
    /// <param name="action"></param>
    /// <param name="token"></param>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    protected static async Task<bool> LinearWaitFor(int waitInterval, int maxWait, Func<bool> action, CancellationToken token)
    {
        var totalWait = 0;
        while (true)
        {
            if (action())
            {
                return true;
            }

            totalWait += waitInterval;
            if (totalWait > maxWait)
            {
                return false;
            }

            await Task.Delay(waitInterval, token);
        }
    }

    protected static bool ExtractUnsafe(ref string command)
    {
        var match = UnsafeModifier.Match(command);
        if (match.Success)
        {
            var modifier = match.Groups["modifier"].Value;
            command = command.Replace(modifier, " ").Trim();
            return true;
        }

        return false;
    }

    protected static TimeSpan ExtractMaxWait(ref string command, float defaultMillis)
    {
        var match = MaxWaitModifier.Match(command);
        if (match.Success)
        {
            var modifier = match.Groups["modifier"].Value;
            var waitTime = match.Groups["time"].Value;
            command = command.Replace(modifier, " ").Trim();
            if (double.TryParse(waitTime, NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }
        }

        return TimeSpan.FromMilliseconds(defaultMillis);
    }
}