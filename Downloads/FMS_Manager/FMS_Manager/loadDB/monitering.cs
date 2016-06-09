using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class cmonitering
    {
        Load ld = new Load();
        public string[,] monitering = new string[79, 18];

        public void LoadMoniteringDB()  // 모니터링DB 로드
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);

            string que1 = "SELECT  syscodenumt.ID, syscodenumt.sysCode, syscodet.sysName, syscodenumt.sysCodeNumName, syscodenumt.monitorCHK, syscodenumt.alarmCHK, syscodenumt.startTime, syscodenumt.endTime, syscodenumt.MON, syscodenumt.TUE, syscodenumt.WED, syscodenumt.THU, syscodenumt.FRI, syscodenumt.SAT, syscodenumt.SUN, syscodenumt.alarmCount, syscodenumt.snmpCount, syscodenumt.sysCodeName FROM syscodenumt INNER JOIN syscodet ON syscodenumt.sysCode = syscodet.sysCode";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    monitering[i, 0] = sqlReader1[0].ToString();
                    monitering[i, 1] = sqlReader1[1].ToString();
                    monitering[i, 2] = sqlReader1[2].ToString();
                    monitering[i, 3] = sqlReader1[3].ToString();
                    monitering[i, 4] = sqlReader1[4].ToString();
                    monitering[i, 5] = sqlReader1[5].ToString();
                    monitering[i, 6] = sqlReader1[6].ToString();
                    monitering[i, 7] = sqlReader1[7].ToString();
                    monitering[i, 8] = sqlReader1[8].ToString();
                    monitering[i, 9] = sqlReader1[9].ToString();
                    monitering[i, 10] = sqlReader1[10].ToString();
                    monitering[i, 11] = sqlReader1[11].ToString();
                    monitering[i, 12] = sqlReader1[12].ToString();
                    monitering[i, 13] = sqlReader1[13].ToString();
                    monitering[i, 14] = sqlReader1[14].ToString();
                    monitering[i, 15] = sqlReader1[15].ToString();
                    monitering[i, 16] = sqlReader1[16].ToString();
                    monitering[i, 17] = sqlReader1[17].ToString();
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
