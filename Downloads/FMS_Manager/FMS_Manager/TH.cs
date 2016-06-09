using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;
using FMS_Manager.WebFms1;

namespace FMS_Manager
{
    class TH
    {
        Timer t1 = new Timer();
        Timer t2 = new Timer();
        Load ld = new Load();
        ClassConvert c = new ClassConvert();
        Config c1 = new Config();
        //시리얼포트 선언
        private System.IO.Ports.SerialPort serialPortTH = new System.IO.Ports.SerialPort();
        private int serialTHSendCount = 0;
        public bool isTHReceiveData = false;
        //private byte[] msgTHhex = new byte[9];  //온습도 핵사값저장
        //private string[] msgTHstr = new string[9];  //온습도 스트링값저장
        private byte[] msgTHhex = new byte[8];  //온습도 핵사값저장
        private string[] msgTHstr = new string[8];  //온습도 스트링값저장

        private System.IO.Ports.SerialPort serialPortTH2 = new System.IO.Ports.SerialPort();
        public bool isTHReceiveData2 = false;
        //private byte[] msgTHhex = new byte[9];  //온습도 핵사값저장
        //private string[] msgTHstr = new string[9];  //온습도 스트링값저장
        //private byte[] msgTHhex2 = new byte[8];  //온습도 핵사값저장
        private string[] msgTHstr2 = new string[8];  //온습도 스트링값저장
        private byte[] msgTHhex2 = new byte[0];  //온습도 핵사값저장
        public void Serialtimer3()  //온습도
        {
            cschedule sch = new cschedule();
            sch.LoadScheduleDB();
            //온습도
            serialPortTH.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortTH_DataReceived);
            serialPortTH.PortName = global::FMS_Manager.Properties.Settings.Default.TH1COM;
            serialPortTH.BaudRate = global::FMS_Manager.Properties.Settings.Default.TH1BITRATE;
            //serialPortTH.ReceivedBytesThreshold = 9;
            serialPortTH.ReceivedBytesThreshold = 8;

            serialPortTH2.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortTH2_DataReceived);
            serialPortTH2.PortName = global::FMS_Manager.Properties.Settings.Default.TH2COM;
            serialPortTH2.BaudRate = global::FMS_Manager.Properties.Settings.Default.TH2BITRATE;
            //serialPortTH.ReceivedBytesThreshold = 9;
            serialPortTH2.ReceivedBytesThreshold = 1;

            t1.Enabled = true;
            t1.Interval = Convert.ToInt32( sch.schedule[2]) * 1000; // 점점점검주기(초) * 1000
            t2.Enabled = false;
            t2.Interval = 1000;

            t1.Elapsed += new ElapsedEventHandler(OnTimedEvent1);
            t2.Elapsed += new ElapsedEventHandler(OnTimedEvent2);
        }
        private void OnTimedEvent1(object source, ElapsedEventArgs e)
        {
            Firedataload1();

            //온습도
            if (serialPortTH.IsOpen)
            {
                serialPortTH.Close();
                serialPortTH.Open();
            }
            else
            {
                serialPortTH.Open();
            }

            if (serialTHSendCount > 9)
            {
                serialTHSendCount = 0;
            }

            int num = serialTHSendCount + 1;
            byte[] sMagTH = new byte[] { (byte)0xFA, (byte)num, (byte)0x0A };
            byte[] chkCRCok = new byte[2];
            int chkCRC2 = 0;
            Array.Copy(sMagTH, 1, chkCRCok, 0, 2);
            chkCRC2 = c.EXOR(chkCRCok.Length, chkCRCok);
            byte[] bin = new byte[] { (byte)0xFA, (byte)num, (byte)0x0A, (byte)chkCRC2, (byte)0x03 };
            //byte[] sMagTH = new byte[] { (byte)num, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x02 };
            //byte[] bin = new byte[sMagTH.Length + 2]; //CRC체크상환받을곳
            //Array.ConstrainedCopy(sMagTH, 0, bin, 0, sMagTH.Length);  //CRC체크하기
            //Array.ConstrainedCopy(c.CRC16(sMagTH.Length, sMagTH), 0, bin, sMagTH.Length, 2);
            serialTHSendCount++;
            serialPortTH.Write(bin, 0, bin.Length);


            //온습도ups
            if (!serialPortTH2.IsOpen)
            {
                //serialPortTH2.Close();
                serialPortTH2.Open();
            }
            //else
            //{
            //    serialPortTH2.Open();
            //}

            byte[] bin2 = new byte[] { (byte)0xFA, (byte)0x01, (byte)0x0A, (byte)0x0B, (byte)0x03 };
            //byte[] sMagTH = new byte[] { (byte)num, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x02 };
            //byte[] bin = new byte[sMagTH.Length + 2]; //CRC체크상환받을곳
            //Array.ConstrainedCopy(sMagTH, 0, bin, 0, sMagTH.Length);  //CRC체크하기
            //Array.ConstrainedCopy(c.CRC16(sMagTH.Length, sMagTH), 0, bin, sMagTH.Length, 2);
            serialPortTH2.Write(bin2, 0, bin2.Length);

            //통신 요청 타이머 가동

            t2.Start();

            
        }

        private void OnTimedEvent2(object source, ElapsedEventArgs e)
        {
            if (isTHReceiveData == true)
            {
                t2.Stop();
            }
            else
            {
                Console.WriteLine("온습도계" + serialTHSendCount + "장비 통신오류");
                ld.logDate("온습도계" + serialTHSendCount + "장비 통신오류");
                string que1 = "update thtempt SET vol1='0', vol2='0' where ID = " + serialTHSendCount + " ";
                ld.update(que1);
                t2.Stop();
            }
            isTHReceiveData = false;

            if (isTHReceiveData2 == false)
            {
                Console.WriteLine("지하UPS 온습도계 통신오류");
                ld.logDate("지하UPS 온습도계 통신오류");
                string que2 = "update thtempt SET vol1='0', vol2='0' where ID = 11 ";
                ld.update(que2);
            }
            else
            {
                isTHReceiveData2 = false;
            }
        }
        //온습도
        private void serialPortTH_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isTHReceiveData = true;
                serialPortTH.Read(msgTHhex, 0, msgTHhex.Length);
                for (int i = 0; i < msgTHhex.Length; i++)
                {
                    msgTHstr[i] = String.Format("{0:X2}", (int)msgTHhex[i]);
                    Console.Write(msgTHstr[i]);
                }
                Console.WriteLine("온습도계1");

                //수신데이터 CRC확인하기
                //byte[] chkCRC1 = new byte[2];
                //byte[] chkCRC2 = new byte[2];
                //byte[] chkCRCok = new byte[23];
                //Array.Copy(msgTHhex, 7, chkCRC1, 0, 2);
                //Array.Copy(msgTHhex, 0, chkCRCok, 0, 7);
                //chkCRC2 = c.CRC16(chkCRCok.Length, chkCRCok);
                byte[] chkCRCok = new byte[5];
                int chkCRC1 = msgTHhex[6];
                int chkCRC2;
                Array.Copy(msgTHhex, 1, chkCRCok, 0, 5);
                chkCRC2 = c.EXOR(chkCRCok.Length, chkCRCok);
                if (chkCRC1 != chkCRC2)
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1}", chkCRC1, chkCRC2);
                    return;
                }
                else
                {
                    AddReceiveDataTH1();
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        private void AddReceiveDataTH1()
        {
            //byte[] chkMsg1 = new byte[2];
            //byte[] chkMsg2 = new byte[2];
            int chkMsg1 = msgTHhex[2];
            int chkMsg2 = msgTHhex[3];
            int chkMsg3 = msgTHhex[4];
            //Array.Copy(msgTHhex, 3, chkMsg1, 0, 2);
            //Array.Copy(msgTHhex, 5, chkMsg2, 0, 2);

            int add = Convert.ToInt32(msgTHhex[1]);
            //int add = Convert.ToInt32(msgTHhex[0]);

            //double markMsg1 = Convert.ToDouble(c.hexToDec(chkMsg1));
            //double markMsg2 = Convert.ToDouble(c.hexToDec(chkMsg2));

            string que1 = "update thtempt SET vol1='" + chkMsg2.ToString("#.#") + "." + chkMsg3.ToString("#.#") + "', vol2='" + chkMsg1.ToString() + "', logDate = now() where ID = " + add + " ";
            //string que1 = "update thtempt SET vol1='" + markMsg1.ToString("#.#") + "', vol2='" + markMsg2.ToString("#.#") + "' where ID = " + add + " ";
            //string que2 = "update thtempt SET logDate = now()  where ID = 1 ";
            ld.update(que1);
            //ld.update(que2);
        }
        //ups 온습도계
        //private void serialPortTH2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        isTHReceiveData2 = true;
        //        serialPortTH2.Read(msgTHhex2, 0, msgTHhex2.Length);
        //        for (int i = 0; i < msgTHhex2.Length; i++)
        //        {
        //            msgTHstr2[i] = String.Format("{0:X2}", (int)msgTHhex2[i]);
        //            Console.Write(msgTHstr2[i]);
        //        }

        //        //수신데이터 CRC확인하기
        //        //byte[] chkCRC1 = new byte[2];
        //        //byte[] chkCRC2 = new byte[2];
        //        //byte[] chkCRCok = new byte[23];
        //        //Array.Copy(msgTHhex, 7, chkCRC1, 0, 2);
        //        //Array.Copy(msgTHhex, 0, chkCRCok, 0, 7);
        //        //chkCRC2 = c.CRC16(chkCRCok.Length, chkCRCok);
        //        byte[] chkCRCok = new byte[5];
        //        int chkCRC1 = msgTHhex2[6];
        //        int chkCRC2;
        //        Array.Copy(msgTHhex2, 1, chkCRCok, 0, 5);
        //        chkCRC2 = c.EXOR(chkCRCok.Length, chkCRCok);
        //        if (chkCRC1 != chkCRC2)
        //        {
        //            Console.WriteLine("CRC오류 - CRC {0}={1}", chkCRC1, chkCRC2);
        //            return;
        //        }
        //        else
        //        {
        //            AddReceiveDataTH2();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        ld.logDate(err.ToString());
        //    }
        //}
        int TotalCount = 0;
        //byte[] TotalRecv = new byte[0];
        byte[] SrcRecv = new byte[0];
        byte[] CurRecv = new byte[0];
        private void serialPortTH2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buffer;
                int size;

                while (serialPortTH2.BytesToRead > 0)
                {
                    

                    buffer = new byte[1024];
                    size = serialPortTH2.Read(buffer, 0, buffer.Length);
                    CurRecv = new byte[size];
                    for (int i = 0; i < size; i++)
                    {
                        CurRecv[i] = buffer[i];
                    }
                    msgTHhex2 = new byte[TotalCount + size];
                    Buffer.BlockCopy(SrcRecv, 0, msgTHhex2, 0, SrcRecv.Length);
                    Buffer.BlockCopy(CurRecv, 0, msgTHhex2, SrcRecv.Length, CurRecv.Length);
                    TotalCount += size;
                    SrcRecv = new byte[TotalCount];
                    SrcRecv = msgTHhex2;
                }

                if (TotalCount >= 8)
                {
                    isTHReceiveData2 = true;
                    foreach (byte b in msgTHhex2)
                    Console.Write("{0:x2}", b);
                    Console.WriteLine("온습도계2");
                    SrcRecv = new byte[0];
                    TotalCount = 0;

                    byte[] chkCRCok = new byte[5];
                    int chkCRC1 = msgTHhex2[6];
                    int chkCRC2;
                    Array.Copy(msgTHhex2, 1, chkCRCok, 0, 5);
                    chkCRC2 = c.EXOR(chkCRCok.Length, chkCRCok);
                    if (chkCRC1 != chkCRC2)
                    {
                        Console.WriteLine("CRC오류 - CRC {0}={1}", chkCRC1, chkCRC2);
                        return;
                    }
                    else
                    {
                        AddReceiveDataTH2();
                    }
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        private void AddReceiveDataTH2()
        {
            //byte[] chkMsg1 = new byte[2];
            //byte[] chkMsg2 = new byte[2];
            int chkMsg1 = msgTHhex2[2];
            int chkMsg2 = msgTHhex2[3];
            int chkMsg3 = msgTHhex2[4];
            //Array.Copy(msgTHhex, 3, chkMsg1, 0, 2);
            //Array.Copy(msgTHhex, 5, chkMsg2, 0, 2);

            //int add = Convert.ToInt32(msgTHhex[0]);

            //double markMsg1 = Convert.ToDouble(c.hexToDec(chkMsg1));
            //double markMsg2 = Convert.ToDouble(c.hexToDec(chkMsg2));

            string que1 = "update thtempt SET vol1='" + chkMsg2.ToString("#.#") + "." + chkMsg3.ToString("#.#") + "', vol2='" + chkMsg1.ToString() + "', logDate = now() where ID = 11 ";
            //string que1 = "update thtempt SET vol1='" + markMsg1.ToString("#.#") + "', vol2='" + markMsg2.ToString("#.#") + "' where ID = " + add + " ";
            //string que2 = "update thtempt SET logDate = now()  where ID = 1 ";
            ld.update(que1);
            //ld.update(que2);
        }

        private void Firedataload1()
        {
            try
            {
                WebFms1.iLON100 myilon = new WebFms1.iLON100();
                myilon.Url = myilon.Url.Replace("localhost", "192.168.1.222");
                myilon.Credentials = new System.Net.NetworkCredential(c1.RtuID, c1.RtuPW);
                myilon.PreAuthenticate = true;

                WebFms1.DS_Read dsRead = new FMS_Manager.WebFms1.DS_Read();
                dsRead.DPType = new WebFms1.DS_ReadDPType[16];
                dsRead.DPType[0] = new FMS_Manager.WebFms1.DS_ReadDPType();
                dsRead.DPType[0].UCPTname = "NVL";

                dsRead.DPType[0].DP = new WebFms1.DS_ReadDPTypeDP[16];
                dsRead.DPType[0].DP[0] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[0].UCPTpointName = "NVL_vol1_200";   //열1
                dsRead.DPType[0].DP[1] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[1].UCPTpointName = "NVL_vol3_200";   //열2
                dsRead.DPType[0].DP[2] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[2].UCPTpointName = "NVL_vol5_200";   //열3
                dsRead.DPType[0].DP[3] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[3].UCPTpointName = "NVL_vol7_200";   //열4
                dsRead.DPType[0].DP[4] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[4].UCPTpointName = "NVL_vol9_200";   //열5
                dsRead.DPType[0].DP[5] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[5].UCPTpointName = "NVL_vol11_200";   //열6
                dsRead.DPType[0].DP[6] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[6].UCPTpointName = "NVL_vol13_200";   //열7
                dsRead.DPType[0].DP[7] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[7].UCPTpointName = "NVL_vol15_200";   //열8
                dsRead.DPType[0].DP[8] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[8].UCPTpointName = "NVL_vol2_200";  //가스1
                dsRead.DPType[0].DP[9] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[9].UCPTpointName = "NVL_vol4_200";   //가스2
                dsRead.DPType[0].DP[10] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[10].UCPTpointName = "NVL_vol6_200";   //가스3
                dsRead.DPType[0].DP[11] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[11].UCPTpointName = "NVL_vol8_200";   //가스4
                dsRead.DPType[0].DP[12] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[12].UCPTpointName = "NVL_vol10_200";   //가스5
                dsRead.DPType[0].DP[13] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[13].UCPTpointName = "NVL_vol12_200";   //가스6
                dsRead.DPType[0].DP[14] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[14].UCPTpointName = "NVL_vol14_200";   //가스7
                dsRead.DPType[0].DP[15] = new WebFms1.DS_ReadDPTypeDP();
                dsRead.DPType[0].DP[15].UCPTpointName = "NVL_vol16_200";   //가스8

                WebFms1.DS_ReadInfo dsReadInfo = myilon.DataServer_Read(dsRead);


                string que1 = "update thfiret SET vol1='" + dsReadInfo.DPType[0].DP[0].UCPTvalue.Value + "', vol2='" + dsReadInfo.DPType[0].DP[1].UCPTvalue.Value + "',  vol3='" + dsReadInfo.DPType[0].DP[2].UCPTvalue.Value + "',  vol4='" + dsReadInfo.DPType[0].DP[3].UCPTvalue.Value + "', vol5='" + dsReadInfo.DPType[0].DP[4].UCPTvalue.Value + "', vol6='" + dsReadInfo.DPType[0].DP[5].UCPTvalue.Value + "', vol7='" + dsReadInfo.DPType[0].DP[6].UCPTvalue.Value + "', vol8='" + dsReadInfo.DPType[0].DP[7].UCPTvalue.Value + "', vol9='" + dsReadInfo.DPType[0].DP[8].UCPTvalue.Value + "',  vol10='" + dsReadInfo.DPType[0].DP[9].UCPTvalue.Value + "', vol11='" + dsReadInfo.DPType[0].DP[10].UCPTvalue.Value + "', vol12='" + dsReadInfo.DPType[0].DP[11].UCPTvalue.Value + "',  vol13='" + dsReadInfo.DPType[0].DP[12].UCPTvalue.Value + "', vol14='" + dsReadInfo.DPType[0].DP[13].UCPTvalue.Value + "',  vol15='" + dsReadInfo.DPType[0].DP[14].UCPTvalue.Value + "', vol16='" + dsReadInfo.DPType[0].DP[15].UCPTvalue.Value + "' where ID = 1 ";
                ld.update(que1);
            }
            catch (Exception err1)
            {
                ld.logDate(err1.ToString());
            }
        }
    }
}
