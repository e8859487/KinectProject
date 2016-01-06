using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using Img_Serializable;
namespace SocketPackage
{
   public  class ClientRequestHandler
    {
        static public ImageInfo_Serializable imgObj = null;
        #region private property

        /// socket client 識別號碼
        private int _ClientNo;

        /// socket client reuqest
        private TcpClient _TcpClient;

        #endregion

        #region constructor

        /// constructor
        //socket client 識別號碼
        //socket client reuqest
        public ClientRequestHandler(int inClientNo, TcpClient inTcpClient)
        {
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
            NetworkStream netStream =null;
            BinaryFormatter binaryFromatter = new BinaryFormatter();

            //server & client 已經連線完成
            while (_TcpClient.Connected)
            {
                //取得網路串流物件，取得來自 socket client 的訊息
                netStream = _TcpClient.GetStream();
               // byte[] readBuffer = new byte[_TcpClient.ReceiveBufferSize];
                //int count = 0;
             //   if ((count = netStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                if(_TcpClient.ReceiveBufferSize>0 && netStream.CanRead)
                {
                    try { 
                      imgObj = (ImageInfo_Serializable)binaryFromatter.Deserialize(netStream);
                        }
                    catch(Exception ee)
                    {
                        SocketServer.sMessages.Add(ee.ToString());
                    }
                    //SocketServer._memStream.Seek(0, System.IO.SeekOrigin.Begin);
                    //SocketServer._memStream.Write(readBuffer, 0, count);
                }
            }

        }



        #endregion
    }
}
