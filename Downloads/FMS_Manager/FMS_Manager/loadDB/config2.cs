using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cconfig2
    {
        Load ld = new Load();
        public string[] config = new string[6];

        public void LoadConfigDB()  // configDB �ε�
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, reportDB, reportSERVER, reportUSER, reportPASSWD, smsTEL FROM configt";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    config[0] = sqlReader1[0].ToString();
                    config[1] = sqlReader1[1].ToString();
                    config[2] = sqlReader1[2].ToString();
                    config[3] = sqlReader1[3].ToString();
                    config[4] = sqlReader1[4].ToString();
                    config[5] = sqlReader1[5].ToString();
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
