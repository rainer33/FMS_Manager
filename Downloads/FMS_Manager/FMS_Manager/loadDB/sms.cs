using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class csms
    {
        Load ld = new Load();
        public string[,] Sms = new string[50, 53];

        public void LoadSmsDB()  // smsDB ·Îµå
        {
            int i = 0;
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "SELECT ID, smsName, smsNumber, email, smsSend, emailSend, sendTel, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, Za, Zb, Zc, Zd, Ze, Zf, Zg, Zh, Zi, Zj, Zk, Zl, Zm, Zn, Zo, Zp, Zq, Zr, Zs, Zt FROM smssendt";
            MySqlCommand sqlComm = new MySqlCommand(que1, connection2);
            try
            {
                connection2.Open();
                MySqlDataReader sqlReader1 = sqlComm.ExecuteReader();

                while (sqlReader1.Read())
                {
                    Sms[i, 0] = sqlReader1[0].ToString();
                    Sms[i, 1] = sqlReader1[1].ToString();
                    Sms[i, 2] = sqlReader1[2].ToString();
                    Sms[i, 3] = sqlReader1[3].ToString();
                    Sms[i, 4] = sqlReader1[4].ToString();
                    Sms[i, 5] = sqlReader1[5].ToString();
                    Sms[i, 6] = sqlReader1[6].ToString();
                    Sms[i, 7] = sqlReader1[7].ToString();
                    Sms[i, 8] = sqlReader1[8].ToString();
                    Sms[i, 9] = sqlReader1[9].ToString();
                    Sms[i, 10] = sqlReader1[10].ToString();
                    Sms[i, 11] = sqlReader1[11].ToString();
                    Sms[i, 12] = sqlReader1[12].ToString();
                    Sms[i, 13] = sqlReader1[13].ToString();
                    Sms[i, 14] = sqlReader1[14].ToString();
                    Sms[i, 15] = sqlReader1[15].ToString();
                    Sms[i, 16] = sqlReader1[16].ToString();
                    Sms[i, 17] = sqlReader1[17].ToString();
                    Sms[i, 18] = sqlReader1[18].ToString();
                    Sms[i, 19] = sqlReader1[19].ToString();
                    Sms[i, 20] = sqlReader1[20].ToString();
                    Sms[i, 21] = sqlReader1[21].ToString();
                    Sms[i, 22] = sqlReader1[22].ToString();
                    Sms[i, 23] = sqlReader1[23].ToString();
                    Sms[i, 24] = sqlReader1[24].ToString();
                    Sms[i, 25] = sqlReader1[25].ToString();
                    Sms[i, 26] = sqlReader1[26].ToString();
                    Sms[i, 27] = sqlReader1[27].ToString();
                    Sms[i, 28] = sqlReader1[28].ToString();
                    Sms[i, 29] = sqlReader1[29].ToString();
                    Sms[i, 30] = sqlReader1[30].ToString();
                    Sms[i, 31] = sqlReader1[31].ToString();
                    Sms[i, 32] = sqlReader1[32].ToString();
                    Sms[i, 33] = sqlReader1[33].ToString();
                    Sms[i, 34] = sqlReader1[34].ToString();
                    Sms[i, 35] = sqlReader1[35].ToString();
                    Sms[i, 36] = sqlReader1[36].ToString();
                    Sms[i, 37] = sqlReader1[37].ToString();
                    Sms[i, 38] = sqlReader1[38].ToString();
                    Sms[i, 39] = sqlReader1[39].ToString();
                    Sms[i, 40] = sqlReader1[40].ToString();
                    Sms[i, 41] = sqlReader1[41].ToString();
                    Sms[i, 42] = sqlReader1[42].ToString();
                    Sms[i, 43] = sqlReader1[43].ToString();
                    Sms[i, 44] = sqlReader1[44].ToString();
                    Sms[i, 45] = sqlReader1[45].ToString();
                    Sms[i, 46] = sqlReader1[46].ToString();
                    Sms[i, 47] = sqlReader1[47].ToString();
                    Sms[i, 48] = sqlReader1[48].ToString();
                    Sms[i, 49] = sqlReader1[49].ToString();
                    Sms[i, 50] = sqlReader1[50].ToString();
                    Sms[i, 51] = sqlReader1[51].ToString();
                    Sms[i, 52] = sqlReader1[52].ToString();
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
