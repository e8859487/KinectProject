using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using XmlManager;

using log4net;
using log4net.Config;

using System.Activities;
using System.Threading;
using MotionFSM;

using Microsoft.Activities.Extensions.Tracking;
namespace ConsoleTest
{
    using MyCollections;
    using System.Diagnostics;
    using MathNet.Numerics.LinearAlgebra;
    using System.Globalization;
    using DatabaseService;
    using SocketPackage;

    class Program 
    {
        class EventListener
        {
            private ListWithChangeEvent List;

            public EventListener(ListWithChangeEvent list)
            {
                List = list;
                List.Changed += new ChangeEventHandler(ListChanged);
            }

            private void ListChanged(object sender, EventArgs e)
            {
                Console.WriteLine("This is called when the event fires.");
            }

            public void Detach()
            {
                List.Changed -= new ChangeEventHandler(ListChanged);
                List = null;
            }
        }
 
         class aaa
       {
           public int i = 0;
           public aaa(int input)
           {
               i = input;
           }
       }

         private static FusionTableServices fusionTableServices; 


        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss",
                                CultureInfo.InvariantCulture));
            const string USERMOTIONSID = "1VTk9u3ZyUCgvEbbt8gbFyHagrTfdYpcjfs7INwOl";
            const string USERMOTIONSID1 = "16gJUCOJnU-9OTSxAX6QQ0wFd3lFUzA3MF909cDAo";
            const string USERMOTIONSID2 = "1gDJnT4RCKXzFBHu-S60FSF_cCd_tSaa_sdPtN5FC";
            const string USERMOTIONSID3 = "1jK__pXEYjimr0aDQ9fzr_LsLaqQftk6EqjiTexzW";

            fusionTableServices = new FusionTableServices();
            Console.WriteLine(Enum.GetName(typeof(DEVICE_ID), 1));
           // fusionTableServices.Delete(USERMOTIONSID2, "");

            //List<int> FirstArray = new List<int>();
            //Random r = new Random();
            //r.Next(0, 100);

            //for (int i = 0; i < 10; i++)
            //{
            //    FirstArray.Add(r.Next(0,100));
            //}
            //foreach (int val in FirstArray)
            //{
            //    Console.WriteLine(val);
            //}

                //MotionsAnalyze motionAnalyzer = new MotionsAnalyze();
                //Thread.Sleep(1000);
                //motionAnalyzer.ResumeBookmark(MotionTransitions.E_Walk);
                //Thread.Sleep(1000);
                //motionAnalyzer.ResumeBookmark(MotionTransitions.E_Stop);


                #region matrix operate

                //float[,] x = { { 2, 2, 5 }, { -2, 1, 2 }, { 6, 3, 9 } };

                //var M = Matrix<float>.Build;
                //Matrix<float> matrix = M.DenseOfArray(x);
                //float a = matrix[0, 1];
                //Console.WriteLine(matrix.Inverse());


                #endregion
            int b = 2;
            int c;
            int a = (b == 1) ? c = 2 : c = 3;
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);





                #region test object shadowcopy
                /*
            aaa[] a3 = new aaa[3];
            aaa[] b3 = new aaa[3];
            for (int i = 0; i < a3.Length; i++)
            {
                a3[i] = new aaa(1);
                b3[i] = new aaa(2);
            }

            Array.Copy(a3, b3, 3);

            Console.WriteLine("b3" + b3[1].i);
            a3[1].i = 10;
            Console.WriteLine("b3" + b3[1].i);
            b3[1].i = 15;
            Console.WriteLine("b3" + b3[1].i);
            Console.WriteLine("a3" + a3[1].i); 
 */
                #endregion

                #region WWF test 2
                /*
            
            Program pg = new Program();

            WorkflowInstance wfInstance = new WorkflowInstance(pg);
            wfInstance.Run();

            statemachineStateTracker = wfInstance.StateTracker;
            string eventName = string.Empty;

            string str;
            string a;

            while ((a = Console.ReadLine()) != null)
            {
                switch (a.ToLower())
                {
                    case "walk":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_Walk);//
                        break;

                    case "falldown":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_FallDown);//
                        break;

                    case "standup":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_StandUp);//
                        break;

                    case "getup":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_GetUp);//
                        break;

                    case "stop":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_Stop);//
                        break;

                    case "sitdown":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_SitDown);//
                        break;

                    case "liedown":
                        eventName = Enum.GetName(typeof(MotionTransitions), MotionTransitions.E_LieDown);//
                        break;

                    case "print":
                        printOutEvent();

                        break;

                    case "globelState":
                        Console.WriteLine(statemachineStateTracker.CurrentState);
                        break;

                }

                if (IsEventExist(statemachineStateTracker, eventName))
                {
                    wfInstance.ResumeBookmark(eventName);
                }
                else
                {
                    wfInstance.ResumeBookmark("E_Unknow");
                }

            } 
            */
                #endregion

                #region test wf print state

                /*

            Program pg = new Program();
            pg.startFSMRuntime();
            System.Console.WriteLine(Program.StateTracker.CurrentStateMachine);

            string a;
            while ((a = Console.ReadLine()) != null)
            {
                switch (a)
                {
                    case "n":
                        pg.wfApp.ResumeBookmark("E_Walk", string.Empty);
                        Program.StateTracker.Trace();
                        System.Console.WriteLine(Program.StateTracker.CurrentState);
                        break;
                    case "p":
                        pg.wfApp.ResumeBookmark("E_su_gu_sd_l", string.Empty);
                        Program.StateTracker.Trace();
                        System.Console.WriteLine(Program.StateTracker.CurrentState);
                        
                        break;
                    case "o":
                        System.Console.WriteLine(Program.StateTracker.CurrentState);
                        break;
                }
                
            }
            */

                #endregion

                #region test Enum
                /*

            foreach (string str in Enum.GetNames(typeof(JointType)))
            {
                Console.WriteLine(str);
            }
            */
                #endregion

                #region test callback

                /*
            ListWithChangeEvent list = new ListWithChangeEvent();

            EventListener listener = new EventListener(list);
            list.Add("item 1");
            list.Clear();
            listener.Detach();
            */

                #endregion

                #region test log4net

                /*
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));

            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Debug("test debug");
            log.Info("test Info");
            log.Fatal("test Fatal");
            log.Warn("test Warn");
            log.Error("test Error");
            Console.Write("HI BITCH");
            */

                #endregion

                #region testString length socket transmit

                /*
            string tempStr = "(-0.1302,-0.2052,1.9159)|(-0.1638,0.1214,1.8859)|(-0.1942,0.4356,1.8423)|(-0.2429,0.5843,1.8060)|(-0.3477,0.2696,1.8958)|(-0.3577,-0.0219,1.9069)|(-0.3491,-0.2514,1.8331)|(-0.3399,-0.2885,1.8102)|(-0.0189,0.3171,1.8227)|(0.0899,0.0664,1.8554)|(0.0757,-0.1619,1.7762)|(0.0660,-0.2193,1.7650)|(-0.2024,-0.2120,1.8985)|(-0.2116,-0.5718,1.8008)|(-0.2421,-0.8602,1.7249)|(-0.2541,-0.8841,1.6147)|(-0.0527,-0.1901,1.8574)|(0.0076,-0.5683,1.9041)|(0.0170,-0.9306,2.0352)|(0.0341,-0.9929,1.9632)|(-0.1871,0.3588,1.8555)|(-0.3386,-0.3539,1.7809)|(-0.3672,-0.3091,1.7729)|(0.0460,-0.2963,1.7455)|(0.0787,-0.2310,1.7213)|(-0.1302,-0.2052,1.9159)|(-0.1638,0.1214,1.8859)|(-0.1942,0.4356,1.8423)|(-0.2429,0.5843,1.8060)|(-0.3477,0.2696,1.8958)|(-0.3577,-0.0219,1.9069)|(-0.3491,-0.2514,1.8331)|(-0.3399,-0.2885,1.8102)|(-0.0189,0.3171,1.8227)|(0.0899,0.0664,1.8554)|(0.0757,-0.1619,1.7762)|(0.0660,-0.2193,1.7650)|(-0.2024,-0.2120,1.8985)|(-0.2116,-0.5718,1.8008)|(-0.2421,-0.8602,1.7249)|(-0.2541,-0.8841,1.6147)|(-0.0527,-0.1901,1.8574)|(0.0076,-0.5683,1.9041)|(0.0170,-0.9306,2.0352)|(0.0341,-0.9929,1.9632)|(-0.1871,0.3588,1.8555)|(-0.3386,-0.3539,1.7809)|(-0.3672,-0.3091,1.7729)|(0.0460,-0.2963,1.7455)|(0.0787,-0.2310,1.7213)|";
            
            byte[] tempbyte = System.Text.Encoding.UTF8.GetBytes(tempStr);
            Console.WriteLine(tempbyte.Length);
            foreach(byte b in tempbyte)
            Console.Write(b);

            */

                #endregion

                #region 練習讀取XML讀檔

                /*
             * 
             * 
            XmlReader xmlReader = new XmlReader(@".\Setting.xml");

            Console.WriteLine(xmlReader.getNodeInnerText(@"/Root/IPAddress"));
             * 
             * 
            */

                #endregion

                #region 練習Client端網路連接
                /*
            //方法二
            try
            {
                TcpClient tcpClient = new TcpClient();
                IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                IPEndPoint LocalEP = new IPEndPoint(ipAddress, 82);
                tcpClient.Connect(LocalEP);
            }
            catch (SocketException ee)
            {

            }

            //方法一
            try
            {
                //建立socket物件
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ServerIP = Dns.GetHostEntry("fang").AddressList[0];
                IPEndPoint serverhost = new IPEndPoint(ServerIP, 82);
                //clientSocket.Connect(serverhost);
                Console.WriteLine("IP: " + ServerIP.ToString());
                //啟動非同步對伺服端主機連線要求
                clientSocket.BeginConnect(serverhost, new AsyncCallback(Async_Send_Receive.Connect_Callback), clientSocket);
                //完成非同步對伺服端主機連接的要求
            }
            catch (SocketException ee)
            {
                Console.WriteLine("IP: " + ee.ToString());

            }
            Console.ReadLine(); 
             * */

                #endregion

                #region 練習double to string format

                /*
            float ff = 0.123456789f;
            Console.Write(ff.ToString("0.0000"));

            Console.Read();
            */

                #endregion

                #region 練習memroystream
                /*
            MemoryStream ms = new MemoryStream();
            string str1 = "First String";
            string str2 = "Second String";
            byte[] strBuffer = Encoding.UTF8.GetBytes(str1);


            ms.Write(strBuffer, 0, strBuffer.Length);

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());

            ms.Write(strBuffer, 0, strBuffer.Length);


            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());


            int count = (int)ms.Length;
            byte[] outBuffer = new byte[count];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(outBuffer, 0, count);

            string outputStr = Encoding.UTF8.GetString(outBuffer);

            Console.WriteLine(outputStr);
            Console.ReadLine();

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());


            count = (int)ms.Length;
            outBuffer = new byte[count];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(outBuffer, 0, count);



            ms.Seek(0, SeekOrigin.Begin);

            str2 = "testtesttesttest";
            strBuffer = Encoding.UTF8.GetBytes(str2);

            ms.Write(strBuffer, 0, strBuffer.Length);







            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(outBuffer, 0, count);

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());

            outputStr = Encoding.UTF8.GetString(outBuffer);

            Console.WriteLine(outputStr);
            Console.ReadLine();
             */
                #endregion

                Console.ReadLine();

        }


    }
}
