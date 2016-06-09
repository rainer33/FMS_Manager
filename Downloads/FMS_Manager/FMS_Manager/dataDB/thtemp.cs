using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cthtemp
    {
        Load ld = new Load();
        public string[,] thtemp = new string[11, 4];

        public void LoadthtempDB()  // 온습도DB 로드
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, vol1, vol2, logDate FROM thtempt";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    thtemp[i, 0] = sqlReader1[0].ToString();
                    thtemp[i, 1] = sqlReader1[1].ToString();
                    thtemp[i, 2] = sqlReader1[2].ToString();
                    thtemp[i, 3] = sqlReader1[3].ToString();
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
