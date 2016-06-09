using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading;

namespace FMS_Manager
{
    class Program
    {
        public static Load ld = new Load();
        public static Water ww = new Water();
        public static Listener ls = new Listener();
        static void Main(string[] args)
        {
            bool bNewProcess;
            Mutex mtex = new Mutex(true, "MU", out bNewProcess);
            if (bNewProcess)
            {
                mtex.ReleaseMutex();


                //Listener lt = new Listener();
                ls.CreateListener();

                Console.ReadLine();

                //// Keep the timer alive until the end of Main.
                //GC.KeepAlive(t1);
            }
            else
            {
                Console.WriteLine("프로그램이 이미 실행중입니다");
            }

        }
    }
}
