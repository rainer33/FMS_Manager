using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cschedule
    {
        Load ld = new Load();
        public string[] schedule = new string[9];

        public void LoadScheduleDB()  // configDB ·Îµå
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, rtu, hvic, elec, eventlog, alarmlog, minlog, hourlog, daylog FROM schedulet";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    schedule[0] = sqlReader1[0].ToString();
                    schedule[1] = sqlReader1[1].ToString();
                    schedule[2] = sqlReader1[2].ToString();
                    schedule[3] = sqlReader1[3].ToString();
                    schedule[4] = sqlReader1[4].ToString();
                    schedule[5] = sqlReader1[5].ToString();
                    schedule[6] = sqlReader1[6].ToString();
                    schedule[7] = sqlReader1[7].ToString();
                    schedule[8] = sqlReader1[8].ToString();
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
