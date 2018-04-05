using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
//cand ii da un input box daca detecteaza alte caractere (ab...z) sa ii dea return;
namespace ServerGTMP
{
    class inventorysystem : Script
    {
        public int invmaxweap = 5;
        public int invmaxmanc = 30;
        public int invmaxbull = 9999;
        public int invmaxbaut = 30;
        public int invmaxechp = 30;
        public int invmaxfolo = 30;
        public int invmaxarun = 50;

        public inventorysystem()
        {
            API.onClientEventTrigger += OnClientEvent;
        }

        public void OnClientEvent(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "Inventory_arme_equip") 
            {
                string numearma = arguments[0].ToString();
                WeaponHash[] playerweaps = API.getPlayerWeapons(player);
                bool aredeja = false;
                for (int i = 0; i < playerweaps.Length; i++)
                {
                    if(numearma == playerweaps[i].ToString())
                    {
                        aredeja = true;
                        break;
                    }
                }
                if(aredeja == true)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Ai deja o arma de acest tip in mana.");
                } else
                {
                    for (int i = 0; i < player.getData("inventorysize"); i++)
                    {
                        int ca = i + 1;
                        if(player.getData("slot" + ca.ToString()) == numearma)
                        {
                            API.sendNotificationToPlayer(player, "Ai scos 1 " + numearma);
                            API.givePlayerWeapon(player, API.weaponNameToModel(numearma), 0, true, true);
                            player.setData("slot" + ca.ToString() + "cont", player.getData("slot" + ca.ToString() + "cont") - 1);

                            if (player.getData("slot" + ca.ToString() + "cont") > 0)
                                gamemode.PlayerUpdate(player, "pInvetory");

                            if (player.getData("slot" + ca.ToString() + "cont") < 1)
                            {
                                player.setData("slot" + ca.ToString(), "");
                                gamemode.PlayerUpdate(player, "pInvetory");
                            }
                        }
                    }
                }
            }

            if (eventName == "Inventory_arme_equip_aruncabila")
            {
                string numearma = arguments[0].ToString();
                bool maimare = false;
                int descos = Int32.Parse(arguments[2].ToString());
                WeaponHash[] playerweaps = API.getPlayerWeapons(player);
                bool aredeja = false;
                for (int i = 0; i < playerweaps.Length; i++)
                {
                    if (numearma == playerweaps[i].ToString())
                    {
                        aredeja = true;
                        break;
                    }
                }
                for (int i = 0; i < player.getData("inventorysize"); i++)
                {
                    int ca = i + 1;
                    if (player.getData("slot" + ca.ToString()) == numearma)
                    {
                        if (descos > player.getData("slot" + ca.ToString() + "cont"))
                        {
                            maimare = true;
                            break;
                        }
                        else { maimare = false; }
                    }

                }
                if (maimare == true)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Nu dispui de atatea iteme.");
                    return;
                }
                for (int i = 0; i < player.getData("inventorysize"); i++)
                {
                    int ca = i + 1;
                    if (player.getData("slot" + ca.ToString()) == numearma)
                    {
                        API.sendNotificationToPlayer(player, "Ai scos " + descos + " de " + numearma);
                        if (aredeja == false)
                            API.givePlayerWeapon(player, API.weaponNameToModel(numearma), descos, true, true);
                        else
                            API.setPlayerWeaponAmmo(player, API.weaponNameToModel(numearma), API.getPlayerWeaponAmmo(player, API.weaponNameToModel(numearma)) + descos);
                        player.setData("slot" + ca.ToString() + "cont", player.getData("slot" + ca.ToString() + "cont") - descos);

                        if (player.getData("slot" + ca.ToString() + "cont") > 0)
                            gamemode.PlayerUpdate(player, "pInvetory");

                        if (player.getData("slot" + ca.ToString() + "cont") < 1)
                        {
                            player.setData("slot" + ca.ToString(), "");
                            gamemode.PlayerUpdate(player, "pInvetory");
                        }
                    }
                }
            }
            if (eventName == "Inventory_arme_drop")
            {
                string numearma = arguments[0].ToString();
                if(arguments[1].ToString().Length > 0)
                {
                    int cantitate = Int32.Parse(arguments[1].ToString());
                    int itemdisp = Int32.Parse(arguments[2].ToString());
                    if (cantitate > itemdisp)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Info]Nu dispui de atatea iteme.");
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < player.getData("inventorysize"); i++)
                        {
                            int ca = i - 1;
                            if (player.getData("slot" + ca.ToString()) == numearma)
                            {
                                API.sendNotificationToPlayer(player, "Ai aruncat " + cantitate + " " + numearma);
                                player.setData("slot" + ca.ToString() + "cont", player.getData("slot" + ca.ToString() + "cont") - cantitate);

                                if (player.getData("slot" + ca.ToString() + "cont") > 0)
                                    gamemode.PlayerUpdate(player, "pInvetory");

                                if (player.getData("slot" + ca.ToString() + "cont") < 1)
                                {
                                    player.setData("slot" + ca.ToString(), "");
                                    gamemode.PlayerUpdate(player, "pInvetory");
                                }
                            }
                        }
                    }
                } else { API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta."); }
            }
            if (eventName == "inventory_transferitem")
            {
                if (arguments[2].ToString().Length > 0)
                {
                    string numeitem = arguments[0].ToString();
                    int cantitate = Int32.Parse(arguments[1].ToString());
                    int iplayer = -1;
                    int cantitatedetrimis = Int32.Parse(arguments[2].ToString());

                    List<Client> playerList = API.getPlayersInRadiusOfPlayer(2, player);
                    if (cantitate >= cantitatedetrimis)
                    {
                        if (playerList.Count > 1)
                        {
                            for (int i = 0; i < playerList.Count; i++)
                            {
                                if (playerList[i].name != player.name)
                                {
                                    iplayer = i;
                                    break;
                                }
                            }
                            if (playerList[iplayer].name.Length > 0)
                            {
                                API.triggerClientEvent(player, "inventory_transferitem", playerList[iplayer].name, cantitatedetrimis, numeitem);
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "~r~[Info]Nu este niciun jucator in preajma.");
                        }
                    }
                    else { API.sendChatMessageToPlayer(player, "~r~[Info]Nu dispui de atatea iteme."); }
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        API.sendChatMessageToAll(playerList[i].name);
                    }
                }
                else { API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta."); }
            }
            if (eventName == "inventory_transferitem_accept")
            {
                string numejucator = arguments[0].ToString();
                int cantitate = Int32.Parse(arguments[1].ToString());
                int iplayer = -1;
                string numeitem = arguments[2].ToString();
                string tipitem = arguments[3].ToString();
                bool maieste = false;
                int currentmaxsize = 0;
                bool aredeja = false;
                bool nuare = false;
                API.sendChatMessageToAll("ds");
                List<Client> playerlist = API.getPlayersInRadiusOfPlayer(2, player);
                for (int i = 0; i < playerlist.Count; i++)
                {
                    if (playerlist[i].name != player.name)
                    {
                        iplayer = i;
                        break;
                    }
                }
                for (int i = 0; i < playerlist.Count; i++)
                {
                    if(playerlist[i].name == numejucator)
                    {
                        maieste = true;
                        break;
                    }
                }
                if(tipitem == "arma")
                {
                    currentmaxsize = invmaxweap;
                } else if(tipitem == "aruncabila")
                {
                    currentmaxsize = invmaxarun;
                } else if(tipitem == "mancare")
                {
                    currentmaxsize = invmaxmanc;
                } else if(tipitem == "bautura")
                {
                    currentmaxsize = invmaxbaut;
                } else if(tipitem == "echipament")
                {
                    currentmaxsize = invmaxechp;
                } else if(tipitem == "folosit")
                {
                    currentmaxsize = invmaxfolo;
                }else if(tipitem == "gloante")
                {
                    currentmaxsize = invmaxbull;
                }

                if(maieste == true)
                {
                    Client targetpl = API.getPlayerFromName(numejucator);
                    for (int i = 0; i < targetpl.getData("inventorysize"); i++)
                    {
                        int cal = i + 1;
                        if(numeitem == targetpl.getData("slot" + cal.ToString()))
                        {
                            aredeja = true;
                            if(targetpl.getData("slot" + cal.ToString() + "cont") >= currentmaxsize)
                            {
                                aredeja = false;
                                nuare = true;
                                break;
                            }
                        }
                        else
                        {
                            nuare = true;
                        }
                    }

                    if (nuare == true)
                    {
                        bool areloc = false;
                        string slot = "";
                        string slotsender = "";
                        for (int i = 0; i < targetpl.getData("inventorysize"); i++)
                        {
                            int ca = i + 1;
                            if (targetpl.getData("slot" + ca.ToString()) == "")
                            {
                                areloc = true;
                                slot = "slot" + ca.ToString();
                                break;
                            }
                        }
                        if (areloc == true)
                        {
                            targetpl.setData(slot, numeitem);
                            targetpl.setData(slot + "cont", cantitate);
                            API.sendNotificationToPlayer(targetpl, "Ai primit " + cantitate.ToString() + " " + numeitem + " de la " + player.name);
                            API.sendNotificationToPlayer(player, "I-ai trimis " + cantitate.ToString() + " " + numeitem + " lui " + targetpl.name);
                            for (int i = 0; i < player.getData("inventorysize"); i++)
                            {
                                int ca = i + 1;
                                if (player.getData("slot" + ca.ToString()) == numeitem && player.getData("slot" + ca.ToString() + "cont") >= cantitate)
                                {
                                    slotsender = "slot" + ca.ToString();
                                    break;
                                }
                            }
                            player.setData(slotsender + "cont", player.getData(slotsender + "cont") - cantitate);
                            if(player.getData(slotsender + "cont") <= 0)
                            {
                                player.setData(slotsender, "");
                            }

                            gamemode.PlayerUpdate(player, "pInvetory");
                            gamemode.PlayerUpdate(targetpl, "pInvetory");
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "~r~[Info]Jucatorul nu are loc in ghiozdan.");
                        }
                    }

                    if (aredeja == true)
                    {

                        string slotsender = "";
                        string[] sdisponibile = new string[targetpl.getData("inventorysize")];
                        bool pregatat = false;
                        int remainitems = cantitate;
                        sus:
                        for (int i = 0; i < targetpl.getData("inventorysize"); i++)
                        {
                            int ca = i + 1;
                            if(targetpl.getData("slot" + ca.ToString()) == numeitem && targetpl.getData("slot" + ca.ToString() + "cont") > 0)
                            {
                                sdisponibile[i] = "slot" + ca.ToString();
                            }
                        }
                        int rezultat = 0;

                        for (int i = 0; i < sdisponibile.Length; i++)
                        {
                            if(sdisponibile[i] != null)
                            {
                                for (int z = 0; z < sdisponibile.Length; z++)
                                {
                                    if(sdisponibile[i] != null)
                                    {
                                        int diff = currentmaxsize - targetpl.getData(sdisponibile[i]);
                                        rezultat = rezultat + diff;
                                    }
                                }
                            }
                        }
                        if(rezultat >= cantitate)
                        {
                            for (int i = 0; i < remainitems; i++)
                            {
                                for (int z = 0; z < sdisponibile.Length; z++)
                                {
                                    targetpl.setData(sdisponibile[i], targetpl.getData(sdisponibile[i] + 1));
                                    remainitems -= 1;
                                    if(remainitems < 1)
                                    {
                                        pregatat = true;
                                        break;
                                    }
                                    goto sus;
                                }
                            }
                        } else {
                            for (int x = 0; x < targetpl.getData("inventorysize"); x++)
                            {
                                int ca = x + 1;
                                if (targetpl.getData("slot" + ca.ToString()) == "")
                                {
                                    bool areloc = false;
                                    string slot = "";
                                    string slotsenderr = "";
                                    for (int i = 0; i < targetpl.getData("inventorysize"); i++)
                                    {
                                        int cas = i + 1;
                                        if (targetpl.getData("slot" + ca.ToString()) == "")
                                        {
                                            areloc = true;
                                            slot = "slot" + ca.ToString();
                                            break;
                                        }
                                    }
                                    if (areloc == true)
                                    {
                                        targetpl.setData(slot, numeitem);
                                        targetpl.setData(slot + "cont", cantitate);
                                        API.sendNotificationToPlayer(targetpl, "Ai primit " + cantitate.ToString() + " " + numeitem + " de la " + player.name);
                                        API.sendNotificationToPlayer(player, "I-ai trimis " + cantitate.ToString() + " " + numeitem + " lui " + targetpl.name);
                                        for (int i = 0; i < player.getData("inventorysize"); i++)
                                        {
                                            int cas = i + 1;
                                            if (player.getData("slot" + cas.ToString()) == numeitem && player.getData("slot" + cas.ToString() + "cont") >= cantitate)
                                            {
                                                slotsenderr = "slot" + cas.ToString();
                                                break;
                                            }
                                        }
                                        player.setData(slotsenderr + "cont", player.getData(slotsenderr + "cont") - cantitate);
                                        if (player.getData(slotsenderr + "cont") <= 0)
                                        {
                                            player.setData(slotsenderr, "");
                                        }

                                        gamemode.PlayerUpdate(player, "pInvetory");
                                        gamemode.PlayerUpdate(targetpl, "pInvetory");
                                        break;
                                    }
                                } else { API.sendChatMessageToPlayer(player, "~r~[Info]Jucatorul nu are loc in ghiozdan."); }
                            }
                        }

                        if(pregatat == true)
                        {
                            API.sendNotificationToPlayer(targetpl, "Ai primit " + cantitate.ToString() + " " + numeitem + " de la " + player.name);
                            API.sendNotificationToPlayer(player, "I-ai trimis " + cantitate.ToString() + " " + numeitem + " lui " + targetpl.name);
                            for (int i = 0; i < player.getData("inventorysize"); i++)
                            {
                                int ca = i + 1;
                                if (player.getData("slot" + ca.ToString()) == numeitem && player.getData("slot" + ca.ToString() + "cont") >= cantitate)
                                {
                                    slotsender = "slot" + ca.ToString();
                                    break;
                                }
                            }
                            player.setData(slotsender + "cont", player.getData(slotsender + "cont") - cantitate);
                            if (player.getData(slotsender + "cont") <= 0)
                            {
                                player.setData(slotsender, "");
                            }
                        }
                    }
                }
                else { API.sendChatMessageToPlayer(player, "~r~[Info]Jucatorul nu mai este in preajma."); }
            }
        }
    }

}
