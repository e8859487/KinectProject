using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using Img_Serializable;
using System.IO;

using log4net;
using log4net.Config;

namespace SocketPackage
{
    public class ClientRequestHandler
    {
        static public ImageInfo_Serializable imgObj = new ImageInfo_Serializable(217088);
        #region private property

        /// socket client 識別號碼
        private int _ClientNo;

        /// socket client reuqest
        private TcpClient _TcpClient;

        //Log Declare
        private ILog log = null;
        static public byte[] depthByte = new byte[217088];

        static public byte[] DepthByte
        {
            get {
                return depthByte;
            }
            set{
                if (value != depthByte)
                {
                    depthByte = value;
                }
            }
        }
        #endregion

        #region constructor

        /// constructor
        //socket client 識別號碼
        //socket client reuqest
        public ClientRequestHandler(int inClientNo, TcpClient inTcpClient)
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Info(string.Format("ClientNo:{0} , Create successfully!!", inClientNo));
            this._ClientNo = inClientNo;
            this._TcpClient = inTcpClient;
        }

        #endregion

        #region public method


        public void DoCommunicate()
        {
            //產生 BackgroundWorker 負責處理每一個 socket client 的 reuqest
            BackgroundWorker bgwSocket = new BackgroundWorker();
            bgwSocket.DoWork += new DoWorkEventHandler(bgwSocket_DoWork);
            bgwSocket.RunWorkerAsync();
        }


        private void bgwSocket_DoWork(object sender, DoWorkEventArgs e)
        {
            NetworkStream netStream = null;
            BinaryReader binaryReader = null;
            BinaryFormatter binaryFromatter = new BinaryFormatter();
            //server & client 已經連線完成
            while (_TcpClient.Connected)
            {
                //取得網路串流物件，取得來自 socket client 的訊息
                netStream = _TcpClient.GetStream();
                // byte[] readBuffer = new byte[_TcpClient.ReceiveBufferSize];
                //int count = 0;
                //   if ((count = netStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                if (_TcpClient.ReceiveBufferSize > 0 && netStream.CanRead)
                {
                  //  try
                   {

                       //imgObj = (ImageInfo_Serializable)binaryFromatter.Deserialize(netStream);
                       
                        binaryReader = new BinaryReader(netStream);
                        DepthByte = binaryReader.ReadBytes(217088 );
                        imgObj._ImgBuffer = DepthByte;

                       string a ;
                        if ( (a =System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(1))) != "0")
                        {
                           // log.Error(a);
                       }
                         int bodyNumbers = int.Parse(a);
                         


                         for (int i = 0; i < bodyNumbers; i++)
                         {
                             int strLength = int.Parse(System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(3)));
                             imgObj._SBodyJoints[i] = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(strLength));
                             imgObj._IsTracked[i] = true;
                         }
                         //待修改
                         for (int i = bodyNumbers; i < 7; i++)
                         {
                             imgObj._IsTracked[i] = false;
                         }
                        
                    }
                  //  catch (Exception ee)
                  //  {
                 //       SocketServer.sMessages.Add(ee.ToString());
                   // }
                }
            }

        }



        #endregion
    }
}
