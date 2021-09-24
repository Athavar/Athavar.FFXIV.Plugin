using System;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin;
using Dalamud.Game.Gui.Toast;

namespace Inviter
{
    internal class InviterTimed
    {
        internal InviterModule plugin;
        internal long runUntil = 0;
        internal long nextNotification = 0;
        internal bool isRunning = false;
        internal Localizer Localizer => plugin.Localizer;

        internal InviterTimed(InviterModule plugin)
        {
            this.plugin = plugin;
        }

        internal async Task Run()
        {
            isRunning = true;
            nextNotification = DateTimeOffset.Now.ToUnixTimeSeconds() + (runUntil - DateTimeOffset.Now.ToUnixTimeSeconds())/2;
            try
            {
                plugin.InviterConfig.Enable = true;
                while (DateTimeOffset.Now.ToUnixTimeSeconds() < runUntil)
                {
                    await Task.Delay(1000);
                    if (!plugin.InviterConfig.Enable)
                    {
                        runUntil = 0;
                        break;
                    }

                    if(DateTimeOffset.Now.ToUnixTimeSeconds() >= nextNotification && DateTimeOffset.Now.ToUnixTimeSeconds() < runUntil)
                    {
                        DalamudBinding.ToastGui.ShowQuest(
                            String.Format(Localizer.Localize("Automatic recruitment enabled, {0} minutes left"),
                            Math.Ceiling((runUntil - DateTimeOffset.Now.ToUnixTimeSeconds()) / 60d))
                            );

                        UpdateTimeNextNotification();
                    }
                }
                DalamudBinding.ToastGui.ShowQuest(Localizer.Localize("Automatic recruitment finished"), new QuestToastOptions()
                {
                    DisplayCheckmark = true,
                    PlaySound = true
                });
                plugin.InviterConfig.Enable = false;
            }
            catch (Exception e)
            {
                DalamudBinding.ChatGui.Print("Error: " + e.Message + "\n" + e.StackTrace);
            }
            isRunning = false;
        }

        internal void UpdateTimeNextNotification()
        {
            nextNotification = DateTimeOffset.Now.ToUnixTimeSeconds() + Math.Max(60, (runUntil - DateTimeOffset.Now.ToUnixTimeSeconds()) / 2);
        }

        internal void ProcessCommandTimedEnable(int timeInMinutes)
        {
            if (plugin.InviterConfig.Enable && !isRunning)
            {
                DalamudBinding.ToastGui.ShowError(Localizer.Localize(
                        "Can't start timed recruitment because Inviter is turned on permanently"
                    ));
            }
            else
            {
                try
                {
                    var time = timeInMinutes;
                    if (time > 0)
                    {
                        runUntil = DateTimeOffset.Now.ToUnixTimeSeconds() + time * 60;
                        if (isRunning) 
                        {
                            UpdateTimeNextNotification();
                        }
                        else
                        {
                            Task.Run(Run);
                        }

                        DalamudBinding.ToastGui.ShowQuest(
                            String.Format(Localizer.Localize("Commenced automatic recruitment for {0} minutes"), time)
                            , new QuestToastOptions()
                            {
                                DisplayCheckmark = true,
                                PlaySound = true
                            });
                    }
                    else if (time == 0)
                    {
                        if (isRunning)
                        {
                            runUntil = 0;
                        }
                        else
                        {
                            DalamudBinding.ToastGui.ShowError(Localizer.Localize("Recruitment is not running, can not cancel"));
                        }
                    }
                    else
                    {
                        DalamudBinding.ToastGui.ShowError(Localizer.Localize("Time can not be negative"));
                    }
                }
                catch (Exception e)
                {
                    // plugin.Interface.Framework.Gui.Chat.Print("Error: " + e.Message + "\n" + e.StackTrace);
                    DalamudBinding.ToastGui.ShowError(Localizer.Localize("Please enter amount of time in minutes"));
                }
            }
        }
    }
}
