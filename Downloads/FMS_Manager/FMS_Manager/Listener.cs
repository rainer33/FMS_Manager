using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FMS_Manager
{
    class Listener
    {
        ClassConvert c = new ClassConvert();
        Water ww
        {
            get
            {
                return Program.ww;
            }
        }

        public Load ld
        {
            get
            {
                return Program.ld;
            }
        }
        //Water ww = new Water();
        //Load ld = new Load();
        TH th = new TH();
        private Socket listen = null;    // Socket
        private bool _IsListen = false;  // listen loop control
        private System.Threading.Thread _thListen = null; // Listen 대기 쓰레드
        private string[] sendProcess = new string[4096];
        public System.IO.Ports.SerialPort serialPortHvic1 = new System.IO.Ports.SerialPort();
        public System.IO.Ports.SerialPort serialPortHvic2 = new System.IO.Ports.SerialPort();

        public bool CreateListener()
        {
            try
            {
                ld.Loading();
                
                IPEndPoint _Ep = new IPEndPoint(IPAddress.Any, 10001);
                this.listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                this.listen.Bind(_Ep);
                this.listen.Listen(-1);				// Accept 동작은 요청시 바로바로
                this._IsListen = true;				// Listen 상태(동작중)
                // Listen 스레드 할당과 시작
                _thListen = new System.Threading.Thread(new System.Threading.ThreadStart(this.Listen));
                _thListen.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        private void Listen()
        {
            while (_IsListen)
            {
                byte[] buffer = new Byte[1024]; // 수신되는 버퍼

                Socket sockMCS = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    Console.WriteLine("Waiting Connect - FMS");
                    sockMCS = listen.Accept(); // 접속때까지 기다림.
                    Console.WriteLine("connected - FMS");

                    try
                    {
                        if ((sockMCS == null) || (!sockMCS.Connected))
                        { // 연결이상, 다시 루프로
                            sockMCS = null;
                            continue;
                        }
                        else if (sockMCS.Connected)
                        {
                            int length = sockMCS.Receive(buffer);
                            byte[] tmp_arr = new byte[length];
                            //Console.WriteLine("buffer's length : " + length + "tmp_arr's length : " + tmp_arr.Length);
                            for (int i = 0; i < length; i++)
                            {
                                tmp_arr[i] = buffer[i];
                                ///Console.Write(tmp_arr[i].ToString());
                            }
                            ClassConvert c = new ClassConvert();
                            Console.WriteLine(c.byteToString(tmp_arr));
                            process(tmp_arr); // data process
                            sockMCS.Shutdown(SocketShutdown.Both);
                            sockMCS.Close();
                            sockMCS = null;
                        }
                    }
                    catch (Exception err)
                    {
                        ld.logDate(err.ToString());
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    try
                    {
                        sockMCS.Shutdown(SocketShutdown.Both);
                        sockMCS.Close();
                        sockMCS = null;
                    }
                    catch { }
                    if (sockMCS != null) sockMCS = null;
                    continue;
                }
            }
        }
        public byte[] OnOffmsg;
        private void process(byte[] buffer)
        {

            cmonitering mo = new cmonitering();
            climitied lim = new climitied();
            cconfig2 con2 = new cconfig2();
            cemail em = new cemail();
            clogin log = new clogin();
            cschedule sch = new cschedule();
            csnmp sp = new csnmp();
            csyscode sys = new csyscode();
            csyssavelist ssave = new csyssavelist();
            csms sms = new csms();
            elec el = new elec();
            //Water ww = new Water();
            TH thh = new TH();

            try
            {
                string str = c.byteToString(buffer);
                switch (str)
                {
                    case "65":

                        mo.LoadMoniteringDB();
                        ld.limitedLoad1();
                        Console.WriteLine("A수신완료");
                        break;
                    case "66":
                        sch.LoadScheduleDB();
                        lim.LoadLimiteDB();
                        Console.WriteLine("B수신완료");
                        break;
                    case "67":
                        log.LoadLoginDB();
                        sms.LoadSmsDB();
                        Console.WriteLine("C수신완료");
                        break;
                    case "68":
                        con2.LoadConfigDB();
                        Console.WriteLine("D수신완료");
                        break;
                    case "69":
                        con2.LoadConfigDB();
                        Console.WriteLine("E수신완료");
                        break;
                    case "70":
                        em.LoadEmailDB();
                        sp.LoadSnmpDB();
                        Console.WriteLine("F수신완료");
                        break;
                    case "71":
                        em.LoadEmailDB();
                        Console.WriteLine("G수신완료");
                        break;
                    case "73":
                        log.LoadLoginDB();
                        Console.WriteLine("I수신완료");
                        break;
                    case "74":
                        sms.LoadSmsDB();
                        Console.WriteLine("J수신완료");
                        break;

                    case "49":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("1번 항온항습기 전원ON");
                        Console.WriteLine("1번 항온항습기 전원ON");
                        break;
                    case "50":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("2번 항온항습기 전원ON");
                        Console.WriteLine("2번 항온항습기 전원ON");
                        break;
                    case "51":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("3번 항온항습기 전원ON");
                        Console.WriteLine("3번 항온항습기 전원ON");
                        break;
                    case "52":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("4번 항온항습기 전원ON");
                        Console.WriteLine("4번 항온항습기 전원ON");
                        break;
                    case "53":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("5번 항온항습기 전원ON");
                        Console.WriteLine("5번 항온항습기 전원ON");
                        break;
                    case "54":
                        OnOffmsg = new byte[] { (byte)0x66, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("6번 항온항습기 전원ON");
                        Console.WriteLine("6번 항온항습기 전원ON");
                        break;
                    case "55":
                        OnOffmsg = new byte[] { (byte)0x67, (byte)0x5C, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("7번 항온항습기 전원ON");
                        Console.WriteLine("7번 항온항습기 전원ON");
                        break;
                    case "56":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x5C, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("8번 항온항습기 전원ON");
                        Console.WriteLine("8번 항온항습기 전원ON");
                        break;
                    case "57":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x5C, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("9번 항온항습기 전원ON");
                        Console.WriteLine("9번 항온항습기 전원ON");
                        break;
                    case "49,48":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x5C, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("10번 항온항습기 전원ON");
                        Console.WriteLine("10번 항온항습기 전원ON");
                        break;
                    case "49,49":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x5C, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("11번 항온항습기 전원ON");
                        Console.WriteLine("11번 항온항습기 전원ON");
                        break;
                    case "49,50":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x5C, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("12번 항온항습기 전원ON");
                        Console.WriteLine("12번 항온항습기 전원ON");
                        break;
                    case "49,51":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("1번 항온항습기 전원OFF");
                        Console.WriteLine("1번 항온항습기 전원OFF");
                        break;
                    case "49,52":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("2번 항온항습기 전원OFF");
                        Console.WriteLine("2번 항온항습기 전원OFF");
                        break;
                    case "49,53":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("3번 항온항습기 전원OFF");
                        Console.WriteLine("3번 항온항습기 전원OFF");
                        break;
                    case "49,54":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("4번 항온항습기 전원OFF");
                        Console.WriteLine("4번 항온항습기 전원OFF");
                        break;
                    case "49,55":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("5번 항온항습기 전원OFF");
                        Console.WriteLine("5번 항온항습기 전원OFF");
                        break;
                    case "49,56":
                        OnOffmsg = new byte[] { (byte)0x66, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("6번 항온항습기 전원OFF");
                        Console.WriteLine("6번 항온항습기 전원OFF");
                        break;
                    case "49,57":
                        OnOffmsg = new byte[] { (byte)0x67, (byte)0x2F, (byte)0x3F };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("7번 항온항습기 전원OFF");
                        Console.WriteLine("7번 항온항습기 전원OFF");
                        break;
                    case "50,48":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2F, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("8번 항온항습기 전원OFF");
                        Console.WriteLine("8번 항온항습기 전원OFF");
                        break;
                    case "50,49":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2F, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("9번 항온항습기 전원OFF");
                        Console.WriteLine("9번 항온항습기 전원OFF");
                        break;
                    case "50,50":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2F, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("10번 항온항습기 전원OFF");
                        Console.WriteLine("10번 항온항습기 전원OFF");
                        break;
                    case "50,51":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2F, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("11번 항온항습기 전원OFF");
                        Console.WriteLine("11번 항온항습기 전원OFF");
                        break;
                    case "50,52":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2F, (byte)0x3F };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("12번 항온항습기 전원OFF");
                        Console.WriteLine("12번 항온항습기 전원OFF");
                        break;

             //----------온도 증가
                    case "50,53":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("1번 항온항습기 설정온도 증가");
                        Console.WriteLine("1번 항온항습기 설정온도 증가");
                        break;
                    case "50,54":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("2번 항온항습기 설정온도 증가");
                        Console.WriteLine("2번 항온항습기 설정온도 증가");
                        break;
                    case "50,55":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("3번 항온항습기 설정온도 증가");
                        Console.WriteLine("3번 항온항습기 설정온도 증가");
                        break;
                    case "50,56":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("4번 항온항습기 설정온도 증가");
                        Console.WriteLine("4번 항온항습기 설정온도 증가");
                        break;
                    case "50,57":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("5번 항온항습기 설정온도 증가");
                        Console.WriteLine("5번 항온항습기 설정온도 증가");
                        break;
                    case "51,48":
                        OnOffmsg = new byte[] { (byte)0x66, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("6번 항온항습기 설정온도 증가");
                        Console.WriteLine("6번 항온항습기 설정온도 증가");
                        break;
                    case "51,49":
                        OnOffmsg = new byte[] { (byte)0x67, (byte)0x2B, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("7번 항온항습기 설정온도 증가");
                        Console.WriteLine("7번 항온항습기 설정온도 증가");
                        break;
                    case "51,50":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2B, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("8번 항온항습기 설정온도 증가");
                        Console.WriteLine("8번 항온항습기 설정온도 증가");
                        break;
                    case "51,51":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2B, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("9번 항온항습기 설정온도 증가");
                        Console.WriteLine("9번 항온항습기 설정온도 증가");
                        break;
                    case "51,52":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2B, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("10번 항온항습기 설정온도 증가");
                        Console.WriteLine("10번 항온항습기 설정온도 증가");
                        break;
                    case "51,53":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2B, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("11번 항온항습기 설정온도 증가");
                        Console.WriteLine("11번 항온항습기 설정온도 증가");
                        break;
                    case "51,54":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2B, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("12번 항온항습기 설정온도 증가");
                        Console.WriteLine("12번 항온항습기 설정온도 증가");
                        break;
             //----------온도 감소
                    case "51,55":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("1번 항온항습기 설정온도 감소");
                        Console.WriteLine("1번 항온항습기 설정온도 감소");
                        break;
                    case "51,56":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("2번 항온항습기 설정온도 감소");
                        Console.WriteLine("2번 항온항습기 설정온도 감소");
                        break;
                    case "51,57":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("3번 항온항습기 설정온도 감소");
                        Console.WriteLine("3번 항온항습기 설정온도 감소");
                        break;
                    case "52,48":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("4번 항온항습기 설정온도 감소");
                        Console.WriteLine("4번 항온항습기 설정온도 감소");
                        break;
                    case "52,49":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("5번 항온항습기 설정온도 감소");
                        Console.WriteLine("5번 항온항습기 설정온도 감소");
                        break;
                    case "52,50":
                        OnOffmsg = new byte[] { (byte)0x66, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("6번 항온항습기 설정온도 감소");
                        Console.WriteLine("6번 항온항습기 설정온도 감소");
                        break;
                    case "52,51":
                        OnOffmsg = new byte[] { (byte)0x67, (byte)0x2D, (byte)0x32 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("7번 항온항습기 설정온도 감소");
                        Console.WriteLine("7번 항온항습기 설정온도 감소");
                        break;
                    case "52,52":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2D, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("8번 항온항습기 설정온도 감소");
                        Console.WriteLine("8번 항온항습기 설정온도 감소");
                        break;
                    case "52,53":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2D, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("9번 항온항습기 설정온도 감소");
                        Console.WriteLine("9번 항온항습기 설정온도 감소");
                        break;
                    case "52,54":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2D, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("10번 항온항습기 설정온도 감소");
                        Console.WriteLine("10번 항온항습기 설정온도 감소");
                        break;
                    case "52,55":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2D, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("11번 항온항습기 설정온도 감소");
                        Console.WriteLine("11번 항온항습기 설정온도 감소");
                        break;
                    case "52,56":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2D, (byte)0x32 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("12번 항온항습기 설정온도 감소");
                        Console.WriteLine("12번 항온항습기 설정온도 감소");
                        break;
             //----------습도 증가
                    case "52,57":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("1번 항온항습기 설정습도 증가");
                        Console.WriteLine("1번 항온항습기 설정습도 증가");
                        break;
                    case "53,48":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("2번 항온항습기 설정습도 증가");
                        Console.WriteLine("2번 항온항습기 설정습도 증가");
                        break;
                    case "53,49":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("3번 항온항습기 설정습도 증가");
                        Console.WriteLine("3번 항온항습기 설정습도 증가");
                        break;
                    case "53,50":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("4번 항온항습기 설정습도 증가");
                        Console.WriteLine("4번 항온항습기 설정습도 증가");
                        break;
                    case "53,51":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("5번 항온항습기 설정습도 증가");
                        Console.WriteLine("5번 항온항습기 설정습도 증가");
                        break;
                    case "53,52":
                        OnOffmsg = new byte[] { (byte)0x66, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("6번 항온항습기 설정습도 증가");
                        Console.WriteLine("6번 항온항습기 설정습도 증가");
                        break;
                    case "53,53":
                        OnOffmsg = new byte[] { (byte)0x67, (byte)0x2B, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("7번 항온항습기 설정습도 증가");
                        Console.WriteLine("7번 항온항습기 설정습도 증가");
                        break;
                    case "53,54":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2B, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("8번 항온항습기 설정습도 증가");
                        Console.WriteLine("8번 항온항습기 설정습도 증가");
                        break;
                    case "53,55":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2B, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("9번 항온항습기 설정습도 증가");
                        Console.WriteLine("9번 항온항습기 설정습도 증가");
                        break;
                    case "53,56":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2B, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("10번 항온항습기 설정습도 증가");
                        Console.WriteLine("10번 항온항습기 설정습도 증가");
                        break;
                    case "53,57":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2B, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("11번 항온항습기 설정습도 증가");
                        Console.WriteLine("11번 항온항습기 설정습도 증가");
                        break;
                    case "54,48":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2B, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("12번 항온항습기 설정습도 증가");
                        Console.WriteLine("12번 항온항습기 설정습도 증가");
                        break;
            //----------습도 감소
                    case "54,49":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("1번 항온항습기 설정습도 감소");
                        Console.WriteLine("1번 항온항습기 설정습도 감소");
                        break;
                    case "54,50":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("2번 항온항습기 설정습도 감소");
                        Console.WriteLine("2번 항온항습기 설정습도 감소");
                        break;
                    case "54,51":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("3번 항온항습기 설정습도 감소");
                        Console.WriteLine("3번 항온항습기 설정습도 감소");
                        break;
                    case "54,52":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("4번 항온항습기 설정습도 감소");
                        Console.WriteLine("4번 항온항습기 설정습도 감소");
                        break;
                    case "54,53":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("5번 항온항습기 설정습도 감소");
                        Console.WriteLine("5번 항온항습기 설정습도 감소");
                        break;
                    case "54,54":
                        OnOffmsg = new byte[] { (byte)0x66, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("6번 항온항습기 설정습도 감소");
                        Console.WriteLine("6번 항온항습기 설정습도 감소");
                        break;
                    case "54,55":
                        OnOffmsg = new byte[] { (byte)0x67, (byte)0x2D, (byte)0x35 };
                        ww.Stop1(OnOffmsg);
                        ld.logEvent("7번 항온항습기 설정습도 감소");
                        Console.WriteLine("7번 항온항습기 설정습도 감소");
                        break;
                    case "54,56":
                        OnOffmsg = new byte[] { (byte)0x61, (byte)0x2D, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("8번 항온항습기 설정습도 감소");
                        Console.WriteLine("8번 항온항습기 설정습도 감소");
                        break;
                    case "55,57":
                        OnOffmsg = new byte[] { (byte)0x62, (byte)0x2D, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("9번 항온항습기 설정습도 감소");
                        Console.WriteLine("9번 항온항습기 설정습도 감소");
                        break;
                    case "56,48":
                        OnOffmsg = new byte[] { (byte)0x63, (byte)0x2D, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("10번 항온항습기 설정습도 감소");
                        Console.WriteLine("10번 항온항습기 설정습도 감소");
                        break;
                    case "56,49":
                        OnOffmsg = new byte[] { (byte)0x64, (byte)0x2D, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("11번 항온항습기 설정습도 감소");
                        Console.WriteLine("11번 항온항습기 설정습도 감소");
                        break;
                    case "56,50":
                        OnOffmsg = new byte[] { (byte)0x65, (byte)0x2D, (byte)0x35 };
                        ww.Stop2(OnOffmsg);
                        ld.logEvent("12번 항온항습기 설정습도 감소");
                        Console.WriteLine("12번 항온항습기 설정습도 감소");
                        break;
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
        }
    }
}
