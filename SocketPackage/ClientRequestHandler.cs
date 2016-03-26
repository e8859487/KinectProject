using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using Img_Serializable;
using System.IO;

using log4net;
using log4net.Config;
using System.Diagnostics;

namespace SocketPackage
{
    public class ClientRequestHandler
    {
          public  const int IMG_SIZE = 0;
          public  const int BODY_NUMBER_SIZE = 1;
          public  const int BODY_SKELETION_STR_SIZE = 3;
 
        #region private property

        /// socket client 識別號碼
        private int _ClientNo;

        /// socket client reuqest
        private TcpClient _TcpClient;

        //Log Declare
        private ILog log = null;
 
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
            //server & client 已經連線完成


            while (_TcpClient.Connected)
            {

                //取得網路串流物件，取得來自 socket client 的訊息
                netStream = _TcpClient.GetStream();
                if (_TcpClient.ReceiveBufferSize > 0 && netStream.CanRead)
                {
                   // try
                    {

                        binaryReader = new BinaryReader(netStream);
                        //使用2D版本時需要uncommant
                        //SocketServer.imgObj.ImgBuffer = binaryReader.ReadBytes(IMG_SIZE);

                        SocketServer.imgObj.ImgBuffer = null;

                        string a;
                        a = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(BODY_NUMBER_SIZE));

                        int bodyNumbers = int.Parse(a);

                        MyBody body; 
                        
                        for (int i = 0; i < bodyNumbers; i++)
                        {
                            body = SocketServer.imgObj.Body[i];

                            int strLength = int.Parse(System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(BODY_SKELETION_STR_SIZE)));
                            SocketServer.imgObj.SBodyJoints[i] = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(strLength));
                            body.isTracked = true;

                        }

                        //設定其他骨架為未追蹤
                        for (int i = bodyNumbers; i < 7; i++)
                        {
                            body = SocketServer.imgObj.Body[i];

                            body.isTracked = false;
                        }
                        
                        
                        //寫入狀態
                        if (netStream.CanWrite)
                        {
                            int status = (int)SocketServer.status.SocketStatus;

                            netStream.Write(System.Text.Encoding.UTF8.GetBytes(status.ToString()), 0, 1);

                            if (status == (int)SocketPackage.TRANSMIT_STATUS.StartRecord)
                            {
                                SocketServer.status.SocketStatus = SocketPackage.TRANSMIT_STATUS.Recording;
                            }
                            if (status == (int)SocketPackage.TRANSMIT_STATUS.StartPlaybackClip)
                            {
                                SocketServer.status.SocketStatus = SocketPackage.TRANSMIT_STATUS.PlaybackCliping;
                            }
                        }
                    }
                  //  catch (Exception ee)
                    {
                      //  log.Error(string.Format("_ClientNo:{0} ", _ClientNo), ee);
                    }
                }
            }

        }



        #endregion
    }
}
