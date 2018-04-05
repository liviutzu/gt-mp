using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;

namespace TimeScript
{
    public class DayNight : Script
    {
        string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thrusday", "Friday", "Saturday", "Sunday" };
        Thread timeThread = null;

        public DayNight()
        {
            API.onResourceStart += DayNightInit;
            API.onResourceStop += DayNightExit;
        }

        public void DayNightInit()
        {
            API.setWorldSyncedData("DAYNIGHT_DAY", 0); // aka dayNames[0] - Monday
            API.setWorldSyncedData("DAYNIGHT_DAY_STRING", dayNames[API.getWorldSyncedData("DAYNIGHT_DAY")]);
            API.setWorldSyncedData("DAYNIGHT_HOUR", 12);
            API.setWorldSyncedData("DAYNIGHT_MINUTE", 0);
            API.setWorldSyncedData("DAYNIGHT_RENDER_ICON", true); // set to false if you don't want an icon based on hour. (will be a player option in the future)
            DayNightPrepareText();

            foreach (var player in API.getAllPlayers()) API.freezePlayerTime(player, true);

            timeThread = new Thread(UpdateTime);
            timeThread.Start();
        }

        public void DayNightExit()
        {
            if (timeThread != null) timeThread.Abort();
            timeThread = null;
        }

        public void DayNightPrepareText()
        {
            API.setWorldSyncedData("DAYNIGHT_TEXT", API.getWorldSyncedData("DAYNIGHT_HOUR").ToString("D2") + ":" + API.getWorldSyncedData("DAYNIGHT_MINUTE").ToString("D2"));
        }

        public void UpdateTime()
        {
            while (true)
            {
                API.setWorldSyncedData("DAYNIGHT_MINUTE", API.getWorldSyncedData("DAYNIGHT_MINUTE") + 1);

                if (API.getWorldSyncedData("DAYNIGHT_MINUTE") == 60)
                {
                    API.setWorldSyncedData("DAYNIGHT_MINUTE", 0);
                    API.setWorldSyncedData("DAYNIGHT_HOUR", API.getWorldSyncedData("DAYNIGHT_HOUR") + 1);

                    if (API.getWorldSyncedData("DAYNIGHT_HOUR") == 24)
                    {
                        API.setWorldSyncedData("DAYNIGHT_HOUR", 0);

                        API.setWorldSyncedData("DAYNIGHT_DAY", API.getWorldSyncedData("DAYNIGHT_DAY") + 1);
                        if (API.getWorldSyncedData("DAYNIGHT_DAY") == dayNames.Length) API.setWorldSyncedData("DAYNIGHT_DAY", 0);
                        API.setWorldSyncedData("DAYNIGHT_DAY_STRING", dayNames[API.getWorldSyncedData("DAYNIGHT_DAY")]);
                    }
                }

                API.setTime(API.getWorldSyncedData("DAYNIGHT_HOUR"), API.getWorldSyncedData("DAYNIGHT_MINUTE"));
                DayNightPrepareText();
                Thread.Sleep(1000);
            }
        }

        [Command("setartempo")]
        public void CMDWorldTime(Client sender, int hour, int minute)
        {
            if (API.getPlayerAclGroup(sender) != "Admin")
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You don't have permission to do this command.");
                return;
            }

            if (hour < 0 || hour > 23)
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Invalid hour.");
                return;
            }

            if (minute < 0 || minute > 59)
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Invalid minute.");
                return;
            }

            API.setWorldSyncedData("DAYNIGHT_HOUR", hour);
            API.setWorldSyncedData("DAYNIGHT_MINUTE", minute);
            API.setTime(API.getWorldSyncedData("DAYNIGHT_HOUR"), API.getWorldSyncedData("DAYNIGHT_MINUTE"));
            DayNightPrepareText();
        }

        [Command("setardia")]
        public void CMDWorldDay(Client sender, int day)
        {
            if (API.getPlayerAclGroup(sender) != "Admin")
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You don't have permission to do this command.");
                return;
            }

            if (day < 1 || day > dayNames.Length)
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Invalid day.");
                return;
            }

            API.setWorldSyncedData("DAYNIGHT_DAY", day - 1);
            API.setWorldSyncedData("DAYNIGHT_DAY_STRING", dayNames[API.getWorldSyncedData("DAYNIGHT_DAY")]);
        }

        [Command("trocariconedetempo")]
        public void CMDToggleIcon(Client sender)
        {
            if (API.getPlayerAclGroup(sender) != "Admin")
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROr: ~w~You don't have permission to do this command.");
                return;
            }

            API.setWorldSyncedData("DAYNIGHT_RENDER_ICON", !API.getWorldSyncedData("DAYNIGHT_RENDER_ICON"));
        }
    }
}
