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
        public int invmaxbull = 9999999;
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
            if (eventName == "Inventory_munitie_equip")
            {
                string numeglont = arguments[0].ToString();
                if (isValidInt(arguments[1].ToString()) == false)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta!");
                    return;
                }
                int cantitate = Int32.Parse(arguments[1].ToString());
                inventory_echipare_munitie(player, numeglont, cantitate);
            }

            if (eventName == "Inventory_arme_equip") 
            {
                string numearma = arguments[0].ToString();
                inventory_echipare(player, numearma);
            }

            if (eventName == "Inventory_arme_equip_aruncabila")
            {
                if (isValidInt(arguments[1].ToString()) == false)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta!");
                    return;
                }
                string numearma = arguments[0].ToString();
                int descos = Int32.Parse(arguments[1].ToString());
                inventory_echipare_aruncabila(player, numearma, descos);

            }
            if (eventName == "Inventory_arme_drop")
            {
                string numearma = arguments[0].ToString();
                if(arguments[1].ToString().Length > 0)
                {
                    if (isValidInt(arguments[1].ToString()) == false)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta!");
                        return;
                    }
                    int cantitate = Int32.Parse(arguments[1].ToString());
                    int itemdisp = Int32.Parse(arguments[2].ToString());
                    inventory_aruncare_arme(player, numearma, cantitate, itemdisp);
                    
                } else { API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta."); }
            }
            if (eventName == "inventory_transferitem")
            {
                if (arguments[2].ToString().Length > 0)
                {
                    if (isValidInt(arguments[1].ToString()) == false)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta!");
                        return;
                    }
                    string numeitem = arguments[0].ToString();
                    int cantitate = Int32.Parse(arguments[1].ToString());
                    int cantitatedetrimis = Int32.Parse(arguments[2].ToString());
                    string tipitem = arguments[3].ToString();
                    inventory_transfer_iteme(player, numeitem, cantitate, cantitatedetrimis, tipitem);
                }
                else { API.sendChatMessageToPlayer(player, "~r~[Info]Introdu o suma corecta."); }
            }
            if (eventName == "inventory_transferitem_accept")
            {
                //ii adauga inca un minigun gen: are x 1 mg si trimite lui y 1 mg y primeste mg are doua dar in sloturi diferite

                API.sendChatMessageToAll("ds");
                string numejucator = arguments[0].ToString();
                int cantitate = Int32.Parse(arguments[1].ToString());
                string numeitem = arguments[2].ToString();
                string tipitem = arguments[3].ToString();
                Transfer_Items_Accept(player, numejucator, cantitate, numeitem, tipitem);
            }
        }

        private void Transfer_Items_Accept(Client player, string numejucator, int cantitate, string numeitem, string tipitem)
        {
            int iplayer = -1;
            bool mai_este_jucatorul_inzona = false;
            int currentmaxsize = 0;
            bool aredeja = false;
            bool nuare = false;
            API.sendChatMessageToAll("ds");
            List<Client> playerlist = API.getPlayersInRadiusOfPlayer(2, player);
            player.sendChatMessage("linia 81");
            for (int i = 0; i < playerlist.Count; i++)
            {
                if (playerlist[i].name != player.name)
                {
                    iplayer = i;
                    break;
                }
            }
            player.sendChatMessage("linia 90");
            for (int i = 0; i < playerlist.Count; i++)
            {
                if (playerlist[i].name == numejucator)
                {
                    mai_este_jucatorul_inzona = true;
                    break;
                }
            }
            if (tipitem == "arma")
            {
                currentmaxsize = invmaxweap;
            }
            else if (tipitem == "aruncabila")
            {
                currentmaxsize = invmaxarun;
            }
            else if (tipitem == "mancare")
            {
                currentmaxsize = invmaxmanc;
            }
            else if (tipitem == "bautura")
            {
                currentmaxsize = invmaxbaut;
            }
            else if (tipitem == "echipament")
            {
                currentmaxsize = invmaxechp;
            }
            else if (tipitem == "folosit")
            {
                currentmaxsize = invmaxfolo;
            }
            else if (tipitem == "gloante")
            {
                currentmaxsize = invmaxbull;
            }

            if (mai_este_jucatorul_inzona == true)
            {
                Client targetpl = API.getPlayerFromName(numejucator); //se transforma numele jucatorului target in client
                for (int i = 0; i < targetpl.getData("inventorysize"); i++) //se trece prin toate sloturile inventarului
                {
                    int cal = i + 1;
                    if (numeitem == targetpl.getData("slot" + cal.ToString())) //daca numele itemului este egal cu slotul 1-30
                    {

                        if (targetpl.getData("slot" + cal.ToString() + "cont") >= currentmaxsize) // daca are mai mult de cate iteme sunt acceptate in inv
                        {
                            aredeja = false;
                            nuare = true;
                            break;
                        }
                        else
                        {
                            nuare = false;
                            aredeja = true;
                        }
                    }
                    else
                    {
                        nuare = true;
                    }
                }
                player.sendChatMessage("linia 145");
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
                        if (player.getData(slotsender + "cont") <= 0)
                        {
                            player.setData(slotsender, "");
                        }

                        gamemode.PlayerUpdate(player, "pInventory");
                        gamemode.PlayerUpdate(targetpl, "pInventory");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Info]Jucatorul nu are loc in ghiozdan.");
                    }
                }
                player.sendChatMessage("incepe 194");
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
                        if (targetpl.getData("slot" + ca.ToString()) == numeitem && targetpl.getData("slot" + ca.ToString() + "cont") > 0)
                        {
                            sdisponibile[i] = "slot" + ca.ToString();
                        }
                    }
                    int rezultat = 0;

                    for (int i = 0; i < sdisponibile.Length; i++)
                    {
                        if (sdisponibile[i] != null)
                        {
                            for (int z = 0; z < sdisponibile.Length; z++)
                            {
                                if (sdisponibile[i] != null)
                                {
                                    int diff = currentmaxsize - targetpl.getData(sdisponibile[i]);
                                    rezultat = rezultat + diff;
                                }
                            }
                        }
                    }
                    player.sendChatMessage("linia 223");
                    if (rezultat >= cantitate)
                    {
                        for (int i = 0; i < remainitems; i++)
                        {
                            for (int z = 0; z < sdisponibile.Length;)
                            {
                                targetpl.setData(sdisponibile[i], targetpl.getData(sdisponibile[i] + 1));
                                remainitems -= 1;
                                if (remainitems < 1)
                                {
                                    pregatat = true;
                                    break;
                                }
                                goto sus;
                            }
                        }
                    }
                    else
                    {
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

                                    gamemode.PlayerUpdate(player, "pInventory");
                                    gamemode.PlayerUpdate(targetpl, "pInventory");
                                    break;
                                }
                            }
                            else { API.sendChatMessageToPlayer(player, "~r~[Info]Jucatorul nu are loc in ghiozdan."); }
                        }
                    }
                    player.sendChatMessage("linia 287");
                    if (pregatat == true)
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
        private void inventory_echipare_munitie(Client player, string numeglont, int cantitate)
        {
            string[] separators = { " " }; //separatorul
            string[] separator = numeglont.Split(separators, StringSplitOptions.RemoveEmptyEntries); //se face separarea dintre ex. munitie si nume gen munitie rpg
            bool nu_are_destule_iteme = false;
            string slot_cuarma = "";
            string slot_cuarmacont = "";
            //se face o lista cu toate armele jucatorului
            WeaponHash[] playerweaps = API.getPlayerWeapons(player);
            WeaponHash weapon = API.weaponNameToModel(separator[1]); //se defineste numele arme ide la care face parte munitia
            bool inventory_arearma = false; // defineste variabila tip bool
            for (int i = 0; i < playerweaps.Length; i++) // se trece prin toate armele
            {
                if (weapon.ToString() == playerweaps[i].ToString()) // se verifica daca are deja in mana
                {
                    inventory_arearma = true; // se seteaza variabila true, adica are deja in mana
                    break;
                }
            }
            if (inventory_arearma == true) // daca are în mână să poată scoate munitia
            {
                for (int i = 0; i < player.getData("inventorysize"); i++) //se trece prin tot inventarul
                {
                    int ca = i + 1; //se face calculul
                    if (player.getData("slot" + ca.ToString()) == numeglont) //se verifica care slot are arma
                    {
                        if (cantitate > player.getData("slot" + ca.ToString() + "cont")) //se verifica care slot are atatea iteme cat trebe
                        {
                            nu_are_destule_iteme = true; //daca nu are destule sa se seteze true
                        }
                        else
                        {
                            nu_are_destule_iteme = false;// daca are itemele cerute sa fie false
                            slot_cuarma = "slot" + ca.ToString(); //se seteaza slotul care are arma
                            slot_cuarmacont = "slot" + ca.ToString() + "cont"; //se seteaza slotul cu continutul
                            break;
                        }
                    }

                }
                if (nu_are_destule_iteme == false)
                {

                    API.sendNotificationToPlayer(player, "Ai scos " + cantitate.ToString() + " de " + numeglont); //trimite notificare

                    API.setPlayerWeaponAmmo(player, weapon, cantitate);

                    player.setData(slot_cuarmacont, player.getData(slot_cuarmacont) - cantitate); //ii seteaza data la jucator -1 la continut

                    if (player.getData(slot_cuarmacont) > 0) //daca mai are cv in slot
                        gamemode.PlayerUpdate(player, "pInventory"); //sa ii dea update in db

                    if (player.getData(slot_cuarmacont) < 1) // daca nu mai are in slot
                    {
                        player.setData(slot_cuarma, ""); //sa ii scoata itemul din inv
                        gamemode.PlayerUpdate(player, "pInventory"); //update in inv

                    }
                }
                else
                {
                    player.sendChatMessage("~r~[Info]Nu dispui de atatea iteme.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "~r~[Info]Nu ai acest tip de arma in mana."); //eroare in chat
            }
        }
        private void inventory_transfer_iteme(Client player, string numeitem, int cantitate, int cantitatedetrimis, string tipitem)
        {
            int iplayer = -1; //index jucator
            List<Client> playerList = API.getPlayersInRadiusOfPlayer(2, player); //se ia o lista cu jucatorii din raza de 2 feet(cred ca feet)
            if (cantitate >= cantitatedetrimis) //daca are mai multe sau egale iteme sa
            {
                if (playerList.Count > 1) //verifice daca lista de jucatori e mai mare ca 1(deoarece 1 este jucatorul)
                {
                    for (int i = 0; i < playerList.Count; i++) //pentru lista de jucatori se face un loop
                    {
                        if (playerList[i].name != player.name) //daca jucatorul i nu este egal cu tine (jucatorul local)
                        {
                            iplayer = i; // sa ii seteze indexul la i
                            break;
                        }
                    }
                    if (playerList[iplayer].name.Length > 0) //daca are numele mai mare ca 0 caractere (adica daca chiar exista acel jucator)
                    {
                        API.triggerClientEvent(player, "inventory_transferitem", playerList[iplayer].name, cantitatedetrimis, numeitem, tipitem); //sa dea trigger la eventul din javascript
                    }
                }
                else //daca nu este nici un jucator in plus fata de tine
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Nu este niciun jucator in preajma."); // sa ii dea eroare in chat
                }
            }
            else { API.sendChatMessageToPlayer(player, "~r~[Info]Nu dispui de atatea iteme."); }//daca nu are destule iteme
        }
        private void inventory_aruncare_arme(Client player, string numearma, int cantitate, int itemdisp)
        {
            if (cantitate > itemdisp) //daca vrea sa arunce mai multe iteme decat are
            {
                API.sendChatMessageToPlayer(player, "~r~[Info]Nu dispui de atatea iteme."); // sa ii dea eroare in chat
                return; //si return
            }
            else //daca nu
            {
                for (int i = 0; i < player.getData("inventorysize"); i++)//se trece prin tot inventarul
                {
                    int ca = i + 1; //se face calculul
                    if (player.getData("slot" + ca.ToString()) == numearma) // daca pe acest slot este arma
                    {
                        API.sendNotificationToPlayer(player, "Ai aruncat " + cantitate + " " + numearma); // sa ii trimita notificare
                        player.setData("slot" + ca.ToString() + "cont", player.getData("slot" + ca.ToString() + "cont") - cantitate); //sa ii stearga cantitatea

                        if (player.getData("slot" + ca.ToString() + "cont") > 0) //daca mai are cv sa 
                            gamemode.PlayerUpdate(player, "pInventory"); // ii dea update

                        if (player.getData("slot" + ca.ToString() + "cont") < 1) //daca nu mai are
                        {
                            player.setData("slot" + ca.ToString(), ""); //sa ii stearga itemul din inv 
                            gamemode.PlayerUpdate(player, "pInventory"); //si sa ii dea update
                        }
                    }
                }
            }
        }
        private void inventory_echipare_aruncabila(Client player, string equip_numearma, int equip_cantitate)
        {
            string numearma = equip_numearma; //numele aruncabilei (ex molotov)
            int cantitate_pentru_scos = equip_cantitate; //cate sa scoata
            WeaponHash[] playerweaps = API.getPlayerWeapons(player); // se ia o lista cu arme
            bool nu_are_destule_iteme = false; //variabila de tip bool daca are sau nu atatea iteme in mana
            bool are_deja_in_mana = false; //daca are deja in mana arma de tip
            string slot_cuarma = "";
            string slot_cuarmacont = "";
            for (int i = 0; i < playerweaps.Length; i++) //se trece prin toate itemele
            {
                if (numearma == playerweaps[i].ToString()) //se verifica daca are deja in mana
                {
                    are_deja_in_mana = true; //daca are deja in mana variabila se face true
                    break; //opreste loop-ul
                }
            }
            for (int i = 0; i < player.getData("inventorysize"); i++) //se trece prin tot inventarul
            {
                int ca = i + 1; //se face calculul
                if (player.getData("slot" + ca.ToString()) == numearma) //se verifica care slot are arma
                {
                    if (cantitate_pentru_scos > player.getData("slot" + ca.ToString() + "cont")) //se verifica care slot are atatea iteme cat trebe
                    {
                        nu_are_destule_iteme = true; //daca nu are destule sa se seteze true
                    }
                    else
                    {
                        nu_are_destule_iteme = false;// daca are itemele cerute sa fie false
                        slot_cuarma = "slot" + ca.ToString(); //se seteaza slotul care are arma
                        slot_cuarmacont = "slot" + ca.ToString() + "cont"; //se seteaza slotul cu continutul
                        break;
                    }
                }

            }
            if (nu_are_destule_iteme == true) //daca nu are destule sa ii dea eroare
            {
                API.sendChatMessageToPlayer(player, "~r~[Info]Nu dispui de atatea iteme."); //eroare in chat
                return;
            }
            else //daca are destule
            {
                API.sendNotificationToPlayer(player, "Ai scos " + cantitate_pentru_scos + " de " + numearma); //trimite notificare
                if (are_deja_in_mana == false) //daca nu are deja in mana
                    API.givePlayerWeapon(player, API.weaponNameToModel(numearma), cantitate_pentru_scos, true, true); //ii seteaza arma in mana cu cantitatea data
                else //daca are deja in mana
                    API.setPlayerWeaponAmmo(player, API.weaponNameToModel(numearma), API.getPlayerWeaponAmmo(player, API.weaponNameToModel(numearma)) + cantitate_pentru_scos); //ii aduna cele din mana cu cele care le scoate
                player.setData(slot_cuarmacont, player.getData(slot_cuarmacont) - cantitate_pentru_scos); //ii scade din inv cantitatea scoasa

                if (player.getData(slot_cuarmacont) > 0) //daca mai are cv in slot
                    gamemode.PlayerUpdate(player, "pInventory"); //sa ii dea update in db

                if (player.getData(slot_cuarmacont) < 1) // daca nu mai are in slot
                {
                    player.setData(slot_cuarma, ""); //sa ii scoata itemul din inv
                    gamemode.PlayerUpdate(player, "pInventory"); //update in inv
                }
            }
        }

        private void inventory_echipare(Client player, string equip_numearma)
        {
            //vine numele armei
            string numearma = equip_numearma;
            //se face o lista cu toate armele jucatorului
            WeaponHash[] playerweaps = API.getPlayerWeapons(player);
            bool inventory_aredejainmana = false; // defineste variabila tip bool
            for (int i = 0; i < playerweaps.Length; i++) // se trece prin toate armele
            {
                if (numearma == playerweaps[i].ToString()) // se verifica daca are deja in mana
                {
                    inventory_aredejainmana = true; // se seteaza variabila true, adica are deja in mana
                    break;
                }
            }
            if (inventory_aredejainmana == true) // daca are deja in mana sa nu ii mai echipeze
            {
                API.sendChatMessageToPlayer(player, "~r~[Info]Ai deja o arma de acest tip in mana."); //eroare in chat
                return;
            }
            else // daca nu are in mana
            {
                for (int i = 0; i < player.getData("inventorysize"); i++) //se trece prin toate sloturile 
                {
                    int ca = i + 1; //se face calculul pentru slot, deoarece lista incepe de la 0 iar slot incepe de la 1 ex slot1 vs slot0 unde slot0 nu exista
                    if (player.getData("slot" + ca.ToString()) == numearma) // daca acel slot are arma necesara
                    {
                        API.sendNotificationToPlayer(player, "Ai scos 1 " + numearma); //trimite notificare
                        API.givePlayerWeapon(player, API.weaponNameToModel(numearma), 0, true, true); // ii da arma respectiva (se stie din variabila numearma)
                        player.setData("slot" + ca.ToString() + "cont", player.getData("slot" + ca.ToString() + "cont") - 1); //ii seteaza data la jucator -1 la continut

                        if (player.getData("slot" + ca.ToString() + "cont") > 0) //daca mai are pe acel slot cv sa ii dea doar update
                            gamemode.PlayerUpdate(player, "pInventory"); // ii da update in db

                        if (player.getData("slot" + ca.ToString() + "cont") < 1) // daca nu mai are pe acel slot nimic, sa ii stearga itemul din inv
                        {
                            player.setData("slot" + ca.ToString(), ""); //ii sterge itemul din inv ca nu mai are nimic
                            gamemode.PlayerUpdate(player, "pInventory"); //update in db
                        }
                        break;
                    }
                }
            }
        }
        public bool isValidInt(string numere)
        {
            foreach (char character in numere)
            {
                if (!new misc().isAllInt(character.ToString(), databaseMaster.validinttype)) { return false; }
            }
            return true;
        }
    }

}
