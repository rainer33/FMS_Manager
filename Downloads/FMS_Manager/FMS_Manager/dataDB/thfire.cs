using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cthfire
    {
        Load ld = new Load();
        public string[] thfire = new string[17];

        public void LoadthfireDB()  // 화재감지DB 로드
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, vol1, vol2, vol3, vol4, vol5, vol6, vol7, vol8, vol9, vol10, vol11, vol12, vol13, vol14, vol15, vol16 FROM thfiret";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    thfire[0] = sqlReader1[0].ToString();
                    thfire[1] = sqlReader1[1].ToString();
                    thfire[2] = sqlReader1[2].ToString();
                    thfire[3] = sqlReader1[3].ToString();
                    thfire[4] = sqlReader1[4].ToString();
                    thfire[5] = sqlReader1[5].ToString();
                    thfire[6] = sqlReader1[6].ToString();
                    thfire[7] = sqlReader1[7].ToString();
                    thfire[8] = sqlReader1[8].ToString();
                    thfire[9] = sqlReader1[9].ToString();
                    thfire[10] = sqlReader1[10].ToString();
                    thfire[11] = sqlReader1[11].ToString();
                    thfire[12] = sqlReader1[12].ToString();
                    thfire[13] = sqlReader1[13].ToString();
                    thfire[14] = sqlReader1[14].ToString();
                    thfire[15] = sqlReader1[15].ToString();
                    thfire[16] = sqlReader1[16].ToString();
                }
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
