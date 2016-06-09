using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class clogin
    {
        Load ld = new Load();
        public string[] login = new string[9];

        public void LoadLoginDB()  // 로그인DB 로드
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT NO_SEQ, userid, userpwd, userip, isAdmin, isUpdate1, isUpdate2, isUpdate3, updatetime FROM logint";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    login[0] = sqlReader1[0].ToString();
                    login[1] = sqlReader1[1].ToString();
                    login[2] = sqlReader1[2].ToString();
                    login[3] = sqlReader1[3].ToString();
                    login[4] = sqlReader1[4].ToString();
                    login[5] = sqlReader1[5].ToString();
                    login[6] = sqlReader1[6].ToString();
                    login[7] = sqlReader1[7].ToString();
                    login[8] = sqlReader1[8].ToString();
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
