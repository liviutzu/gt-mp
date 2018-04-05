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
    class conector: Script
    {
        public conector()
        {
            API.onPlayerFinishedDownload += OnPlayerFinishedDownloadHandler;
            API.onClientEventTrigger += OnClientEvent;
        }
        public void OnClientEvent(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "database_CheckRegister")
            {
                string email = arguments[0].ToString();
                string password = arguments[1].ToString();
                RegisterToDB(player, password, email); //jucatorul se logheaza
            }
            if (eventName == "database_RegisterCaracter")
            {
                string numer = arguments[0].ToString();
                string prenumer = arguments[1].ToString();
                string varstar = arguments[2].ToString();
                string tarar = arguments[3].ToString();
                CreateCharacter(player, numer, prenumer, varstar, tarar);
            }
            if (eventName == "database_CheckLogin")
            {
                string password = arguments[0].ToString();
                if (password == getAccountPassword(player.name))
                {
                    LoginToDB(player, password);
                }
                else
                {
                    API.triggerClientEvent(player, "login_wrongpw");
                }
            }
        }
        //cand intra pe sv sa il verifice daca are cont sau nu
        private void OnPlayerFinishedDownloadHandler(Client player)
        {
            PreparePlayer(player);
            if (isAccountRegistered(player.name) == true)
            {
                API.triggerClientEvent(player, "database_loginCallBack"); //are cont si se duce la login
            }
            else
            {
                API.triggerClientEvent(player, "database_registerCallBack");//nu are si se duce la register
            }
        }

        #region metode

        public void LoginToDB(Client player, string password)
        {

            WarmUp(player);

            API.triggerClientEvent(player, "database_loggedin");

            if (player.getData("isAccountBanned") == true)
            {
                player.kick("You are banned on this server!");
            }

            if (isAccountRegistered(player.name))
            {
                IncarcareCaracter(player);
                IncarcareInventar(player);
                caractercreator.SendToWorld(player);
                API.triggerClientEvent(player, "Conectare_DestroyAuthCam");
            }
            else
            {
                API.triggerClientEvent(player, "database_CreateCaracter");
            }
        }
        public bool isAccountRegistered(String username)
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
                player.setData("isAccountBanned", true);
            else
                player.setData("isAccountBanned", false);

            if (completat == "Da")
            {
                player.setData("isAccountCompleted", true);
            }
            else
            {
                player.setData("isAccountCompleted", false);
            }
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
        public void RegisterToDB(Client player, string _password, string _email)
        {
            string password = _password;
            string email = _email;
            MySqlCommand readerMySQL = new MySqlCommand("INSERT INTO `users` (`id`, `username`, `password`, `email`) VALUES(NULL, '" + player.name + "', '" + password + "', '" + email + "')", mysqlHandler.connectionMySQL);
            readerMySQL.ExecuteNonQuery();


            MySqlCommand mysql = new MySqlCommand("INSERT INTO `playerinventory` (`playername`) VALUES('" + player.name + "')", mysqlHandler.connectionMySQL);
            mysql.ExecuteNonQuery();

            mysql.Cancel();
            readerMySQL.Cancel();

            API.triggerClientEvent(player, "database_registred");
            API.triggerClientEvent(player, "database_CreateCaracter");
        }


        public void CreateCharacter(Client player, string numer, string prenumer, string varstar, string tarar)
        {
            string cnpr = API.shared.getEntityData(player, "id") + varstar + "18" + "OPS" + "666";
            string stringMySQLQuery = "UPDATE `users` SET `inventorysize` = '20', `telefon` = 'Nu', `taxapescuit` = 'Nu', `permis` = 'Nu', `job` = 'Civil', `nume` = '" + numer + "', `prenume` = '" + prenumer + "', `varsta` = '" + varstar + "', `tara` = '" + tarar + "', `cnp` = '" + cnpr + "', `inregistrat` = 'Da',  `bani` = '500', `card` = '100', `viata` = '100', `mancare` = '100', `sete` = '100', `armura` = '100' WHERE `username` = '" + player.name + "'";

            MySqlCommand readerMySQL = new MySqlCommand(stringMySQLQuery, mysqlHandler.connectionMySQL);

            readerMySQL.ExecuteNonQuery();

            readerMySQL.Cancel();
            player.setData("isAccountCompleted", true);
            IncarcareCaracter(player);
            IncarcareInventar(player);
            caractercreator.SendToWorld(player);

            API.triggerClientEvent(player, "Conectare_DestroyAuthCam");
        }

        public void IncarcareCaracter(Client player)
        {
            API.triggerClientEvent(player, "Player_CreateMenu", player.name);
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
        }
        public void IncarcareInventar(Client player)
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
        void PreparePlayer(Client player)
        {
            API.triggerClientEvent(player, "Conectare_CreateAuthCam");
            player.dimension = 10;
            player.invincible = true;
            player.freezePosition = true;
            player.freeze(true);
            API.setEntityTransparency(player, 0);
        }

        #endregion
    }
}
