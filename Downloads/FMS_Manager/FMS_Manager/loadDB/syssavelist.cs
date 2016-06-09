using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class csyssavelist
    {
        Load ld = new Load();
        public string[,] syssavelist = new string[11, 5];

        public void LoadSyssavelistDB()  // SyssavelistDB ·Îµå
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, sysCode, sysName, saveCHK, saveTime, etc FROM syssavelistt";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    syssavelist[i, 0] = sqlReader1[0].ToString();
                    syssavelist[i, 1] = sqlReader1[1].ToString();
                    syssavelist[i, 2] = sqlReader1[2].ToString();
                    syssavelist[i, 3] = sqlReader1[3].ToString();
                    syssavelist[i, 4] = sqlReader1[4].ToString();
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
