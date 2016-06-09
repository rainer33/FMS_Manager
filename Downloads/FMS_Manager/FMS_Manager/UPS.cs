using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using MySql.Data;
using MySql.Data.MySqlClient;
using nsoftware.IPWorks;

namespace FMS_Manager
{
    class UPS
    {
        //시리얼포트 선언
        Timer t1 = new Timer();
        Load ld = new Load();
        ClassConvert c = new ClassConvert();

        private nsoftware.IPWorks.Snmpmgr snmpmgr1 = new nsoftware.IPWorks.Snmpmgr();
        private string[] lvwResults1 = new string[18];  //snmp1값저장
        private string[] lvwResults2 = new string[18];  //snmp2값저장
        private string[] lvwResults3 = new string[18];  //snmp3값저장
        private int upsCountNum = 0;

        public void snmptimer()  //UPS
        {
            cschedule sch = new cschedule();
            sch.LoadScheduleDB();
            //this.snmpmgr1.About = "";
            //this.snmpmgr1.Community = "neisapc";
            //this.snmpmgr1.InvokeThrough = null;
            //this.snmpmgr1.LocalEngineId = "";
            //this.snmpmgr1.LocalEngineIdB = new byte[0];
            //this.snmpmgr1.RemoteEngineId = "";
            //this.snmpmgr1.RemoteEngineIdB = new byte[0];
            this.snmpmgr1.SNMPVersion = SnmpmgrSNMPVersions.snmpverV2c;
            this.snmpmgr1.Timeout = 1;
            this.snmpmgr1.OnTrap += new nsoftware.IPWorks.Snmpmgr.OnTrapHandler(this.snmpmgr1_OnTrap);
            this.snmpmgr1.OnResponse += new nsoftware.IPWorks.Snmpmgr.OnResponseHandler(this.snmpmgr1_OnResponse);
            this.snmpmgr1.OnError += new nsoftware.IPWorks.Snmpmgr.OnErrorHandler(this.snmpmgr1_OnError);
            this.snmpmgr1.OnPacketTrace += new nsoftware.IPWorks.Snmpmgr.OnPacketTraceHandler(this.snmpmgr1_OnPacketTrace);


            t1.Enabled = true;
            t1.Interval = Convert.ToInt32(sch.schedule[5]) * 1000; // 점점점검주기(초) * 1000
            t1.Elapsed += new ElapsedEventHandler(OnTimedEvent1);

        }

        private string oidToName(string oid)
        {
            switch (oid)
            {
                //system descriptors
                case "1.3.6.1.2.1.1.1.0": return "sysDescr";
                case "1.3.6.1.2.1.1.2.0": return "sysObjectID";
                case "1.3.6.1.2.1.1.3.0": return "sysUpTime";
                case "1.3.6.1.2.1.1.4.0": return "sysContact";
                case "1.3.6.1.2.1.1.5.0": return "sysName";
                case "1.3.6.1.2.1.1.6.0": return "sysLocation";
                case "1.3.6.1.2.1.1.7.0": return "sysServices";

                //traps
                case "1.3.6.1.2.1.33.1.6.3.1": return "upsAlarmBatteryBad";
                case "1.3.6.1.2.1.33.1.6.3.2": return "upsAlarmOnBattery";	
                case "1.3.6.1.2.1.33.1.6.3.3": return "upsAlarmLowBattery";	
                case "1.3.6.1.2.1.33.1.6.3.4": return "upsAlarmDepletedBattery";	
                case "1.3.6.1.2.1.33.1.6.3.5": return "upsAlarmTempBad";	
                case "1.3.6.1.2.1.33.1.6.3.6": return "upsAlarmInputBad";
                case "1.3.6.1.2.1.33.1.6.3.7": return "upsAlarmOutputBad";	
                case "1.3.6.1.2.1.33.1.6.3.8": return "upsAlarmOutputOverload";
                case "1.3.6.1.2.1.33.1.6.3.9": return "upsAlarmOnBypass";
                case "1.3.6.1.2.1.33.1.6.3.10": return "upsAlarmBypassBad";	
                case "1.3.6.1.2.1.33.1.6.3.11": return "upsAlarmOutputOffAsRequested";	
                case "1.3.6.1.2.1.33.1.6.3.12": return "upsAlarmUpsOffAsRequested";
                case "1.3.6.1.2.1.33.1.6.3.13": return "upsAlarmChargerFailed";	
                case "1.3.6.1.2.1.33.1.6.3.14": return "upsAlarmUpsOutputOff";
                case "1.3.6.1.2.1.33.1.6.3.15": return "upsAlarmUpsSystemOff";	
                case "1.3.6.1.2.1.33.1.6.3.16": return "upsAlarmFanFailure";	
                case "1.3.6.1.2.1.33.1.6.3.17": return "upsAlarmFuseFailure";	
                case "1.3.6.1.2.1.33.1.6.3.18": return "upsAlarmGeneralFault";
                case "1.3.6.1.2.1.33.1.6.3.19": return "upsAlarmDiagnosticTestFailed";	
                case "1.3.6.1.2.1.33.1.6.3.20": return "upsAlarmCommunicationsLost";
                case "1.3.6.1.2.1.33.1.6.3.21": return "upsAlarmAwaitingPower";	
                case "1.3.6.1.2.1.33.1.6.3.22": return "upsAlarmShutdownPending";	
                case "1.3.6.1.2.1.33.1.6.3.23": return "upsAlarmShutdownImminent";	
                case "1.3.6.1.2.1.33.1.6.3.24": return "upsAlarmTestInProgress";	
            }

            return oid;
        }

        private void OnTimedEvent1(object source, ElapsedEventArgs e)
        {
            if (upsCountNum > 2)
            {
                upsCountNum = 0;
            }
            try
            {

                //for (int i = 0; i < upsAlarmListBox[upsCountNum].Items.Count; i++)
                //{
                //    if (upsAlarmListBox[upsCountNum].Items[i].ToString() == "Time error")
                //    {
                //        upsAlarmListBox[upsCountNum].Items.Remove("Time error");
                //    }
                //}

                switch (upsCountNum)
                {
                    case 0:
                        snmpLoad1();
                        break;
                    case 1:
                        snmpLoad2();
                        break;
                    case 2:
                        snmpLoad3();
                        break;
                }
            }
            catch (Exception err)
            {
                ld.logDate(err.ToString());
            }
            upsCountNum++;
        }
        private void snmpLoad1()
        {
            snmpmgr1.RemoteHost = global::FMS_Manager.Properties.Settings.Default.snmpIP1;

            snmpmgr1.ObjCount = 17;
            snmpmgr1.ObjId[1] = "1.3.6.1.2.1.33.1.4.1.0"; //출력공급원
            snmpmgr1.ObjId[2] = "1.3.6.1.2.1.33.1.2.4.0"; //축전지 충전 레벨
            snmpmgr1.ObjId[3] = "1.3.6.1.2.1.33.1.3.3.1.3.1"; //입력전압a
            snmpmgr1.ObjId[4] = "1.3.6.1.2.1.33.1.3.3.1.3.2"; //입력전압b
            snmpmgr1.ObjId[5] = "1.3.6.1.2.1.33.1.3.3.1.3.3"; //입력전압c
            snmpmgr1.ObjId[6] = "1.3.6.1.2.1.33.1.3.3.1.4.1"; //입력전류a
            snmpmgr1.ObjId[7] = "1.3.6.1.2.1.33.1.3.3.1.4.2"; //입력전류b
            snmpmgr1.ObjId[8] = "1.3.6.1.2.1.33.1.3.3.1.4.3"; //입력전류c
            snmpmgr1.ObjId[9] = "1.3.6.1.2.1.33.1.3.3.1.5.1"; //입력전력
            snmpmgr1.ObjId[10] = "1.3.6.1.2.1.33.1.4.4.1.2.1"; //출력전압a
            snmpmgr1.ObjId[11] = "1.3.6.1.2.1.33.1.4.4.1.2.2"; //출력전압b
            snmpmgr1.ObjId[12] = "1.3.6.1.2.1.33.1.4.4.1.2.3"; //출력전압c
            snmpmgr1.ObjId[13] = "1.3.6.1.2.1.33.1.4.4.1.3.1"; //출력전류a
            snmpmgr1.ObjId[14] = "1.3.6.1.2.1.33.1.4.4.1.3.2"; //출력전류b
            snmpmgr1.ObjId[15] = "1.3.6.1.2.1.33.1.4.4.1.3.3"; //출력전류c
            snmpmgr1.ObjId[16] = "1.3.6.1.2.1.33.1.4.4.1.4.1"; //출력전력
            snmpmgr1.ObjId[17] = "1.3.6.1.2.1.33.1.4.4.1.5.1"; //출력부하량

            try
            {
                snmpmgr1.SendGetRequest();
            }
            catch (Exception ex1)
            {
                string que1 = "update thupst SET vol18 = 'Time error' where ID = '1'";
                ld.update(que1);
                ld.logDate(ex1.ToString());
                return;
            }
            //lvwResults1[0] = snmpmgr1.ObjId[1];
            //lvwResults1[1] = oidToName(snmpmgr1.ObjId[1]);
            //lvwResults1[3] = snmpmgr1.ObjId[2];
            //lvwResults1[4] = oidToName(snmpmgr1.ObjId[2]);
            //lvwResults1[6] = snmpmgr1.ObjId[3];
            //lvwResults1[7] = oidToName(snmpmgr1.ObjId[3]);

            if (snmpmgr1.ErrorStatus == 0)
            {
                lvwResults3[0] = snmpmgr1.ObjValue[1];
                lvwResults3[1] = snmpmgr1.ObjValue[2];
                lvwResults3[2] = snmpmgr1.ObjValue[3];
                lvwResults3[3] = snmpmgr1.ObjValue[4];
                lvwResults3[4] = snmpmgr1.ObjValue[5];
                lvwResults3[5] = (Convert.ToInt32(snmpmgr1.ObjValue[6]) / 10).ToString();
                lvwResults3[6] = (Convert.ToInt32(snmpmgr1.ObjValue[7]) / 10).ToString();
                lvwResults3[7] = (Convert.ToInt32(snmpmgr1.ObjValue[8]) / 10).ToString();
                lvwResults3[8] = (Convert.ToInt32(snmpmgr1.ObjValue[9]) / 1000).ToString();
                lvwResults3[9] = snmpmgr1.ObjValue[10];
                lvwResults3[10] = snmpmgr1.ObjValue[11];
                lvwResults3[11] = snmpmgr1.ObjValue[12];
                lvwResults3[12] = (Convert.ToInt32(snmpmgr1.ObjValue[13]) / 10).ToString();
                lvwResults3[13] = (Convert.ToInt32(snmpmgr1.ObjValue[14]) / 10).ToString();
                lvwResults3[14] = (Convert.ToInt32(snmpmgr1.ObjValue[15]) / 10).ToString();
                lvwResults3[15] = (Convert.ToInt32(snmpmgr1.ObjValue[16]) / 1000).ToString();
                lvwResults3[16] = snmpmgr1.ObjValue[17];
            }
            else
            {
                ld.logDate(snmpmgr1.ErrorDescription + "[" + snmpmgr1.ErrorStatus.ToString() + "]");
            }


            string que11 = "update thupst SET vol1='" + lvwResults3[0] + "', vol2='" + lvwResults3[1] + "', vol3='" + lvwResults3[2] + "', vol4='" + lvwResults3[3] + "', vol5='" + lvwResults3[4] + "', vol6='" + lvwResults3[5] + "', vol7='" + lvwResults3[6] + "', vol8='" + lvwResults3[7] + "', vol9='" + lvwResults3[8] + "', vol10='" + lvwResults3[9] + "', vol11='" + lvwResults3[10] + "', vol12='" + lvwResults3[11] + "', vol13='" + lvwResults3[12] + "', vol14='" + lvwResults3[13] + "', vol15='" + lvwResults3[14] + "', vol16='" + lvwResults3[15] + "', vol17='" + lvwResults3[16] + "', vol18 = '0', logDate = now() where ID = '1'";
            ld.update(que11);


        }

        private void snmpLoad2()
        {
            snmpmgr1.RemoteHost = global::FMS_Manager.Properties.Settings.Default.snmpIP2;

            snmpmgr1.ObjCount = 17;
            snmpmgr1.ObjId[1] = "1.3.6.1.2.1.33.1.4.1.0"; //출력공급원
            snmpmgr1.ObjId[2] = "1.3.6.1.2.1.33.1.2.4.0"; //축전지 충전 레벨
            snmpmgr1.ObjId[3] = "1.3.6.1.2.1.33.1.3.3.1.3.1"; //입력전압a
            snmpmgr1.ObjId[4] = "1.3.6.1.2.1.33.1.3.3.1.3.2"; //입력전압b
            snmpmgr1.ObjId[5] = "1.3.6.1.2.1.33.1.3.3.1.3.3"; //입력전압c
            snmpmgr1.ObjId[6] = "1.3.6.1.2.1.33.1.3.3.1.4.1"; //입력전류a
            snmpmgr1.ObjId[7] = "1.3.6.1.2.1.33.1.3.3.1.4.2"; //입력전류b
            snmpmgr1.ObjId[8] = "1.3.6.1.2.1.33.1.3.3.1.4.3"; //입력전류c
            snmpmgr1.ObjId[9] = "1.3.6.1.2.1.33.1.3.3.1.5.1"; //입력전력
            snmpmgr1.ObjId[10] = "1.3.6.1.2.1.33.1.4.4.1.2.1"; //출력전압a
            snmpmgr1.ObjId[11] = "1.3.6.1.2.1.33.1.4.4.1.2.2"; //출력전압b
            snmpmgr1.ObjId[12] = "1.3.6.1.2.1.33.1.4.4.1.2.3"; //출력전압c
            snmpmgr1.ObjId[13] = "1.3.6.1.2.1.33.1.4.4.1.3.1"; //출력전류a
            snmpmgr1.ObjId[14] = "1.3.6.1.2.1.33.1.4.4.1.3.2"; //출력전류b
            snmpmgr1.ObjId[15] = "1.3.6.1.2.1.33.1.4.4.1.3.3"; //출력전류c
            snmpmgr1.ObjId[16] = "1.3.6.1.2.1.33.1.4.4.1.4.1"; //출력전력
            snmpmgr1.ObjId[17] = "1.3.6.1.2.1.33.1.4.4.1.5.1"; //출력부하량

            try
            {
                snmpmgr1.SendGetRequest();
            }
            catch (Exception ex1)
            {
                string que1 = "update thupst SET vol18 = 'Time error' where ID = '2'";
                ld.update(que1);
                ld.logDate(ex1.ToString());
                return;
            }
            //lvwResults1[0] = snmpmgr1.ObjId[1];
            //lvwResults1[1] = oidToName(snmpmgr1.ObjId[1]);
            //lvwResults1[3] = snmpmgr1.ObjId[2];
            //lvwResults1[4] = oidToName(snmpmgr1.ObjId[2]);
            //lvwResults1[6] = snmpmgr1.ObjId[3];
            //lvwResults1[7] = oidToName(snmpmgr1.ObjId[3]);

            if (snmpmgr1.ErrorStatus == 0)
            {
                lvwResults3[0] = snmpmgr1.ObjValue[1];
                lvwResults3[1] = snmpmgr1.ObjValue[2];
                lvwResults3[2] = snmpmgr1.ObjValue[3];
                lvwResults3[3] = snmpmgr1.ObjValue[4];
                lvwResults3[4] = snmpmgr1.ObjValue[5];
                lvwResults3[5] = (Convert.ToInt32(snmpmgr1.ObjValue[6]) / 10).ToString();
                lvwResults3[6] = (Convert.ToInt32(snmpmgr1.ObjValue[7]) / 10).ToString();
                lvwResults3[7] = (Convert.ToInt32(snmpmgr1.ObjValue[8]) / 10).ToString();
                lvwResults3[8] = (Convert.ToInt32(snmpmgr1.ObjValue[9]) / 1000).ToString();
                lvwResults3[9] = snmpmgr1.ObjValue[10];
                lvwResults3[10] = snmpmgr1.ObjValue[11];
                lvwResults3[11] = snmpmgr1.ObjValue[12];
                lvwResults3[12] = (Convert.ToInt32(snmpmgr1.ObjValue[13]) / 10).ToString();
                lvwResults3[13] = (Convert.ToInt32(snmpmgr1.ObjValue[14]) / 10).ToString();
                lvwResults3[14] = (Convert.ToInt32(snmpmgr1.ObjValue[15]) / 10).ToString();
                lvwResults3[15] = (Convert.ToInt32(snmpmgr1.ObjValue[16]) / 1000).ToString();
                lvwResults3[16] = snmpmgr1.ObjValue[17];
            }
            else
            {
                ld.logDate(snmpmgr1.ErrorDescription + "[" + snmpmgr1.ErrorStatus.ToString() + "]");
            }


            string que12 = "update thupst SET vol1='" + lvwResults3[0] + "', vol2='" + lvwResults3[1] + "', vol3='" + lvwResults3[2] + "', vol4='" + lvwResults3[3] + "', vol5='" + lvwResults3[4] + "', vol6='" + lvwResults3[5] + "', vol7='" + lvwResults3[6] + "', vol8='" + lvwResults3[7] + "', vol9='" + lvwResults3[8] + "', vol10='" + lvwResults3[9] + "', vol11='" + lvwResults3[10] + "', vol12='" + lvwResults3[11] + "', vol13='" + lvwResults3[12] + "', vol14='" + lvwResults3[13] + "', vol15='" + lvwResults3[14] + "', vol16='" + lvwResults3[15] + "', vol17='" + lvwResults3[16] + "', vol18 = '0', logDate = now() where ID = '2'";
            ld.update(que12);


        }

        private void snmpLoad3()
        {
            snmpmgr1.RemoteHost = global::FMS_Manager.Properties.Settings.Default.snmpIP3;

            snmpmgr1.ObjCount = 17;
            snmpmgr1.ObjId[1] = "1.3.6.1.2.1.33.1.4.1.0"; //출력공급원
            snmpmgr1.ObjId[2] = "1.3.6.1.2.1.33.1.2.4.0"; //축전지 충전 레벨
            snmpmgr1.ObjId[3] = "1.3.6.1.2.1.33.1.3.3.1.3.1"; //입력전압a
            snmpmgr1.ObjId[4] = "1.3.6.1.2.1.33.1.3.3.1.3.2"; //입력전압b
            snmpmgr1.ObjId[5] = "1.3.6.1.2.1.33.1.3.3.1.3.3"; //입력전압c
            snmpmgr1.ObjId[6] = "1.3.6.1.2.1.33.1.3.3.1.4.1"; //입력전류a
            snmpmgr1.ObjId[7] = "1.3.6.1.2.1.33.1.3.3.1.4.2"; //입력전류b
            snmpmgr1.ObjId[8] = "1.3.6.1.2.1.33.1.3.3.1.4.3"; //입력전류c
            snmpmgr1.ObjId[9] = "1.3.6.1.2.1.33.1.3.3.1.5.1"; //입력전력
            snmpmgr1.ObjId[10] = "1.3.6.1.2.1.33.1.4.4.1.2.1"; //출력전압a
            snmpmgr1.ObjId[11] = "1.3.6.1.2.1.33.1.4.4.1.2.2"; //출력전압b
            snmpmgr1.ObjId[12] = "1.3.6.1.2.1.33.1.4.4.1.2.3"; //출력전압c
            snmpmgr1.ObjId[13] = "1.3.6.1.2.1.33.1.4.4.1.3.1"; //출력전류a
            snmpmgr1.ObjId[14] = "1.3.6.1.2.1.33.1.4.4.1.3.2"; //출력전류b
            snmpmgr1.ObjId[15] = "1.3.6.1.2.1.33.1.4.4.1.3.3"; //출력전류c
            snmpmgr1.ObjId[16] = "1.3.6.1.2.1.33.1.4.4.1.4.1"; //출력전력
            snmpmgr1.ObjId[17] = "1.3.6.1.2.1.33.1.4.4.1.5.1"; //출력부하량

            try
            {
                snmpmgr1.SendGetRequest();
            }
            catch (Exception ex1)
            {
                string que1 = "update thupst SET vol18 = 'Time error' where ID = '3'";
                ld.update(que1);
                ld.logDate(ex1.ToString());
                return;
            }
            //lvwResults1[0] = snmpmgr1.ObjId[1];
            //lvwResults1[1] = oidToName(snmpmgr1.ObjId[1]);
            //lvwResults1[3] = snmpmgr1.ObjId[2];
            //lvwResults1[4] = oidToName(snmpmgr1.ObjId[2]);
            //lvwResults1[6] = snmpmgr1.ObjId[3];
            //lvwResults1[7] = oidToName(snmpmgr1.ObjId[3]);

            if (snmpmgr1.ErrorStatus == 0)
            {
                lvwResults3[0] = snmpmgr1.ObjValue[1];
                lvwResults3[1] = snmpmgr1.ObjValue[2];
                lvwResults3[2] = snmpmgr1.ObjValue[3];
                lvwResults3[3] = snmpmgr1.ObjValue[4];
                lvwResults3[4] = snmpmgr1.ObjValue[5];
                lvwResults3[5] = (Convert.ToInt32(snmpmgr1.ObjValue[6]) / 10).ToString();
                lvwResults3[6] = (Convert.ToInt32(snmpmgr1.ObjValue[7]) / 10).ToString();
                lvwResults3[7] = (Convert.ToInt32(snmpmgr1.ObjValue[8]) / 10).ToString();
                lvwResults3[8] = (Convert.ToInt32(snmpmgr1.ObjValue[9]) / 1000).ToString();
                lvwResults3[9] = snmpmgr1.ObjValue[10];
                lvwResults3[10] = snmpmgr1.ObjValue[11];
                lvwResults3[11] = snmpmgr1.ObjValue[12];
                lvwResults3[12] = (Convert.ToInt32(snmpmgr1.ObjValue[13]) / 10).ToString();
                lvwResults3[13] = (Convert.ToInt32(snmpmgr1.ObjValue[14]) / 10).ToString();
                lvwResults3[14] = (Convert.ToInt32(snmpmgr1.ObjValue[15]) / 10).ToString();
                lvwResults3[15] = (Convert.ToInt32(snmpmgr1.ObjValue[16]) / 1000).ToString();
                lvwResults3[16] = snmpmgr1.ObjValue[17];
            }
            else
            {
                ld.logDate(snmpmgr1.ErrorDescription + "[" + snmpmgr1.ErrorStatus.ToString() + "]");
            }


            string que13 = "update thupst SET vol1='" + lvwResults3[0] + "', vol2='" + lvwResults3[1] + "', vol3='" + lvwResults3[2] + "', vol4='" + lvwResults3[3] + "', vol5='" + lvwResults3[4] + "', vol6='" + lvwResults3[5] + "', vol7='" + lvwResults3[6] + "', vol8='" + lvwResults3[7] + "', vol9='" + lvwResults3[8] + "', vol10='" + lvwResults3[9] + "', vol11='" + lvwResults3[10] + "', vol12='" + lvwResults3[11] + "', vol13='" + lvwResults3[12] + "', vol14='" + lvwResults3[13] + "', vol15='" + lvwResults3[14] + "', vol16='" + lvwResults3[15] + "', vol17='" + lvwResults3[16] + "', vol18 = '0', logDate = now() where ID = '3'";
            ld.update(que13);

        }
        

        internal string snmptrapMsg1;
        internal string snmptrapMsg2;

        int num = 0;
        private void snmpmgr1_OnTrap(object sender, SnmpmgrTrapEventArgs e)
        {
            snmptrapMsg1 = e.SourceAddress;
            snmptrapMsg2 = oidToName(e.TrapOID);

            Console.WriteLine(DateTime.Now + "\t" + "Trap");
            Console.WriteLine(DateTime.Now + "\t" + snmptrapMsg1 + "\t" + snmptrapMsg2);


            if (snmptrapMsg1 == global::FMS_Manager.Properties.Settings.Default.snmpIP1)
            {
                string que1 = "update thupst SET vol18 ='" + snmptrapMsg2 + "' where ID = '1'";
                ld.update(que1);
                ld.logDate("UPS1 장비 통신오류" + snmptrapMsg2 + "");
                num = 46;
                //upsAlarmListBox[0].Items.Add(snmptrapMsg2);
                //upsAlarmListBox[0].SelectedIndex = upsAlarmListBox[0].Items.Count - 1;
            }
            else if (snmptrapMsg1 == global::FMS_Manager.Properties.Settings.Default.snmpIP2)
            {
                string que1 = "update thupst SET vol18 ='" + snmptrapMsg2 + "' where ID = '2'";
                ld.update(que1);
                ld.logDate("UPS2 장비 통신오류" + snmptrapMsg2 + "");
                num = 47;
                //upsAlarmListBox[1].Items.Add(snmptrapMsg2);
                //upsAlarmListBox[1].SelectedIndex = upsAlarmListBox[1].Items.Count - 1;
            }
            else if (snmptrapMsg1 == global::FMS_Manager.Properties.Settings.Default.snmpIP3)
            {
                string que1 = "update thupst SET vol18 ='" + snmptrapMsg2 + "' where ID = '3'";
                ld.update(que1);
                ld.logDate("UPS3 장비 통신오류" + snmptrapMsg2 + "");
                num = 48;
                //upsAlarmListBox[2].Items.Add(snmptrapMsg2);
                //upsAlarmListBox[2].SelectedIndex = upsAlarmListBox[2].Items.Count - 1;
            }
            cmonitering mo = new cmonitering();
            mo.LoadMoniteringDB();
            string dt1 = ld.timeText1;
            string dt2 = ld.timeText2;
            string que2 = "Insert into AlarmLogT" + "(logDate, logTime, alarmLevel, sysCode, sysCodeNum,alarmMsg,recoverState)" +
                               "values('" + dt1 + "','" + dt2 + "', 'F', '" + mo.monitering[num, 1] +
                               "','" + mo.monitering[num, 17] +
                               "','" + snmptrapMsg2 +
                               "','C')";
            ld.update(que2);
        }

        internal int broadcastId = 0;
        private void snmpmgr1_OnResponse(object sender, SnmpmgrResponseEventArgs e)
        {
            Console.WriteLine(DateTime.Now + "\t" + "Response");

            //this handles the "Find" broadcasts
            if (e.RequestId == broadcastId && snmpmgr1.ObjCount >= 1 && snmpmgr1.ObjId[1] == "1.3.6.1.2.1.1.5.0")
            {//sysName
                Console.WriteLine(e.SourceAddress);
                Console.WriteLine(snmpmgr1.ObjValue[1]);
            }
        }

        private void snmpmgr1_OnPacketTrace(object sender, SnmpmgrPacketTraceEventArgs e)
        {
            switch (e.Direction)
            {
                case 1: Console.WriteLine("Packet received from: " + e.PacketAddress + ":" + e.PacketPort); break;
                case 2: Console.WriteLine("Packet sent to: " + e.PacketAddress + ":" + e.PacketPort); break;
            }
        }

        private void snmpmgr1_OnError(object sender, SnmpmgrErrorEventArgs e)
        {
            ld.logDate("Error: " + e.Description);
        }
    }
}
