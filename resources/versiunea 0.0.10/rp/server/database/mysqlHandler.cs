using GrandTheftMultiplayer.Server.API;
using MySql.Data.MySqlClient;

namespace ServerGTMP
{
    class mysqlHandler : Script
    {
        public static string host = "188.241.14.224";
        public static string port = "3306";
        public static string username = "servergtmp";
        public static string parola = "QZLkswqM4v4AhREy";
        public static string database = "servergtmp";

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
