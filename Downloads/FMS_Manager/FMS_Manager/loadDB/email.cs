using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cemail
    {
        Load ld = new Load();
        public string[] Email = new string[7];

        public void LoadEmailDB()  // emailDB ·Îµå
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, userID, userPwd, fromMail, smtpHost, smtpPort, etc FROM sendmailt";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    Email[0] = sqlReader1[0].ToString();
                    Email[1] = sqlReader1[1].ToString();
                    Email[2] = sqlReader1[2].ToString();
                    Email[3] = sqlReader1[3].ToString();
                    Email[4] = sqlReader1[4].ToString();
                    Email[5] = sqlReader1[5].ToString();
                    Email[6] = sqlReader1[6].ToString();
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
