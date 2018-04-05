using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;


namespace ServerGTMP
{
    class misc:Script
    {
        public bool isLetterInArray(string search, char[] input)
        {
            search = search.ToLower();

            foreach (char character in input)
            {
                if (search == character.ToString())
                {
                    return true;
                }
            }
            return false;
        }
        public bool isAllInt(string search, char[] input)
        {
            search = search.ToLower();

            foreach (char character in input)
            {
                if (search == character.ToString())
                {
                    return true;
                }
            }
            return false;
        }
        public bool AntiSQLI(string search, char[] input)
        {
            search = search.ToLower();

            foreach (char character in input)
            {
                if (search == character.ToString())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
