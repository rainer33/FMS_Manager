using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cthups
    {
        Load ld = new Load();
        public string[,] thups = new string[3, 19];

        public void LoadthupsDB()  // upsDB �ε�
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, vol1, vol2, vol3, vol4, vol5, vol6, vol7, vol8, vol9, vol10, vol11, vol12, vol13, vol14, vol15, vol16, vol17, vol18 FROM thupst";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    thups[i, 0] = sqlReader1[0].ToString();
                    thups[i, 1] = sqlReader1[1].ToString();
                    thups[i, 2] = sqlReader1[2].ToString();
                    thups[i, 3] = sqlReader1[3].ToString();
                    thups[i, 4] = sqlReader1[4].ToString();
                    thups[i, 5] = sqlReader1[5].ToString();
                    thups[i, 6] = sqlReader1[6].ToString();
                    thups[i, 7] = sqlReader1[7].ToString();
                    thups[i, 8] = sqlReader1[8].ToString();
                    thups[i, 9] = sqlReader1[9].ToString();
                    thups[i, 10] = sqlReader1[10].ToString();
                    thups[i, 11] = sqlReader1[11].ToString();
                    thups[i, 12] = sqlReader1[12].ToString();
                    thups[i, 13] = sqlReader1[13].ToString();
                    thups[i, 14] = sqlReader1[14].ToString();
                    thups[i, 15] = sqlReader1[15].ToString();
                    thups[i, 16] = sqlReader1[16].ToString();
                    thups[i, 17] = sqlReader1[17].ToString();
                    thups[i, 18] = sqlReader1[18].ToString();
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
