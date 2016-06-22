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
        //定義socket封包資料格式長度
        /// <summary>
        /// 0
        /// </summary>
        public const int IMG_LENTH = 0;
        /// <summary>
        /// 1
        /// </summary>
        public const int BODY_NUMBER_LENTH = 1;
        /// <summary>
        /// 3
        /// </summary>
        public const int BODY_SKELETION_STR_LENTH = 3;

        #region private property

        /// socket client 識別號碼
        private int clientNo;

        /// socket client reuqest
        private TcpClient tcpClient;

        //Log Declare
        private ILog log = null;

        //Socket Data
        private SocketData socketData = null;
        #endregion

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="inClientNo">socket client 識別號碼</param>
        /// <param name="inTcpClient">Socket client reuqest</param>
        public ClientRequestHandler(int clientNo, TcpClient tcpClient,SocketData socketData )
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Info(string.Format("ClientNo:{0} , Create successfully!!", clientNo));

            Debug.Print(string.Format("ClientNo:{0} , Create successfully!!", clientNo));

            this.clientNo = clientNo;
            this.tcpClient = tcpClient;
            this.socketData = socketData;
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
            BinaryWriter binaryWritter = null;

            //送出裝置ID
            if (tcpClient.Connected)
            {
                //取得網路串流物件，取得來自 socket client 的訊息
                netStream = tcpClient.GetStream();
                binaryWritter = new BinaryWriter(netStream);

                binaryWritter.Write(System.Text.Encoding.UTF8.GetBytes(clientNo.ToString()));
                binaryWritter.Flush();
            }


            //server & client 已經連線完成
            while (tcpClient.Connected)
            {
                if (tcpClient.ReceiveBufferSize > 0 && netStream.CanRead)
                {
                    int socketHead = 0;  //封包標頭 - 區分封包內容
                    int clientID = 0;   //裝置ID - 區分不同裝置

                    try
                    {
                        if (netStream.CanRead)
                        {
                            binaryReader = new BinaryReader(netStream);

                            //裝置ID (1bit)
                            clientID = int.Parse(System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(1)));
                            //Debug.Print(">>ClientRequestHandker.bgwSocket_DoWork: devicesID {0}", devicesID);

                            //取得封包標頭 (1bit)
                            socketHead = int.Parse(System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(1)));
                            //Debug.Print(">>ClientRequestHandker.bgwSocket_DoWork: socketHead {0}", socketHead);
                        }
                    }
                    catch
                    {
                        Debug.Print(">>ClientRequestHandler read socket head error");
                    }

                    try
                    {
                        switch (socketHead)
                        {
                            case 1:
                                #region 讀取3D骨架資料
                                binaryReader = new BinaryReader(netStream);
                                //使用2D版本時需要uncommant
                                //SocketServer.imgObj.ImgBuffer = binaryReader.ReadBytes(IMG_SIZE);


                                //人體總數
                                int bodyNumbers = int.Parse(System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(BODY_NUMBER_LENTH)));

                                MyBody body;

                                for (int i = 0; i < bodyNumbers; i++)
                                {
                                    body = socketData.Body[i];

                                    int strLength = int.Parse(System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(BODY_SKELETION_STR_LENTH)));
                                    socketData.SBodyJoints[i] = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(strLength));
                                    body.isTracked = true;
                                }

                                //設定其他骨架為未追蹤
                                for (int i = bodyNumbers; i < 7; i++)
                                {
                                    body = socketData.Body[i];

                                    body.isTracked = false;
                                }

                                //寫入狀態
                                if (netStream.CanWrite)
                                {
                                    //int status = (int)SocketServer.status.SocketStatus;
                                    int status = (int)this.socketData.playbackStatus.SocketStatus;
                                    netStream.Write(System.Text.Encoding.UTF8.GetBytes(status.ToString()), 0, 1);

                                    if (status == (int)SocketPackage.TRANSMIT_STATUS.StartRecord)
                                    {
                                        this.socketData.playbackStatus.SocketStatus = SocketPackage.TRANSMIT_STATUS.Recording;
                                    }
                                    if (status == (int)SocketPackage.TRANSMIT_STATUS.StartPlaybackClip)
                                    {
                                        this.socketData.playbackStatus.SocketStatus = SocketPackage.TRANSMIT_STATUS.PlaybackCliping;
                                        //SocketServer.status.SocketStatus = SocketPackage.TRANSMIT_STATUS.PlaybackCliping;
                                    }
                                }

                                //此處只是觸動onChanged
                                socketData.ImgBuffer = System.Text.Encoding.UTF8.GetBytes("0");

                                #endregion
                                break;
                            //For test
                            case 2:
                                Debug.Print(">>Get Device number's info :{0} ", clientID);
                                    
                                break;

                        }

                    }
                    catch (Exception ee)
                    {
                        Debug.Print(ee.ToString());
                    }
                }
            }


        }

 

        #endregion
    }
}
