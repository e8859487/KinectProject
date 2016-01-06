using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.Runtime.Serialization.Formatters.Binary;

using Img_Serializable;
namespace SocketPackage
{
    public class SocketClient
    {
        #region private property

        /// 遠端 socket server IP 位址
        private string _RemoteIpAddress;
    
       /// 遠端 socket server 所監聽的 port number
        private int _RemotePortNumber;

        /// socket client 物件(連接遠端 socket server 用)
        private TcpClient _TcpClient;
        #endregion

        #region public static property

        public static List<string> sMessages = new List<string>();

        #endregion

        #region constructor

        /// constructor
        /// 遠端 socket server IP 位址
        /// 遠端 socket server 所監聽的 port number
        public SocketClient(string inRemoteIpAddr, int inRemotePortNum)
        {
            this._RemoteIpAddress = inRemoteIpAddr;
            this._RemotePortNumber = inRemotePortNum;
        }

        #endregion

        #region public method
        /// 連線至 socket server
        public void Connect()
        {
            //初始化 socket client
            _TcpClient = new TcpClient();
            _TcpClient.Connect(_RemoteIpAddress, _RemotePortNumber);
            sMessages.Add("Client Socket Program - Server Connected ...");
        }

        //與伺服器終止連線
        public void DisCounect()
        {
            if (_TcpClient.Connected)
            {
                _TcpClient.Close();
                _TcpClient = null;
                sMessages.Add("Client Socket Program - Server DisConnected");
            }

        }

        public void Send(string inMessage)
        {
            try
            {
                //取得用來傳送訊息至 socket server 的 stream 物件
                NetworkStream serverStream = _TcpClient.GetStream();

                //將資料轉為 byte[]
                byte[] outStream = System.Text.Encoding.UTF8.GetBytes(inMessage);

                //將資料寫入 stream object (表示傳送資料至 socket server)
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

            }
            catch(Exception ee) 
            {
                sMessages.Add(ee.ToString());
            }
        }

        public void SendImg(byte[] bytes)
        {
            try
            {
                //取得用來傳送訊息至 socket server 的 stream 物件
                NetworkStream serverStream = _TcpClient.GetStream();

                //將資料寫入 stream object (表示傳送資料至 socket server)
                serverStream.Write(bytes, 0, bytes.Length);
                serverStream.Flush();

            }
            catch (Exception ee)
            {
                sMessages.Add(ee.ToString());
            }

        }
        /// <summary>
        /// 傳送序列化的物件
        /// </summary>
        /// <param name="imgObj"></param>
        public void SendImgInfo(ImageInfo_Serializable imgObj)
        {
            try
            {
                //取得用來傳送訊息至 socket server 的 stream 物件
                NetworkStream serverStream = _TcpClient.GetStream();

                //將資料序列化
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(serverStream, imgObj);

                //將資料寫入 stream object (表示傳送資料至 socket server)
             //   serverStream.Flush();
            }
            catch (Exception ee)
            {
                sMessages.Add(ee.ToString());
            }
        }
        #endregion

    }
}
