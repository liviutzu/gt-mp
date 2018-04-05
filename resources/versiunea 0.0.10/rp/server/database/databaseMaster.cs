using System;
using System.IO;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MySql.Data.MySqlClient;

namespace ServerGTMP
{
    class databaseMaster : Script
    {
        public static bool isSpawned = false;
        public static bool isAccountBanned = false;
        public static bool isAccountCompleted = false;

        public static Client Jucator;

        public databaseMaster()
        {
            API.onPlayerConnected += onPlayerConnectedHandler;
            API.onResourceStart += onResourceStartHandler;
            API.onPlayerFinishedDownload += OnPlayerDownloaded;
            API.onClientEventTrigger += OnClientEvent;
            API.onPlayerDisconnected += onPlayerDisconnectedHandler;
        }

        public void OnClientEvent(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "CheckLogin")
            {
                string password = arguments[0].ToString();
                if (password == getAccountPassword(player.name))
                {
                    LoginToDB(player, password);
                }
                else
                {
                    API.triggerClientEvent(player, "wrongpw");
                }
            }
            if (eventName == "RegisterCaracter")
            {
                string numer = arguments[0].ToString();
                string prenumer = arguments[1].ToString();
                string varstar = arguments[2].ToString();
                string tarar = arguments[3].ToString();
                RegisterCaracter(player, numer, prenumer, varstar, tarar);
            }
            if (eventName == "CheckRegister")
            {
                string email = arguments[0].ToString();
                string password = arguments[1].ToString();

                if (isAccountRegistered(player.name) == false)
                {
                    RegisterToDB(player, password, email);
                }
            }
        }

        #region set
        public static void setPlayerBan(string target, int ban)
        {
            string stringMySQLQuery = "UPDATE `users` SET `banned` = '" + ban + "' WHERE `username` = '" + target + "'";

            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);

            readerMySQL.ExecuteNonQuery();

            readerMySQL.Cancel();
        }

        #endregion


        public void onPlayerConnectedHandler(Client player)
        {
        }
        public void onResourceStartHandler()
        {
            new mysqlHandler().connectToDatabase();

        }
        public void OnPlayerDownloaded(Client player)
        {
            API.triggerClientEvent(player, "CreateAuthCam");
            PreparePlayer(player);
            if (isAccountRegistered(player.name) == true)
            {
                API.triggerClientEvent(player, "loginCallBack");
            }
            else
            {
                API.triggerClientEvent(player, "registerCallBack");
            }
        }

        void PreparePlayer(Client player)
        {
            player.dimension = 10;
            player.invincible = true;
            player.freezePosition = true;
            player.freeze(true);
            API.setEntityTransparency(player, 0);
        }

        void ReleasePlayer(Client player)
        {
            player.dimension = 0;
            player.invincible = false;
            player.freezePosition = false;

            if (LoadPosition(player) == null)
                player.position = gamemode.defaultspawn;
            else
                player.position = LoadPosition(player);

            player.freeze(false);
            API.setEntityTransparency(player, 255);
        }

        public void LoadCharacter(Client player)
        {
            API.triggerClientEvent(player, "Player_CreateMenu", player.name);   
            player.setData("data_trucker_incursa", false);
            string stringMySQLQuery = "SELECT * FROM `users` WHERE `username` = '" + player.name + "'";
            MySqlDataReader readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL).ExecuteReader();

            
            readerMySQL.Read();

            API.setEntityData(player, "id", readerMySQL.GetInt32("id"));
            API.setEntityData(player, "username", readerMySQL.GetString("username"));
            API.setEntityData(player, "email", readerMySQL.GetString("email"));
            API.setEntityData(player, "admin", readerMySQL.GetInt32("admin"));
            API.setEntityData(player, "nume", readerMySQL.GetString("nume"));
            API.setEntityData(player, "prenume", readerMySQL.GetString("prenume"));
            API.setEntityData(player, "job", readerMySQL.GetString("job"));
            API.setEntityData(player, "cnp", readerMySQL.GetString("cnp"));
            API.setEntityData(player, "taxapescuit", readerMySQL.GetString("taxapescuit"));
            API.setEntityData(player, "telefon", readerMySQL.GetString("telefon"));
            API.setEntityData(player, "cartela", readerMySQL.GetInt32("cartela"));
            API.setEntityData(player, "euro", readerMySQL.GetInt32("euro"));
            API.setEntityData(player, "varsta", readerMySQL.GetString("varsta"));
            API.setEntityData(player, "warning", readerMySQL.GetInt32("warning"));
            API.setEntityData(player, "bani", readerMySQL.GetInt32("bani"));
            API.setEntityData(player, "card", readerMySQL.GetInt32("card"));
            API.setEntityData(player, "tara", readerMySQL.GetString("tara"));
            API.setEntityData(player, "viata", readerMySQL.GetInt32("viata"));
            API.setEntityData(player, "armura", readerMySQL.GetInt32("armura"));
            API.setEntityData(player, "omoruri", readerMySQL.GetInt32("omoruri"));
            API.setEntityData(player, "decese", readerMySQL.GetInt32("decese"));
            API.setEntityData(player, "inventorysize", readerMySQL.GetInt32("inventorysize"));
            API.setEntityData(player, "mancare", readerMySQL.GetFloat("mancare"));
            API.setEntityData(player, "sete", readerMySQL.GetFloat("sete"));
            API.setEntityData(player, "permis", readerMySQL.GetString("permis"));
            readerMySQL.Close();
            API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(API.getEntityData(player, "bani")));
            API.triggerClientEvent(player, "UpdateBankHUD", Convert.ToString(API.getEntityData(player, "card")));

            API.setPlayerHealth(player, API.getEntityData(player, "viata"));
            API.setPlayerArmor(player, API.getEntityData(player, "armura"));

            if (player.getData("admin") > 0)
            {
                player.setData("isAdmin", true);
            }

            LoadInventory(player);
            ReleasePlayer(player);
        }
        public void LoadInventory(Client player)
        {
            string stringMySQLQuery = "SELECT * FROM `playerinventory` WHERE `playername` = '" + player.name + "'";
            MySqlDataReader readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL).ExecuteReader();

            readerMySQL.Read();
            API.setEntityData(player, "slot1", readerMySQL.GetString("slot1"));
            API.setEntityData(player, "slot2", readerMySQL.GetString("slot2"));
            API.setEntityData(player, "slot3", readerMySQL.GetString("slot3"));
            API.setEntityData(player, "slot4", readerMySQL.GetString("slot4"));
            API.setEntityData(player, "slot5", readerMySQL.GetString("slot5"));
            API.setEntityData(player, "slot6", readerMySQL.GetString("slot6"));
            API.setEntityData(player, "slot7", readerMySQL.GetString("slot7"));
            API.setEntityData(player, "slot8", readerMySQL.GetString("slot8"));
            API.setEntityData(player, "slot9", readerMySQL.GetString("slot9"));
            API.setEntityData(player, "slot10", readerMySQL.GetString("slot10"));
            API.setEntityData(player, "slot11", readerMySQL.GetString("slot11"));
            API.setEntityData(player, "slot12", readerMySQL.GetString("slot12"));
            API.setEntityData(player, "slot13", readerMySQL.GetString("slot13"));
            API.setEntityData(player, "slot14", readerMySQL.GetString("slot14"));
            API.setEntityData(player, "slot15", readerMySQL.GetString("slot15"));
            API.setEntityData(player, "slot16", readerMySQL.GetString("slot16"));
            API.setEntityData(player, "slot17", readerMySQL.GetString("slot17"));
            API.setEntityData(player, "slot18", readerMySQL.GetString("slot18"));
            API.setEntityData(player, "slot19", readerMySQL.GetString("slot19"));
            API.setEntityData(player, "slot20", readerMySQL.GetString("slot20"));
            API.setEntityData(player, "slot21", readerMySQL.GetString("slot21"));
            API.setEntityData(player, "slot22", readerMySQL.GetString("slot22"));
            API.setEntityData(player, "slot23", readerMySQL.GetString("slot23"));
            API.setEntityData(player, "slot24", readerMySQL.GetString("slot24"));
            API.setEntityData(player, "slot25", readerMySQL.GetString("slot25"));
            API.setEntityData(player, "slot26", readerMySQL.GetString("slot26"));
            API.setEntityData(player, "slot27", readerMySQL.GetString("slot27"));
            API.setEntityData(player, "slot28", readerMySQL.GetString("slot28"));
            API.setEntityData(player, "slot29", readerMySQL.GetString("slot29"));
            API.setEntityData(player, "slot30", readerMySQL.GetString("slot30"));
            API.setEntityData(player, "slot1cont", readerMySQL.GetInt32("slot1cont"));
            API.setEntityData(player, "slot2cont", readerMySQL.GetInt32("slot2cont"));
            API.setEntityData(player, "slot3cont", readerMySQL.GetInt32("slot3cont"));
            API.setEntityData(player, "slot4cont", readerMySQL.GetInt32("slot4cont"));
            API.setEntityData(player, "slot5cont", readerMySQL.GetInt32("slot5cont"));
            API.setEntityData(player, "slot6cont", readerMySQL.GetInt32("slot6cont"));
            API.setEntityData(player, "slot7cont", readerMySQL.GetInt32("slot7cont"));
            API.setEntityData(player, "slot8cont", readerMySQL.GetInt32("slot8cont"));
            API.setEntityData(player, "slot9cont", readerMySQL.GetInt32("slot9cont"));
            API.setEntityData(player, "slot10cont", readerMySQL.GetInt32("slot10cont"));
            API.setEntityData(player, "slot11cont", readerMySQL.GetInt32("slot11cont"));
            API.setEntityData(player, "slot12cont", readerMySQL.GetInt32("slot12cont"));
            API.setEntityData(player, "slot13cont", readerMySQL.GetInt32("slot13cont"));
            API.setEntityData(player, "slot14cont", readerMySQL.GetInt32("slot14cont"));
            API.setEntityData(player, "slot15cont", readerMySQL.GetInt32("slot15cont"));
            API.setEntityData(player, "slot16cont", readerMySQL.GetInt32("slot16cont"));
            API.setEntityData(player, "slot17cont", readerMySQL.GetInt32("slot17cont"));
            API.setEntityData(player, "slot18cont", readerMySQL.GetInt32("slot18cont"));
            API.setEntityData(player, "slot19cont", readerMySQL.GetInt32("slot19cont"));
            API.setEntityData(player, "slot20cont", readerMySQL.GetInt32("slot20cont"));
            API.setEntityData(player, "slot21cont", readerMySQL.GetInt32("slot21cont"));
            API.setEntityData(player, "slot22cont", readerMySQL.GetInt32("slot22cont"));
            API.setEntityData(player, "slot23cont", readerMySQL.GetInt32("slot23cont"));
            API.setEntityData(player, "slot24cont", readerMySQL.GetInt32("slot24cont"));
            API.setEntityData(player, "slot25cont", readerMySQL.GetInt32("slot25cont"));
            API.setEntityData(player, "slot26cont", readerMySQL.GetInt32("slot26cont"));
            API.setEntityData(player, "slot27cont", readerMySQL.GetInt32("slot27cont"));
            API.setEntityData(player, "slot28cont", readerMySQL.GetInt32("slot28cont"));
            API.setEntityData(player, "slot29cont", readerMySQL.GetInt32("slot29cont"));
            API.setEntityData(player, "slot30cont", readerMySQL.GetInt32("slot30cont"));
            readerMySQL.Close();
        }
        private void RegisterCaracter(Client player, string numer, string prenumer, string varstar, string tarar)
        {
            string cnpr = API.getEntityData(player, "id") + varstar + "18" + "OPS" + "666";
            string stringMySQLQuery = "UPDATE `users` SET `inventorysize` = '20', `telefon` = 'Nu', `taxapescuit` = 'Nu', `permis` = 'Nu', `job` = 'Civil', `nume` = '" + numer + "', `prenume` = '" + prenumer + "', `varsta` = '" + varstar + "', `tara` = '" + tarar + "', `cnp` = '" + cnpr + "', `inregistrat` = 'Da',  `bani` = '500', `card` = '100', `viata` = '100', `mancare` = '100', `sete` = '100', `armura` = '100' WHERE `username` = '" + player.name + "'";

            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);

            readerMySQL.ExecuteNonQuery();

            readerMySQL.Cancel();
            LoadCharacter(player);

            caractercreator.SendToWorld(player);
            API.triggerClientEvent(player, "DestroyAuthCam");
        }

        public void LoginToDB(Client player, string password)
        {
            WarmUp(player);
            API.triggerClientEvent(player, "loggedin");
            API.setEntityData(player, "isConnected", true);
            Jucator = player;
            if (isAccountBanned == true)
            {
                player.kick("You are banned on this server!");
            }

            if (isAccountCompleted == true)
            {
                LoadCharacter(player);
                caractercreator.SendToWorld(player);
                API.triggerClientEvent(player, "DestroyAuthCam");
            }
            else
            {
                API.triggerClientEvent(player, "CreateCaracter");
            }
        }

        public void RegisterToDB(Client player, string password, string email)
        {
            MySqlCommand readerMySQL = new MySqlCommand("INSERT INTO `users` (`id`, `username`, `password`, `email`) VALUES(NULL, '" + player.name + "', '" + password + "', '" + email + "')", mysqlHandler.connectionMySQL);
            readerMySQL.ExecuteNonQuery();

            readerMySQL.Cancel();
            MySqlCommand mysql = new MySqlCommand("INSERT INTO `playerinventory` (`playername`) VALUES('" + player.name + "')", mysqlHandler.connectionMySQL);
            mysql.ExecuteNonQuery();

            mysql.Cancel();

            API.triggerClientEvent(player, "registred");
            if (password != null && email != null)
            {
                LoginToDB(player, password);
            }
           
        }

        public void onPlayerDisconnectedHandler(Client player, String motiv)
        {
            if (isSpawned == true)
            {
                SavePosition(player);
                
                SavePlayer(player);

            }
        }

        void SavePlayer(Client player)
        {
            float posX = player.position.X;
            float posY = player.position.Y;
            float posZ = player.position.Z;

            string stringMySQLQuery = "UPDATE `users` SET `inventorysize` = '" + API.getEntityData(player, "inventorysize") + "', `euro` = '" + API.getEntityData(player, "euro") + "', `cartela` = '" + API.getEntityData(player, "cartela") + "', `telefon` = '" + API.getEntityData(player, "telefon") + "', `taxapescuit` = '" + API.getEntityData(player, "taxapescuit") + "', `decese` = '" + API.getEntityData(player, "decese") + "', `omoruri` = '" + API.getEntityData(player, "omoruri") + "', `permis` = '" + API.getEntityData(player, "permis") + "', `job` = '" + API.getEntityData(player, "job") + "', `warning` = '" + API.getEntityData(player, "warning") + "', `admin` = '" + API.getEntityData(player, "admin") + "', `nume` = '" + API.getEntityData(player, "nume") + "', `prenume` = '" + API.getEntityData(player, "prenume") + "', `cnp` = '" + API.getEntityData(player, "cnp") + "', `varsta` = '" + API.getEntityData(player, "varsta") + "', `bani` = '" + API.getEntityData(player, "bani") + "', `card` = '" + API.getEntityData(player, "card") + "', `viata` = '" + API.getEntityData(player, "viata") + "', `armura` = '" + API.getEntityData(player, "armura") + "', `mancare` = '" + API.getEntityData(player, "mancare") + "', `sete` = '" + API.getEntityData(player, "sete") + "', `posX` = '" + posX + "', `posY` = '" + posY + "', `posZ` = '" + posZ + "' WHERE `username` = '" + player.name + "'";

            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);

            readerMySQL.ExecuteNonQuery();

            readerMySQL.Cancel();
        }

        void SavePosition(Client player)
        {
            float posX = player.position.X;
            float posY = player.position.Y;
            float posZ = player.position.Z;

            string stringMySQLQuery = "UPDATE `users` SET `posX` = '" + posX + "', `posY` = '" + posY + "', `posZ` = '" + posZ + "' WHERE `username` = '" + player.name + "'";

            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);

            readerMySQL.ExecuteNonQuery();

            readerMySQL.Cancel();
        }
        public Vector3 LoadPosition(Client player)
        {
            Vector3 pozitie = new Vector3();
            string stringMySQLQuery = "SELECT * FROM `users` WHERE `username` = '" + player.name + "'";
            MySqlDataReader readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL).ExecuteReader();

            readerMySQL.Read();
            float posX = readerMySQL.GetFloat("posX");
            float posY = readerMySQL.GetFloat("posY");
            float posZ = readerMySQL.GetFloat("posZ");
            readerMySQL.Close();
            pozitie = new Vector3(posX, posY, posZ);
            if (pozitie == new Vector3(0, 0, 0))
                pozitie = null;
            return pozitie;
        }

        public class AccountData
        {
            public Client Client;
            public NetHandle NetHandle;
            public int IDDatabase;
            public bool isConnected { get; set; }
            public int id { get; set; }
            public string username { get; set; }
            public int admin { get; set; }
            public string email { get; set; }
            public string PlayerFullName { get; set; }
            public string job { get; set; }
            public string telefon { get; set; }
            public int cartela { get; set; }
            public int bani { get; set; }
            public int euro { get; set; }
            public int card { get; set; }
            public string tara { get; set; }
            public string taxapescuit { get; set; }
            public int warning { get; set; }
            public int viata { get; set; }
            public int armura { get; set; }
            public int decese { get; set; }
            public int factiune { get; set; }
            public int inventorysize { get; set; }
            public int omoruri { get; set; }
            public float sete { get; set; }
            public float mancare { get; set; }
            public string varsta { get; set; }
            public string cnp { get; set; }
            public string nume { get; set; }
            public string prenume { get; set; }
            public string permis { get; set; }
            public bool Logged { get; set; }
            public bool data_trucker_incursa { get; set; }
            public bool Job_Trucker_AreCamion { get; set; }
            public int Job_Trucker_Reward { get;set; }
            public int Job_Trucker_IndexCamion { get; set; }
            public int Job_Trucker_IndexRemorca { get; set; }
            public ColShape Job_Trucker_Cursa_ColShape { get; set; }
            public bool TaxaPermisPlatita { get; set; }
            public int ScoalaDeSoferi_IndexMasina { get; set; }
            public Marker ScoalaDeSofer_Marker { get; set; }
            public ColShape ScoalaDeSoferi_ColShape { get; set; }
            public ColShape ScoalaDeSoferi_PerimetruExamen { get; set; }
            public Blip ScoalaDeSoferi_Blip { get; set; }
            public int ScoalaDeSoferi_Index { get; set; }
            public bool ScoalaDeSoferi_InExamen { get; set; }
            public bool Player_Arestat { get; set; }
            public bool Player_Jailed { get; set; }
            public bool isAdmin { get; set; }
            #region INV
            public string slot1 { get; set; }
            public string slot2 { get; set; }
            public string slot3 { get; set; }
            public string slot4 { get; set; }
            public string slot5 { get; set; }
            public string slot6 { get; set; }
            public string slot7 { get; set; }
            public string slot8 { get; set; }
            public string slot9 { get; set; }
            public string slot10 { get; set; }
            public string slot11 { get; set; }
            public string slot12 { get; set; }
            public string slot13 { get; set; }
            public string slot14 { get; set; }
            public string slot15 { get; set; }
            public string slot16 { get; set; }
            public string slot17 { get; set; }
            public string slot18 { get; set; }
            public string slot19 { get; set; }
            public string slot20 { get; set; }
            public string slot21 { get; set; }
            public string slot22 { get; set; }
            public string slot23 { get; set; }
            public string slot24 { get; set; }
            public string slot25 { get; set; }
            public string slot26 { get; set; }
            public string slot27 { get; set; }
            public string slot28 { get; set; }
            public string slot29 { get; set; }
            public string slot30 { get; set; }
            public string slot1cont { get; set; }
            public string slot2cont { get; set; }
            public string slot3cont { get; set; }
            public string slot4cont { get; set; }
            public string slot5cont { get; set; }
            public string slot6cont { get; set; }
            public string slot7cont { get; set; }
            public string slot8cont { get; set; }
            public string slot9cont { get; set; }
            public string slot10cont { get; set; }
            public string slot11cont { get; set; }
            public string slot12cont { get; set; }
            public string slot13cont { get; set; }
            public string slot14cont { get; set; }
            public string slot15cont { get; set; }
            public string slot16cont { get; set; }
            public string slot17cont { get; set; }
            public string slot18cont { get; set; }
            public string slot19cont { get; set; }
            public string slot20cont { get; set; }
            public string slot21cont { get; set; }
            public string slot22cont { get; set; }
            public string slot23cont { get; set; }
            public string slot24cont { get; set; }
            public string slot25cont { get; set; }
            public string slot26cont { get; set; }
            public string slot27cont { get; set; }
            public string slot28cont { get; set; }
            public string slot29cont { get; set; }
            public string slot30cont { get; set; }

            #endregion
            public AccountData(Client _client)
            {
                Client = _client;
                Logged = false;
                id = 0;
                NetHandle = _client.handle;
                IDDatabase = -1;
            }

        }

        public void WarmUp(Client player)
        {
            string stringMySQLQuery = "SELECT * FROM `users` WHERE `username` = '" + player.name + "'";
            MySqlDataReader readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL).ExecuteReader();

            readerMySQL.Read();
            string completat = readerMySQL.GetString("inregistrat");
            int banat = readerMySQL.GetInt32("banned");
            API.setEntityData(player, "id", readerMySQL.GetInt32("id"));
            readerMySQL.Close();

            if (banat > 0)
                isAccountBanned = true;
            else
                isAccountBanned = false;

            if(completat == "Da")
            {
                isAccountCompleted = true;
            }
            else
            {
                isAccountCompleted = false;
            }
        }

        public static bool isAccountRegistered(String username)
        {
            string stringMySQLQuery = "SELECT * FROM `users` WHERE `username` = '" + username + "'";
            MySqlDataReader readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL).ExecuteReader();
            bool result;

            if (readerMySQL.HasRows)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            readerMySQL.Close();
            return result;
        }


        public bool isPlayerConnected(Client sender, Client target)
        {
            bool result;
            if(API.getEntityData(target, "isConnected") == true)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public string getAccountPassword(String username)
        {
            string stringMySQLQuery = "SELECT * FROM `users` WHERE `username` = '" + username + "'";
            MySqlDataReader readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL).ExecuteReader();
            string password;

            readerMySQL.Read();
            password = readerMySQL.GetString("password");
            readerMySQL.Close();
            return password;
        }
    }
}