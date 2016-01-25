using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.Runtime.Serialization.Formatters.Binary;

using Img_Serializable;
using System.IO;

using log4net;
using log4net.Config;


namespace SocketPackage
{
    public delegate void clientDataChangeEventHandler(object sender, StatusEventArgs e);

    public class SocketClient
    {
        #region private property

        /// 遠端 socket server IP 位址
        private string _RemoteIpAddress;
    
       /// 遠端 socket server 所監聽的 port number
        private int _RemotePortNumber;

        /// socket client 物件(連接遠端 socket server 用)
        private TcpClient _TcpClient;

        //Log
        private ILog log = null;

        #endregion

        #region public static property
       /// <summary>
       /// Indicate server socket status
       /// </summary>
        public static Status socketStatus = new Status();
       
        #endregion

        #region constructor
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="inRemoteIpAddr">遠端 socket server IP 位址</param>
        /// <param name="inRemotePortNum">遠端 socket server 所監聽的 port number</param>
        public SocketClient(string inRemoteIpAddr, int inRemotePortNum)
        {
            this._RemoteIpAddress = inRemoteIpAddr;
            this._RemotePortNumber = inRemotePortNum;

            //設定Log
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            
            //
            socketStatus.changed += new changedEventHandler(socketStatusChange);
        }

        #endregion

        #region public method

        /// 連線至 socket server
        public void Connect()
        {
            //初始化 socket client
            _TcpClient = new TcpClient();
            _TcpClient.Connect(_RemoteIpAddress, _RemotePortNumber);
            log.Info("Client Socket Program - Server Connected ...");
        }

        //與伺服器終止連線
        public void DisCounect()
        {
            if (_TcpClient.Connected)
            {
                _TcpClient.Close();
                _TcpClient = null;
                log.Info("Client Socket Program - Server DisConnected");
            }
        }

        /// <summary>
        /// Send String
        /// </summary>
        /// <param name="inMessage"></param>
        public void Send(string inMessage)
        {
            try
            {
                //取得用來傳送訊息至 socket server 的 stream 物件
                NetworkStream serverStream = _TcpClient.GetStream();

                BinaryWriter binaryWriter = new BinaryWriter(serverStream);
                binaryWriter.Write(inMessage);
                binaryWriter.Flush();
            }
            catch (Exception ee)
            {
                log.Error("Send String Error. ", ee);
            }
        }

        /// <summary>
        /// Send int
        /// </summary>
        /// <param name="inMessage"></param>
        public void Send(int inMessage)
        {
            try
            {
                //取得用來傳送訊息至 socket server 的 stream 物件
                NetworkStream serverStream = _TcpClient.GetStream();

                BinaryWriter binaryWriter = new BinaryWriter(serverStream);
                binaryWriter.Write(inMessage);
                binaryWriter.Flush();
            }
            catch (Exception ee)
            {
                log.Error("Send String Error. ", ee);
            }
        }


        public async void SendBytes(byte[] bytes)
        {
          //  try
            {
                //取得用來傳送訊息至 socket server 的 stream 物件
                NetworkStream serverStream = _TcpClient.GetStream();

                //將資料寫入 stream object (表示傳送資料至 socket server)
                await serverStream.WriteAsync(bytes, 0, bytes.Length);

                {
                    byte[] status_byte = new byte[1];
                    //int status = serverStream.ReadByte();
                    await serverStream.ReadAsync(status_byte, 0, 1);

                    int status = int.Parse(System.Text.Encoding.UTF8.GetString(status_byte));

                    SocketClient.socketStatus.SocketStatus = (SocketPackage.TRANSMIT_STATUS)Enum.ToObject(typeof(SocketPackage.TRANSMIT_STATUS), status);
 
                }
                
            }
           // catch (Exception ee)
            {

               // log.Error("SendImg Error", ee);
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
            }
            catch (Exception ee)
            {
                log.Error("Send String Error. ", ee);
            }
        }

        public void Send_oldVersion(string inMessage)
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
            catch (Exception ee)
            {
                log.Info("Send_oldVersion Error", ee);
            }
        }
        #endregion

        #region CallBack Event and Event handler

        /// <summary>
        /// Active when the static variable [status] changed. check the Socket status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void socketStatusChange(object sender, EventArgs e)
        {

            if (socketStatus.SocketStatus == SocketPackage.TRANSMIT_STATUS.StartRecord || socketStatus.SocketStatus == SocketPackage.TRANSMIT_STATUS.StopRecord ||
                socketStatus.SocketStatus == SocketPackage.TRANSMIT_STATUS.StartPlaybackClip
                )
            {
                //指定封包內容
                this.OnDataChanged(new StatusEventArgs(socketStatus.SocketStatus));
            }
        }

        //Event Handler
        public event clientDataChangeEventHandler clientDataChanged;

        protected virtual void OnDataChanged(StatusEventArgs e)
        {
            if (clientDataChanged != null)
            {
                clientDataChanged(this, e);
            }
        }
        #endregion
    }
}
