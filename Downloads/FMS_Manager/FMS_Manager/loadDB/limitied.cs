using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class climitied
    {
        Load ld = new Load();
        public string[,] Limite = new string[22, 10];

        public void LoadLimiteDB()  // limiteDB ·Îµå
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, sysCode, sysCodeNum, sysCodeNumName, dnF, dnC, dnW, upW, upC, upF FROM limitet";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    Limite[i, 0] = sqlReader1[0].ToString();
                    Limite[i, 1] = sqlReader1[1].ToString();
                    Limite[i, 2] = sqlReader1[2].ToString();
                    Limite[i, 3] = sqlReader1[3].ToString();
                    Limite[i, 4] = sqlReader1[4].ToString();
                    Limite[i, 5] = sqlReader1[5].ToString();
                    Limite[i, 6] = sqlReader1[6].ToString();
                    Limite[i, 7] = sqlReader1[7].ToString();
                    Limite[i, 8] = sqlReader1[8].ToString();
                    Limite[i, 9] = sqlReader1[9].ToString();
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
