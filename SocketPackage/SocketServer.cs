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

namespace SocketPackage
{
    //public delegate void changeEventHandler(object sender, EventArgs e);


    public class SocketServer
    {
        #region private property

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

        #endregion

        #region static public property



        public static  Status status = new Status();

        static public SocketData imgObj = new SocketData();
        
        private void StatusChange(object sender,EventArgs e)
        {
            log.Info("StatusChange Fired");
        }

        //my callback
        public event changeEventHandler changed;

        protected virtual void OnChanged(EventArgs e){
            if (changed != null)
            {
                changed(this, e);
            }
        }

        private void SocketDataChanged(object sender , EventArgs e)
        {
            this.OnChanged(EventArgs.Empty);
        }

        #endregion

        #region constructor

        /// constructor
        //socket server TCP port number
        public SocketServer(int inPortNumber)
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            this._PortNumber = inPortNumber;
            _bgwServer.DoWork += new DoWorkEventHandler(_bgwServer_DoWork);

//            status.changed += new changedEventHandler(StatusChange);

            imgObj.changed += new changeEventHandler(SocketDataChanged);
        }
        
        #endregion

        #region Public Method

        public void Start()
        {
            if (!_bgwServer.IsBusy)
                _bgwServer.RunWorkerAsync();
        }

        public void SendStartAsychronousRecord(){
            status.SocketStatus = TRANSMIT_STATUS.StartRecord;
        }

        public void SendStopAsychronousRecord()
        {
            status.SocketStatus = TRANSMIT_STATUS.StopRecord;
        }

        public void SendStartAsychronousPlay()
        {
            status.SocketStatus = TRANSMIT_STATUS.StartPlaybackClip;
        }

        public void SendStopAsychronousPlay()
        {
            status.SocketStatus = TRANSMIT_STATUS.StopPlaybackClip;
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
                //========== 持續接受監聽 socket client 的連線 ========== (start)
                while (true)
                {
                    //監聽到來自 socket client 的連線要求
                    TcpClient socket4Client = _tcpListener.AcceptTcpClient();

                    //累加 socket client 識別編號
                    _ClientNo++;
                    log.Info(" >> " + "Client Request No:" + Convert.ToString(_ClientNo) + " started!");

                    //sMessages.Add(" >> " + "Client Request No:" + Convert.ToString(_ClientNo) + " started!");

                    //產生 BackgroundWorker 負責處理每一個 Socket Client 的要求
                    ClientRequestHandler handler = new ClientRequestHandler(_ClientNo, socket4Client);
                    handler.DoCommunicate();

                }
            }
            //catch (Exception exp)
            {
                //sMessages.Add(exp.ToString());
              //  log.Error("_bgwServer_DoWork", exp);
            }
        }
        
        #endregion
    }
}
