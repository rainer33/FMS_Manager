using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cthleak
    {
        Load ld = new Load();

        public string[,] thleak = new string[2, 7];

        public void LoadthleakDB()  // 누수감지DB 로드
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, vol1, vol2, vol3, vol4, vol5, vol6 FROM thleakt";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    thleak[i, 0] = sqlReader1[0].ToString();
                    thleak[i, 1] = sqlReader1[1].ToString();
                    thleak[i, 2] = sqlReader1[2].ToString();
                    thleak[i, 3] = sqlReader1[3].ToString();
                    thleak[i, 4] = sqlReader1[4].ToString();
                    thleak[i, 5] = sqlReader1[5].ToString();
                    thleak[i, 6] = sqlReader1[6].ToString();
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
