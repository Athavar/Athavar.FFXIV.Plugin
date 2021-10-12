namespace Inviter
{
    using System;
    using System.Threading.Tasks;
    using Athavar.FFXIV.Plugin;
    using Dalamud.Game.Gui.Toast;

    internal class InviterTimed
    {
        internal InviterModule plugin;
        internal long runUntil = 0;
        internal long nextNotification = 0;
        internal bool isRunning = false;

        internal Localizer Localizer => this.plugin.Localizer;

        internal InviterTimed(InviterModule plugin)
        {
            this.plugin = plugin;
        }

        internal async Task Run()
        {
            this.isRunning = true;
            this.nextNotification = DateTimeOffset.Now.ToUnixTimeSeconds() + ((this.runUntil - DateTimeOffset.Now.ToUnixTimeSeconds()) / 2);
            try
            {
                this.plugin.InviterConfig.Enable = true;
                while (DateTimeOffset.Now.ToUnixTimeSeconds() < this.runUntil)
                {
                    await Task.Delay(1000);
                    if (!this.plugin.InviterConfig.Enable)
                    {
                        this.runUntil = 0;
                        break;
                    }

                    if(DateTimeOffset.Now.ToUnixTimeSeconds() >= this.nextNotification && DateTimeOffset.Now.ToUnixTimeSeconds() < this.runUntil)
                    {
                        DalamudBinding.ToastGui.ShowQuest(
                            string.Format(this.Localizer.Localize("Automatic recruitment enabled, {0} minutes left"),
                            Math.Ceiling((this.runUntil - DateTimeOffset.Now.ToUnixTimeSeconds()) / 60d)));

                        this.UpdateTimeNextNotification();
                    }
                }

                DalamudBinding.ToastGui.ShowQuest(this.Localizer.Localize("Automatic recruitment finished"), new QuestToastOptions()
                {
                    DisplayCheckmark = true,
                    PlaySound = true,
                });
                this.plugin.InviterConfig.Enable = false;
            }
            catch (Exception e)
            {
                DalamudBinding.ChatGui.Print("Error: " + e.Message + "\n" + e.StackTrace);
            }

            this.isRunning = false;
        }

        internal void UpdateTimeNextNotification()
        {
            this.nextNotification = DateTimeOffset.Now.ToUnixTimeSeconds() + Math.Max(60, (this.runUntil - DateTimeOffset.Now.ToUnixTimeSeconds()) / 2);
        }

        internal void ProcessCommandTimedEnable(int timeInMinutes)
        {
            if (this.plugin.InviterConfig.Enable && !this.isRunning)
            {
                DalamudBinding.ToastGui.ShowError(this.Localizer.Localize(
                        "Can't start timed recruitment because Inviter is turned on permanently"));
            }
            else
            {
                try
                {
                    var time = timeInMinutes;
                    if (time > 0)
                    {
                        this.runUntil = DateTimeOffset.Now.ToUnixTimeSeconds() + (time * 60);

                        if (this.isRunning)
                        {
                            this.UpdateTimeNextNotification();
                        }
                        else
                        {
                            Task.Run(this.Run);
                        }

                        DalamudBinding.ToastGui.ShowQuest(
                            string.Format(this.Localizer.Localize("Commenced automatic recruitment for {0} minutes"), time)
                            , new QuestToastOptions()
                            {
                                DisplayCheckmark = true,
                                PlaySound = true,
                            });
                    }
                    else if (time == 0)
                    {
                        if (this.isRunning)
                        {
                            this.runUntil = 0;
                        }
                        else
                        {
                            DalamudBinding.ToastGui.ShowError(this.Localizer.Localize("Recruitment is not running, can not cancel"));
                        }
                    }
                    else
                    {
                        DalamudBinding.ToastGui.ShowError(this.Localizer.Localize("Time can not be negative"));
                    }
                }
                catch (Exception)
                {
                    // plugin.Interface.Framework.Gui.Chat.Print("Error: " + e.Message + "\n" + e.StackTrace);
                    DalamudBinding.ToastGui.ShowError(this.Localizer.Localize("Please enter amount of time in minutes"));
                }
            }
        }
    }
}
