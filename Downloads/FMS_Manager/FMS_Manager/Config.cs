using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace FMS_Manager
{
    class Config
    {
        private int getIDInfo = 0;

        public int GetIDInfo    //GetIDInfo
        {
            get
            {
                MySqlConnection conn = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
                MySqlCommand sqlComm = new MySqlCommand();
                //sqlComm.CommandType = CommandType.StoredProcedure;
                sqlComm.CommandText = "call sp_getID";
                sqlComm.Connection = conn;

                conn.Open();
                MySqlDataReader reader = sqlComm.ExecuteReader();
                if (reader.Read())
                {
                    getIDInfo = (int)reader.GetValue(0);
                    reader.Close();
                    conn.Close();
                    return getIDInfo;
                }
                else
                {
                    conn.Close();
                    return 0;
                }
            }
        }

        static string rtuID = "ilon";
        public string RtuID
        {
            set { rtuID = value; }
            get { return rtuID; }
        }
        static string rtuPW = "ilon";
        public string RtuPW
        {
            set { rtuPW = value; }
            get { return rtuPW; }
        }
    }
}
