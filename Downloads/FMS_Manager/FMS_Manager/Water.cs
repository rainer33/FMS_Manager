using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class Water
    {
        //시리얼포트 선언
        public Timer t1 = new Timer();
        public Timer t2 = new Timer();
        Load ld = new Load();
        ClassConvert c = new ClassConvert();
        private System.IO.Ports.SerialPort serialPortLeak = new System.IO.Ports.SerialPort();
        public System.IO.Ports.SerialPort serialPortHvic1 = new System.IO.Ports.SerialPort();
        public System.IO.Ports.SerialPort serialPortHvic2 = new System.IO.Ports.SerialPort();
        private int serialLeakSendCount = 1;
        private int serialHvicSendCount1 = 0;
        private int serialHvicSendCount2 = 0;
        public bool isleakReceiveData = false;
        public bool isHvicReceiveData1 = false;
        public bool isHvicReceiveData2 = false;
        private byte[] msgLEAKhex = new byte[15];  //누수감지 핵사값저장
        private byte[] msgHvichex1 = new byte[56];  //항온항습1 핵사값저장
        private byte[] msgHvichex2 = new byte[56];  //항온항습2 핵사값저장
        private string[] msgLEAKstr = new string[15];  //누수감지 스트링값저장
        private string[] msgHvicstr1 = new string[56];  //항온항습1 스트링값저장
        private string[] msgHvicstr2 = new string[56];  //항온항습2 스트링값저장
        private byte hvicnum1;
        private byte hvicnum2;
        public void Serialtimer2()  //누수, 항온항습
        {
            cschedule sch = new cschedule();
            sch.LoadScheduleDB();
            //누수감지
            serialPortLeak.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortLeak_DataReceived);
            serialPortLeak.PortName = global::FMS_Manager.Properties.Settings.Default.LEAKCOM;
            serialPortLeak.BaudRate = global::FMS_Manager.Properties.Settings.Default.LEAKBITRATE;
            serialPortLeak.ReceivedBytesThreshold = 15;

            //항온항습기1(장비실7)
            serialPortHvic1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortHvic1_DataReceived);
            serialPortHvic1.PortName = global::FMS_Manager.Properties.Settings.Default.HVIC1COM;
            serialPortHvic1.BaudRate = global::FMS_Manager.Properties.Settings.Default.HVIC1BITRATE;
            serialPortHvic1.ReceivedBytesThreshold = 56;

            //항온항습기2(8층5)
            serialPortHvic2.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortHvic2_DataReceived);
            serialPortHvic2.PortName = global::FMS_Manager.Properties.Settings.Default.HVIC2COM;
            serialPortHvic2.BaudRate = global::FMS_Manager.Properties.Settings.Default.HVIC2BITRATE;
            serialPortHvic2.ReceivedBytesThreshold = 56;

            t1.Enabled = true;
            t1.Interval = Convert.ToInt32(sch.schedule[3]) * 1000; // 점점점검주기(초) * 1000
            t2.Enabled = false;
            t2.Interval = 1000;

            t1.Elapsed += new ElapsedEventHandler(OnTimedEvent1);
            t2.Elapsed += new ElapsedEventHandler(OnTimedEvent2);
        }
        public void Stop1(byte[] sMagHvic1)
        {
            t1.Enabled = false;
            while (Timer1)
            {
                System.Threading.Thread.Sleep(100);
            }
            try
            {
                if (serialPortHvic1.IsOpen)
                {
                    serialPortHvic1.Close();
                    serialPortHvic1.Open();
                }
                else
                {
                    serialPortHvic1.Open();
                }
                serialPortHvic1.Write(sMagHvic1, 0, sMagHvic1.Length);
                serialPortHvic1.Close();
                t1.Start();
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        public void Stop2(byte[] sMagHvic2)
        {
            t1.Enabled = false;
            while (Timer1)
            {
                System.Threading.Thread.Sleep(100);
            }
            try
            {
                if (serialPortHvic2.IsOpen)
                {
                    serialPortHvic2.Close();
                    serialPortHvic2.Open();
                }
                else
                {
                    serialPortHvic2.Open();
                }
                serialPortHvic2.Write(sMagHvic2, 0, sMagHvic2.Length);
                serialPortHvic2.Close();
                t1.Start();
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        private bool Timer1;
        private void OnTimedEvent1(object source, ElapsedEventArgs e)
        {
            Timer1 = true;
            //누수
            if (serialPortLeak.IsOpen)
            {
                serialPortLeak.Close();
                serialPortLeak.Open();
            }
            else
            {
                serialPortLeak.Open();
            }

            if (serialLeakSendCount > 2)
            {
                serialLeakSendCount = 1;
            }
            switch (serialLeakSendCount)
            {
                case 1:
                    byte[] sMagLeak1 = new byte[] { (byte)01, (byte)0x03, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05, (byte)0x85, (byte)0xC9 }; //전송커멘더
                    serialPortLeak.Write(sMagLeak1, 0, sMagLeak1.Length);
                    break;
                case 2:
                    byte[] sMagLeak2 = new byte[] { (byte)02, (byte)0x03, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x05, (byte)0x85, (byte)0xFA }; //전송커멘더
                    serialPortLeak.Write(sMagLeak2, 0, sMagLeak2.Length);
                    break;
            }
            serialLeakSendCount++;

            //항온항습기1
            if (serialPortHvic1.IsOpen)
            {
                serialPortHvic1.Close();
                serialPortHvic1.Open();
            }
            else
            {
                serialPortHvic1.Open();
            }

            if (serialHvicSendCount1 > 6)
            {
                serialHvicSendCount1 = 0;
            }

            int num1 = serialHvicSendCount1 + 1;
            
            switch (num1)
            {
                case 1:
                    hvicnum1 = 0x61;
                    break;
                case 2:
                    hvicnum1 = 0x62;
                    break;
                case 3:
                    hvicnum1 = 0x63;
                    break;
                case 4:
                    hvicnum1 = 0x64;
                    break;
                case 5:
                    hvicnum1 = 0x65;
                    break;
                case 6:
                    hvicnum1 = 0x66;
                    break;
                case 7:
                    hvicnum1 = 0x67;
                    break;
            }

            byte[] sMagHvic1 = new byte[] { hvicnum1, (byte)0x3F, (byte)0x3F };
            serialPortHvic1.Write(sMagHvic1, 0, sMagHvic1.Length);

            serialHvicSendCount1++;


            //항온항습기2
            if (serialPortHvic2.IsOpen)
            {
                serialPortHvic2.Close();
                serialPortHvic2.Open();
            }
            else
            {
                serialPortHvic2.Open();
            }

            if (serialHvicSendCount2 > 4)
            {
                serialHvicSendCount2 = 0;
            }

            int num2 = serialHvicSendCount2 + 1;

            switch (num2)
            {
                case 1:
                    hvicnum2 = 0x61;
                    break;
                case 2:
                    hvicnum2 = 0x62;
                    break;
                case 3:
                    hvicnum2 = 0x63;
                    break;
                case 4:
                    hvicnum2 = 0x64;
                    break;
                case 5:
                    hvicnum2 = 0x65;
                    break;
            }

            byte[] sMagHvic4 = new byte[] { hvicnum2, (byte)0x3F, (byte)0x3F };
            serialPortHvic2.Write(sMagHvic4, 0, sMagHvic4.Length);

            serialHvicSendCount2++;


            //통신 요청 타이머 가동

            t2.Start();
            Timer1 = false;
        }


        private void OnTimedEvent2(object source, ElapsedEventArgs e)
        {
            //누수
            if (isleakReceiveData == true)
            {
                t2.Stop();
            }
            else
            {
                Console.WriteLine("누수" + (serialLeakSendCount - 1) + "장비 통신오류");
                ld.logDate("누수" + (serialLeakSendCount - 1) + "장비 통신오류");
                string que1 = "update thleakt SET vol1='err' where ID = " + (serialLeakSendCount - 1) + " ";
                ld.update(que1);
                t2.Stop();
            }
            isleakReceiveData = false;

            //항온항습기1
            if (isHvicReceiveData1 == false)
            {
                Console.WriteLine("항온항습" + serialHvicSendCount1 + "번 장비 통신오류");
                ld.logDate("항온항습" + serialHvicSendCount1 + "번 장비 통신오류");
                string que2 = "update thhvict SET vol1='err' where ID = " + serialHvicSendCount1 + " ";
                ld.update(que2);
            }
            else
            {
                isHvicReceiveData1 = false;
            }
            //항온항습기2
            if (isHvicReceiveData2 == false)
            {
                Console.WriteLine("항온항습" + (serialHvicSendCount2 + 7) + "번 장비 통신오류");
                ld.logDate("항온항습" + (serialHvicSendCount2 + 7) + "번 장비 통신오류");
                string que3 = "update thhvict SET vol1='err' where ID = " + (serialHvicSendCount2 + 7) + " ";
                ld.update(que3);
            }
            else
            {
                isHvicReceiveData2 = false;
            }
        } 



        //누수감지
        private void serialPortLeak_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isleakReceiveData = true;
                serialPortLeak.Read(msgLEAKhex, 0, msgLEAKhex.Length);
                for (int i = 0; i < msgLEAKhex.Length; i++)
                {
                    msgLEAKstr[i] = String.Format("{0:X2}", (int)msgLEAKhex[i]);
                    Console.Write(msgLEAKstr[i]);
                }
                Console.WriteLine("누수감지");

                //수신데이터 CRC확인하기
                byte[] chkCRC1 = new byte[2];
                byte[] chkCRC2 = new byte[2];
                byte[] chkCRCok = new byte[13];
                Array.Copy(msgLEAKhex, 13, chkCRC1, 0, 2);
                Array.Copy(msgLEAKhex, 0, chkCRCok, 0, 13);
                chkCRC2 = c.CRC16(chkCRCok.Length, chkCRCok);
                if (chkCRC1[0] != chkCRC2[0] || chkCRC1[1] != chkCRC2[1])
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1} or CRC {2}={3}", chkCRC1[0], chkCRC2[0], chkCRC1[1], chkCRC2[1]);
                    return;
                }
                else
                {
                    AddReceiveDataLEAK();
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        private void AddReceiveDataLEAK()
        {
            byte[] chkMsg1 = new byte[2];
            byte[] chkMsg2 = new byte[2];
            byte[] chkMsg3 = new byte[2];
            byte[] chkMsg4 = new byte[2];

            int addLeak = (int)msgLEAKhex[0];
            int isFault = (int)msgLEAKhex[3];
            int isLeak = (int)msgLEAKhex[4];
            if ((int)msgLEAKhex[4] != 0)
            {
                System.Collections.BitArray leakBit = new System.Collections.BitArray(8);
                leakBit = c.bitToString(msgLEAKhex[4]);
                if (leakBit[0] == true)
                {
                    Array.Copy(msgLEAKhex, 5, chkMsg1, 0, 2);
                }
                if (leakBit[1] == true)
                {
                    Array.Copy(msgLEAKhex, 7, chkMsg2, 0, 2);
                }
                if (leakBit[2] == true)
                {
                    Array.Copy(msgLEAKhex, 9, chkMsg3, 0, 2);
                }
                if (leakBit[3] == true)
                {
                    Array.Copy(msgLEAKhex, 11, chkMsg4, 0, 2);
                }

                string que1 = "update thleakt SET vol1='" + isFault.ToString() + "', vol2='" + isLeak.ToString() + "', vol3='" + c.hexToDec(chkMsg1).ToString() + "', vol4='" + c.hexToDec(chkMsg2).ToString() + "', vol5='" + c.hexToDec(chkMsg3).ToString() + "', vol6='" + c.hexToDec(chkMsg4).ToString() + "', logDate = now() where ID = " + addLeak + " ";
                ld.update(que1);
            }
            else
            {
                string que1 = "update thleakt SET vol1='" + isFault.ToString() + "', vol2='" + isLeak.ToString() + "', vol3='0', vol4='0', vol5='0', vol6='0', logDate = now() where ID = " + addLeak + " ";
                ld.update(que1);
            }

        }
        
        //항온항습기(전산실 - 7)
        private void serialPortHvic1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isHvicReceiveData1 = true;
                serialPortHvic1.Read(msgHvichex1, 0, msgHvichex1.Length);
                for (int i = 0; i < msgHvichex1.Length; i++)
                {
                    msgHvicstr1[i] = String.Format("{0:X2}", (int)msgHvichex1[i]);
                    Console.Write(msgHvicstr1[i]);
                }
                Console.WriteLine("항온항습1");

                //수신데이터 CRC확인하기
                int chkCRC1 = msgHvichex1[55];
                int chkCRC2 = 0;
                byte[] chkCRCok = new byte[55];
                Array.Copy(msgHvichex1, 0, chkCRCok, 0, 55);
                chkCRC2 = c.EXOR(chkCRCok.Length, chkCRCok);
                if (chkCRC1 != chkCRC2)
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1}", chkCRC1, chkCRC2);
                    return;
                }
                else
                {
                    AddReceiveDataHVIC1();
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        private void AddReceiveDataHVIC1()
        {
            byte[] chkMsg1 = new byte[3];
            byte[] chkMsg2 = new byte[3];
            byte[] chkMsg3 = new byte[3];
            byte[] chkMsg4 = new byte[3];
            byte[] chkMsg5 = new byte[1];
            byte[] chkMsg6 = new byte[1];
            byte[] chkMsg7 = new byte[1];
            byte[] chkMsg8 = new byte[1];
            byte[] chkMsg9 = new byte[1];
            byte[] chkMsg10 = new byte[1];
            byte[] chkMsg11 = new byte[1];
            byte[] chkMsg12 = new byte[1];
            byte[] chkMsg13 = new byte[1];
            byte[] chkMsg14 = new byte[1];
            byte[] chkMsg15 = new byte[1];
            byte[] chkMsg16 = new byte[1];
            byte[] chkMsg17 = new byte[1];
            byte[] chkMsg18 = new byte[1];
            byte[] chkMsg19 = new byte[1];
            byte[] chkMsg20 = new byte[1];
            byte[] chkMsg21 = new byte[1];
            byte[] chkMsg22 = new byte[1];
            byte[] chkMsg23 = new byte[1];
            byte[] chkMsg24 = new byte[1];
            byte[] chkMsg25 = new byte[1];
            byte[] chkMsg26 = new byte[2];
            byte[] chkMsg27 = new byte[2];
            byte[] chkMsg28 = new byte[3];
            byte[] chkMsg29 = new byte[3];

            Array.Copy(msgHvichex1, 0, chkMsg1, 0, 3);                                                      
            Array.Copy(msgHvichex1, 3, chkMsg2, 0, 3);
            Array.Copy(msgHvichex1, 6, chkMsg3, 0, 3);
            Array.Copy(msgHvichex1, 13, chkMsg4, 0, 3);
            Array.Copy(msgHvichex1, 9, chkMsg26, 0, 2);
            Array.Copy(msgHvichex1, 11, chkMsg27, 0, 2);
            Array.Copy(msgHvichex1, 16, chkMsg28, 0, 3);
            Array.Copy(msgHvichex1, 19, chkMsg29, 0, 3);
            Array.Copy(msgHvichex1, 32, chkMsg5, 0, 1);
            Array.Copy(msgHvichex1, 33, chkMsg6, 0, 1);
            Array.Copy(msgHvichex1, 34, chkMsg7, 0, 1);
            Array.Copy(msgHvichex1, 35, chkMsg8, 0, 1);
            Array.Copy(msgHvichex1, 36, chkMsg9, 0, 1);
            Array.Copy(msgHvichex1, 37, chkMsg10, 0, 1);
            Array.Copy(msgHvichex1, 38, chkMsg11, 0, 1);
            Array.Copy(msgHvichex1, 39, chkMsg12, 0, 1);
            Array.Copy(msgHvichex1, 40, chkMsg13, 0, 1);
            Array.Copy(msgHvichex1, 41, chkMsg14, 0, 1);
            Array.Copy(msgHvichex1, 42, chkMsg15, 0, 1);
            Array.Copy(msgHvichex1, 44, chkMsg16, 0, 1);
            Array.Copy(msgHvichex1, 45, chkMsg17, 0, 1);
            Array.Copy(msgHvichex1, 46, chkMsg18, 0, 1);
            Array.Copy(msgHvichex1, 47, chkMsg19, 0, 1);
            Array.Copy(msgHvichex1, 48, chkMsg20, 0, 1);
            Array.Copy(msgHvichex1, 49, chkMsg21, 0, 1);
            Array.Copy(msgHvichex1, 50, chkMsg22, 0, 1);
            Array.Copy(msgHvichex1, 51, chkMsg23, 0, 1);
            Array.Copy(msgHvichex1, 52, chkMsg24, 0, 1);
            Array.Copy(msgHvichex1, 54, chkMsg25, 0, 1);

            int add = serialHvicSendCount1;

            double markMsg1 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg1)) / 10;
            double markMsg2 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg2)) / 10;
            double markMsg3 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg3)) / 10;
            double markMsg4 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg4)) / 10;
            int markMsg5 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg5));
            int markMsg6 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg6));
            int markMsg7 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg7));
            int markMsg8 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg8));
            int markMsg9 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg9));
            int markMsg10 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg10));
            int markMsg11 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg11));
            int markMsg12 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg12));
            int markMsg13 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg13));
            int markMsg14 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg14));
            int markMsg15 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg15));
            int markMsg16 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg16));
            int markMsg17 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg17));
            int markMsg18 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg18));
            int markMsg19 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg19));
            int markMsg20 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg20));
            int markMsg21 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg21));
            int markMsg22 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg22));
            int markMsg23 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg23));
            int markMsg24 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg24));
            int markMsg25 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg25));
            double markMsg26 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg26)) / 10;
            double markMsg27 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg27)) / 10;
            double markMsg28 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg28)) / 10;
            double markMsg29 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg29)) / 10;

            string que1 = "update thhvict SET vol1='" + markMsg25.ToString() + "', vol2='" + markMsg1.ToString("#.#") + "', vol3='" + markMsg2.ToString("#.#") + "', vol4='" + markMsg3.ToString("#.#") + "', vol5='" + markMsg4.ToString("#.#") + "', vol6='" + markMsg5.ToString() + "', vol7='" + markMsg6.ToString() + "', vol8='" + markMsg7.ToString() + "', vol9='" + markMsg8.ToString() + "', vol10='" + markMsg9.ToString() + "', vol11='" + markMsg10.ToString() + "', vol12='" + markMsg11.ToString() + "', vol13='" + markMsg12.ToString() + "', vol14='" + markMsg13.ToString() + "', vol15='" + markMsg14.ToString() + "', vol16='" + markMsg15.ToString() + "', vol17='" + markMsg16.ToString() + "', vol18='" + markMsg17.ToString() + "', vol19='" + markMsg18.ToString() + "', vol20='" + markMsg19.ToString() + "', vol21='" + markMsg20.ToString() + "', vol22='" + markMsg21.ToString() + "', vol23='" + markMsg22.ToString() + "', vol24='" + markMsg23.ToString() + "', vol25='" + markMsg24.ToString() + "', vol26='" + markMsg26.ToString() + "', vol27='" + markMsg27.ToString() + "', vol28='" + markMsg28.ToString() + "', vol29='" + markMsg29.ToString() + "', logDate = now() where ID = " + add + " ";
            ld.update(que1);
        }

        //항온항습기(상황실 - 5)
        private void serialPortHvic2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isHvicReceiveData2 = true;
                serialPortHvic2.Read(msgHvichex2, 0, msgHvichex2.Length);
                for (int i = 0; i < msgHvichex2.Length; i++)
                {
                    msgHvicstr2[i] = String.Format("{0:X2}", (int)msgHvichex2[i]);
                    Console.Write(msgHvicstr2[i]);
                }
                Console.WriteLine("항온항습2");

                //수신데이터 CRC확인하기
                int chkCRC1 = msgHvichex2[55];
                int chkCRC2 = 0;
                byte[] chkCRCok = new byte[55];
                Array.Copy(msgHvichex2, 0, chkCRCok, 0, 55);
                chkCRC2 = c.EXOR(chkCRCok.Length, chkCRCok);
                if (chkCRC1 != chkCRC2)
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1}", chkCRC1, chkCRC2);
                    return;
                }
                 else
                    {
                        AddReceiveDataHVIC2();
                    }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        private void AddReceiveDataHVIC2()
         {
            byte[] chkMsg1 = new byte[3];
            byte[] chkMsg2 = new byte[3];
            byte[] chkMsg3 = new byte[3];
            byte[] chkMsg4 = new byte[3];
            byte[] chkMsg5 = new byte[1];
            byte[] chkMsg6 = new byte[1];
            byte[] chkMsg7 = new byte[1];
            byte[] chkMsg8 = new byte[1];
            byte[] chkMsg9 = new byte[1];
            byte[] chkMsg10 = new byte[1];
            byte[] chkMsg11 = new byte[1];
            byte[] chkMsg12 = new byte[1];
            byte[] chkMsg13 = new byte[1];
            byte[] chkMsg14 = new byte[1];
            byte[] chkMsg15 = new byte[1];
            byte[] chkMsg16 = new byte[1];
            byte[] chkMsg17 = new byte[1];
            byte[] chkMsg18 = new byte[1];
            byte[] chkMsg19 = new byte[1];
            byte[] chkMsg20 = new byte[1];
            byte[] chkMsg21 = new byte[1];
            byte[] chkMsg22 = new byte[1];
            byte[] chkMsg23 = new byte[1];
            byte[] chkMsg24 = new byte[1];
            byte[] chkMsg25 = new byte[1];
            byte[] chkMsg26 = new byte[2];
            byte[] chkMsg27 = new byte[2];
            byte[] chkMsg28 = new byte[3];
            byte[] chkMsg29 = new byte[3];

            Array.Copy(msgHvichex2, 0, chkMsg1, 0, 3);
            Array.Copy(msgHvichex2, 3, chkMsg2, 0, 3);
            Array.Copy(msgHvichex2, 6, chkMsg3, 0, 3);
            Array.Copy(msgHvichex2, 13, chkMsg4, 0, 3);
            Array.Copy(msgHvichex2, 9, chkMsg26, 0, 2);
            Array.Copy(msgHvichex2, 11, chkMsg27, 0, 2);
            Array.Copy(msgHvichex2, 16, chkMsg28, 0, 3);
            Array.Copy(msgHvichex2, 19, chkMsg29, 0, 3);
            Array.Copy(msgHvichex2, 32, chkMsg5, 0, 1);
            Array.Copy(msgHvichex2, 33, chkMsg6, 0, 1);
            Array.Copy(msgHvichex2, 34, chkMsg7, 0, 1);
            Array.Copy(msgHvichex2, 35, chkMsg8, 0, 1);
            Array.Copy(msgHvichex2, 36, chkMsg9, 0, 1);
            Array.Copy(msgHvichex2, 37, chkMsg10, 0, 1);
            Array.Copy(msgHvichex2, 38, chkMsg11, 0, 1);
            Array.Copy(msgHvichex2, 39, chkMsg12, 0, 1);
            Array.Copy(msgHvichex2, 40, chkMsg13, 0, 1);
            Array.Copy(msgHvichex2, 41, chkMsg14, 0, 1);
            Array.Copy(msgHvichex2, 42, chkMsg15, 0, 1);
            Array.Copy(msgHvichex2, 44, chkMsg16, 0, 1);
            Array.Copy(msgHvichex2, 45, chkMsg17, 0, 1);
            Array.Copy(msgHvichex2, 46, chkMsg18, 0, 1);
            Array.Copy(msgHvichex2, 47, chkMsg19, 0, 1);
            Array.Copy(msgHvichex2, 48, chkMsg20, 0, 1);
            Array.Copy(msgHvichex2, 49, chkMsg21, 0, 1);
            Array.Copy(msgHvichex2, 50, chkMsg22, 0, 1);
            Array.Copy(msgHvichex2, 51, chkMsg23, 0, 1);
            Array.Copy(msgHvichex2, 52, chkMsg24, 0, 1);
            Array.Copy(msgHvichex2, 54, chkMsg25, 0, 1);

            int add = serialHvicSendCount2 + 7;

            double markMsg1 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg1)) / 10;
            double markMsg2 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg2)) / 10;
            double markMsg3 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg3)) / 10;
            double markMsg4 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg4)) / 10;
            int markMsg5 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg5));
            int markMsg6 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg6));
            int markMsg7 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg7));
            int markMsg8 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg8));
            int markMsg9 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg9));
            int markMsg10 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg10));
            int markMsg11 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg11));
            int markMsg12 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg12));
            int markMsg13 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg13));
            int markMsg14 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg14));
            int markMsg15 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg15));
            int markMsg16 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg16));
            int markMsg17 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg17));
            int markMsg18 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg18));
            int markMsg19 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg19));
            int markMsg20 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg20));
            int markMsg21 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg21));
            int markMsg22 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg22));
            int markMsg23 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg23));
            int markMsg24 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg24));
            int markMsg25 = Convert.ToInt32(Encoding.ASCII.GetString(chkMsg25));
            double markMsg26 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg26)) / 10;
            double markMsg27 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg27)) / 10;
            double markMsg28 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg28)) / 10;
            double markMsg29 = Convert.ToDouble(Encoding.ASCII.GetString(chkMsg29)) / 10;

            string que1 = "update thhvict SET vol1='" + markMsg25.ToString() + "', vol2='" + markMsg1.ToString("#.#") + "', vol3='" + markMsg2.ToString("#.#") + "', vol4='" + markMsg3.ToString("#.#") + "', vol5='" + markMsg4.ToString("#.#") + "', vol6='" + markMsg5.ToString() + "', vol7='" + markMsg6.ToString() + "', vol8='" + markMsg7.ToString() + "', vol9='" + markMsg8.ToString() + "', vol10='" + markMsg9.ToString() + "', vol11='" + markMsg10.ToString() + "', vol12='" + markMsg11.ToString() + "', vol13='" + markMsg12.ToString() + "', vol14='" + markMsg13.ToString() + "', vol15='" + markMsg14.ToString() + "', vol16='" + markMsg15.ToString() + "', vol17='" + markMsg16.ToString() + "', vol18='" + markMsg17.ToString() + "', vol19='" + markMsg18.ToString() + "', vol20='" + markMsg19.ToString() + "', vol21='" + markMsg20.ToString() + "', vol22='" + markMsg21.ToString() + "', vol23='" + markMsg22.ToString() + "', vol24='" + markMsg23.ToString() + "', vol25='" + markMsg24.ToString() + "', vol26='" + markMsg26.ToString() + "', vol27='" + markMsg27.ToString() + "', vol28='" + markMsg28.ToString() + "', vol29='" + markMsg29.ToString() + "', logDate = now() where ID = " + add + " ";
            ld.update(que1);
        }
    }
}
