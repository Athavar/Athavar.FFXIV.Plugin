using Athavar.FFXIV.Plugin;
using ClickLib;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Events;

namespace SomethingNeedDoing
{
    internal enum LoopState
    {
        NotLoggedIn,
        Waiting,
        Running,
        Paused,
        Stopped,
    }

    internal class MacroManager : IDisposable
    {
        private readonly MacroModule plugin;
        private readonly CancellationTokenSource EventLoopTokenSource = new();
        private readonly List<ActiveMacro> RunningMacros = new();

        private readonly ManualResetEvent DataAvailableWaiter = new(false);
        private readonly List<string> CraftingActionNames = new();
        private CraftingData CraftingData = default;

        private delegate IntPtr EventFrameworkDelegate(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize);

        private readonly Hook<EventFrameworkDelegate> EventFrameworkHook;

        public LoopState LoopState { get; private set; } = LoopState.Waiting;

        private InvokeOptions invokeOptions = new();
        private bool IsPaused = true;
        private bool IsLoggedIn = false;

        public MacroManager(MacroModule plugin)
        {
            this.plugin = plugin;
            DalamudBinding.ClientState.Login += ClientState_OnLogin;
            DalamudBinding.ClientState.Logout += ClientState_OnLogout;

            Click.Initialize();
            invokeOptions.SendInput.BatchDelay = TimeSpan.FromMilliseconds(100);


            PopulateCraftingActionNames();

            if (DalamudBinding.ClientState.LocalPlayer != null)
                IsLoggedIn = true;

            EventFrameworkHook = new Hook<EventFrameworkDelegate>(plugin.Address.EventFrameworkFunctionAddress, EventFrameworkDetour);
            EventFrameworkHook.Enable();

            Task.Run(() => EventLoop(EventLoopTokenSource.Token));
        }

        public void Dispose()
        {
            DalamudBinding.ClientState.Login -= ClientState_OnLogin;
            DalamudBinding.ClientState.Logout -= ClientState_OnLogout;

            EventLoopTokenSource.Cancel();
            EventLoopTokenSource.Dispose();
            EventFrameworkHook.Dispose();
        }

        private void OnEventFrameworkDetour(IntPtr dataPtr, byte dataSize)
        {
            if (dataSize >= 4)
            {
                var dataType = (ActionCategory)Marshal.ReadInt32(dataPtr);
                if (dataType == ActionCategory.Action || dataType == ActionCategory.CraftAction)
                {
                    CraftingData = Marshal.PtrToStructure<CraftingData>(dataPtr);
                    DataAvailableWaiter.Set();
                }
            }
        }

        private IntPtr EventFrameworkDetour(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize)
        {
            try { OnEventFrameworkDetour(dataPtr, dataSize); }
            catch (Exception ex) { PluginLog.Error(ex, "Don't crash the game."); }

            return EventFrameworkHook.Original(a1, a2, a3, a4, a5, dataPtr, dataSize);
        }

        private void ClientState_OnLogin(object? sender, EventArgs e)
        {
            IsLoggedIn = true;
            LoopState = LoopState.Waiting;
        }

        private void ClientState_OnLogout(object? sender, EventArgs e)
        {
            IsLoggedIn = false;
            LoopState = LoopState.NotLoggedIn;
        }

        private async Task EventLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    while (!IsLoggedIn)
                    {
                        LoopState = LoopState.NotLoggedIn;
                        await Task.Delay(100, token);
                    }


                    while (IsPaused)
                    {
                        LoopState = RunningMacros.Count == 0 ? LoopState.Waiting : LoopState.Paused;
                        await Task.Delay(100, token);
                    }

                    var macro = RunningMacros.FirstOrDefault();
                    if (macro == default(ActiveMacro))
                    {
                        IsPaused = true;
                        continue;
                    }

                    LoopState = LoopState.Running;
                    if (await ProcessMacro(macro, token))
                    {
                        RunningMacros.Remove(macro);
                    }
                }
                catch (OperationCanceledException)
                {
                    PluginLog.Verbose("Event loop has stopped");
                    LoopState = LoopState.Stopped;
                    break;
                }
                catch (ObjectDisposedException)
                {
                    PluginLog.Verbose($"Event loop has stopped");
                    LoopState = LoopState.Stopped;
                    break;
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Unhandled exception occurred");
                    plugin.ChatManager.PrintError($"[SomethingNeedDoing] Peon has died unexpectedly.");
                    RunningMacros.Clear();
                }
            }
        }

        private async Task<bool> ProcessMacro(ActiveMacro macro, CancellationToken token)
        {
            var step = macro.GetCurrentStep();

            if (step == null)
                return true;

            var wait = ExtractWait(ref step);

            try
            {
                var command = step.ToLower().Split(' ').First();
                switch (command)
                {
                    case "/ac":
                    case "/action":
                        wait = await ProcessActionCommand(step, token, wait);
                        break;
                    case "/require":
                        await ProcessRequireCommand(step, token);
                        break;
                    case "/runmacro":
                        ProcessRunMacroCommand(step);
                        break;
                    case "/wait":
                        await ProcessWaitCommand(step, token);
                        break;
                    case "/waitaddon":
                        await ProcessWaitAddonCommand(step, token);
                        break;
                    case "/send":
                        await ProcessSendCommand(step);
                        break;
                    case "/target":
                        ProcessTargetCommand(step);
                        break;
                    case "/click":
                        ProcessClickCommand(step);
                        break;
                    case "/loop":
                        ProcessLoopCommand(step, macro);
                        break;
                    default:
                        plugin.ChatManager.SendChatBoxMessage(step);
                        break;
                };
            }
            catch (EffectNotPresentError ex)
            {
                plugin.ChatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
                IsPaused = true;
                return false;
            }
            catch (EventFrameworkTimeoutError ex)
            {
                plugin.ChatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
                IsPaused = true;
                return false;
            }
            catch (InvalidMacroOperationException ex)
            {
                plugin.ChatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
                IsPaused = true;
                return true;
            }

            if (wait.TotalSeconds > 0)
            {
                Task.Delay(wait, token).Wait(token);
            }

            macro.StepIndex++;

            return false;
        }

        private readonly Regex RUNMACRO_COMMAND = new(@"^/runmacro\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex ACTION_COMMAND = new(@"^/(ac|action)\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex WAIT_COMMAND = new(@"^/wait\s+(?<time>\d+(?:\.\d+)?)(?:-(?<maxtime>\d+(?:\.\d+)?))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex WAITADDON_COMMAND = new(@"^/waitaddon\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex REQUIRE_COMMAND = new(@"^/require\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex SEND_COMMAND = new(@"^/send\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex TARGET_COMMAND = new(@"^/target\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex CLICK_COMMAND = new(@"^/click\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex LOOP_COMMAND = new(@"^/loop(?: (?<count>\d+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex WAIT_MODIFIER = new(@"(?<modifier>\s*<wait\.(?<time>\d+(?:\.\d+)?)(?:-(?<maxtime>\d+(?:\.\d+)?))?>\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex UNSAFE_MODIFIER = new(@"(?<modifier>\s*<unsafe>\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex MAXWAIT_MODIFIER = new(@"(?<modifier>\s*<maxwait\.(?<time>\d+(?:\.\d+)?)>\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void ProcessRunMacroCommand(string step)
        {
            var match = RUNMACRO_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var macroName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' });
            var macroNode = plugin.Configuration.GetAllNodes().FirstOrDefault(macro => macro.Name == macroName) as MacroNode;
            if (macroNode == default(MacroNode))
                throw new InvalidMacroOperationException("Unknown macro");

            RunningMacros.Insert(0, new ActiveMacro(macroNode));
        }

        private async Task<TimeSpan> ProcessActionCommand(string step, CancellationToken token, TimeSpan wait)
        {
            var unsafeAction = ExtractUnsafe(ref step);

            var match = ACTION_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var actionName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();

            if (IsCraftingAction(actionName))
            {
                DataAvailableWaiter.Reset();

                plugin.ChatManager.SendChatBoxMessage(step);

                await Task.Delay(wait, token);
                wait = TimeSpan.Zero;

                if (!unsafeAction && !DataAvailableWaiter.WaitOne(5000))
                    throw new EventFrameworkTimeoutError("Did not receive a response from the game");
            }
            else
            {
                plugin.ChatManager.SendChatBoxMessage(step);
            }

            return wait;
        }

        private Task ProcessWaitCommand(string step, CancellationToken token)
        {
            var match = WAIT_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var waitTime = TimeSpan.Zero;
            var waitMatch = match.Groups["time"];
            if (waitMatch.Success && double.TryParse(waitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double seconds))
            {
                waitTime = TimeSpan.FromSeconds(seconds);
                //PluginLog.Debug($"Wait is {waitTime.TotalMilliseconds}ms");
            }

            var maxWaitMatch = match.Groups["maxtime"];
            if (maxWaitMatch.Success && double.TryParse(maxWaitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxSeconds))
            {
                var rand = new Random();

                var maxWaitTime = TimeSpan.FromSeconds(maxSeconds);
                var diff = rand.Next((int)maxWaitTime.TotalMilliseconds - (int)waitTime.TotalMilliseconds);

                waitTime = TimeSpan.FromMilliseconds((int)waitTime.TotalMilliseconds + diff);
                //PluginLog.Debug($"Wait (variable) is now {waitTime.TotalMilliseconds}ms");
            }

            return Task.Delay(waitTime, token);
        }

        private async Task ProcessWaitAddonCommand(string step, CancellationToken token)
        {
            var maxwait = ExtractMaxWait(ref step, 5000);

            var match = WAITADDON_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var addonPtr = IntPtr.Zero;
            var addonName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' });

            var isVisible = await LinearWaitFor(500, Convert.ToInt32(maxwait.TotalMilliseconds), () =>
            {
                addonPtr = DalamudBinding.GameGui.GetAddonByName(addonName, 1);
                if (addonPtr != IntPtr.Zero)
                {
                    unsafe
                    {
                        var addon = (AtkUnitBase*)addonPtr;
                        return addon->IsVisible && addon->UldManager.LoadedState == 3;
                    }
                }
                return false;
            }, token);

            if (addonPtr == IntPtr.Zero)
                throw new InvalidMacroOperationException("Could not find Addon");

            if (!isVisible)
                throw new InvalidMacroOperationException("Addon not visible");
        }

        private async Task ProcessRequireCommand(string step, CancellationToken token)
        {
            var maxwait = ExtractMaxWait(ref step, 1000);

            var match = REQUIRE_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var effectName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();

            var sheet = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>() ?? throw new NotSupportedException("Status Sheet not found.");
            var effectIDs = sheet.Where(row => row.Name.RawString.ToLower() == effectName).Select(row => row.RowId).ToList();

            var hasEffect = await LinearWaitFor(250, Convert.ToInt32(maxwait.TotalMilliseconds),
                () => DalamudBinding.ClientState.LocalPlayer?.StatusList.Select(se => se.StatusId).ToList().Intersect(effectIDs).Any() ?? false,
                token);

            if (!hasEffect)
                throw new EffectNotPresentError("Effect not present");
        }

        private async Task ProcessSendCommand(string step)
        {
            var match = SEND_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var vkNames = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower().Split(' ');

            var vkCodes = vkNames.Select(n => Enum.Parse<KeyCode>(n, true)).ToArray();
            if (vkCodes.Any(c => !Enum.IsDefined(c)))
            {
                throw new InvalidMacroOperationException($"Invalid virtual key");
            }
            else
            {
                for(int i = 0; i < vkCodes.Length; i++)
                {
                    await Simulate.Events().Hold(vkCodes).Invoke(invokeOptions);
                }
                await Task.Delay(15);

                for (int i = 0; i < vkCodes.Length; i++)
                {
                    await Simulate.Events().Release(vkCodes).Invoke(invokeOptions);
                }
                // var result = await Simulate.Events().Click(vkCodes).Invoke(invokeOptions);
            }
        }

        private void ProcessTargetCommand(string step)
        {
            var match = TARGET_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var actorName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();
            GameObject? npc = null;
            try
            {
                npc = DalamudBinding.ObjectTable.Where(actor => actor.Name.TextValue.ToLower() == actorName).First();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidMacroOperationException($"Unknown actor");
            }

            if (npc != null)
            {
                DalamudBinding.TargetManager.SetTarget(npc);
            }
        }

        private void ProcessClickCommand(string step)
        {
            var match = CLICK_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var name = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();

            try
            {
                Click.SendClick(name);
            }
            catch (InvalidClickException ex)
            {
                PluginLog.Error(ex, $"Error while performing {name} click");
                throw new InvalidMacroOperationException($"Click error");
            }
        }

        private void ProcessLoopCommand(string step, ActiveMacro macro)
        {
            var match = LOOP_COMMAND.Match(step);
            if (!match.Success)
                throw new InvalidMacroOperationException("Syntax error");

            var countMatch = match.Groups["count"];
            if (!countMatch.Success)
            {
                macro.StepIndex = -1;
            }
            else if (countMatch.Success && int.TryParse(countMatch.Value, out var loopMax) && macro.LoopCount < loopMax)
            {
                macro.StepIndex = -1;
                macro.LoopCount++;
            }
            else
            {
                macro.LoopCount = 0;
            }
        }

        private async Task<bool> LinearWaitFor(int waitInterval, int maxWait, Func<bool> action, CancellationToken token)
        {
            var totalWait = 0;
            while (true)
            {
                if (action())
                    return true;

                totalWait += waitInterval;
                if (totalWait > maxWait)
                    return false;

                await Task.Delay(waitInterval, token);
            }
        }

        private TimeSpan ExtractWait(ref string command)
        {
            var match = WAIT_MODIFIER.Match(command);

            var waitTime = TimeSpan.Zero;

            if (!match.Success)
                return waitTime;

            var modifier = match.Groups["modifier"].Value;
            command = command.Replace(modifier, " ").Trim();

            var waitMatch = match.Groups["time"];
            if (waitMatch.Success && double.TryParse(waitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double seconds))
            {
                waitTime = TimeSpan.FromSeconds(seconds);
            }

            var maxWaitMatch = match.Groups["maxtime"];
            if (maxWaitMatch.Success && double.TryParse(maxWaitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxSeconds))
            {
                var rand = new Random();

                var maxWaitTime = TimeSpan.FromSeconds(maxSeconds);
                var diff = rand.Next((int)maxWaitTime.TotalMilliseconds - (int)waitTime.TotalMilliseconds);

                waitTime = TimeSpan.FromMilliseconds((int)waitTime.TotalMilliseconds + diff);
            }

            return waitTime;
        }

        private bool ExtractUnsafe(ref string command)
        {
            var match = UNSAFE_MODIFIER.Match(command);
            if (match.Success)
            {
                var modifier = match.Groups["modifier"].Value;
                command = command.Replace(modifier, " ").Trim();
                return true;
            }
            return false;
        }

        private TimeSpan ExtractMaxWait(ref string command, float defaultMillis)
        {
            var match = MAXWAIT_MODIFIER.Match(command);
            if (match.Success)
            {
                var modifier = match.Groups["modifier"].Value;
                var waitTime = match.Groups["time"].Value;
                command = command.Replace(modifier, " ").Trim();
                if (double.TryParse(waitTime, NumberStyles.Any, CultureInfo.InvariantCulture, out double seconds))
                {
                    return TimeSpan.FromSeconds(seconds);
                }
            }
            return TimeSpan.FromMilliseconds(defaultMillis);
        }

        private void PopulateCraftingActionNames()
        {
            var actions = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>() ?? throw new NotSupportedException("Action Sheet not found.");
            foreach (var row in actions)
            {
                if (row is null)
                {
                    continue;
                }

                var job = row.ClassJob?.Value?.ClassJobCategory?.Value;
                if (job != null && (job.CRP || job.BSM || job.ARM || job.GSM || job.LTW || job.WVR || job.ALC || job.CUL))
                {
                    var name = row.Name.RawString.Trim(new char[] { ' ', '"', '\'' }).ToLower();
                    if (!CraftingActionNames.Contains(name))
                    {
                        CraftingActionNames.Add(name);
                    }
                }
            }
            var craftActions = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.CraftAction>() ?? throw new NotSupportedException("CraftAction Sheet not found.");
            foreach (var row in craftActions)
            {
                var name = row.Name.RawString.Trim(new char[] { ' ', '"', '\'' }).ToLower();
                if (name.Length > 0 && !CraftingActionNames.Contains(name))
                {
                    CraftingActionNames.Add(name);
                }
            }
        }

        private bool IsCraftingAction(string name) => CraftingActionNames.Contains(name.Trim(new char[] { ' ', '"', '\'' }).ToLower());

        #region public api

        public void RunMacro(MacroNode node)
        {
            RunningMacros.Add(new ActiveMacro(node));
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Clear()
        {
            RunningMacros.Clear();
        }

        public int MacroCount => RunningMacros.Count;

        public (string, int)[] MacroStatus => RunningMacros.Select(macro => (macro.Node.Name, macro.StepIndex + 1)).ToArray();

        public string[] CurrentMacroContent()
        {
            if (RunningMacros.Count == 0)
                return Array.Empty<string>();
            return (string[])RunningMacros.First().Steps.Clone();
        }

        public int CurrentMacroStep()
        {
            if (RunningMacros.Count == 0)
                return 0;
            return RunningMacros.First().StepIndex;
        }

        #endregion

        private class ActiveMacro
        {
            public MacroNode Node { get; private set; }

            public ActiveMacro? Parent { get; private set; }

            public ActiveMacro(MacroNode node) : this(node, null) { }

            public ActiveMacro(MacroNode node, ActiveMacro? parent)
            {
                Node = node;
                Parent = parent;
                Steps = node.Contents.Split(new[] { "\n", "\r", "\n\r" }, StringSplitOptions.RemoveEmptyEntries).Where(line => !line.StartsWith("#")).ToArray();
            }

            public string[] Steps { get; private set; }

            public int StepIndex { get; set; }

            public int LoopCount { get; set; }

            public string? GetCurrentStep()
            {
                if (StepIndex >= Steps.Length)
                    return null;

                return Steps[StepIndex];
            }
        }
    }
}
