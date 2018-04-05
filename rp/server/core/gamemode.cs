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
using MySql.Data.MySqlClient;


//cand apasa k si e in dmv sa nu ii deschida nimic
namespace ServerGTMP
{
    public class gamemode : Script
    {
        public static int MAX_PLAYERS = 200;

        ColShape PrimarieShape,
                 Job_Trucker_Intrare,
                 ScoalaDeSoferi_Intrare;

        //Trucker Job

        List<Vehicle> Job_Trucker_Camioane = new List<Vehicle>(MAX_PLAYERS);
        List<Vehicle> Job_Trucker_Remorci = new List<Vehicle>(MAX_PLAYERS);
        List<int> Job_Trucker_ModeleCamioane = new List<int>(5);
        List<int> Job_Trucker_ModeleRemorci = new List<int>(15);
        List<Vector3> Job_Trucker_LocatiiDestinatii = new List<Vector3>(8);
        List<Vector3> Job_Trucker_LocatiiSpawnCamioane = new List<Vector3>(11);

        //end

        //DMV
        List<Vehicle> ScoalaDeSoferi_Masina = new List<Vehicle>(MAX_PLAYERS);
        //end

        public string noacces = "~r~ERROR! Nu ai acces la aceasta comanda!";
        public string noplayer = "~r~ERROR! Jucatorul nu este connectat!";


        public int minute;
        public int ore;
        public DateTime ultimultimp;

        public static Vector3 defaultspawn = new Vector3(300.407776, 193.051071, 103.590019);

        public gamemode()
        {
            API.onResourceStart += OnResourceStartHandler;
            API.onPlayerDeath += OnPlayerDeathHandler;
            API.onPlayerConnected += OnPlayerConnectedHandler;
            API.onPlayerDisconnected += OnPlayerDisconnectedHandler;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            API.onUpdate += OnUpdateHandler;
            API.onChatMessage += OnChatMessageHandler;
            API.onPlayerExitVehicle += OnPlayerExitVehicleHandler;
            API.onPlayerHealthChange += OnPlayerHealthChangeHandler;
            API.onPlayerArmorChange += OnPlayerArmorChangeHandler;
            API.onEntityEnterColShape += OnEntityEnterColShapeHandler;
            API.onEntityExitColShape += OnEntityExitColShapeHandler;
            API.onClientEventTrigger += OnClientEvent;
            API.onVehicleDeath += OnVehicleDeathHandler;
            API.onVehicleHealthChange += OnVehicleHealthChangeHandler;
        }


        #region Eventuri
        private void OnPlayerConnectedHandler(Client player)
        {
            API.sendNotificationToAll("Jucatorul " + player.name + " s-a trezit.");
        }
        private void OnPlayerDeathHandler(Client player, NetHandle entityKiller, int weapon)
        {
            Client killer = API.getPlayerFromHandle(entityKiller);

            if (API.getEntityType(entityKiller) == EntityType.Player)
            {
                if (player.name != killer.name)
                {
                    API.sendNotificationToAll(player.name + " a fost omorat de " + killer.name);
                    player.setData("decese", player.getData("decese") + 1);
                    killer.setData("omoruri", killer.getData("omoruri") + 1);
                    PlayerUpdate(player, "pDecese");
                    PlayerUpdate(killer, "pOmoruri");
               }else if(player.name == killer.name)
                {
                    API.sendNotificationToAll("Jucatorul " + player.name + " s-a sinucis.");
                    player.setData("decese", player.getData("decese") + 1);
                    PlayerUpdate(player, "pDecese");
                }
            }
            if (player.getData("ScoalaDeSoferi_InExamen") == true)
                PicaExamenSoferi(player);
        }
        private void OnPlayerDisconnectedHandler(Client player, string reason)
        {
            API.sendNotificationToAll("Jucatorul " + player.name + " a adormit.");
            if (API.getPlayerVehicle(player) != null)
            {
                API.deleteEntity(API.getPlayerVehicle(player));
            }
            if (player.getData("inregistrat") == true)
            {
                if (player.getData("data_trucker_incursa"))
                {
                    API.deleteEntity(Job_Trucker_Camioane[player.getData("Job_Trucker_IndexRemorca")]);
                }
            }
        }
        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle, int targetSeat)
        {
            //sa ii zica ca nu are permis
            if (targetSeat == -1)
            {
                if (player.getData("data_trucker_incursa") == false)
                {
                    if (player.getData("Job_Trucker_AreCamion") == true)
                    {
                        if (vehicle != Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")])
                        {
                            API.deleteEntity(Job_Trucker_Camioane[(player.getData("Job_Trucker_IndexCamion"))]);
                            Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")] = null;
                            player.setData("Job_Trucker_IndexCamion", -1);
                            player.setData("Job_Trucker_AreCamion", false);
                        }
                    }
                }
                for (int i = 0; i < Job_Trucker_ModeleCamioane.Count; i++)
                {
                    if (API.getEntityModel(vehicle) == Job_Trucker_ModeleCamioane[i])
                    {
                        if (player.getData("job") != "Camionagiu")
                        {
                            API.sendChatMessageToPlayer(player, "~r~ Trebuie sa fii sofer de tir pentru a conduce.");
                            API.warpPlayerOutOfVehicle(player);
                        }
                    }
                }
            }
        }
        public void OnResourceStartHandler()
        {
            TruckerSetup();

            CreateMarkers();
            CreateBlips();
        }
        public void OnUpdateHandler()
        {
            if (DateTime.Now.Subtract(ultimultimp).TotalSeconds >= 1)
            {
                ultimultimp = DateTime.Now;
                if (minute < 59)
                {
                    minute++;
                    API.setTime(ore, minute);
                }
                else
                {
                    minute = 0;
                    ore++;
                    API.setTime(ore, minute);
                }
                if (ore > 23)
                    ore = 0;
            }

        }
        public void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
        {
            List<Client> playerList = API.getPlayersInRadiusOfPlayer(50, player);
            foreach (Client playerItem in playerList)
            {
                if (player.getData("admin") >= 1)
                    API.sendChatMessageToPlayer(playerItem, "~r~[Admin]~w~ " + API.getEntityData(player, "PlayerFullName") + "(" + player.name + "): " + message);
                else
                    API.sendChatMessageToPlayer(playerItem, "[Civil] " + API.getEntityData(player, "PlayerFullName") + "(" + player.name + "): " + message);
            }
            e.Cancel = true;
        }
        private void OnPlayerExitVehicleHandler(Client player, NetHandle vehicle, int fromSeat)
        {
            if (player.getData("ScoalaDeSoferi_InExamen") == true)
            {
                PicaExamenSoferi(player);
            }
        }
        private void OnPlayerHealthChangeHandler(Client entity, int oldValue)
        {
            API.setEntityData(entity, "viata", API.getPlayerHealth(entity));
        }
        private void OnPlayerArmorChangeHandler(Client entity, int oldValue)
        {
            API.setEntityData(entity, "armura", API.getPlayerArmor(entity));
        }

        private void OnEntityEnterColShapeHandler(ColShape shape, NetHandle entity)
        {
            if (API.getEntityType(entity) == EntityType.Player)
            {
                Client player = API.getPlayerFromHandle(entity);
                if (player.getData("ScoalaDeSoferi_InExamen") == true)
                {
                    if (shape == player.getData("ScoalaDeSoferi_ColShape"))
                    {
                        int index = player.getData("ScoalaDeSoferi_Index");
                        switch (index)
                        {
                            case 0:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-459.3424, -263.9496, 34), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 1);
                                    break;
                                }
                            case 1:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-417.1331, -289.7937, 34), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 2);
                                    break;
                                }
                            case 2:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-329.7383, -352.5923, 29), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 3);
                                    break;
                                }
                            case 3:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-368.7236, -378.5137, 29), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 4);
                                    break;
                                }
                            case 4:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-482.4003, -364.2168, 33), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 5);
                                    break;
                                }
                            case 5:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-570.5078, -360.9365, 33), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 6);
                                    break;
                                }
                            case 6:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-605.0607, -333.5119, 33), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 7);
                                    break;
                                }
                            case 7:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-538.4241, -296.6289, 33), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 8);
                                    break;
                                }
                            case 8:
                                {
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-515.8102, -263.9507, 34), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                                    player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                                    player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                                    API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                                    API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                                    player.setData("ScoalaDeSoferi_Index", 9);
                                    break;
                                }
                            case 9:
                                {
                                    API.sendPictureNotificationToPlayer(player, "Felicitari, ai trecut testul si ai primit permisul de conducere.", "CHAR_ACTING_UP", 0, 1, "Instructor Dan", "Scoala de soferi");
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
                                    API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
                                    API.deleteEntity(ScoalaDeSoferi_Masina[player.getData("ScoalaDeSoferi_IndexMasina")]);
                                    player.setData("ScoalaDeSoferi_IndexMasina", -1);
                                    API.deleteColShape(player.getData("ScoalaDeSoferi_PerimetruExamen"));
                                    player.setData("ScoalaDeSoferi_InExamen", false);
                                    player.setData("TaxaPermisPlatita", false);
                                    player.dimension = 0;
                                    player.setData("permis", "Da");
                                    PlayerUpdate(player, "pPermis");
                                    player.setData("ScoalaDeSoferi_Index", 0);
                                    break;
                                }
                        }
                    }
                }
                if (shape == ScoalaDeSoferi_Intrare)
                {
                    if (player.getData("ScoalaDeSoferi_InExamen") == true)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Esti deja in examen.");
                        return;
                    }
                    if (player.getData("permis") == "Da")
                    {
                        API.sendChatMessageToPlayer(player, "~r~Ai deja permis de conducere.");
                        return;
                    }
                    else
                    {
                        if (player.getData("TaxaPermisPlatita") == true)
                        {
                            API.triggerClientEvent(player, "ScoalaDeSoferi_Exam", player.getData("bani"));
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "~r~Nu ai taxa pentru permis platia. Pentru a o plati, mergi la primarie.");
                        }
                    }
                }
                if (shape == PrimarieShape)
                {
                    if (player.getData("ScoalaDeSoferi_InExamen") == true)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Nu poti merge la primarie in timp ce dai examenul de conducere.");
                        return;
                    }
                    bool permis,
                         angajat,
                         taxapescuit;

                    if (player.getData("job") == "Civil")
                        angajat = false;
                    else
                        angajat = true;
                    if (player.getData("permis") == "Da")
                        permis = true;
                    else
                        permis = false;
                    if (player.getData("taxapescuit") == "Da")
                        taxapescuit = true;
                    else
                        taxapescuit = false;

                    API.triggerClientEvent(player, "primarie_meniu", angajat, permis, player.getData("TaxaPermisPlatita"), player.getData("bani"), taxapescuit);
                }
                if (shape == Job_Trucker_Intrare)
                {
                    if (player.getData("ScoalaDeSoferi_InExamen") == true)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Nu poti merge la job in timp ce dai examenul de conducere.");
                        return;
                    }
                    if (player.getData("job") == "Camionagiu")
                    {
                        API.triggerClientEvent(player, "job_trucker", player.getData("data_trucker_incursa"));
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~ Nu esti camionagiu, mergi la primarie pentru ati lua jobul");
                        //rework needed
                    }
                }
                if (player.getData("data_trucker_incursa") == true)
                {
                    if (shape == player.getData("Job_Trucker_Cursa_ColShape"))
                    {
                        if (API.getPlayerVehicle(player) == null)
                        {
                            API.sendChatMessageToPlayer(player, "Nu esti in camion!");
                            return;
                        }

                        if (Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")].getData("numejucator") == player.name)
                        {
                            if(API.getVehicleTrailer(API.getPlayerVehicle(player)) == null)
                            {
                                API.sendChatMessageToPlayer(player, "~r~[Info]Reconecteaza remorca!");
                                return;
                            }
                            if (Job_Trucker_Remorci[player.getData("Job_Trucker_IndexRemorca")] == API.getVehicleTrailer(API.getPlayerVehicle(player)))
                            {

                                API.triggerClientEvent(player, "Job_Trucker_DestroyStuffs");
                                API.deleteColShape(player.getData("Job_Trucker_Cursa_ColShape"));
                                API.deleteEntity(Job_Trucker_Remorci[player.getData("Job_Trucker_IndexRemorca")]);
                                Job_Trucker_Remorci[player.getData("Job_Trucker_IndexRemorca")] = null;
                                player.setData("Job_Trucker_IndexRemorca", -1);
                                player.setData("data_trucker_incursa", false);
                                API.sendChatMessageToPlayer(player, "~g~[Dispecerat]Ai completat cu succes cursa. Vino la sediu pentru alta cursa");
                                API.sendPictureNotificationToPlayer(player, string.Format("Ai completat cu succes cursa si ai primit {0}. Vino la sediu pentru altele.", player.getData("Job_Trucker_Reward")), "CHAR_ACTING_UP", 0, 1, "Dispecerat", "Informatii cursa");
                                GivePlayerCash(player, player.getData("Job_Trucker_Reward"));
                                PlayerUpdate(player, "pMoney");

                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player, "Nu este remorca ta!");
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "Nu esti in camionul tau!");
                        }
                    }
                }
            }
        }
        private void OnEntityExitColShapeHandler(ColShape shape, NetHandle entity)
        {
            if (API.getEntityType(entity) == EntityType.Player)
            {
                Client player = API.getPlayerFromHandle(entity);
                if (shape == PrimarieShape)
                    API.triggerClientEvent(player, "destroy_primarie");
                if (shape == Job_Trucker_Intrare)
                    if (player.getData("job") == "Camionagiu")
                        API.triggerClientEvent(player, "dostroy_trcucker_menu");
                if (shape == ScoalaDeSoferi_Intrare)
                {
                    if (player.getData("TaxaPermisPlatita") == true)
                    {
                        API.triggerClientEvent(player, "destroy_scoaladesoferi");
                    }
                }
                if (player.getData("ScoalaDeSoferi_InExamen") == true)
                    if (shape == player.getData("ScoalaDeSoferi_PerimetruExamen"))
                    {
                        PicaExamenSoferi(player);
                    }
            }
        }
        public void OnClientEvent(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "Primarie_Taxa_Pescuit":
                    {
                        RemovePlayerCash(player, 50);
                        API.sendPictureNotificationToPlayer(player, "Ai cumparat licenta pentru pescuit. Acum poti pescui in legalitate.", "CHAR_ACTING_UP", 0, 1, "Klaus Iohannis", "Taxa pescar");
                        player.setData("taxapescuit", "Da");
                        PlayerUpdate(player, "pTaxaPescuit");
                        PlayerUpdate(player, "pBani");
                        break;
                    }
                case "Player_Menu_Asistenta":
                    {
                        string text = arguments[0].ToString();
                        List<Client> playerList = API.getAllPlayers();
                        foreach (var target in playerList)
                        {
                            if (target.getData("admin") > 0)
                            {
                                API.sendChatMessageToPlayer(target,"~b~[AdminHelp]Jucatorul " + player.name + " cere asistenta. Mesaj: " + text);
                            }
                        }
                        API.sendNotificationToPlayer(player, "Un admin iti va raspunde in scurt timp.");
                        break;
                    }
                case "Player_Menu_Ajutor":
                    {
                        string text = arguments[0].ToString();
                        List<Client> playerList = API.getAllPlayers();
                        foreach (var target in playerList)
                        {
                            if (target.getData("helper") > 0)
                            {
                                API.sendChatMessageToPlayer(target, "~b~[HelperHelp]Jucatorul " + player.name + " cere asistenta. Mesaj: " + text);
                            }
                        }
                        API.sendNotificationToPlayer(player, "Un helper iti va raspunde in scurt timp.");
                        break;
                    }
                case "Player_Menu_Acte":
                    {
                        //rework
                        API.triggerClientEvent(player, "Player_Menu_Acte", player.name);
                        break;
                    }
                case "Player_Menu_TransferBani":
                    {
                        int suma = Int32.Parse(arguments[0].ToString());
                        List<Client> PlayerList = API.getPlayersInRadiusOfPlayer(2, player);
                        if(PlayerList[0] != null && PlayerList[0].name != player.name)
                        {
                            if(suma > 1000000)
                            {
                                API.sendChatMessageToPlayer(player, "~r~[Info]Nu poti transfera mai mult de 1.000.000");
                                return;
                            }
                            if(player.getData("bani") > suma)
                            {
                                API.triggerClientEvent(player, "Player_Menu_TransferBani", PlayerList[0].name, suma);
                            } else { API.sendChatMessageToPlayer(player, "~r~[Info]Nu ai destui bani."); }
                        }else { API.sendChatMessageToPlayer(player, "~r~[Info]Nu este nici un jucator in apropiere"); }
                        break;
                    }
                case "Player_Menu_TransferBani_Accept":
                    {
                        string nume = arguments[0].ToString();
                        int suma = Int32.Parse(arguments[1].ToString());
                        List<Client> PlayerList = API.getPlayersInRadiusOfPlayer(2, player);
                        if(PlayerList[0].name == nume)
                        {
                            if (player.getData("bani") > suma)
                            {
                                //aici conditie daca a dat in ultimele 30min 100k
                                API.sendChatMessageToPlayer(player, string.Format("~g~[Info]I-ai trimis {0}$ lui {1}.", suma, PlayerList[0].name));
                                API.sendChatMessageToPlayer(PlayerList[0], string.Format("~g~[Info]Ai primit {0}$ de la {1}.", suma, PlayerList[0].name));
                                GivePlayerCash(PlayerList[0], suma);
                                RemovePlayerCash(player, suma);
                                PlayerUpdate(player, "pBani");
                                PlayerUpdate(PlayerList[0], "pBani");
                            }
                            else { API.sendChatMessageToPlayer(player, "~r~[Info]Nu mai ai destui bani."); }
                        }
                        else { API.sendChatMessageToPlayer(player, string.Format("~r~[Info]Jucatorul {0} nu mai este in apropiere.", nume)); }
                        break;
                    }
                case "Player_OpenMenu":
                    {
                        if (isPlayerConnected(player))
                        {
                            if(player.getData("ScoalaDeSoferi_InExamen") == true)
                            {
                                return;
                            }
                            if (player.getData("Player_Arestat") == true)
                                API.sendChatMessageToPlayer(player, "~r~[Info]Esti arestat, nu poti deschide meniul.");
                            else
                                API.triggerClientEvent(player, "Player_OpenMenu");
                        }
                        break;
                    }
                case "ScoalaDeSoferi_APlatit":
                    {
                        API.sendChatMessageToPlayer(player, "~g~[Instructor]Pentru a porni motorul apasa pe I.");
                        API.sendChatMessageToPlayer(player, "~r~[Instructor]Daca parasesti perimentrul examenului vei pica testul.");
                        API.sendChatMessageToPlayer(player, "~r~[Instructor]Daca iesi din masina vei pica testul.");
                        API.sendChatMessageToPlayer(player, "~r~[Instructor]Daca lovesti masina prea tare vei pica testul.");
                        API.sendChatMessageToPlayer(player, "~r~[Instructor]Daca te deconectezi vei pica testul.");
                        API.sendChatMessageToPlayer(player, "~g~[Instructor]Bafta!");
                        player.dimension = RandomRange(10, 999);
                        API.sendPictureNotificationToPlayer(player, "Buna ziua, urmati traseul si veti primi permisul de conducere.", "CHAR_ACTING_UP", 0, 1, "Instructor Dan", "Scoala de soferi");
                        for (int i = 0; i < ScoalaDeSoferi_Masina.Count; i++)
                        {
                            if(ScoalaDeSoferi_Masina[i] == null)
                            {
                                ScoalaDeSoferi_Masina[i] = API.createVehicle(1348744438, new Vector3(-515.4116, -264.1898, 35.19346), new Vector3(0.6661813, -0.8240706, -64.69328), 0, 0, player.dimension);
                                player.setData("ScoalaDeSoferi_IndexMasina", i);
                                ScoalaDeSoferi_Masina[i].setData("numejucator", player.name);
                                API.setVehicleNumberPlate(ScoalaDeSoferi_Masina[i], "EXAMEN");
                                API.setVehicleEngineStatus(ScoalaDeSoferi_Masina[i], false);
                                API.setVehicleFuelLevel(ScoalaDeSoferi_Masina[i], 100f);
                                API.setPlayerIntoVehicle(player, ScoalaDeSoferi_Masina[i], -1);
                                break;
                            }
                        }
                        player.setData("ScoalaDeSoferi_PerimetruExamen", API.createCylinderColShape(new Vector3(-469.2379, -324.377, 34.36325), 200f, 50f));
                        player.setData("ScoalaDeSoferi_InExamen", true);
                        player.setData("ScoalaDeSoferi_Marker", API.createMarker(1, new Vector3(-515.4116, -264.1898, 34), new Vector3(), new Vector3(), new Vector3(2, 2, 2), 255, 255, 0, 255, player.dimension));
                        player.setData("ScoalaDeSoferi_ColShape", API.createCylinderColShape(player.getData("ScoalaDeSoferi_Marker").position, 2, 3));
                        player.setData("ScoalaDeSoferi_Blip", API.createBlip(player.getData("ScoalaDeSoferi_Marker").position, player.dimension));
                        API.setBlipRouteColor(player.getData("ScoalaDeSoferi_Blip"), 81);
                        API.setBlipRouteVisible(player.getData("ScoalaDeSoferi_Blip"), true);
                        player.setData("ScoalaDeSoferi_Index", 0);
                        RemovePlayerCash(player, 200);
                        PlayerUpdate(player, "pMoney");
                        break;
                    }
                case "ScoalaDeSoferi_TaxaTrigger":
                    {
                        API.sendPictureNotificationToPlayer(player, "Ai platit taxa pentru permis, acum poti da examenul. Tine minte, daca iesi de pe server vei pierde taxa.", "CHAR_ACTING_UP", 0, 1, "Klaus Iohannis", "Taxa permis");
                        player.setData("TaxaPermisPlatita", true);
                        RemovePlayerCash(player, 20);
                        PlayerUpdate(player, "pMoney");
                        break;
                    }
                case "job_trucker_cursa":
                    {
                        int castig = Int32.Parse(arguments[0].ToString());
                        int Destinatie = Int32.Parse(arguments[1].ToString());
                        int remorca = Job_Trucker_ModeleRemorci[Int32.Parse(arguments[2].ToString())];
                        string AdresaPlecare = arguments[3].ToString();
                        string AdresaDestinatie = arguments[4].ToString();
                        string ObiecteTransport = arguments[5].ToString();

                        if (player.getData("Job_Trucker_AreCamion") == true)
                        {
                            API.deleteEntity(Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")]);
                            Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")] = null;
                            player.setData("Job_Trucker_IndexCamion", -1);
                        }
                        player.setData("data_trucker_incursa", true);
                        player.setData("Job_Trucker_Reward", arguments[0]);
                        API.sendPictureNotificationToPlayer(player, string.Format("Ai ales sa transporti {0} de la {1} la {2}, vei primi {3} la finalul cursei", ObiecteTransport, AdresaPlecare, AdresaDestinatie, player.getData("Job_Trucker_Reward")), "CHAR_ACTING_UP", 0, 1, "Dispecerat", "Informatii cursa");

                        for (int i = 0; i < Job_Trucker_Camioane.Count; i++)
                        {
                            if (Job_Trucker_Camioane[i] == null)
                            {
                                Job_Trucker_Camioane[i] = API.createVehicle(Job_Trucker_ModeleCamioane[RandomRange(0, Job_Trucker_ModeleCamioane.Count)], Job_Trucker_LocatiiSpawnCamioane[RandomRange(0, Job_Trucker_LocatiiSpawnCamioane.Count)], new Vector3(), RandomRange(0, 255), RandomRange(0, 255), 0);
                                player.setData("Job_Trucker_IndexCamion", i);
                                Job_Trucker_Camioane[i].setData("numejucator", player.name);
                                API.setVehicleEngineStatus(Job_Trucker_Camioane[i], false);
                                API.setVehicleNumberPlate(Job_Trucker_Camioane[i], "[RO]EuroLogistic");
                                API.setPlayerIntoVehicle(player, Job_Trucker_Camioane[i], -1);
                                break;
                            }
                        }
                        for (int i = 0; i < Job_Trucker_Remorci.Count; i++)
                        {
                            if (Job_Trucker_Remorci[i] == null)
                            {
                                Vector3 position = Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")].position;
                                Vector3 rotation = Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")].rotation;
                                Job_Trucker_Remorci[i] = API.createVehicle(remorca, position - new Vector3(0, 9, 0) + new Vector3(0, 0, 0.5), rotation, RandomRange(0, 255), RandomRange(0, 255), 0); //se face remoca pt camion
                                API.setVehicleNumberPlate(Job_Trucker_Remorci[i], "[RO]EuroLogistic");
                                Job_Trucker_Remorci[i].setData("numejucator", player.name);
                                player.setData("Job_Trucker_IndexRemorca", i);
                                break;
                            }
                        }
                        player.setData("Job_Trucker_AreCamion", true);
                        API.sendChatMessageToPlayer(player, "~y~Apasa I pentru a porni motorul.");
                        player.setData("Job_Trucker_Cursa_ColShape", API.createCylinderColShape(Job_Trucker_LocatiiDestinatii[Destinatie], 2.5f, 2.5f));

                        API.triggerClientEvent(player, "Job_Trucker_CreateStuffs", Job_Trucker_LocatiiDestinatii[Destinatie]);


                        break;
                    }
                case "trucker_anularecursa":
                    {   //daca vrea sa anuleze cursa sa ii reseteze urmatoarele:
                        API.triggerClientEvent(player, "Job_Trucker_DestroyStuffs");
                        API.deleteColShape(player.getData("Job_Trucker_Cursa_ColShape"));
                        API.deleteEntity(Job_Trucker_Camioane[(player.getData("Job_Trucker_IndexCamion"))]);
                        API.deleteEntity(Job_Trucker_Remorci[player.getData("Job_Trucker_IndexRemorca")]);
                        Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")] = null;
                        Job_Trucker_Remorci[player.getData("Job_Trucker_IndexRemorca")] = null;
                        player.setData("Job_Trucker_IndexRemorca", -1);
                        player.setData("Job_Trucker_IndexCamion", -1);
                        player.setData("Job_Trucker_AreCamion", false);
                        player.setData("data_trucker_incursa", false);
                        RemovePlayerCash(player, 100);
                        PlayerUpdate(player, "pMoney");
                        break;
                    }
                case "joburi_camionagiu":
                    {
                        player.setData("job", "Camionagiu");
                        PlayerUpdate(player, "pJob");
                        break;
                    }
                case "joburi_demisioneaza":
                    {
                        if (player.getData("Job_Trucker_AreCamion") == true)
                        {
                            API.deleteEntity(Job_Trucker_Camioane[(player.getData("Job_Trucker_IndexCamion"))]);
                            Job_Trucker_Camioane[player.getData("Job_Trucker_IndexCamion")] = null;
                            player.setData("Job_Trucker_IndexCamion", -1);
                            player.setData("Job_Trucker_AreCamion", false);
                        }
                        player.setData("job", "Civil");
                        PlayerUpdate(player, "pJob");
                        break;
                    }
            }
        }
        private void OnVehicleDeathHandler(NetHandle vehicle)
        {
            API.setVehicleHealth(vehicle, 0f);
            API.setVehicleEngineStatus(vehicle, false);
            if(Job_Trucker_Camioane != null)
            {
                for (int i = 0; i < Job_Trucker_Camioane.Count; i++)
                {
                    if (Job_Trucker_Camioane[i] != null)
                    {
                        if (vehicle == Job_Trucker_Camioane[i])
                        {
                            if (Job_Trucker_Camioane[i].getData("numejucator") != null)
                            {
                                Client Jucator = API.getPlayerFromName(Job_Trucker_Camioane[i].getData("numejucator"));
                                if (Jucator.getData("data_trucker_incursa") == true)
                                {
                                    API.triggerClientEvent(Jucator, "Job_Trucker_DestroyStuffs");
                                    API.deleteColShape(Jucator.getData("Job_Trucker_Cursa_ColShape"));
                                    API.sendPictureNotificationToPlayer(Jucator, "Ai distrus camionul si ai fost penalizat cu 2000$.", "CHAR_ACTING_UP", 0, 1, "Dispecerat", "Informatii cursa");
                                    API.deleteEntity(Job_Trucker_Camioane[(Jucator.getData("Job_Trucker_IndexCamion"))]);
                                    API.deleteEntity(Job_Trucker_Remorci[Jucator.getData("Job_Trucker_IndexRemorca")]);
                                    Job_Trucker_Camioane[Jucator.getData("Job_Trucker_IndexCamion")] = null;
                                    Job_Trucker_Remorci[Jucator.getData("Job_Trucker_IndexRemorca")] = null;
                                    Jucator.setData("Job_Trucker_IndexRemorca", -1);
                                    Jucator.setData("Job_Trucker_IndexCamion", -1);
                                    Jucator.setData("Job_Trucker_AreCamion", false);
                                    Jucator.setData("data_trucker_incursa", false);
                                    RemovePlayerCash(Jucator, 100);
                                    PlayerUpdate(Jucator, "pMoney");
                                    break;
                                } else
                                {
                                    API.deleteEntity(Job_Trucker_Camioane[(Jucator.getData("Job_Trucker_IndexCamion"))]);
                                    Job_Trucker_Camioane[Jucator.getData("Job_Trucker_IndexCamion")] = null;
                                    Jucator.setData("Job_Trucker_IndexCamion", -1);
                                    Jucator.setData("Job_Trucker_AreCamion", false);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (Job_Trucker_Remorci != null)
            {
                for (int z = 0; z < Job_Trucker_Remorci.Count; z++)
                {
                    if (Job_Trucker_Remorci[z] != null)
                    {
                        if (vehicle == Job_Trucker_Remorci[z])
                        {
                            if (Job_Trucker_Remorci[z].getData("numejucator") != null)
                            {
                                Client Jucator = API.getPlayerFromName(Job_Trucker_Remorci[z].getData("numejucator"));
                                if (Jucator.getData("data_trucker_incursa") == true)
                                {
                                    API.triggerClientEvent(Jucator, "Job_Trucker_DestroyStuffs");
                                    API.deleteColShape(Jucator.getData("Job_Trucker_Cursa_ColShape"));
                                    API.sendPictureNotificationToPlayer(Jucator, "Ai distrus remorca si ai fost penalizat cu 2000$.", "CHAR_ACTING_UP", 0, 1, "Dispecerat", "Informatii cursa");
                                    API.deleteEntity(Job_Trucker_Camioane[(Jucator.getData("Job_Trucker_IndexCamion"))]);
                                    API.deleteEntity(Job_Trucker_Remorci[Jucator.getData("Job_Trucker_IndexRemorca")]);
                                    Job_Trucker_Camioane[Jucator.getData("Job_Trucker_IndexCamion")] = null;
                                    Job_Trucker_Remorci[Jucator.getData("Job_Trucker_IndexRemorca")] = null;
                                    Jucator.setData("Job_Trucker_IndexCamion", -1);
                                    Jucator.setData("Job_Trucker_IndexRemorca", -1);
                                    Jucator.setData("Job_Trucker_AreCamion", false);
                                    Jucator.setData("data_trucker_incursa", false);
                                    RemovePlayerCash(Jucator, 100);
                                    PlayerUpdate(Jucator, "pMoney");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void OnVehicleHealthChangeHandler(NetHandle entity, float oldValue)
        {
            
            if(ScoalaDeSoferi_Masina != null)
            {
                for (int i = 0; i < ScoalaDeSoferi_Masina.Count; i++)
                {
                    if(ScoalaDeSoferi_Masina[i] != null)
                    {
                        if(entity == ScoalaDeSoferi_Masina[i])
                        {
                            if(ScoalaDeSoferi_Masina[i].getData("numejucator") != null)
                            {
                                Client jucator = API.getPlayerFromName(ScoalaDeSoferi_Masina[i].getData("numejucator"));
                                {
                                    if (jucator.getData("ScoalaDeSoferi_InExamen") == true)
                                    {
                                        if (API.getVehicleHealth(API.getPlayerVehicle(jucator)) < 500)
                                        {
                                            PicaExamenSoferi(jucator);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

            #region Comenzi
        [Command("taxa")]
        public void taxa(Client player, Client target)
        {
            string da;
            if (player.getData("Player_Arestat") == true)
                da = "da";
            else
                da = "nu";
            API.sendChatMessageToPlayer(player, da);
        }

            //de sters
        [Command("acolo")]
        public void acolo(Client player, int id)
        {
            switch(id)
            {
                case 0:
                    {
                        player.position = new Vector3(-2215.405, 4243.988, 47.41663);
                        break;
                    }
                case 1:
                    {
                        player.position = new Vector3(-828.2751, 5414.585, 34.34522);
                        break;
                    }
                case 2:
                    {
                        player.position = new Vector3(-142.9672, 6203.084, 31.24014);
                        break;
                    }
                case 3:
                    {
                        player.position = new Vector3(2296.743, 4896.278, 41.19694);
                        break;
                    }
                case 4:
                    {
                        player.position = new Vector3(2906.117, 4378.565, 50.37653);
                        break;
                    }
                case 5:
                    {
                        player.position = new Vector3(286.4767, 2861.571, 43.64242);
                        break;
                    }
                case 6:
                    {
                        player.position = new Vector3(888.4153, -1656.211, 29);
                        break;
                    }
                case 7:
                    {
                        player.position = new Vector3(237.4909, -1507.839, 28);
                        break;
                    }
                case 8:
                    {
                        player.position = new Vector3(861.3986, -3185.125, 5);
                        break;
                    }
            }
        }

        //de sters dupa ce termin
        [Command("save", GreedyArg = true)]
        public void SavePosition_Command(Client sender, string name = "")
        {
            var pos = API.getEntityPosition(sender.handle);
            var angle = API.getEntityRotation(sender.handle);
            if (sender.isInVehicle)
            {
                var dim = API.getEntityDimension(sender.handle);
                var playerVehicleHash = API.getEntityModel(API.getPlayerVehicle(sender));
                var playerVehicleColor1 = API.getVehiclePrimaryColor(API.getPlayerVehicle(sender));
                var playerVehicleColor2 = API.getVehicleSecondaryColor(API.getPlayerVehicle(sender));
                File.AppendAllText(@"savedpositions.txt", string.Format("API.createVehicle((VehicleHash){0}, new Vector3({1}, {2}, {3}), new Vector3({4}, {5}, {6}), {7}, {8}, {9}); // {10}\n", playerVehicleHash, pos.X.cc(), pos.Y.cc(), pos.Z.cc(), angle.X.cc(), angle.Y.cc(), angle.Z.cc(), playerVehicleColor1, playerVehicleColor2, dim, name));
                API.sendChatMessageToPlayer(sender, "~#92a079~", "-> InCar position saved (" + name + ")");
            }
            else
            {
                File.AppendAllText(@"savedpositions.txt", string.Format("({0}, {1}, {2}, {3}, {4}, {5}) // {6}\n", pos.X.cc(), pos.Y.cc(), pos.Z.cc(), angle.X, angle.Y, angle.Z.cc(), name));
                API.sendChatMessageToPlayer(sender, "~#92a079~", "-> OnFoot position saved (" + name + ")");
            }
        }

        [Command("ajutor", Alias = "n")]
        public void cmd_n(Client player, string text)
        {
            List<Client> playerList = API.getAllPlayers();
            foreach (var target in playerList)
            {
                if (target.getData("helper") > 0)
                {
                    API.sendChatMessageToPlayer(target, "~b~[HelperHelp]Jucatorul " + player.name + " cere asistenta. Mesaj: " + text);
                }
            }
            API.sendNotificationToPlayer(player, "Un helper iti va raspunde in scurt timp.");
        }
        [Command("report", Alias = "raportare")]
        public void cmd_report(Client player, string text)
        {
            List<Client> playerList = API.getAllPlayers();
            foreach (var target in playerList)
            {
                if (target.getData("admin") > 0)
                {
                    API.sendChatMessageToPlayer(target, "~b~[AdminHelp]Jucatorul " + player.name + " cere asistenta. Mesaj: " + text);
                }
            }
            API.sendNotificationToPlayer(player, "Un admin iti va raspunde in scurt timp.");
        }


        [Command("pay", Alias = "plateste")]
        public void cmd_pay(Client player, Client target, int suma)
        {
            if (isPlayerConnected(player))
            {
                if (!isPlayerConnected(target))
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Jucatorul nu este online.");
                    return;
                }
                if (player == target)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Nu iti poti da bani singur.");
                    return;
                }
                if (player.getData("bani") < suma)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Info]Nu ai destui bani.");
                    return;
                }
                //aici conditie daca a dat in ultimele 30min 100k
                API.sendChatMessageToPlayer(player, string.Format("~g~[Info]I-ai trimis {0}$ lui {1}.", suma, target.name));
                API.sendChatMessageToPlayer(target, string.Format("~g~[Info]Ai primit {0}$ de la {1}.", suma, target.name));
                GivePlayerCash(target, suma);
                RemovePlayerCash(player, suma);
                PlayerUpdate(player, "pBani");
                PlayerUpdate(target, "pBani");
            }
        }
        [Command("admini", Alias = "admins")]
        public void cmd_admini(Client sender)
        {
            if (isPlayerConnected(sender) == true)
            {
                API.sendChatMessageToPlayer(sender, "---------- Admini online ----------");
                List<Client> playerlist = API.getAllPlayers();
                List<string> admini = new List<string>(playerlist.Count);
                for (int i = 0; i < playerlist.Count; i++)
                {
                    if (playerlist[i].getData("admin") > 0)
                    {
                        admini.Add(playerlist[i].name);
                    }
                }
                if (admini.Count < 1)
                    API.sendChatMessageToPlayer(sender, "~r~Nu este nici un admin online.");
                foreach (var player in playerlist)
                {
                    if(player.getData("admin") == 1)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Novice", player.name));
                    }
                    if (player.getData("admin") == 2)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Moderator", player.name));
                    }
                    if (player.getData("admin") == 3)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Manager", player.name));
                    }
                    if (player.getData("admin") == 4)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Administrator", player.name));
                    }
                    if (player.getData("admin") == 5)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Co-Detinator", player.name));
                    }
                    if (player.getData("admin") == 6)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Detinator", player.name));
                    }
                    if (player.getData("admin") >= 7)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Fondator & Scripter", player.name));
                    }
                }
                API.sendChatMessageToPlayer(sender, "---------------------------------------");
            }
        }

        [Command("helpers", Alias = "helperi")]
        public void cmd_helpers(Client sender)
        {
            if (isPlayerConnected(sender) == true)
            {
                API.sendChatMessageToPlayer(sender, "---------- Helperi Online ----------");
                List<Client> playerlist = API.getAllPlayers();
                List<string> helperi = new List<string>(playerlist.Count);
                for (int i = 0; i < playerlist.Count; i++)
                {
                    if (playerlist[i].getData("helper") > 0)
                    {
                        helperi.Add(playerlist[i].name);
                    }
                }
                if (helperi.Count < 1)
                    API.sendChatMessageToPlayer(sender, "~r~Nu este nici un helper online.");
                foreach (var player in playerlist)
                { 
                    if (player.getData("helper") == 1)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Helper 1", player.name));
                    }
                    if (player.getData("helper") == 2)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Helper 2", player.name));
                    }
                    if (player.getData("helper") == 3)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Helper 3", player.name));
                    }
                    if (player.getData("helper") >= 4)
                    {
                        API.sendChatMessageToPlayer(sender, string.Format("~b~{0} - Helper 4", player.name));
                    }
                }
                API.sendChatMessageToPlayer(sender, "----------------------------------------");
            }
        }

        #endregion

        #region AdminCMDS

        [Command("tpcoords")]
        public void cmd_tpcoords(Client sender, Vector3 coords)
        {
            if (API.getEntityData(sender, "admin") >= 1)
            {
                sender.position = coords;
                //aici text
                //rework
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }

        [Command("givemoney")]
        public void givemoney(Client sender, Client target, int suma)
        {
            if (API.getEntityData(sender, "admin") >= 5)
            {
                if (isPlayerConnected(target) == true)
                {
                    int banitotali = target.getData("bani");
                    GivePlayerCash(target, suma);
                    API.sendChatMessageToPlayer(target, string.Format("~g~[Server]Adminul {0} ti-a oferit suma de {1}", sender.name, suma));
                    PlayerUpdate(target, "pMoney");
                }
                else { API.sendChatMessageToPlayer(sender, noplayer); }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }


        [Command("car")]
        public void car(Client sender, VehicleHash model)
        {
            Vehicle masina = API.createVehicle(model, sender.position, sender.rotation, 0, 0, 0);
            API.setPlayerIntoVehicle(sender, masina, -1);
            API.setVehicleNumberPlate(masina, "Admin");
        }

        [Command("respawn")]
        public void cmd_respawn(Client sender, Client target)
        {
            if (sender.getData("admin") >= 1)
            {
                if (isPlayerConnected(target) == true)
                {
                    //aici text
                    target.position = defaultspawn;
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("tp")]
        public void cmd_tp(Client sender, Client target)
        {
            if (sender != target)
            {
                if (sender.getData("admin") >= 1)
                {
                    if (isPlayerConnected(target) == true)
                    {
                        sender.position = target.position;
                        //aici text
                    }
                }
                else { API.sendChatMessageToPlayer(sender, noacces); }
            }
        }
        [Command("get")]
        public void cmd_get(Client sender, Client target)
        {
            if (sender != target)
            {
                if (sender.getData("admin") >= 1)
                {
                    if (isPlayerConnected(target) == true)
                    {
                        //aici text
                        target.position = sender.position;
                    }
                }
                else { API.sendChatMessageToPlayer(sender, noacces); }
            }
        }
        [Command("sethealth")]
        public void cmd_sethealth(Client sender, Client target, int valoare)
        {
            if (sender.getData("admin") >= 2)
            {
                if (isPlayerConnected(target) == true)
                {
                    //aici text
                    API.setPlayerHealth(target, valoare);
                    PlayerUpdate(target, "pViata");
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("setarmor")]
        public void cmd_setarmor(Client sender, Client target, int valoare)
        {
            if (sender.getData("admin") >= 2)
            {
                if (isPlayerConnected(target) == true)
                {
                    // aici text
                    API.setPlayerArmor(target, valoare);
                    PlayerUpdate(target, "pArmura");
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("sethunger")]
        public void cmd_sethunger(Client sender, Client target, int valoare)
        {
            if (sender.getData("admin") >= 2)
            {
                if (isPlayerConnected(target) == true)
                {
                    // aici text
                    target.setData("mancare", valoare);
                    PlayerUpdate(target, "pMancare");
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("setsete")]
        public void cmd_setsete(Client sender, Client target, int valoare)
        {
            if (sender.getData("admin") >= 2)
            {
                if (isPlayerConnected(target) == true)
                {
                    // aici text
                    target.setData("sete", valoare);
                    PlayerUpdate(target, "pSete");
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("setadmin")]
        public void cmd_setadmin(Client sender, Client target, int level)
        {
            if (sender.getData("admin") >= 5)
            {
                if (isPlayerConnected(target) == true)
                {
                    target.setData("admin", level);
                    PlayerUpdate(target, "pAdmin");
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("givegun")]
        public void cmd_givegun(Client sender, Client target, WeaponHash weaponHash, int ammo)
        {
            if (sender.getData("admin") >= 3)
            {
                if (isPlayerConnected(target) == true)
                {
                    API.givePlayerWeapon(target, weaponHash, ammo, true, true);
                    // aici text
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }

        [Command("godmode")]
        public void cmd_godmode(Client sender)
        {
            if (sender.getData("admin") >= 3)
            {
                if (sender.invincible == false)
                {
                    sender.invincible = true;
                    // aici text
                }
                else
                {
                    sender.invincible = false;
                    //aici text
                }
            }
            else { API.sendChatMessageToPlayer(sender, noacces); }
        }
        [Command("ban")]
        public void cmd_ban(Client sender, Client target, string reason)
        {
            if (isPlayerConnected(target) == true)
            {
                if (API.getEntityData(sender, "admin") >= 5)
                {
                    databaseMaster.setPlayerBan(target.name, 1);
                    target.kick(reason);
                    API.sendChatMessageToAll(string.Format("[AdmBot] Jucatorul {0} a primit ban de la adminul {1}. Motiv: {2}", target.name, sender.name, reason));
                }
                else { API.sendChatMessageToPlayer(sender, noacces); }
            }
            else { API.sendChatMessageToPlayer(sender, noplayer); }
        }
        [Command("kick")]
        public void cmd_kick(Client sender, Client target, string reason)
        {
            if (isPlayerConnected(target) == true)
            {
                if (API.getEntityData(sender, "admin") >= 3)
                {
                    target.kick(reason);
                    API.sendChatMessageToAll(string.Format("[AdmBot] Jucatorul {0} a primit kick de la adminul {1}. Motiv: {2}", target.name, sender.name, reason));
                }
                else { API.sendChatMessageToPlayer(sender, noacces); }
            }
            else { API.sendChatMessageToPlayer(sender, noplayer); }
        }
        [Command("warn")]
        public void cmd_warn(Client sender, Client target, string reason)
        {
            if (isPlayerConnected(target) == true)
            {
                if (API.getEntityData(sender, "admin") >= 3)
                {
                    int warns = API.getEntityData(target, "warning");
                    int newwarns = warns + 1;
                    API.setEntityData(target, "warning", newwarns);
                    API.sendChatMessageToPlayer(target, string.Format("[AdmBot] Ai primit warn de la adminul {0}. Motiv: {1}", target.name, reason));
                    PlayerUpdate(target, "pWarn");
                    if (warns >= 3)
                        cmd_ban(sender, target, "Prea multe warninguri");
                }
                else { API.sendChatMessageToPlayer(sender, noacces); }
            }
            else { API.sendChatMessageToPlayer(sender, noplayer); }
        }
        #endregion

        #region Metode
        public void PicaExamenSoferi(Client player)
        {

            API.sendChatMessageToPlayer(player, "~r~[Instructor]Ai picat examenul de conducere.");
            API.sendPictureNotificationToPlayer(player, "Ai picat examenul..", "CHAR_ACTING_UP", 0, 1, "Instructor Dan", "Scoala de soferi");
            API.deleteEntity(player.getData("ScoalaDeSoferi_Blip"));
            API.deleteEntity(player.getData("ScoalaDeSoferi_Marker"));
            API.deleteColShape(player.getData("ScoalaDeSoferi_ColShape"));
            API.deleteEntity(ScoalaDeSoferi_Masina[player.getData("ScoalaDeSoferi_IndexMasina")]);
            ScoalaDeSoferi_Masina[player.getData("ScoalaDeSoferi_IndexMasina")] = null;
            player.setData("ScoalaDeSoferi_IndexMasina", -1);
            API.deleteColShape(player.getData("ScoalaDeSoferi_PerimetruExamen"));
            player.setData("ScoalaDeSoferi_InExamen", false);
            player.setData("TaxaPermisPlatita", false);
            player.dimension = 0;
            player.position = new Vector3(-545.1738, -205.9771, 39);
           

        }
        public int RandomRange(int min, int max)
        {
            Random RandomRange = new Random();
            return RandomRange.Next(min, max);
        }
        public void TruckerSetup()
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                Job_Trucker_Remorci.Add(null);
                Job_Trucker_Camioane.Add(null);
                ScoalaDeSoferi_Masina.Add(null);
            }
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(900.6706, -3128.501, 5.968551));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(900.5068, -3153.292, 5.968419));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(900.545, -3183.075, 5.96743));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(924.8944, -3130.856, 5.96894));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(941.1559, -3206.973, 5.967567));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(961.6379, -3158.271, 5.969982));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(961.4535, -3189.052, 5.968738));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(953.77, -3213.168, 5.967686));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(997.3158, -3205.504, 5.969512));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(997.074, -3180.917, 5.968716));
            Job_Trucker_LocatiiSpawnCamioane.Add(new Vector3(1026.019, -3214.414, 5.933601));

            Job_Trucker_ModeleCamioane.Add(1518533038);
            Job_Trucker_ModeleCamioane.Add(569305213);
            Job_Trucker_ModeleCamioane.Add(-2137348917);
            Job_Trucker_ModeleCamioane.Add(177270108);
            Job_Trucker_ModeleCamioane.Add(1074326203);

            Job_Trucker_ModeleRemorci.Add(-1207431159);
            Job_Trucker_ModeleRemorci.Add(-1476447243);
            Job_Trucker_ModeleRemorci.Add(-1637149482);
            Job_Trucker_ModeleRemorci.Add(-2140210194);
            Job_Trucker_ModeleRemorci.Add(2078290630);
            Job_Trucker_ModeleRemorci.Add(1784254509);
            Job_Trucker_ModeleRemorci.Add(2091594960);
            Job_Trucker_ModeleRemorci.Add(-1352468814);
            Job_Trucker_ModeleRemorci.Add(-1770643266);
            Job_Trucker_ModeleRemorci.Add(-730904777);
            Job_Trucker_ModeleRemorci.Add(1956216962);
            Job_Trucker_ModeleRemorci.Add(2016027501);
            Job_Trucker_ModeleRemorci.Add(-877478386);
            Job_Trucker_ModeleRemorci.Add(-1579533167);
            Job_Trucker_ModeleRemorci.Add(-2058878099);
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(888.4153, -1656.211, 29));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(237.4909, -1507.839, 28));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(-2215.405, 4243.988, 46));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(-828.2751, 5414.585, 33));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(-142.9672, 6203.084, 30));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(2296.743, 4896.278, 40));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(2906.117, 4378.565, 49));
            Job_Trucker_LocatiiDestinatii.Add(new Vector3(286.4767, 2861.571, 42));
        }
        public void CreateMarkers()
        {
            API.createMarker(1, new Vector3(236.4959, -408.3344, 46.8000), new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 0, 255); //marker primărie
            PrimarieShape = API.createCylinderColShape(new Vector3(236.4959, -408.3344, 46.8000), 1.2f, 1.2f); // primarie

            API.createMarker(1, new Vector3(861.3986, -3185.125, 5), new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 0, 255); //marker trucker
            Job_Trucker_Intrare = API.createCylinderColShape(new Vector3(861.3986, -3185.125, 5), 1.2f, 1.2f); // trucker

            API.createMarker(1, new Vector3(-545.1738, -203.9771, 37), new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 0, 255);//marker scoala de soferi
            ScoalaDeSoferi_Intrare = API.createCylinderColShape(new Vector3(-545.1738, -203.9771, 38), 1.2f, 1.2f); // scoala de soferi
        }
        public void CreateBlips()
        {
            Blip Primarie = API.createBlip(new Vector3(236.4959, -408.3344, 0));
            API.setBlipName(Primarie, "Primarie");
            API.setBlipSprite(Primarie, 419);
            API.setBlipScale(Primarie, 1.5f);

            Blip Job_Trucker_Intrare_Blip = API.createBlip(new Vector3(861.3986, -3185.125, 0), 1000, 0);
            API.setBlipColor(Job_Trucker_Intrare_Blip, 2);
            API.setBlipName(Job_Trucker_Intrare_Blip, "Centru distributie");
            API.setBlipScale(Job_Trucker_Intrare_Blip, 1.5f);
            API.setBlipSprite(Job_Trucker_Intrare_Blip, 477);

            Blip ScoalaDeSoferi_Blip = API.createBlip(new Vector3(-545.1738, -203.9771, 37), 1000, 0);
            API.setBlipColor(ScoalaDeSoferi_Blip, 2);
            API.setBlipName(ScoalaDeSoferi_Blip, "Scoala de soferi");
            API.setBlipScale(ScoalaDeSoferi_Blip, 1.5f);
            API.setBlipSprite(ScoalaDeSoferi_Blip, 225);
        }
 
        public bool isPlayerConnected(Client target)
        {
            bool result;

            if (API.getEntityData(target, "isConnected") == true)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public void GivePlayerCash(Client target, int suma)
        {
            int targetm = API.getEntityData(target, "bani");
            int newm = targetm + suma;
            API.setEntityData(target, "bani", newm);
            API.triggerClientEvent(target, "UpdateMoneyHUD", Convert.ToString(newm), Convert.ToString(suma));
        }
        public void RemovePlayerCash(Client target, int suma)
        {
            int targetm = API.getEntityData(target, "bani");
            int newm = targetm - suma;
            API.setEntityData(target, "bani", newm);
            API.triggerClientEvent(target, "UpdateMoneyHUD", Convert.ToString(newm), Convert.ToString(suma));
        }
        #endregion

        #region PlayerUpdate 
        public void PlayerUpdate(Client target, string type)
        {
            if (API.getEntityData(target, "isConnected") == true)
            {
                switch (type)
                {
                    case "pCartela":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `cartela` = '" + API.getEntityData(target, "cartela") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pTelefon":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `telefon` = '" + API.getEntityData(target, "telefon") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pTaxaPescuit":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `taxapescuit` = '" + API.getEntityData(target, "taxapescuit") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pOmoruri":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `omoruri` = '" + API.getEntityData(target, "omoruri") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pDecese":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `decese` = '" + API.getEntityData(target, "decese") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pPermis":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `permis` = '" + API.getEntityData(target, "permis") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pJob":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `job` = '" + API.getEntityData(target, "job") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pNume":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `nume` = '" + API.getEntityData(target, "nume") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pPrenume":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `prenume` = '" + API.getEntityData(target, "prenume") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pCNP":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `cnp` = '" + API.getEntityData(target, "cnp") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pMoney":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `bani` = '" + API.getEntityData(target, "bani") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pCard":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `card` = '" + API.getEntityData(target, "card") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pTara":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `tara` = '" + API.getEntityData(target, "tara") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pWarn":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `warning` = '" + API.getEntityData(target, "warning") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pViata":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `viata` = '" + API.getEntityData(target, "viata") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pAdmin":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `admin` = '" + API.getEntityData(target, "admin") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pArmura":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `armura` = '" + API.getEntityData(target, "armura") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pSete":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `sete` = '" + API.getEntityData(target, "sete") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                    case "pMancare":
                        {
                            string stringMySQLQuery = "UPDATE `users` SET `mancare` = '" + API.getEntityData(target, "mancare") + "' WHERE `username` = '" + target.name + "'";
                            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);
                            readerMySQL.ExecuteNonQuery();
                            readerMySQL.Cancel();
                            break;
                        }
                }
            }
        }
        #endregion

    }
    public static class CultChange
    {
        public static string cc(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}