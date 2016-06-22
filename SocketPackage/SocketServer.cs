using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using log4net;
using log4net.Config;
using System.Diagnostics;

namespace SocketPackage
{
    //public delegate void changeEventHandler(object sender, EventArgs e);


    public class SocketServer
    {
        #region Private property

        /// socket server TCP port number
        private int _PortNumber;

        /// TCP Listener
        private TcpListener _tcpListener;

        /// 用來處理 Listener 工作的執行緒
        private BackgroundWorker _bgwServer = new BackgroundWorker();

        /// socker client 識別編號
        private int _ClientNo = 0;

        //Log
        private ILog log = null;

        //封包資料 依照機台來分辨
        public List<SocketData> socketDataList = null;

        #endregion

        #region === Static Public Property ===

       // public static  PlayBackStatus status = new PlayBackStatus();

        //my callback
        public event changeEventHandler changed;

        protected virtual void OnChanged(EventArgs e){
            if (changed != null)
            {
                changed(this, e);
            }
        }

        //private void SocketDataChanged(object sender , EventArgs e)
        //{
        //    this.OnChanged(EventArgs.Empty);
        //}

        #endregion

        #region constructor

        /// constructor
        //socket server TCP port number
        public SocketServer(int inPortNumber,List<SocketData> SocketDataStorage)
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            this._PortNumber = inPortNumber;
            _bgwServer.DoWork += new DoWorkEventHandler(_bgwServer_DoWork);

            socketDataList = SocketDataStorage;

            //socketData.changed += new changeEventHandler(SocketDataChanged);
        }
        
        #endregion

        #region Public Method
        /// <summary>
        /// Start network socket work.
        /// </summary>
        public void Start()
        {
            if (!_bgwServer.IsBusy)
                _bgwServer.RunWorkerAsync();
        }

        public  void SendStartAsychronousRecord(){
           // status.SocketStatus = TRANSMIT_STATUS.StartRecord;
            Debug.Print(">>SocketServer.SendStartAsychronousRecord Error ");
        }

        public void SendStopAsychronousRecord()
        {
            //status.SocketStatus = TRANSMIT_STATUS.StopRecord;
            Debug.Print(">>SocketServer.SendStopAsychronousRecord Error ");

        }

        public static void SendStartAsychronousPlay()
        {
            //status.SocketStatus = TRANSMIT_STATUS.StartPlaybackClip;
            Debug.Print(">>SocketServer.SendStartAsychronousPlay Error ");

        }

        public void SendStopAsychronousPlay()
        {
            //status.SocketStatus = TRANSMIT_STATUS.StopPlaybackClip;
            Debug.Print(">>SocketServer.SendStopAsychronousPlay Error ");

        }
  
        #endregion

        #region Private Method
        
        private void _bgwServer_DoWork(object sender, DoWorkEventArgs e)
        {
           // try
            {
                //若 TCP Listner 正在工作，則停止
                if (_tcpListener != null)
                    _tcpListener.Stop();

                //初始化 TCPListener
                _tcpListener = new TcpListener(IPAddress.Any, _PortNumber);

                //啟動 listener
                _tcpListener.Start();
                log.Info(">> Server Started");

                Debug.Print(">> Server Started");


                //========== 持續接受監聽 socket client 的連線 ========== (start)
                while (true)
                {
                    //監聽到來自 socket client 的連線要求
                    TcpClient socket4Client = _tcpListener.AcceptTcpClient();

                    //累加 socket client 識別編號
                    _ClientNo++;
                    log.Info(" >> " + "Client Request No:" + Convert.ToString(_ClientNo) + " started!");
                    
                    Debug.Print(string.Format(" >> " + "Client Request No: {0} started!", Convert.ToString(_ClientNo)));
                    
                    //新增一個儲存物件
                    SocketData socketData = new SocketData();
                    socketDataList.Add(socketData);

                    //產生 BackgroundWorker 負責處理每一個 Socket Client 的要求
                    ClientRequestHandler handler = new ClientRequestHandler(_ClientNo, socket4Client, socketData);
                    handler.DoCommunicate();

                }
            }

        }
        
        #endregion
    }
}
