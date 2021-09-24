using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Dalamud.Hooking;
using Dalamud.Logging;
using Athavar.FFXIV.Plugin;
using Dalamud.Game.Gui.Toast;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Dalamud.Game.Network;

namespace Inviter
{
    internal class InviterModule
    {
        public string Name => "Inviter";

        private InviterUi InvoterUi;

        public Localizer Localizer { get; private set; }

        public Configuration Config { get; private set; }
        public InviterConfiguration InviterConfig => Config.Inviter;

        private delegate IntPtr GetUIBaseDelegate();
        private delegate IntPtr GetUIModuleDelegate(IntPtr basePtr);
        private delegate char EasierProcessInviteDelegate(Int64 a1, Int64 a2, IntPtr name, Int16 world_id);
        private delegate char EasierProcessEurekaInviteDelegate(Int64 a1, Int64 a2);
        private delegate char EasierProcessCIDDelegate(Int64 a1, Int64 a2);
        private EasierProcessInviteDelegate _EasierProcessInvite;
        private EasierProcessEurekaInviteDelegate _EasierProcessEurekaInvite;
        private Hook<EasierProcessCIDDelegate> easierProcessCIDHook;
        private GetUIModuleDelegate GetUIModule;
        private delegate IntPtr GetMagicUIDelegate(IntPtr basePtr);
        private IntPtr getUIModulePtr;
        private IntPtr uiModulePtr;
        private IntPtr uiModule;
        private Int64 uiInvite;
        private IntPtr groupManagerAddress;
        private Dictionary<string, Int64> name2CID;
        internal InviterTimed timedRecruitment;
        internal Regex? invitePattern;

        public InviterModule(Configuration configuration, Localizer localizer)
        {
            this.Config = configuration;
            this.Localizer = localizer;
            this.InvoterUi = new(this);

            name2CID = new Dictionary<string, long> { };

            var easierProcessInvitePtr = DalamudBinding.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 3E 44 0F B7 83 ?? ?? ?? ??");
            var easierProcessEurekaInvitePtr = DalamudBinding.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 83 ?? ?? ?? ?? 48 85 C0 74 62");
            var easierProcessCIDPtr = DalamudBinding.SigScanner.ScanText("40 53 48 83 EC 20 48 8B DA 48 8D 0D ?? ?? ?? ?? 8B 52 08");
            getUIModulePtr = DalamudBinding.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 83 7F ?? 00 48 8B F0");
            uiModulePtr = DalamudBinding.SigScanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 48 8D 54 24 ?? 48 83 C1 10 E8 ?? ?? ?? ??");
            InitUi();
            groupManagerAddress = DalamudBinding.PartyList.GroupManagerAddress;
            PluginLog.Log("===== I N V I T E R =====");
            PluginLog.Log("Process Invite address {Address}", easierProcessInvitePtr);
            PluginLog.Log("Process CID address {Address}", easierProcessCIDPtr);
            PluginLog.Log("uiModule address {Address}", uiModule);
            PluginLog.Log("uiInvite address {Address}", uiInvite);

            _EasierProcessInvite = Marshal.GetDelegateForFunctionPointer<EasierProcessInviteDelegate>(easierProcessInvitePtr);
            _EasierProcessEurekaInvite = Marshal.GetDelegateForFunctionPointer<EasierProcessEurekaInviteDelegate>(easierProcessEurekaInvitePtr);

            easierProcessCIDHook = new Hook<EasierProcessCIDDelegate>(easierProcessCIDPtr, new EasierProcessCIDDelegate(EasierProcessCIDDetour));
            easierProcessCIDHook.Enable();

            DalamudBinding.CommandManager.AddHandler("/xinvite", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/xinvite <on/off> - turn the auto invite on/off.\n" +
                    "/xinvite <integer> - enable auto invite in minutes."
            });

            InvoterUi = new InviterUi(this);
            DalamudBinding.ChatGui.ChatMessage += OnChatMessage;
            timedRecruitment = new InviterTimed(this);
        }

        public void Dispose()
        {
            if (timedRecruitment.isRunning)
            {
                timedRecruitment.runUntil = 0;
                InviterConfig.Enable = false;
            }
            easierProcessCIDHook.Dispose();
            DalamudBinding.ChatGui.ChatMessage -= OnChatMessage;
            DalamudBinding.CommandManager.RemoveHandler("/xinvite");

            InvoterUi?.Dispose();
        }

        public void Draw()
        {
            this.InvoterUi.UiBuilder_OnBuildUi_ConfigTab();
        }

        public void CommandHandler(string command, string arguments)
        {
            var args = arguments.Trim().Replace("\"", string.Empty);

            if (args == "on")
            {
                InviterConfig.Enable = true;
                DalamudBinding.ToastGui.ShowQuest(
                        String.Format(Localizer.Localize("Auto invite is turned on for \"{0}\""), InviterConfig.TextPattern)
                    , new QuestToastOptions
                    {
                        DisplayCheckmark = true,
                        PlaySound = true
                    });

                Configuration.Save();
            }
            else if (args == "off")
            {
                InviterConfig.Enable = false;
                DalamudBinding.ToastGui.ShowQuest(Localizer.Localize("Auto invite is turned off"), new QuestToastOptions
                    {
                        DisplayCheckmark = true,
                        PlaySound = true
                    });

                Configuration.Save();
            }
            else if (int.TryParse(args, out int timeInMinutes))
            {
                timedRecruitment.ProcessCommandTimedEnable(timeInMinutes);
            }
        }

        private void InitUi()
        {
            GetUIModule = Marshal.GetDelegateForFunctionPointer<GetUIModuleDelegate>(getUIModulePtr);
            uiModule = GetUIModule(Marshal.ReadIntPtr(uiModulePtr));
            if (uiModule == IntPtr.Zero)
                throw new ApplicationException("uiModule was null");
            IntPtr step2 = Marshal.ReadIntPtr(uiModule) + 264;
            PluginLog.Log($"step2:0x{step2:X}");
            if (step2 == IntPtr.Zero)
                throw new ApplicationException("step2 was null");
            IntPtr step3 = Marshal.ReadIntPtr(step2);
            PluginLog.Log($"step3:0x{step3:X}");
            if (step3 == IntPtr.Zero)
                throw new ApplicationException("step3 was null");
            IntPtr step4 = Marshal.GetDelegateForFunctionPointer<GetMagicUIDelegate>(step3)(uiModule) + 6528;
            PluginLog.Log($"step4:0x{step4:X}");
            if (step4 == (IntPtr.Zero + 6528))
                throw new ApplicationException("step4 was null");
            uiInvite = Marshal.ReadInt64(step4);
            PluginLog.Log($"uiInvite:{uiInvite:X}");
        }

        public void Log(string message)
        {
            if (!InviterConfig.PrintMessage) return;
            var msg = $"[{Name}] {message}";
            PluginLog.Log(msg);
        }

        public void LogError(string message)
        {
            if (!InviterConfig.PrintError) return;
            var msg = $"[{Name}] {message}";
            PluginLog.LogError(msg);
        }

        public static IntPtr NativeUtf8FromString(string managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }

        public static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Enum.IsDefined(type))
                return;
            if (!InviterConfig.Enable ||
                InviterConfig.FilteredChannels is null || InviterConfig.FilteredChannels.IndexOf(type) != -1 ||
                InviterConfig.HiddenChatType is null || InviterConfig.HiddenChatType.IndexOf(type) != -1)
            {
                return;
            }

            Log($"OnChatMessage: Type:{type}, SenderId: {senderId}, Sender: {sender.TextValue}, Message: {message.TextValue}, TextPayload: {string.Join('|', message.Payloads)}");

            var pattern = InviterConfig.TextPattern;
            bool matched = false;
            if (!InviterConfig.RegexMatch)
            {
                matched = (message.TextValue.IndexOf(pattern) != -1);
            }
            else
            {
                var regex = invitePattern ??= new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                matched = regex.Matches(message.TextValue).Count > 0;
            }

            if (matched)
            {

                var senderPayload = sender.Payloads.Where(payload => payload is PlayerPayload).FirstOrDefault();
                if (senderPayload != default(Payload) && senderPayload is PlayerPayload playerPayload)
                {
                    var player = DalamudBinding.ClientState.LocalPlayer;
                    if (groupManagerAddress != IntPtr.Zero)
                    {
                        unsafe
                        {
                            GroupManager* groupManager = (GroupManager*)groupManagerAddress;
                            if (groupManager->MemberCount >= 8)
                            {
                                Log($"Full party, won't invite.");
                                if (timedRecruitment.isRunning)
                                {
                                    timedRecruitment.runUntil = 0;
                                }
                                return;
                            }
                            else
                            {
                                if (groupManager->MemberCount > 0)
                                {
                                    var partyMembers = (PartyMember*)groupManager->PartyMembers;
                                    var leader = partyMembers[groupManager->PartyLeaderIndex];
                                    string leaderName = StringFromNativeUtf8(new IntPtr(leader.Name));

                                    if (player?.Name.TextValue != leaderName &&  player?.HomeWorld.Id != leader.HomeWorld)
                                    {
                                        Log($"Not leader, won't invite. (Leader: {leaderName})");
                                        return;
                                    }
                                }
                                Log($"Party Count:{groupManager->MemberCount}");
                            }
                        }
                    }

                    if (DalamudBinding.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty])
                    {
                        Task.Run(() =>
                        {
                            ProcessEurekaInvite(playerPayload);
                        });
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            ProcessInvite(playerPayload);
                        });
                    }
                }
            }

        }

        public void ProcessInvite(PlayerPayload player)
        {
            int delay = Math.Max(100, InviterConfig.Delay);
            Task.Delay(delay).Wait();
            Log($"Invite:{player.PlayerName}@{player.World.Name}");
            var player_bytes = Encoding.UTF8.GetBytes(player.PlayerName);
            IntPtr mem1 = Marshal.AllocHGlobal(player_bytes.Length + 1);
            Marshal.Copy(player_bytes, 0, mem1, player_bytes.Length);
            Marshal.WriteByte(mem1, player_bytes.Length, 0);
            _EasierProcessInvite(uiInvite, 0, mem1, (short)player.World.RowId);
            Marshal.FreeHGlobal(mem1);
        }

        public void ProcessEurekaInvite(PlayerPayload player)
        {
            var delay = Math.Max(100, InviterConfig.Delay);
            string playerNameKey = $"{player.PlayerName}@{player.World.RowId}";
            long cid = long.MinValue;
            int count = (int)Math.Ceiling(delay / 50f);
            while (count > 0)
            {
                Task.Delay(50).Wait();
                if (name2CID.TryGetValue(playerNameKey, out cid))
                {
                    break;
                }
                count--;
            }

            if (cid == long.MinValue)
            {
                LogError($"Unable to get CID:{playerNameKey}");
                return;
            }

            Log($"Invite2:{player.PlayerName}@{player.World.Name}");
            // _EasierProcessEurekaInvite(uiInvite, cid);
        }

        public char EasierProcessCIDDetour(Int64 a1, Int64 a2)
        {
            var dataPtr = (IntPtr)a2;
            // Log($"CID hook a1:{a1}");
            // Log($"CID hook a2:{dataPtr}");
            if (InviterConfig.Enable && dataPtr != IntPtr.Zero)
            {
                var cid = Marshal.ReadInt64(dataPtr);
                var world_id = Marshal.ReadInt16(dataPtr, 12);
                var name = StringFromNativeUtf8(dataPtr + 16);
                Log($"{name}@{world_id}:{cid}");
                string playerNameKey = $"{name}@{world_id}";
                if (!name2CID.ContainsKey(playerNameKey))
                {
                    name2CID.Add(playerNameKey, cid);
                }
            }

            return easierProcessCIDHook.Original(a1, a2);
        }
    }
}
