using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class csyscode
    {
        Load ld = new Load();
        public string[,] syscode = new string[46, 4];

        public void LoadSyscodeDB()  // SyscodeDB ·Îµå
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT sysCode, sysName, X, Y FROM syscodet";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    syscode[i, 0] = sqlReader1[0].ToString();
                    syscode[i, 1] = sqlReader1[1].ToString();
                    syscode[i, 2] = sqlReader1[2].ToString();
                    syscode[i, 3] = sqlReader1[3].ToString();
                    i++;
                }
                sqlReader1.Close();
            }


            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
            finally
            {
                connection2.Close();
            }
        }
    }
}
