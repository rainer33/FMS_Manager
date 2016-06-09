using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Globalization;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace FMS_Manager
{

    class Load
    {
        private string[,] dataGet = new string[5, 79]; // 0:현재수집데이터 1:이전수집데이터 2:현재알람레벨값: 3:이전알람레벨값 4: 최근알람레벨값(감지횟수체크로인한 레벨변환전값저장)
        private bool[,] monitorCHK = new bool[2, 79];   // 0:모니터링 유무 설정 체크 1:알람체크 유무
        private int[,] alarmState = new int[5, 79];    // 0:발생한알람ID저장 1:알람횟수 2:알람횟수카운트3:통보횟수4:통보횟수카운트
        private bool monitorRate;   // 모니터링 딜레이
        private DateTime currentdt = DateTime.Now;
        private string msgInfo = "";
        //private DateTime[] savedt = new DateTime[] { DateTime.Now, DateTime.Now };
        private double[] th = new double[22];  //온도 및 습도 보관하는곳 비교처리를 위해 Double형식으로 보관중
        public string timeText1;
        public string timeText2;
        public string hvicAlram;
        private string alram1, alram2, alram3, alram4, alram5, alram6, alram7, alram8, alram9; 
        

        Timer t1 = new Timer();
        Timer t2 = new Timer();
        Timer t3 = new Timer();

        Config c1 = new Config();

        public void logDate(string result)     //날짜별 로그파일 생성
        {
            try
            {
                DateTime dt = DateTime.Now;
                string aa = dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                string bb = dt.ToString("hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
                File.AppendAllText(@"c:\log\" + aa + ".txt", aa + bb + " : " + result + "\r\n");
                //로그파일 생성
            }
            catch (Exception)
            {
                Console.WriteLine("C드라이브에 log폴더를 생성하시오!!");
            }
        }

        //update
        public void update(string queryString)
        {
            MySqlConnection connection = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            MySqlCommand command1 = new MySqlCommand();

            command1.CommandText = queryString;

            command1.Connection = connection;
            try
            {
                connection.Open();
                command1.ExecuteNonQuery();
            }

            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
            finally
            {
                connection.Close();
            }
        }
        public Water ww
        {
            get
            {
                return Program.ww;
            }
        }
        //public Water ww = new Water();
        public void Loading()
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
            //
            TH thh = new TH();
            UPS up = new UPS();

            mo.LoadMoniteringDB();
            lim.LoadLimiteDB();
            con2.LoadConfigDB();
            em.LoadEmailDB();
            log.LoadLoginDB();
            sch.LoadScheduleDB();
            sms.LoadSmsDB();
            sp.LoadSnmpDB();
            sys.LoadSyscodeDB();
            ssave.LoadSyssavelistDB();

            limitedLoad1();

            for (int i = 0; i < 79; i++)
            {
                dataGet[2, i] = "N";    //현재알람레벨값
                dataGet[3, i] = "N";    //이전알람레벨값
                dataGet[4, i] = "N";    //최근알람레벨값
            }

            Byte[] fByte = new Byte[4];
            fByte[0] = (byte)0xC5;
            fByte[1] = (byte)0xC2;
            fByte[2] = (byte)0x3F;
            fByte[3] = (byte)0x84;

            ClassConvert c = new ClassConvert();
            c.Floating(fByte);
            //float fOutValue = BitConverter.ToSingle(fByte, 0);
            //Console.WriteLine(decimal.Parse(fOutValue));



            t1.Enabled = true;
            t1.Interval = 1000;
            t2.Enabled = true;
            t2.Interval = Convert.ToInt32(sch.schedule[1]) * 1000; // 점점점검주기(초) * 1000
            t3.Enabled = true;
            t3.Interval = 300000;

            t1.Elapsed += new ElapsedEventHandler(OnTimedEvent1);
            t2.Elapsed += new ElapsedEventHandler(OnTimedEvent2);
            t3.Elapsed += new ElapsedEventHandler(OnTimedEvent3);


            //el.Serialtimer1();
           // ww.Serialtimer2();
            //thh.Serialtimer3();
            up.snmptimer();
        }

        #region 타이머

        private void OnTimedEvent1(object source, ElapsedEventArgs e)   //타이머1
        {
            DateTime dt = DateTime.Now;
            timeText1 = dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);   //날짜
            timeText2 = dt.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo);     //시간
            

            //if (dt >= currentdt.AddMinutes(1))  // 폼오픈한 시간보다 1분 이상 경과되었다면
            //{
                monitorRate = true;             // 모니터링 딜레이값 true
            //}

            if (t3.Enabled == false && (timeText2.Substring(3, 2) == "10" || timeText2.Substring(3, 2) == "25" || timeText2.Substring(3, 2) == "40" || timeText2.Substring(3, 2) == "55"))
            // timer3가 fales이고 매시 10분 또는 25분 또는 40분 또는 55분 이면
            {
                t3.Enabled = true;
                t3.Start();     //  timer3 실행 분당 온습도 저장
            }

            if (dt.ToString("mm:ss", DateTimeFormatInfo.InvariantInfo) == "00:00")      //매시 정각에   온습도시간별 저장
            {
                MySqlConnection connection1 = new MySqlConnection (global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
                MySqlCommand sqlComm1 = new MySqlCommand();    //온습도시간별저장프로시저
                MySqlCommand sqlComm2 = new MySqlCommand();
                MySqlCommand sqlComm3 = new MySqlCommand();
                MySqlCommand sqlComm4 = new MySqlCommand();
                sqlComm1.CommandTimeout = sqlComm2.CommandTimeout = sqlComm3.CommandTimeout = sqlComm4.CommandTimeout = 100;

                sqlComm1.CommandText = "call temphumiHHsave";
                sqlComm2.CommandText = "call hvicHHsave";
                sqlComm3.CommandText = "call elecHHsave";
                sqlComm4.CommandText = "call upsHHsave";
                sqlComm1.Connection = sqlComm2.Connection = sqlComm3.Connection = sqlComm4.Connection = connection1;
                try
                {
                    connection1.Open();
                    sqlComm1.ExecuteNonQuery();
                    sqlComm2.ExecuteNonQuery();
                    sqlComm3.ExecuteNonQuery();
                    sqlComm4.ExecuteNonQuery();
                }
                catch (Exception err1)
                {
                    logDate(err1.ToString());
                    Console.WriteLine(err1.ToString());
                    
                }
                finally
                {
                    connection1.Close();
                }


            }
            
            if (dt.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo) == "00:00:00")   //날짜가 바뀌면 
            {

                MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
                MySqlCommand sqlComm10 = new MySqlCommand();
                MySqlCommand sqlComm11 = new MySqlCommand();
                MySqlCommand sqlComm12 = new MySqlCommand();
                MySqlCommand sqlComm13 = new MySqlCommand();
                sqlComm10.CommandTimeout = sqlComm11.CommandTimeout = sqlComm12.CommandTimeout = sqlComm13.CommandTimeout = 100;

                sqlComm10.CommandText = "call temphumiDDsave";    //온습도날짜별저장프로시저(DB수정)
                sqlComm11.CommandText = "call hvicDDsave";
                sqlComm12.CommandText = "call elecDDsave";
                sqlComm13.CommandText = "call upsDDsave";
                sqlComm10.Connection = sqlComm11.Connection = sqlComm12.Connection = sqlComm13.Connection = connection2;
                try
                {
                    connection2.Open();
                    sqlComm10.ExecuteNonQuery();
                    sqlComm11.ExecuteNonQuery();
                    sqlComm12.ExecuteNonQuery();
                    sqlComm13.ExecuteNonQuery();
                }

                catch (Exception err1)
                {
                    logDate(err1.ToString());
                }
                finally
                {
                    connection2.Close();
                }

            }
        }

        private void OnTimedEvent2(object source, ElapsedEventArgs e)   //타이머2
        {
            //Console.WriteLine("타이머2");

            //데이타로드

            cthtemp temp = new cthtemp();
            cthhvic hvic = new cthhvic();
            cthups ups = new cthups();
            cthelec elec = new cthelec();
            cthleak leak = new cthleak();
            cthfire fire = new cthfire();
            temp.LoadthtempDB();
            hvic.LoadthhvicDB();
            ups.LoadthupsDB();
            elec.LoadthelecDB();
            leak.LoadthleakDB();
            fire.LoadthfireDB();

            thdataload();
            hvicdataload();
            upsdataload();
            Elecdataload();
            Leakdataload();
            Firedataload();


            for (int i = 0; i < 79; i++)
            {
                if (monitorCHK[0, i] == true) labelprint(i);                            //화면표출

                if (monitorRate == true && monitorCHK[0, i] == true) sendAlarm(i);      //알람전송
            }
        }

        private void OnTimedEvent3(object source, ElapsedEventArgs e)   //타이머3
        {
            
            try
            {
                DateTime dt = DateTime.Now;
                //if (dt >= savedt[0].AddMinutes(Convert.ToInt32(groupSaveGrid[4, 0].Value))) //오픈한시간(savedt[0])에 save주기를 더한 시간보다 크거나 같으면-5분마다-
                {
                    SaveTHmmLog();  //분단위 저장
                    //savedt[0] = dt; //savedt[0]는 저장한 시간
                    Console.WriteLine(dt);

                }
                limitedLoad1();     //성능모니터링 설정 로딩
                limitedLoad2();     //데이타로드 주기(초) 변경
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void SaveTHmmLog()  //THmmLogT 저장(날짜,코드,온도,습도)
        {
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            MySqlCommand sqlComm10 = new MySqlCommand();
            MySqlCommand sqlComm11 = new MySqlCommand();
            MySqlCommand sqlComm12 = new MySqlCommand();
            MySqlCommand sqlComm13 = new MySqlCommand();
            // sqlComm10.CommandType = CommandType.StoredProcedure;    //온습도 분단위 저장프로시저(DB수정)
            sqlComm10.CommandText = "call hvicMMsave";
            sqlComm11.CommandText = "call temphumiMMsave";
            sqlComm12.CommandText = "call elecMMsave";
            sqlComm13.CommandText = "call upsMMsave";
            sqlComm10.Connection = sqlComm11.Connection = sqlComm12.Connection = sqlComm13.Connection = connection2;
            try
            {
                connection2.Open();
                sqlComm10.ExecuteNonQuery();
                sqlComm11.ExecuteNonQuery();
                sqlComm12.ExecuteNonQuery();
                sqlComm13.ExecuteNonQuery();
            }

            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
            finally
            {
                connection2.Close();
            }
        }

        #endregion

        #region RTU 검색부분

        private void thdataload()    //온습도값 로드
        {
            cthtemp temp = new cthtemp();
            temp.LoadthtempDB();
            try
            {
                dataGet[0, 0] = temp.thtemp[0, 1].ToString();
                dataGet[0, 1] = temp.thtemp[0, 2].ToString();
                dataGet[0, 2] = temp.thtemp[1, 1].ToString();
                dataGet[0, 3] = temp.thtemp[1, 2].ToString();
                dataGet[0, 4] = temp.thtemp[2, 1].ToString();
                dataGet[0, 5] = temp.thtemp[2, 2].ToString();
                dataGet[0, 6] = temp.thtemp[3, 1].ToString();
                dataGet[0, 7] = temp.thtemp[3, 2].ToString();
                dataGet[0, 8] = temp.thtemp[4, 1].ToString();
                dataGet[0, 9] = temp.thtemp[4, 2].ToString();
                dataGet[0, 10] = temp.thtemp[5, 1].ToString();
                dataGet[0, 11] = temp.thtemp[5, 2].ToString();
                dataGet[0, 12] = temp.thtemp[6, 1].ToString();
                dataGet[0, 13] = temp.thtemp[6, 2].ToString();
                dataGet[0, 14] = temp.thtemp[7, 1].ToString();
                dataGet[0, 15] = temp.thtemp[7, 2].ToString();
                dataGet[0, 16] = temp.thtemp[8, 1].ToString();
                dataGet[0, 17] = temp.thtemp[8, 2].ToString();
                dataGet[0, 18] = temp.thtemp[9, 1].ToString();
                dataGet[0, 19] = temp.thtemp[9, 2].ToString();
                dataGet[0, 20] = temp.thtemp[10, 1].ToString();
                dataGet[0, 21] = temp.thtemp[10, 2].ToString();
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void hvicdataload()    //항온항습기값 로드
        {
            
            cthhvic hvic = new cthhvic();
            hvic.LoadthhvicDB();
            try
            {
                dataGet[0, 22] = hvic.thhvic[0, 1].ToString();
                dataGet[0, 23] = hvic.thhvic[1, 1].ToString();
                dataGet[0, 24] = hvic.thhvic[2, 1].ToString();
                dataGet[0, 25] = hvic.thhvic[3, 1].ToString();
                dataGet[0, 26] = hvic.thhvic[4, 1].ToString();
                dataGet[0, 27] = hvic.thhvic[5, 1].ToString();
                dataGet[0, 28] = hvic.thhvic[6, 1].ToString();
                dataGet[0, 29] = hvic.thhvic[7, 1].ToString();
                dataGet[0, 30] = hvic.thhvic[8, 1].ToString();
                dataGet[0, 31] = hvic.thhvic[9, 1].ToString();
                dataGet[0, 32] = hvic.thhvic[10, 1].ToString();
                dataGet[0, 33] = hvic.thhvic[11, 1].ToString();

                for (int i = 0; i < 12; i++)
                {
                    if (hvic.thhvic[i, 17].ToString() == "0" && hvic.thhvic[i, 18].ToString() == "0" && hvic.thhvic[i, 19].ToString() == "0" && hvic.thhvic[i, 20].ToString() == "0" && hvic.thhvic[i, 21].ToString() == "0" && hvic.thhvic[i, 22].ToString() == "0" && hvic.thhvic[i, 23].ToString() == "0" && hvic.thhvic[i, 24].ToString() == "0" && hvic.thhvic[i, 25].ToString() == "0")
                    {
                        dataGet[0, 34 + i] = "N";
                    }
                    else
                    {
                        dataGet[0, 34 + i] = "F";

                        alram1 = alram2 = alram3 = alram4 = alram5 = alram6 = alram7 = alram8 = alram9 = hvicAlram = "";

                        if (hvic.thhvic[i, 17].ToString() == "1")
                        { alram1 = "[고온경보]"; }
                        if (hvic.thhvic[i, 18].ToString() == "1")
                        { alram2 = "[COMP1경보]"; }
                        if (hvic.thhvic[i, 19].ToString() == "1")
                        { alram3 = "[COMP2경보]"; }
                        if (hvic.thhvic[i, 20].ToString() == "1")
                        { alram4 = "[난방히터경보]"; }
                        if (hvic.thhvic[i, 21].ToString() == "1")
                        { alram5 = "[가습히터경보]"; }
                        if (hvic.thhvic[i, 22].ToString() == "1")
                        { alram6 = "[FAN경보]"; }
                        if (hvic.thhvic[i, 23].ToString() == "1")
                        { alram7 = "[누수경보]"; }
                        if (hvic.thhvic[i, 24].ToString() == "1")
                        { alram8 = "[온도센서이상]"; }
                        if (hvic.thhvic[i, 25].ToString() == "1")
                        { alram9 = "[습도센서이상]"; }

                        hvicAlram = alram1 + alram2 + alram3 + alram4 + alram5 + alram6 + alram7 + alram8 + alram9;  
                    }
                }
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void upsdataload()    //UPS값 로드
        {
            cthups ups = new cthups();
            ups.LoadthupsDB();
            try
            {
                dataGet[0, 46] = ups.thups[0, 1].ToString();
                dataGet[0, 47] = ups.thups[1, 1].ToString();
                dataGet[0, 48] = ups.thups[2, 1].ToString();
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void Elecdataload()    //분전반값 로드
        {
            cthelec elec = new cthelec();
            elec.LoadthelecDB();
            try
            {
                dataGet[0, 49] = elec.thelec[0, 0].ToString();
                dataGet[0, 50] = elec.thelec[1, 0].ToString();
                dataGet[0, 51] = elec.thelec[2, 0].ToString();
                dataGet[0, 52] = elec.thelec[3, 0].ToString();
                dataGet[0, 53] = elec.thelec[4, 0].ToString();
                dataGet[0, 54] = elec.thelec[5, 0].ToString();
                dataGet[0, 55] = elec.thelec[6, 0].ToString();
                dataGet[0, 56] = elec.thelec[7, 0].ToString();
                dataGet[0, 57] = elec.thelec[8, 0].ToString();
                dataGet[0, 58] = elec.thelec[9, 0].ToString();
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void Leakdataload() //누수감지값 로드
        {
            cthleak leak = new cthleak();
            leak.LoadthleakDB();
            try
            {
                dataGet[0, 59] = leak.thleak[0, 1].ToString(); //단선감지ch1
                dataGet[0, 60] = leak.thleak[0, 2].ToString(); //누수감지ch1
                dataGet[0, 61] = leak.thleak[1, 1].ToString(); //단선감지ch2
                dataGet[0, 62] = leak.thleak[1, 2].ToString(); //누수감지ch2
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void Firedataload() //화재감지값 로드
        {
            cthfire fire = new cthfire();
            fire.LoadthfireDB();
            try
            {
                dataGet[0, 63] = fire.thfire[1].ToString(); //화재감지1 - 열
                dataGet[0, 64] = fire.thfire[2].ToString(); //화재감지2 - 열
                dataGet[0, 65] = fire.thfire[3].ToString(); //화재감지3 - 열
                dataGet[0, 66] = fire.thfire[4].ToString(); //화재감지4 - 열
                dataGet[0, 67] = fire.thfire[5].ToString(); //화재감지5 - 열
                dataGet[0, 68] = fire.thfire[6].ToString(); //화재감지6 - 열
                dataGet[0, 69] = fire.thfire[7].ToString(); //화재감지7 - 열
                dataGet[0, 70] = fire.thfire[8].ToString(); //화재감지8 - 열
                dataGet[0, 71] = fire.thfire[9].ToString(); //화재감지1 - 가스
                dataGet[0, 72] = fire.thfire[10].ToString(); //화재감지2 - 가스
                dataGet[0, 73] = fire.thfire[11].ToString(); //화재감지3 - 가스
                dataGet[0, 74] = fire.thfire[12].ToString(); //화재감지4 - 가스
                dataGet[0, 75] = fire.thfire[13].ToString(); //화재감지5 - 가스
                dataGet[0, 76] = fire.thfire[14].ToString(); //화재감지6 - 가스
                dataGet[0, 77] = fire.thfire[15].ToString(); //화재감지7 - 가스
                dataGet[0, 78] = fire.thfire[16].ToString(); //화재감지8 - 가스


            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        #endregion


        public void logEvent(string result)    //이벤트로그 리스트뷰 출력
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            string dt2 = dt.ToString("T", DateTimeFormatInfo.InvariantInfo);
            MySqlConnection connection2 = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            MySqlCommand command1 = new MySqlCommand();
            command1.CommandText = "insert into eventLogT (logDate, logTime, logMsg) values ('" + dt1 +
                "','" + dt2 +
                "','" + result +
                "')";
            command1.Connection = connection2;
            try
            {
                connection2.Open();
                command1.ExecuteNonQuery(); //eventLogT 데이타 insert
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
            finally
            {
                connection2.Close();
            }
        }

        #region 환경설정체크부분

        private string alarmCheckTH(int num)    //설정된 온도에따른 알람상태
        {
            climitied lim = new climitied();
            lim.LoadLimiteDB();
            string returnTemp = "";
            if (lim.Limite[num, 7].ToString() != "" && lim.Limite[num, 6].ToString() != "" && (th[num] <= Convert.ToDouble(lim.Limite[num, 7]) && th[num] >= Convert.ToDouble(lim.Limite[num, 6]))) returnTemp = "N";
            //임계치설정 W상한,하한이 공백이 아니고 상한보다 낮고 하한보다 높으면 N
            if (lim.Limite[num, 7].ToString() != "" && lim.Limite[num, 6].ToString() != "" && (th[num] > Convert.ToDouble(lim.Limite[num, 7]) || th[num] < Convert.ToDouble(lim.Limite[num, 6]))) returnTemp = "W";
            //임계치설정 W상한,하한이 공백이 아니고 상한보다 높거나 하한보다 낮으면 W
            if (lim.Limite[num, 8].ToString() != "" && lim.Limite[num, 5].ToString() != "" && (th[num] > Convert.ToDouble(lim.Limite[num, 8]) || th[num] < Convert.ToDouble(lim.Limite[num, 5]))) returnTemp = "C";
            //임계치설정 C상한,하한이 공백이 아니고 상한보다 높거나 하한보다 낮으면 C
            if (lim.Limite[num, 9].ToString() != "" && lim.Limite[num, 4].ToString() != "" && (th[num] > Convert.ToDouble(lim.Limite[num, 9]) || th[num] < Convert.ToDouble(lim.Limite[num, 4]))) returnTemp = "F"; //&& alarmPanel.Visible == false
            //임계치설정 F상한,하한이 공백이 아니고 상한보다 높거나 하한보다 낮으면 F
            return returnTemp;
        }

        private bool datacheck(int num)     //변경된 데이터가 있는지 확인
        {
            if (dataGet[0, num] != dataGet[1, num])
            {
                dataGet[1, num] = dataGet[0, num];
                return true;
            }
            return false;
        }

        private bool alarmlvlcheck(int num)         //알람레벨체크
        {




            if (dataGet[2, num] != dataGet[3, num])
            {

                if (alarmState[2, num] > alarmState[1, num])    //알람횟수 카운트가 알람횟수보다 크면
                {

                    alarmState[2, num] = 0;
                    dataGet[3, num] = dataGet[2, num];
                    return true;
                }
                alarmState[2, num] = alarmState[2, num] + 1;

                //if (num == 29)
                //{
                //    DateTime dt = DateTime.Now;
                //    string aa = dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                //    string bb = dt.ToString("hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
                //    Console.Write(alarmState[1, 29] + "----");
                //    Console.Write(alarmState[2, 29]);
                //    Console.WriteLine(bb);
                //}
            }
            if (dataGet[2, num] == dataGet[3, num])
            {
                //if (num == 29)
                //{
                    alarmState[2, num] = 0;
                //}
            }
            return false;

        }

        private bool alarmCHK(int num) //알람설정이 되어 있는지 확인(알람발송유무)
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            return Convert.ToBoolean(Convert.ToInt32(mo.monitering[num, 5]));
        }

        private bool hourCHK(int num) //시간 체크 되어 있는지 확인(시작시간,종료시간)
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            DateTime dt = DateTime.Now;
            int weekHour = Convert.ToInt32(dt.Hour);
            int st, et = 0;
            st = Convert.ToInt32(mo.monitering[num, 6]);
            et = Convert.ToInt32(mo.monitering[num, 7]);
            if ((st <= weekHour && et > weekHour))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool weekCHK(int num)  //요일확인하기
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            try
            {
                DateTime dt = DateTime.Now;
                string dd = dt.DayOfWeek.ToString("d");
                int week = 0;
                if (dd == "0")
                {
                    week = Convert.ToInt32(dd) + 7;
                }
                else
                {
                    week = Convert.ToInt32(dd);
                }
                bool weekchk = Convert.ToBoolean(Convert.ToInt32(mo.monitering[num, week + 7]));
                return weekchk;
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
                return true;
            }
        }

        public void limitedLoad1()   //성능모니터링설정 로딩
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            try
            {

                for (int i = 0; i < 79; i++)
                {
                    monitorCHK[0, i] = Convert.ToBoolean(Convert.ToInt32(mo.monitering[i, 4]));  //모니터링유무 설정 체크
                    if (alarmCHK(i) == true && hourCHK(i) == true && weekCHK(i) == true)
                    {
                        monitorCHK[1, i] = true;    //알람체크유무 true
                    }
                    else
                    {
                        monitorCHK[1, i] = false;   //알람체크유무 false
                    }
                    alarmState[1, i] = Convert.ToInt32(mo.monitering[i, 15]); //3:장애발생횟수기록
                    alarmState[3, i] = Convert.ToInt32(mo.monitering[i, 16]); //5:통보횟수기록
                }
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }

        private void limitedLoad2() //데이타로드 주기(초) 변경
        {
            cschedule sch = new cschedule();
            sch.LoadScheduleDB();
            try
            {
                DateTime dt = DateTime.Now;
                int week = (int)dt.DayOfWeek;   //요일
                int weekHour = (int)dt.Hour;    //시간
                //타이머 재설정하기

                t2.Enabled = false;
                t2.Interval = Convert.ToInt32(sch.schedule[1]) * 1000; // 점점점검주기(초) * 1000
                t2.Enabled = true;

            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }



        #endregion

        #region 화면표출부분
        //화면표출
        private void labelprint(int num)
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            switch (num)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                    th[num] = Convert.ToDouble(dataGet[0, num]);
                    if (alarmCheckTH(num) != "")
                    {
                        dataGet[2, num] = alarmCheckTH(num);
                    }
                    break;
                case 22:    //hvic1 - 동작
                case 23:    //hvic2
                case 24:    //hvic3
                case 25:    //hvic4
                case 26:    //hvic5
                case 27:    //hvic6
                case 28:    //hvic7
                case 29:    //hvic8
                case 30:    //hvic9
                case 31:    //hvic10
                case 32:    //hvic11
                case 33:    //hvic12
                    if (dataGet[0, num] == "0")
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "단선");
                    }
                    if (dataGet[0, num] == "1")
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "정지");
                    }
                    if (dataGet[0, num] == "err")
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "통신오류");
                    }
                    if (dataGet[0, num] == "2")
                    {
                        dataGet[2, num] = "N";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "동작");
                    }
                    break;
                case 34:    //hvic1 - 알람
                case 35:    //hvic2
                case 36:    //hvic3
                case 37:    //hvic4
                case 38:    //hvic5
                case 39:    //hvic6
                case 40:    //hvic7
                case 41:    //hvic8
                case 42:    //hvic9
                case 43:    //hvic10
                case 44:    //hvic11
                case 45:    //hvic12

                    if (dataGet[0, num] == "N")
                    {
                        dataGet[2, num] = "N";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "정상작동");
                    }
                    if (dataGet[0, num] == "F")
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + hvicAlram + "알람발생");
                    }
                    break;

                case 46:    //UPS1
                case 47:    //UPS2
                case 48:    //UPS3
                    if (dataGet[0, num] == "1" || dataGet[0, num] == "2" || dataGet[0, num] == "4" || dataGet[0, num] == "5")
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "정지");
                    }
                    if (dataGet[0, num] == "3")
                    {
                        dataGet[2, num] = "N";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "동작");
                    }
                    break;
                case 49:    //분전반
                case 50:    //분전반
                case 51:    //분전반
                case 52:    //분전반
                case 53:    //분전반
                case 54:    //분전반
                case 55:    //분전반
                case 56:    //UPS분전반
                case 57:    //UPS분전반
                case 58:    //UPS분전반
                    if (dataGet[0, num] == "err")
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "정지");
                    }
                    else
                    {
                        dataGet[2, num] = "N";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "동작");
                    }
                    break;
                case 59:    //단선감지ch1
                case 60:    //누수감지ch1
                case 61:    //단선감지ch2
                case 62:    //누수감지ch2
                    if (dataGet[0, num] == "0")
                    {
                        dataGet[2, num] = "N";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "정상작동");
                    }
                    else
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "알람발생");

                    }
                    break;
                case 63:    //화재감지1 - 열
                case 64:    //화재감지2 - 열
                case 65:    //화재감지3 - 열
                case 66:    //화재감지4 - 열
                case 67:    //화재감지5 - 열
                case 68:    //화재감지6 - 열
                case 69:    //화재감지7 - 열
                case 70:    //화재감지8 - 열
                case 71:    //화재감지1 - 가스
                case 72:    //화재감지2 - 가스
                case 73:    //화재감지3 - 가스
                case 74:    //화재감지4 - 가스
                case 75:    //화재감지5 - 가스
                case 76:    //화재감지6 - 가스
                case 77:    //화재감지7 - 가스
                case 78:    //화재감지8 - 가스
                    if (dataGet[0, num] == "0.0 0")
                    {
                        dataGet[2, num] = "N";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "정상작동");
                    }
                    else
                    {
                        dataGet[2, num] = "F";
                        if (datacheck(num) == true) logEvent(mo.monitering[num, 2].ToString() + "알람발생");
                    }
                    break;
            }
        }
        //알람표출
        private void sendAlarm(int num)
        {
            cthleak leak = new cthleak();
            leak.LoadthleakDB();
            msgInfo = "";
            switch (num)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:

                    msgInfo = "현재값:" + dataGet[0, num];

                    break;
                case 22:    //hvic1 - 동작
                case 23:    //hvic2
                case 24:    //hvic3
                case 25:    //hvic4
                case 26:    //hvic5
                case 27:    //hvic6
                case 28:    //hvic7
                case 29:    //hvic8
                case 30:    //hvic9
                case 31:    //hvic10
                case 32:    //hvic11
                case 33:    //hvic12
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        if (dataGet[0, num] == "0")
                        {
                            msgInfo = " 단선 발생";
                        }
                        if (dataGet[0, num] == "1")
                        {
                            msgInfo = " 정지 발생";
                        }
                        if (dataGet[0, num] == "err")
                        {
                            msgInfo = " 통신오류 발생";
                        }
                    }
                    break;
                case 34:    //hvic1 - 알람
                case 35:    //hvic2
                case 36:    //hvic3
                case 37:    //hvic4
                case 38:    //hvic5
                case 39:    //hvic6
                case 40:    //hvic7
                case 41:    //hvic8
                case 42:    //hvic9
                case 43:    //hvic10
                case 44:    //hvic11
                case 45:    //hvic12
                    if (dataGet[2, num] == "N" && dataGet[2, num - 12] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = hvicAlram + "알람발생";
                    }
                    break;

                case 46:    //UPS1
                case 47:    //UPS2
                case 48:    //UPS3

                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생";
                    }
                    break;
                case 49:    //분전반
                case 50:    //분전반
                case 51:    //분전반
                case 52:    //분전반
                case 53:    //분전반
                case 54:    //분전반
                case 55:    //분전반
                case 56:    //UPS분전반
                case 57:    //UPS분전반
                case 58:    //UPS분전반
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생";
                    }
                    break;
                case 59:    //단선감지ch1
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생 단선감지";
                    }
                    break;
                case 60:    //누수감지ch1
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생 1차누수(m):" + leak.thleak[0, 3].ToString() + " 2차누수(m):" + leak.thleak[0, 4].ToString();
                    }
                    break;
                case 61:    //단선감지ch2
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생 단선감지";
                    }
                    break;
                case 62:    //누수감지ch2
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생 1차누수(m):" + leak.thleak[1, 3].ToString() +" 2차누수(m):" + leak.thleak[1, 4].ToString();
                    }
                    break;
                case 63:    //화재감지1 - 열
                case 64:    //화재감지2 - 열
                case 65:    //화재감지3 - 열
                case 66:    //화재감지4 - 열
                case 67:    //화재감지5 - 열
                case 68:    //화재감지6 - 열
                case 69:    //화재감지7 - 열
                case 70:    //화재감지8 - 열
                case 71:    //화재감지1 - 가스
                case 72:    //화재감지2 - 가스
                case 73:    //화재감지3 - 가스
                case 74:    //화재감지4 - 가스
                case 75:    //화재감지5 - 가스
                case 76:    //화재감지6 - 가스
                case 77:    //화재감지7 - 가스
                case 78:    //화재감지8 - 가스
                    if (dataGet[2, num] == "N")
                    {
                        msgInfo = "정상복구";
                    }
                    if (dataGet[2, num] == "F")
                    {
                        msgInfo = "장애발생";
                    }
                    break;
            }

            if (alarmlvlcheck(num) == true)
            {
                alarmState[4, num] = 0;

                if (dataGet[2, num] == "N" && alarmState[4, num] < alarmState[3, num])
                {
                    logAlarmRecover(num, "A", dataGet[4, num]);
                    if (monitorCHK[1, num] == true)
                    {
                        if (dataGet[4, num] == "F")
                        {
                            smsSendOpen(num, dataGet[2, num], msgInfo);
                        }
                        mailSendOpen(num, dataGet[2, num], msgInfo);
                        SnmpTrapClose(num, dataGet[2, num], msgInfo);
                    }
                    dataGet[4, num] = "N";

                }
                else if (dataGet[2, num] == "W" && alarmState[4, num] < alarmState[3, num])
                {
                    logAlarm(num, dataGet[2, num], msgInfo);
                    alarmState[0, num] = c1.GetIDInfo;
                    if (monitorCHK[1, num] == true)
                    {
                        if (dataGet[4, num] == "F")
                        {
                            smsSendOpen(num, dataGet[2, num], "W으로 변환 " + msgInfo);
                        }
                        mailSendOpen(num, dataGet[2, num], msgInfo);
                        SnmpTrapOpen(num, dataGet[2, num], msgInfo);
                    }
                    dataGet[4, num] = "W";
                }
                else if (dataGet[2, num] == "C" && alarmState[4, num] < alarmState[3, num])
                {
                    logAlarm(num, dataGet[2, num], msgInfo);
                    alarmState[0, num] = c1.GetIDInfo;
                    if (monitorCHK[1, num] == true)
                    {
                        if (dataGet[4, num] == "F")
                        {
                            smsSendOpen(num, dataGet[2, num], "C으로 변환 " + msgInfo);
                        }
                        mailSendOpen(num, dataGet[2, num], msgInfo);
                        SnmpTrapOpen(num, dataGet[2, num], msgInfo);
                    }
                    dataGet[4, num] = "C";
                }
                else if (dataGet[2, num] == "F" && alarmState[4, num] < alarmState[3, num])
                {
                    logAlarm(num, dataGet[2, num], msgInfo);
                    alarmState[0, num] = c1.GetIDInfo;
                    if (monitorCHK[1, num] == true)
                    {
                        smsSendOpen(num, dataGet[2, num], msgInfo);
                        mailSendOpen(num, dataGet[2, num], msgInfo);
                        SnmpTrapOpen(num, dataGet[2, num], msgInfo);
                    }
                    dataGet[4, num] = "F";
                }
                alarmState[4, num] = alarmState[4, num] + 1;
            }
        }
        #endregion

        #region 알람전송부분
        private void mailSendOpen(int num, string smsMsg1, string smsMsg2)
        {
            csms sms = new csms();
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            sms.LoadSmsDB();
            int sysCodeNum = 0;
            string smsMsg = "장비명:" + mo.monitering[num, 2].ToString() + "/" + mo.monitering[num, 3].ToString() + " 알람레벨:" + smsMsg1 + " 내용:" + smsMsg2;
            switch (mo.monitering[num, 1].ToString())
            {
                case "A":
                    sysCodeNum = 7;
                    break;
                case "B":
                    sysCodeNum = 8;
                    break;
                case "C":
                    sysCodeNum = 9;
                    break;
                case "D":
                    sysCodeNum = 10;
                    break;
                case "E":
                    sysCodeNum = 11;
                    break;
                case "F":
                    sysCodeNum = 12;
                    break;
                case "G":
                    sysCodeNum = 13;
                    break;
                case "H":
                    sysCodeNum = 14;
                    break;
                case "I":
                    sysCodeNum = 15;
                    break;
                case "J":
                    sysCodeNum = 16;
                    break;
                case "K":
                    sysCodeNum = 17;
                    break;
                case "L":
                    sysCodeNum = 18;
                    break;
                case "M":
                    sysCodeNum = 19;
                    break;
                case "N":
                    sysCodeNum = 20;
                    break;
                case "O":
                    sysCodeNum = 21;
                    break;
                case "P":
                    sysCodeNum = 22;
                    break;
                case "Q":
                    sysCodeNum = 23;
                    break;
                case "R":
                    sysCodeNum = 24;
                    break;
                case "S":
                    sysCodeNum = 25;
                    break;
                case "T":
                    sysCodeNum = 26;
                    break;
                case "U":
                    sysCodeNum = 27;
                    break;
                case "V":
                    sysCodeNum = 28;
                    break;
                case "W":
                    sysCodeNum = 29;
                    break;
                case "X":
                    sysCodeNum = 30;
                    break;
                case "Y":
                    sysCodeNum = 31;
                    break;
                case "Z":
                    sysCodeNum = 32;
                    break;
                case "Za":
                    sysCodeNum = 33;
                    break;
                case "Zb":
                    sysCodeNum = 34;
                    break;
                case "Zc":
                    sysCodeNum = 35;
                    break;
                case "Zd":
                    sysCodeNum = 36;
                    break;
                case "Ze":
                    sysCodeNum = 37;
                    break;
                case "Zf":
                    sysCodeNum = 38;
                    break;
                case "Zg":
                    sysCodeNum = 39;
                    break;
                case "Zh":
                    sysCodeNum = 40;
                    break;
                case "Zi":
                    sysCodeNum = 41;
                    break;
                case "Zj":
                    sysCodeNum = 42;
                    break;
                case "Zk":
                    sysCodeNum = 43;
                    break;
                case "Zl":
                    sysCodeNum = 44;
                    break;
                case "Zm":
                    sysCodeNum = 45;
                    break;
                case "Zn":
                    sysCodeNum = 46;
                    break;
                case "Zo":
                    sysCodeNum = 47;
                    break;
                case "Zp":
                    sysCodeNum = 48;
                    break;
                case "Zq":
                    sysCodeNum = 49;
                    break;
                case "Zr":
                    sysCodeNum = 50;
                    break;
                case "Zs":
                    sysCodeNum = 51;
                    break;
                case "Zt":
                    sysCodeNum = 52;
                    break;
            }

            for (int i = 0; i < 50; i++)
            {
                if (sms.Sms[i, 0] == null) { return; }
                if (sms.Sms[i, sysCodeNum].ToString() == "1" && sms.Sms[i, 5].ToString() == "1" && sms.Sms[i, 3].ToString() != "")
                {
                    sendMailOpen(num, sms.Sms[i, 3].ToString(), smsMsg);
                }
            }
        }
        // sms
        private void smsSendOpen(int num, string smsMsg1, string smsMsg2)
        {
            csms sms = new csms();
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            sms.LoadSmsDB();
            int sysCodeNum = 0;
            string sendType = "[]";
            if (smsMsg1 == "F")
            {
                sendType = "[발생]";
            }
            else if (smsMsg1 == "N")
            {
                sendType = "[복구]";
            }
            else
            {
                sendType = "";
            }

            string smsMsg = sendType + mo.monitering[num, 2].ToString() + "/" + mo.monitering[num, 3].ToString() + "/" + smsMsg2 + "-성남시청";
            switch (mo.monitering[num, 1].ToString())
            {
                case "A":
                    sysCodeNum = 7;
                    break;
                case "B":
                    sysCodeNum = 8;
                    break;
                case "C":
                    sysCodeNum = 9;
                    break;
                case "D":
                    sysCodeNum = 10;
                    break;
                case "E":
                    sysCodeNum = 11;
                    break;
                case "F":
                    sysCodeNum = 12;
                    break;
                case "G":
                    sysCodeNum = 13;
                    break;
                case "H":
                    sysCodeNum = 14;
                    break;
                case "I":
                    sysCodeNum = 15;
                    break;
                case "J":
                    sysCodeNum = 16;
                    break;
                case "K":
                    sysCodeNum = 17;
                    break;
                case "L":
                    sysCodeNum = 18;
                    break;
                case "M":
                    sysCodeNum = 19;
                    break;
                case "N":
                    sysCodeNum = 20;
                    break;
                case "O":
                    sysCodeNum = 21;
                    break;
                case "P":
                    sysCodeNum = 22;
                    break;
                case "Q":
                    sysCodeNum = 23;
                    break;
                case "R":
                    sysCodeNum = 24;
                    break;
                case "S":
                    sysCodeNum = 25;
                    break;
                case "T":
                    sysCodeNum = 26;
                    break;
                case "U":
                    sysCodeNum = 27;
                    break;
                case "V":
                    sysCodeNum = 28;
                    break;
                case "W":
                    sysCodeNum = 29;
                    break;
                case "X":
                    sysCodeNum = 30;
                    break;
                case "Y":
                    sysCodeNum = 31;
                    break;
                case "Z":
                    sysCodeNum = 32;
                    break;
                case "Za":
                    sysCodeNum = 33;
                    break;
                case "Zb":
                    sysCodeNum = 34;
                    break;
                case "Zc":
                    sysCodeNum = 35;
                    break;
                case "Zd":
                    sysCodeNum = 36;
                    break;
                case "Ze":
                    sysCodeNum = 37;
                    break;
                case "Zf":
                    sysCodeNum = 38;
                    break;
                case "Zg":
                    sysCodeNum = 39;
                    break;
                case "Zh":
                    sysCodeNum = 40;
                    break;
                case "Zi":
                    sysCodeNum = 41;
                    break;
                case "Zj":
                    sysCodeNum = 42;
                    break;
                case "Zk":
                    sysCodeNum = 43;
                    break;
                case "Zl":
                    sysCodeNum = 44;
                    break;
                case "Zm":
                    sysCodeNum = 45;
                    break;
                case "Zn":
                    sysCodeNum = 46;
                    break;
                case "Zo":
                    sysCodeNum = 47;
                    break;
                case "Zp":
                    sysCodeNum = 48;
                    break;
                case "Zq":
                    sysCodeNum = 49;
                    break;
                case "Zr":
                    sysCodeNum = 50;
                    break;
                case "Zs":
                    sysCodeNum = 51;
                    break;
                case "Zt":
                    sysCodeNum = 52;
                    break;
            }





            for (int i = 0; i < 30; i++)
            {
                if (sms.Sms[i, 0] == null) { return; }
                if (sms.Sms[i, sysCodeNum].ToString() == "1" && sms.Sms[i, 4].ToString() == "1" && sms.Sms[i, 2].ToString() != "")
                {
                    Console.WriteLine(smsMsg);
                    CreateMSConnection(num, sms.Sms[i, 2].ToString(), smsMsg, sms.Sms[i, 1].ToString());
                }
            }

        }
        // mail
        private void sendMailOpen(int num, string mailTo, string mailMsg)
        {
            cemail em = new cemail();
            em.LoadEmailDB();
            MailMessage message = new MailMessage();
            message.From = new MailAddress(em.Email[3].ToString(), "FMS발송자", Encoding.UTF8);
            message.To.Add(new MailAddress(mailTo, "FMS수신자", Encoding.UTF8));
            message.Subject = "성남시청FMS전송";
            message.SubjectEncoding = Encoding.UTF8;
            message.Priority = MailPriority.Normal;
            message.IsBodyHtml = false;
            message.BodyEncoding = Encoding.UTF8;
            message.Body = mailMsg;
            SmtpClient smtp = new SmtpClient(em.Email[4].ToString(), Convert.ToInt32(em.Email[5].ToString()));

            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new NetworkCredential(em.Email[1].ToString(), em.Email[2].ToString());
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                smtp.Send(message); // 작성한 메일메세지를 Smtp로 발송합니다.
                logEvent("메일이 발송되었습니다.  메일주소: " + mailTo + "   메세지: " + mailMsg);

                MySqlConnection conn = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
                string dt1 = timeText1;
                string dt2 = timeText2;
                MySqlCommand command1 = new MySqlCommand();

                command1.CommandText = "update AlarmLogT SET sendDate3='" + dt1 +
                    "', sendTime3='" + dt2 + "' WHERE ID='" + alarmState[0, num] + "'";

                command1.Connection = conn;
                try
                {
                    conn.Open();
                    command1.ExecuteNonQuery();
                    Console.WriteLine("메일발송완료.  메일주소: " + mailTo + "   메세지: " + mailMsg);
                }

                catch (Exception err1)
                {
                    logDate(err1.ToString());
                }
                finally
                {
                    conn.Close();
                }

            }
            catch (Exception err1)
            {
                logEvent("메일이 발송실패.  메일주소: " + mailTo + "   메세지: " + mailMsg);
                logDate(err1.ToString());
            }
        }
        //snmp trap open
        private void SnmpTrapOpen(int num, string msg1, string msg2)
        {
            csnmp sp = new csnmp();
            cmonitering mo = new cmonitering();
            sp.LoadSnmpDB();
            mo.LoadMoniteringDB();
            try
            {

                string snmptext = "/c c:\\snmptrap\\snmptrap -v 1 -c " +
                  sp.Snmp[1].ToString() +
                  " " + sp.Snmp[2].ToString() +
                  " " + sp.Snmp[3].ToString() +
                  " " + sp.Snmp[4].ToString() +
                  " " + sp.Snmp[5].ToString() +
                  " " + sp.Snmp[6].ToString() +
                  " " + sp.Snmp[7].ToString() +
                  " " + sp.Snmp[8].ToString() +
                  " s \"" + mo.monitering[num, 2].ToString() +
                  "\" " + sp.Snmp[10].ToString() +
                  " s \"" + mo.monitering[num, 3].ToString() + "-" + msg2 + "/" + msg1 +
                  "\" " + sp.Snmp[12].ToString() +
                  " s \"open\"";
                Process p = new Process();
                ProcessStartInfo pinfo = new ProcessStartInfo();
                p.StartInfo = pinfo;
                pinfo.FileName = "cmd.exe";
                pinfo.Arguments = snmptext;
                //pinfo.Arguments = @"/c copy c:\installer.log c:\installer.txt";
                pinfo.UseShellExecute = false;
                pinfo.CreateNoWindow = true;
                pinfo.RedirectStandardInput = true;
                pinfo.RedirectStandardError = false;
                pinfo.RedirectStandardOutput = false;
                p.Start();
                //p.StandardInput.WriteLine(@"copy c:\installer.log c:\installer.txt");
                //p.StandardInput.Flush();
                //화일이 있으면 덮어쓴다.
                //  p.StandardInput.WriteLine(@"Y");
                //  p.StandardInput.Flush();
                //textBox2.Text = pinfo.Arguments;
                p.Close();

                MySqlConnection conn = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
                string dt1 = timeText1;
                string dt2 = timeText2;
                MySqlCommand command1 = new MySqlCommand();

                command1.CommandText = "update AlarmLogT SET sendDate1='" + dt1 +
                    "', sendTime1='" + dt2 +
                    "' WHERE ID='" + alarmState[0, num] + "'";

                command1.Connection = conn;
                try
                {
                    conn.Open();
                    command1.ExecuteNonQuery();
                    logEvent("snmp Trap 발송되었습니다. 메세지 : 장비명:" + mo.monitering[num, 2].ToString() + "/" + mo.monitering[num, 3].ToString() + " 알람레벨: " + msg1 + " 내용: " + msg2);
                }

                catch (Exception err1)
                {
                    logDate(err1.ToString());
                }
                finally
                {
                    conn.Close();
                }

            }

            catch (Exception err2)
            {
                logDate(err2.ToString());
            }
        }
        //snmp trap close
        private void SnmpTrapClose(int num, string msg1, string msg2)
        {
            csnmp sp = new csnmp();
            cmonitering mo = new cmonitering();
            sp.LoadSnmpDB();
            mo.LoadMoniteringDB();
            string gname = mo.monitering[num, 2].ToString();
            try
            {
                //string snmptext = "/c " + Application.StartupPath + @"\snmptrap\snmptrap -v 1 -c " +
                string snmptext = "/c c:\\snmptrap\\snmptrap -v 1 -c " +
                  sp.Snmp[1].ToString() +
                  " " + sp.Snmp[2].ToString() +
                  " " + sp.Snmp[3].ToString() +
                  " " + sp.Snmp[4].ToString() +
                  " " + sp.Snmp[5].ToString() +
                  " " + sp.Snmp[6].ToString() +
                  " " + sp.Snmp[7].ToString() +
                  " " + sp.Snmp[8].ToString() +
                  " s \"" + mo.monitering[num, 2].ToString() +
                  "\" " + sp.Snmp[10].ToString() +
                  " s \"" + mo.monitering[num, 3].ToString() + "-" + msg2 + "/" + msg1 +
                  "\" " + sp.Snmp[12].ToString() +
                                  " s \"close\"";
                Process p = new Process();
                ProcessStartInfo pinfo = new ProcessStartInfo();
                p.StartInfo = pinfo;
                pinfo.FileName = "cmd.exe";
                pinfo.Arguments = snmptext;
                //pinfo.Arguments = @"/c copy c:\installer.log c:\installer.txt";
                pinfo.UseShellExecute = false;
                pinfo.CreateNoWindow = true;
                pinfo.RedirectStandardInput = true;
                pinfo.RedirectStandardError = false;
                pinfo.RedirectStandardOutput = false;
                p.Start();

                logEvent("snmp Trap 발송되었습니다. 메세지 : 장비명:" + mo.monitering[num, 2].ToString() + "/" + mo.monitering[num, 3].ToString() + " 알람레벨: " + msg1 + " 내용: " + msg2);
                //p.StandardInput.WriteLine(@"copy c:\installer.log c:\installer.txt");
                //p.StandardInput.Flush();
                //화일이 있으면 덮어쓴다.
                //  p.StandardInput.WriteLine(@"Y");
                //  p.StandardInput.Flush();
                //textBox2.Text = pinfo.Arguments;
                p.Close();
            }

            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
        }
        #endregion

        #region 데이터베이스저장부분

        //sms MS_SQLDB 접근
        private void CreateMSConnection(int num, string smsNum, string smsMsg, string smsName)
        {
            cconfig2 con2 = new cconfig2();
            con2.LoadConfigDB();
            DateTime dt = DateTime.Now;
            string aa = dt.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
            string bb = dt.ToString("HHmmss", DateTimeFormatInfo.InvariantInfo);
            string cc = dt.ToString("HH시mm분ss초", DateTimeFormatInfo.InvariantInfo);
            SqlConnection connection = new SqlConnection(global::FMS_Manager.Properties.Settings.Default.RDBConnectionString);
            SqlCommand command1 = new SqlCommand();
            //(targetname, phone, no_send, ms_acs, ms_sms, send_flag, read_flag, reserve_flag, reserve_time, notice_name, dt_create)" + "VALUES ('" + smsName + "', '" + smsNum + "', '" + con2.config[5].ToString() + "', ' ','" + smsMsg + "-" + cc + "', '1', '', 'N', '" + aa + bb + "', 'FMS연동', " + aa + bb + ");";

            command1.CommandText = "INSERT  INTO  em_tran(tran_refkey, tran_id, tran_phone, tran_callback, tran_date, tran_status, tran_msg, tran_etc1, tran_etc2, tran_type)" + "VALUES ('19', '1000', '" + smsNum + "', '" + con2.config[5].ToString() + "', getdate(), '1', '" + smsMsg + "-" + cc + "', '" + smsName + "', '1019', '0');";
            command1.Connection = connection;
            try
            {
                connection.Open();
                command1.ExecuteNonQuery();
                logEvent("단문자 발송되었습니다. 휴대전화번호 :" + smsNum + " 메세지 : " + smsMsg);
                smsbackSave(smsNum, smsMsg);
            }
            catch (Exception err1)
            {
                logEvent("단문자 발송실패하였습니다. 휴대전화번호 :" + smsNum + " 메세지 : " + smsMsg);
                logDate(err1.ToString());
            }
            finally
            {
                connection.Close();
            }



            MySqlConnection conn = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string dt1 = timeText1;
            string dt2 = timeText2;
            MySqlCommand command2 = new MySqlCommand();

            command2.CommandText = "update AlarmLogT SET sendDate2='" + dt1 +
                "', sendTime2='" + dt2 +
                "' WHERE ID='" + alarmState[0, num] + "'";

            command2.Connection = conn;
            try
            {
                conn.Open();
                command2.ExecuteNonQuery();
            }
            catch (Exception err2)
            {
                logDate(err2.ToString());
            }
            finally
            {
                conn.Close();
            }

        }

        private void smsbackSave(string smsNum, string smsMsg)
        {
            cconfig2 con2 = new cconfig2();
            con2.LoadConfigDB();
            MySqlConnection connection = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string que1 = "Insert into sc_tran(tr_senddate,tr_sendtime, tr_phone, tr_callback, tr_msg) values ('" + timeText1 + "','" + timeText2 + "','" + smsNum + "','" + con2.config[5].ToString() + "','" + smsMsg + "')";
            MySqlCommand comm = new MySqlCommand(que1, connection);
            try
            {
                connection.Open();
                comm.ExecuteNonQuery();

            }
            catch (Exception err1)
            {
                logEvent("단문자 발송은성공 했지만 기록에실패하였습니다. 휴대전화번호 :" + smsNum + " 메세지 : " + smsMsg);
                logDate(err1.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        //장애이력기록
        private void logAlarm(int num, string alarmLevel, string alarmMsg)
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            string dt1 = timeText1;
            string dt2 = timeText2;
            MySqlConnection connection = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            
            MySqlCommand command0 = new MySqlCommand();
            MySqlCommand command1 = new MySqlCommand();
            command0.CommandText = "update AlarmLogT SET endDate='" + dt1 +
                                "', endTime='" + dt2 +
                                "', recoverState='A" +
                                "', recoverMsg='알람레벨:" + dataGet[3, num] + "으로 변환" +
                                "' WHERE ID='" + alarmState[0, num] + "'";
            command1.CommandText = "Insert into AlarmLogT" + "(logDate, logTime, alarmLevel, sysCode, sysCodeNum,alarmMsg,recoverState)" +
                               "values('" + dt1 + "','" + dt2 +
                               "','" + alarmLevel +
                               "','" + mo.monitering[num, 1] +
                               "','" + mo.monitering[num, 17] +
                               "','" + alarmMsg +
                               "','C')";
            command0.Connection = connection;
            command1.Connection = connection;
            try
            {
                connection.Open();
                ///
                if (dataGet[4, num] != "N") command0.ExecuteNonQuery();
                ///
                command1.ExecuteNonQuery();
                logEvent("장비명: " + mo.monitering[num, 2] + "/" + mo.monitering[num, 3] + "알람레벨: " + alarmLevel + "내용: " + alarmMsg);
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        //장애이력자동복구기록
        private void logAlarmRecover(int num, string recoverAuto, string recoverID)
        {
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            MySqlConnection connection = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            string dt1 = timeText1;
            string dt2 = timeText2;
            MySqlCommand command1 = new MySqlCommand();

            command1.CommandText = "update AlarmLogT SET endDate='" + dt1 +
                "', endTime='" + dt2 +
                "', recoverState='" + recoverAuto +
                "', recoverMsg='알람레벨:" + recoverID + "에서 복구" +
                "' WHERE ID='" + alarmState[0, num] + "'";
            command1.Connection = connection;
            try
            {
                connection.Open();
                command1.ExecuteNonQuery();
                logEvent(mo.monitering[num, 2] + "/" + mo.monitering[num, 3] + "자동복구");
            }
            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        //패스워드저장
        private void userUpdate(string userid, string userpwd)
        {
            MySqlConnection connection = new MySqlConnection(global::FMS_Manager.Properties.Settings.Default.fmsDBConnectionString);
            MySqlCommand command1 = new MySqlCommand();

            command1.CommandText = "update loginT SET userpwd='" + userpwd + "' WHERE userid='" + userid + "'";

            command1.Connection = connection;
            try
            {
                connection.Open();
                command1.ExecuteNonQuery();
            }

            catch (Exception err1)
            {
                logDate(err1.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

    }
}
