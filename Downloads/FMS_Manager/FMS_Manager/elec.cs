using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class elec
    {
        Timer t1 = new Timer();
        Timer t2 = new Timer();
        Load ld = new Load();
        ClassConvert c = new ClassConvert();
        //시리얼포트 선언
        private System.IO.Ports.SerialPort serialPortElec1 = new System.IO.Ports.SerialPort();
        private System.IO.Ports.SerialPort serialPortElec2 = new System.IO.Ports.SerialPort();
        private System.IO.Ports.SerialPort serialPortElec3 = new System.IO.Ports.SerialPort();
        private int serialElec1SendCount = 0;
        private int serialElec2SendCount = 0;
        private int serialElec3SendCount = 0;
        public bool isElec1ReceiveData = false;
        public bool isElec2ReceiveData = false;
        public bool isElec3ReceiveData = false;
        private byte[] msgElec1hex = new byte[73];  //전산실분전반 핵사값저장
        private byte[] msgElec2hex = new byte[73];  //메인분전반 핵사값저장
        private byte[] msgElec3hex = new byte[77];  //ups분전반 핵사값저장
        private string[] msgElec1str = new string[73];  //전산실분전반 스트링값저장
        private string[] msgElec2str = new string[73];  //메인분전반 스트링값저장
        private string[] msgElec3str = new string[77];  //ups분전반 스트링값저장



        public void Serialtimer1()  //분전반
        {
            cschedule sch = new cschedule();
            sch.LoadScheduleDB();
            // 분전반 5개
            serialPortElec1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortElec1_DataReceived);
            serialPortElec1.PortName = global::FMS_Manager.Properties.Settings.Default.ELEC1COM;
            serialPortElec1.BaudRate = global::FMS_Manager.Properties.Settings.Default.ELEC1BITRATE;
            serialPortElec1.ReceivedBytesThreshold = 73;
            //메인 분전반 2개
            serialPortElec2.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortElec2_DataReceived);
            serialPortElec2.PortName = global::FMS_Manager.Properties.Settings.Default.ELEC2COM;
            serialPortElec2.BaudRate = global::FMS_Manager.Properties.Settings.Default.ELEC2BITRATE;
            serialPortElec2.ReceivedBytesThreshold = 73;
            //UPS실 분전반 3개
            serialPortElec3.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortElec3_DataReceived);
            serialPortElec3.PortName = global::FMS_Manager.Properties.Settings.Default.ELEC3COM;
            serialPortElec3.BaudRate = global::FMS_Manager.Properties.Settings.Default.ELEC3BITRATE;
            serialPortElec3.ReceivedBytesThreshold = 77;

            t1.Enabled = true;
            t1.Interval = Convert.ToInt32(sch.schedule[4]) * 1000; // 점점점검주기(초) * 1000
            t2.Enabled = false;
            t2.Interval = 1000;

            t1.Elapsed += new ElapsedEventHandler(OnTimedEvent1);
            t2.Elapsed += new ElapsedEventHandler(OnTimedEvent2);
        }
        private void OnTimedEvent1(object source, ElapsedEventArgs e) 
        {
            //전산실 분전반5개
            if (serialPortElec1.IsOpen)
            {
                serialPortElec1.Close();
                serialPortElec1.Open();
            }
            else
            {
                serialPortElec1.Open();
            }

            if (serialElec1SendCount > 4)
            {
                serialElec1SendCount = 0;
            }

            int num = serialElec1SendCount + 1;
            byte[] sMagElec1 = new byte[] { (byte)num, (byte)0x03, (byte)0x00, (byte)0x01, (byte)0x00, (byte)0x22 };
            byte[] bin = new byte[sMagElec1.Length + 2]; //CRC체크상환받을곳
            Array.ConstrainedCopy(sMagElec1, 0, bin, 0, sMagElec1.Length);  //CRC체크하기
            Array.ConstrainedCopy(c.CRC16(sMagElec1.Length, sMagElec1), 0, bin, sMagElec1.Length, 2);
            serialElec1SendCount++;
            serialPortElec1.Write(bin, 0, bin.Length);


            //8층 분전반 2개
            if (serialPortElec2.IsOpen)
            {
                serialPortElec2.Close();
                serialPortElec2.Open();
            }
            else
            {
                serialPortElec2.Open();
            }

            if (serialElec2SendCount > 1)
            {
                serialElec2SendCount = 0;
            }

            int num2 = serialElec2SendCount + 1;
            byte[] sMagElec2 = new byte[] { (byte)num2, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x22 };
            byte[] bin2 = new byte[sMagElec2.Length + 2]; //CRC체크상환받을곳
            Array.ConstrainedCopy(sMagElec2, 0, bin2, 0, sMagElec2.Length);  //CRC체크하기
            Array.ConstrainedCopy(c.CRC16(sMagElec2.Length, sMagElec2), 0, bin2, sMagElec2.Length, 2);
            serialElec2SendCount++;
            serialPortElec2.Write(bin2, 0, bin2.Length);

            //UPS실 분전반 3개
            if (serialPortElec3.IsOpen)
            {
                serialPortElec3.Close();
                serialPortElec3.Open();
            }
            else
            {
                serialPortElec3.Open();
            }

            if (serialElec3SendCount > 2)
            {
                serialElec3SendCount = 0;
            }

            int num3 = serialElec3SendCount + 1;
            byte[] sMagElec3 = new byte[] { (byte)num3, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x24 };
            byte[] bin3 = new byte[sMagElec3.Length + 2]; //CRC체크상환받을곳
            Array.ConstrainedCopy(sMagElec3, 0, bin3, 0, sMagElec3.Length);  //CRC체크하기
            Array.ConstrainedCopy(c.CRC16(sMagElec3.Length, sMagElec3), 0, bin3, sMagElec3.Length, 2);
            serialElec3SendCount++;
            serialPortElec3.Write(bin3, 0, bin3.Length);

            //통신 요청 타이머 가동

            t2.Start();


        }

        private void OnTimedEvent2(object source, ElapsedEventArgs e) 
        {
            if (isElec1ReceiveData == true)
            {
                t2.Stop();
            }
            else
            {
                string que1 = "update thelect SET vol1='err', vol2='err', vol3='err', vol4='err', vol5='err', vol6='err', vol7='err', vol8='err', vol9='err', vol10='err', vol11='err', vol12='err', vol13='err', vol14='err', vol15='err' where ID = " + serialElec1SendCount + " ";
                ld.update(que1);

                Console.WriteLine("전산실분전반" + serialElec1SendCount + "장비 통신오류");
                ld.logDate("전산실분전반" + serialElec1SendCount + "장비 통신오류");
                t2.Stop();
            }
            isElec1ReceiveData = false;

            if (isElec2ReceiveData == false)
            {
                string que2 = "update thelect SET vol1='err', vol2='err', vol3='err', vol4='err', vol5='err', vol6='err', vol7='err', vol8='err', vol9='err', vol10='err', vol11='err', vol12='err', vol13='err', vol14='err', vol15='err' where ID = " + (serialElec2SendCount + 5) + " ";
                ld.update(que2);

                Console.WriteLine("8층메인분전반" + serialElec2SendCount + "장비 통신오류");
                ld.logDate("8층메인분전반" + serialElec2SendCount + "장비 통신오류");
            }
            else
            {
                isElec2ReceiveData = false;
            }

            if (isElec3ReceiveData == false)
            {
                string que3 = "update thelect SET vol1='err', vol2='err', vol3='err', vol4='err', vol5='err', vol6='err', vol7='err', vol8='err', vol9='err', vol10='err', vol11='err', vol12='err', vol13='err', vol14='err', vol15='err' where ID = " + (serialElec3SendCount + 7) + " ";
                ld.update(que3);

                Console.WriteLine("UPS실분전반" + serialElec3SendCount + "장비 통신오류");
                ld.logDate("UPS실분전반" + serialElec3SendCount + "장비 통신오류");
            }
            else
            {
                isElec3ReceiveData = false;
            }
        }
        //전산실 분전반 5개
        private void serialPortElec1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isElec1ReceiveData = true;
                serialPortElec1.Read(msgElec1hex, 0, msgElec1hex.Length);
                for (int i = 0; i < msgElec1hex.Length; i++)
                {
                    msgElec1str[i] = String.Format("{0:X2}", (int)msgElec1hex[i]);
                    Console.Write(msgElec1str[i]);
                }
                Console.WriteLine("분전반1");

                //수신데이터 CRC확인하기
                byte[] chkCRC1 = new byte[2];
                byte[] chkCRC2 = new byte[2];
                byte[] chkCRCok = new byte[71];
                Array.Copy(msgElec1hex, 71, chkCRC1, 0, 2);
                Array.Copy(msgElec1hex, 0, chkCRCok, 0, 71);
                chkCRC2 = c.CRC16(chkCRCok.Length, chkCRCok);
                if (chkCRC1[0] != chkCRC2[0] || chkCRC1[1] != chkCRC2[1])
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1} or CRC {2}={3}", chkCRC1[0], chkCRC2[0], chkCRC1[1], chkCRC2[1]);
                    return;
                }
                else
                {
                    AddReceiveData1();
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
        
        private void AddReceiveData1()
        {
            byte[] chkMsg1 = new byte[4];
            byte[] chkMsg2 = new byte[4];
            byte[] chkMsg3 = new byte[4];
            byte[] chkMsg4 = new byte[4];
            byte[] chkMsg5 = new byte[4];
            byte[] chkMsg6 = new byte[4];
            byte[] chkMsg7 = new byte[4];
            byte[] chkMsg8 = new byte[4];
            byte[] chkMsg9 = new byte[4];
            byte[] chkMsg10 = new byte[4];
            byte[] chkMsg11 = new byte[4];
            byte[] chkMsg12 = new byte[4];
            byte[] chkMsg13 = new byte[4];
            byte[] chkMsg14 = new byte[4];
            byte[] chkMsg15 = new byte[4];
            //byte[] chkMsg16 = new byte[4];
            //byte[] chkMsg17 = new byte[4];
            byte[] chk = new byte[4] { (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF };

            Array.Copy(msgElec1hex, 3, chkMsg1, 0, 4);
            Array.Copy(msgElec1hex, 7, chkMsg2, 0, 4);
            Array.Copy(msgElec1hex, 11, chkMsg3, 0, 4);
            Array.Copy(msgElec1hex, 15, chkMsg4, 0, 4);
            Array.Copy(msgElec1hex, 19, chkMsg5, 0, 4);
            Array.Copy(msgElec1hex, 23, chkMsg6, 0, 4);
            Array.Copy(msgElec1hex, 27, chkMsg7, 0, 4);
            Array.Copy(msgElec1hex, 31, chkMsg8, 0, 4);
            Array.Copy(msgElec1hex, 35, chkMsg9, 0, 4);
            Array.Copy(msgElec1hex, 39, chkMsg10, 0, 4);
            Array.Copy(msgElec1hex, 43, chkMsg11, 0, 4);
            Array.Copy(msgElec1hex, 47, chkMsg12, 0, 4);
            Array.Copy(msgElec1hex, 51, chkMsg13, 0, 4);
            Array.Copy(msgElec1hex, 55, chkMsg14, 0, 4);
            Array.Copy(msgElec1hex, 59, chkMsg15, 0, 4);
            //Array.Copy(msgElec1hex, 63, chkMsg16, 0, 4);
            //Array.Copy(msgElec1hex, 67, chkMsg17, 0, 4);

            int add = Convert.ToInt32(msgElec1hex[0]);

            double markMsg1 = Convert.ToDouble(c.hexToDec(chkMsg1)) / 100;
            double markMsg2 = Convert.ToDouble(c.hexToDec(chkMsg2)) / 100;
            double markMsg3 = Convert.ToDouble(c.hexToDec(chkMsg3)) / 100;
            double markMsg4 = Convert.ToDouble(c.hexToDec(chkMsg4)) / 100;
            double markMsg5 = Convert.ToDouble(c.hexToDec(chkMsg5)) / 100;
            double markMsg6 = Convert.ToDouble(c.hexToDec(chkMsg6)) / 100;
            double markMsg7 = Convert.ToDouble(c.hexToDec(chkMsg7)) / 1000;
            double markMsg8 = Convert.ToDouble(c.hexToDec(chkMsg8)) / 1000;
            double markMsg9 = Convert.ToDouble(c.hexToDec(chkMsg9)) / 1000;
            double markMsg10 = Convert.ToDouble(c.hexToDec(chkMsg10)) / 1000;
            double markMsg11 = Convert.ToDouble(c.hexToDec(chkMsg11)) / 1000;
            double markMsg12 = Convert.ToDouble(c.hexToDec(chkMsg12)) / 1000;
            double markMsg13 = Convert.ToDouble(c.hexToDec(chkMsg13)) / 10;
            double markMsg14 = 0;
            if (msgElec1hex[55] == (byte)0xFF)
            {
                markMsg14 = (Convert.ToDouble(c.hexToDec(chk)) - Convert.ToDouble(c.hexToDec(chkMsg14))) / -100;
            }
            else
            {
                markMsg14 = Convert.ToDouble(c.hexToDec(chkMsg14)) / 100;
            }
            double markMsg15 = Convert.ToDouble(c.hexToDec(chkMsg15));
           // double markMsg16 = Convert.ToDouble(c.hexToDec(chkMsg16));
           // double markMsg17 = Convert.ToDouble(c.hexToDec(chkMsg17));

            string que1 = "update thelect SET vol1='" + markMsg1.ToString("#.#") + "', vol2='" + markMsg2.ToString("#.#") + "', vol3='" + markMsg3.ToString("#.#") + "', vol4='" + markMsg4.ToString("#.#") + "', vol5='" + markMsg5.ToString("#.#") + "', vol6='" + markMsg6.ToString("#.#") + "', vol7='" + markMsg7.ToString("#0.##") + "', vol8='" + markMsg8.ToString("#0.##") + "', vol9='" + markMsg9.ToString("#0.##") + "', vol10='" + markMsg10.ToString("#0.##") + "', vol11='" + markMsg11.ToString("#0.##") + "', vol12='" + markMsg12.ToString("#0.##") + "', vol13='" + markMsg13.ToString("#.#") + "', vol14='" + markMsg14.ToString("#0.##") + "', vol15='" + markMsg15.ToString("#0.##") + "', logDate = now() where ID = " + add + " ";
                ld.update(que1);
            }
  
        //메인 분전반 2개
        private void serialPortElec2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isElec2ReceiveData = true;
                serialPortElec2.Read(msgElec2hex, 0, msgElec2hex.Length);
                for (int i = 0; i < msgElec2hex.Length; i++)
                {
                    msgElec2str[i] = String.Format("{0:X2}", (int)msgElec2hex[i]);
                    Console.Write(msgElec2str[i]);
                }
                Console.WriteLine("분전반2");
                //수신데이터 CRC확인하기
                byte[] chkCRC1 = new byte[2];
                byte[] chkCRC2 = new byte[2];
                byte[] chkCRCok = new byte[71];
                Array.Copy(msgElec2hex, 71, chkCRC1, 0, 2);
                Array.Copy(msgElec2hex, 0, chkCRCok, 0, 71);
                chkCRC2 = c.CRC16(chkCRCok.Length, chkCRCok);
                if (chkCRC1[0] != chkCRC2[0] || chkCRC1[1] != chkCRC2[1])
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1} or CRC {2}={3}", chkCRC1[0], chkCRC2[0], chkCRC1[1], chkCRC2[1]);
                    return;
                }
                else
                {
                    AddReceiveData2();
                }
            }
            catch(Exception err)
            {
                ld.logDate(err.ToString());
            }
        }

        private void AddReceiveData2()
        {
            //byte[] chkMsg1 = new byte[4];
            //byte[] chkMsg2 = new byte[4];
            byte[] chkMsg3 = new byte[4];
            byte[] chkMsg4 = new byte[4];
            byte[] chkMsg5 = new byte[4];
            byte[] chkMsg6 = new byte[4];
            byte[] chkMsg7 = new byte[4];
            byte[] chkMsg8 = new byte[4];
            byte[] chkMsg9 = new byte[4];
            byte[] chkMsg10 = new byte[4];
            byte[] chkMsg11 = new byte[4];
            byte[] chkMsg12 = new byte[4];
            byte[] chkMsg13 = new byte[4];
            byte[] chkMsg14 = new byte[4];
            byte[] chkMsg15 = new byte[4];
            byte[] chkMsg16 = new byte[4];
            byte[] chkMsg17 = new byte[4];

            //Array.Copy(msgElec2hex, 3, chkMsg1, 0, 4);
            //Array.Copy(msgElec2hex, 7, chkMsg2, 0, 4);
            Array.Copy(msgElec2hex, 11, chkMsg3, 0, 4);     //A전류     (7)
            Array.Copy(msgElec2hex, 15, chkMsg4, 0, 4);     //B전류     (8)
            Array.Copy(msgElec2hex, 19, chkMsg5, 0, 4);     //C전류     (9)
            Array.Copy(msgElec2hex, 23, chkMsg6, 0, 4);     //A전압     (4)
            Array.Copy(msgElec2hex, 27, chkMsg7, 0, 4);     //B전압     (5)
            Array.Copy(msgElec2hex, 31, chkMsg8, 0, 4);     //C전압     (6)    
            Array.Copy(msgElec2hex, 35, chkMsg9, 0, 4);     //선간전압  (1)
            Array.Copy(msgElec2hex, 39, chkMsg10, 0, 4);    //          (2) 
            Array.Copy(msgElec2hex, 43, chkMsg11, 0, 4);    //          (3)
            Array.Copy(msgElec2hex, 47, chkMsg12, 0, 4);    //역률pf            (14)
            Array.Copy(msgElec2hex, 51, chkMsg13, 0, 4);    //토탈전력kw        (10)
            Array.Copy(msgElec2hex, 55, chkMsg14, 0, 4);    //토탈무효전력kvar  (11)
            Array.Copy(msgElec2hex, 59, chkMsg15, 0, 4);    //토탈파상전력kva   (12)
            Array.Copy(msgElec2hex, 63, chkMsg16, 0, 4);    //주파수hz          (13)
            Array.Copy(msgElec2hex, 67, chkMsg17, 0, 4);    //유효전력량kwh     (15)

            int add = Convert.ToInt32(msgElec2hex[0]) + 5;

            //double markMsg1 = Convert.ToDouble(c.hexToDec(chkMsg1));
            //double markMsg2 = Convert.ToDouble(c.hexToDec(chkMsg2));
            double markMsg3 = c.Floating(chkMsg3);
            double markMsg4 = c.Floating(chkMsg4);
            double markMsg5 = c.Floating(chkMsg5);
            double markMsg6 = c.Floating(chkMsg6);
            double markMsg7 = c.Floating(chkMsg7);
            double markMsg8 = c.Floating(chkMsg8);
            double markMsg9 = c.Floating(chkMsg9);
            double markMsg10 = c.Floating(chkMsg10);
            double markMsg11 = c.Floating(chkMsg11);
            double markMsg12 = c.Floating(chkMsg12);
            double markMsg13 = c.Floating(chkMsg13) / 1000 ;
            double markMsg14 = c.Floating(chkMsg14) / 1000 ;
            double markMsg15 = c.Floating(chkMsg15) / 1000 ;
            double markMsg16 = c.Floating(chkMsg16);
            double markMsg17 = c.Floating(chkMsg17) / 1000 ;

            string que1 = "update thelect SET vol1='" + markMsg9.ToString("#.#") + "', vol2='" + markMsg10.ToString("#.#") + "', vol3='" + markMsg11.ToString("#.#") + "', vol4='" + markMsg6.ToString("#.#") + "', vol5='" + markMsg7.ToString("#.#") + "', vol6='" + markMsg8.ToString("#.#") + "', vol7='" + markMsg3.ToString("#0.##") + "', vol8='" + markMsg4.ToString("#0.##") + "', vol9='" + markMsg5.ToString("#0.##") + "', vol10='" + markMsg13.ToString("#0.##") + "', vol11='" + markMsg14.ToString("#0.##") + "', vol12='" + markMsg15.ToString("#0.##") + "', vol13='" + markMsg16.ToString("#.#") + "', vol14='" + markMsg12.ToString("#0.##") + "', vol15='" + markMsg17.ToString("#0.##") + "', logDate = now() where ID = " + add + " ";
            ld.update(que1);
        }
        //UPS실 분전반 3개
        private void serialPortElec3_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                isElec3ReceiveData = true;
                serialPortElec3.Read(msgElec3hex, 0, msgElec3hex.Length);
                for (int i = 0; i < msgElec3hex.Length; i++)
                {
                    msgElec3str[i] = String.Format("{0:X2}", (int)msgElec3hex[i]);
                    Console.Write(msgElec3str[i]); 
                }
                Console.WriteLine("분전반3");
                //수신데이터 CRC확인하기
                byte[] chkCRC1 = new byte[2];
                byte[] chkCRC2 = new byte[2];
                byte[] chkCRCok = new byte[75];
                Array.Copy(msgElec3hex, 75, chkCRC1, 0, 2);
                Array.Copy(msgElec3hex, 0, chkCRCok, 0, 75);
                chkCRC2 = c.CRC16(chkCRCok.Length, chkCRCok);
                if (chkCRC1[0] != chkCRC2[0] || chkCRC1[1] != chkCRC2[1])
                {
                    Console.WriteLine("CRC오류 - CRC {0}={1} or CRC {2}={3}", chkCRC1[0], chkCRC2[0], chkCRC1[1], chkCRC2[1]);
                    return;
                }
                else
                {
                    AddReceiveData3();
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }

        private void AddReceiveData3()
        {

            byte[] chkMsg1 = new byte[4];
            byte[] chkMsg2 = new byte[4];
            byte[] chkMsg3 = new byte[4];
            byte[] chkMsg4 = new byte[4];
            byte[] chkMsg5 = new byte[4];
            byte[] chkMsg6 = new byte[4];
            byte[] chkMsg7 = new byte[4];
            byte[] chkMsg8 = new byte[4];
            byte[] chkMsg9 = new byte[4];
            byte[] chkMsg10 = new byte[4];
            byte[] chkMsg11 = new byte[4];
            byte[] chkMsg12 = new byte[4];
            byte[] chkMsg13 = new byte[4];
            byte[] chkMsg14 = new byte[4];
            byte[] chkMsg15 = new byte[4];
            byte[] chkMsg16 = new byte[4];
            byte[] chkMsg17 = new byte[4];

            //Array.Copy(msgElec3hex, 5, chkMsg1, 0, 4);  //평균전압
            //Array.Copy(msgElec3hex, 9, chkMsg2, 0, 4);  //평균전류
            Array.Copy(msgElec3hex, 13, chkMsg3, 0, 4); //R상 전류
            Array.Copy(msgElec3hex, 17, chkMsg4, 0, 4); //S상 전류
            Array.Copy(msgElec3hex, 21, chkMsg5, 0, 4); //T상 전류
            Array.Copy(msgElec3hex, 25, chkMsg6, 0, 4); //A상 전압
            Array.Copy(msgElec3hex, 29, chkMsg7, 0, 4); //B상 전압
            Array.Copy(msgElec3hex, 33, chkMsg8, 0, 4); //C상 전압
            Array.Copy(msgElec3hex, 37, chkMsg9, 0, 4); //선간전압
            Array.Copy(msgElec3hex, 41, chkMsg10, 0, 4);
            Array.Copy(msgElec3hex, 45, chkMsg11, 0, 4);
            Array.Copy(msgElec3hex, 49, chkMsg12, 0, 4);    //역률(pf)
            Array.Copy(msgElec3hex, 53, chkMsg13, 0, 4);    //토탈전력(kw)
            Array.Copy(msgElec3hex, 57, chkMsg14, 0, 4);    //토탈무효전력(kvar)
            Array.Copy(msgElec3hex, 61, chkMsg15, 0, 4);    //토탈피상전력(kva)
            Array.Copy(msgElec3hex, 65, chkMsg16, 0, 4);    //주파수(hz)
            Array.Copy(msgElec3hex, 69, chkMsg17, 0, 4);    //유효전력량(kwh)

            int add = Convert.ToInt32(msgElec3hex[0]) + 7;

            //double markMsg1 = Convert.ToDouble(c.hexToDec(chkMsg1));
            //double markMsg2 = Convert.ToDouble(c.hexToDec(chkMsg2));
            double markMsg3 = c.Floating(chkMsg3);
            double markMsg4 = c.Floating(chkMsg4);
            double markMsg5 = c.Floating(chkMsg5);
            double markMsg6 = c.Floating(chkMsg6);
            double markMsg7 = c.Floating(chkMsg7);
            double markMsg8 = c.Floating(chkMsg8);
            double markMsg9 = c.Floating(chkMsg9);
            double markMsg10 = c.Floating(chkMsg10);
            double markMsg11 = c.Floating(chkMsg11);
            double markMsg12 = c.Floating(chkMsg12);
            double markMsg13 = c.Floating(chkMsg13) / 1000;
            double markMsg14 = c.Floating(chkMsg14) / 1000;
            double markMsg15 = c.Floating(chkMsg15) / 1000;
            double markMsg16 = c.Floating(chkMsg16);
            double markMsg17 = c.Floating(chkMsg17) / 1000;

            string que1 = "update thelect SET vol1='" + markMsg9.ToString("#.#") + "', vol2='" + markMsg10.ToString("#.#") + "', vol3='" + markMsg11.ToString("#.#") + "', vol4='" + markMsg6.ToString("#.#") + "', vol5='" + markMsg7.ToString("#.#") + "', vol6='" + markMsg8.ToString("#.#") + "', vol7='" + markMsg3.ToString("#0.##") + "', vol8='" + markMsg4.ToString("#0.##") + "', vol9='" + markMsg5.ToString("#0.##") + "', vol10='" + markMsg13.ToString("#0.##") + "', vol11='" + markMsg14.ToString("#0.##") + "', vol12='" + markMsg15.ToString("#0.##") + "', vol13='" + markMsg16.ToString("#.#") + "', vol14='" + markMsg12.ToString("#0.##") + "', vol15='" + markMsg17.ToString("#0.##") + "', logDate = now() where ID = " + add + " ";
            ld.update(que1);
        }
    }
}
