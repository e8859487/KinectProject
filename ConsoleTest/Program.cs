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

namespace ConsoleTest
{
    using MyCollections;

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
        

        static void Main(string[] args)
        {


            ListWithChangeEvent list = new ListWithChangeEvent();

            EventListener listener = new EventListener(list);
            list.Add("item 1");
            list.Clear();
            listener.Detach();



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
