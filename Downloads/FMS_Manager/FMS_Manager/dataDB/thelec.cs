using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cthelec
    {
        Load ld = new Load();
        public string[,] thelec = new string[10, 10];

        public void LoadthelecDB()  // 분전반DB 로드
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, vol1, vol2, vol3, vol4, vol5, vol6, vol7, vol8, vol9 FROM thelect";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    thelec[i, 0] = sqlReader1[0].ToString();
                    thelec[i, 1] = sqlReader1[1].ToString();
                    thelec[i, 2] = sqlReader1[2].ToString();
                    thelec[i, 3] = sqlReader1[3].ToString();
                    thelec[i, 4] = sqlReader1[4].ToString();
                    thelec[i, 5] = sqlReader1[5].ToString();
                    thelec[i, 6] = sqlReader1[6].ToString();
                    thelec[i, 7] = sqlReader1[7].ToString();
                    thelec[i, 8] = sqlReader1[8].ToString();
                    thelec[i, 9] = sqlReader1[9].ToString();
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
