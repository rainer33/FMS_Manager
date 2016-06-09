using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cthhvic
    {
        Load ld = new Load();
        public string[,] thhvic = new string[12, 26];

        public void LoadthhvicDB()  // 항온항습기DB 로드
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, vol1, vol2, vol3, vol4, vol5, vol6, vol7, vol8, vol9, vol10, vol11, vol12, vol13, vol14, vol15, vol16, vol17, vol18, vol19, vol20, vol21, vol22, vol23, vol24, vol25 FROM thhvict";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    thhvic[i, 0] = sqlReader1[0].ToString();
                    thhvic[i, 1] = sqlReader1[1].ToString();
                    thhvic[i, 2] = sqlReader1[2].ToString();
                    thhvic[i, 3] = sqlReader1[3].ToString();
                    thhvic[i, 4] = sqlReader1[4].ToString();
                    thhvic[i, 5] = sqlReader1[5].ToString();
                    thhvic[i, 6] = sqlReader1[6].ToString();
                    thhvic[i, 7] = sqlReader1[7].ToString();
                    thhvic[i, 8] = sqlReader1[8].ToString();
                    thhvic[i, 9] = sqlReader1[9].ToString();
                    thhvic[i, 10] = sqlReader1[10].ToString();
                    thhvic[i, 11] = sqlReader1[11].ToString();
                    thhvic[i, 12] = sqlReader1[12].ToString();
                    thhvic[i, 13] = sqlReader1[13].ToString();
                    thhvic[i, 14] = sqlReader1[14].ToString();
                    thhvic[i, 15] = sqlReader1[15].ToString();
                    thhvic[i, 16] = sqlReader1[16].ToString();
                    thhvic[i, 17] = sqlReader1[17].ToString();
                    thhvic[i, 18] = sqlReader1[18].ToString();
                    thhvic[i, 19] = sqlReader1[19].ToString();
                    thhvic[i, 20] = sqlReader1[20].ToString();
                    thhvic[i, 21] = sqlReader1[21].ToString();
                    thhvic[i, 22] = sqlReader1[22].ToString();
                    thhvic[i, 23] = sqlReader1[23].ToString();
                    thhvic[i, 24] = sqlReader1[24].ToString();
                    thhvic[i, 25] = sqlReader1[25].ToString();
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
