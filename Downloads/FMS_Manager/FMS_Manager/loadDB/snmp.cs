using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class csnmp
    {
        Load ld = new Load();

        public string[] Snmp = new string[16];

        public void LoadSnmpDB()  // snmpDB ·Îµå
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, snmp1, snmp2, snmp3, snmp4, snmp5, snmp6, snmp7, snmp8, snmp9, snmp10, snmp11, snmp12, snmp13, snmp14, snmp15 FROM snmpcodet";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    Snmp[0] = sqlReader1[0].ToString();
                    Snmp[1] = sqlReader1[1].ToString();
                    Snmp[2] = sqlReader1[2].ToString();
                    Snmp[3] = sqlReader1[3].ToString();
                    Snmp[4] = sqlReader1[4].ToString();
                    Snmp[5] = sqlReader1[5].ToString();
                    Snmp[6] = sqlReader1[6].ToString();
                    Snmp[7] = sqlReader1[7].ToString();
                    Snmp[8] = sqlReader1[8].ToString();
                    Snmp[9] = sqlReader1[9].ToString();
                    Snmp[10] = sqlReader1[10].ToString();
                    Snmp[11] = sqlReader1[11].ToString();
                    Snmp[12] = sqlReader1[12].ToString();
                    Snmp[13] = sqlReader1[13].ToString();
                    Snmp[14] = sqlReader1[14].ToString();
                    Snmp[15] = sqlReader1[15].ToString();
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
