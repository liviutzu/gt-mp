using GrandTheftMultiplayer.Server.API;
using MySql.Data.MySqlClient;

namespace ServerGTMP
{
    class mysqlHandler : Script
    {
        public static string host = "104.196.37.63";
        public static string port = "3306";
        public static string username = "databasegtmp";
        public static string parola = "qsMRjHxPpOEcyqia";
        public static string database = "databasegtmp";

        public static MySqlConnection connectionMySQL = new MySqlConnection("datasource = " + host + "; port = " + port + "; username = " + username + "; password = " + parola + "; database = " + database + ";");


        public mysqlHandler()
        {
            API.onResourceStart += onResourceStartHandler;
            API.onResourceStop += onResourceStopHandler;
        }

        public void onResourceStartHandler()
        {

        }

        public void onResourceStopHandler()
        {

        }

        public void connectToDatabase()
        {
            connectionMySQL.Open();

            API.consoleOutput("Conexiunea a fost stabilita!");
        }

        public void disconnectFromDatabase()
        {
            connectionMySQL.Close();

            API.consoleOutput("Conexiunea a crapat!");
        }
    }
}
