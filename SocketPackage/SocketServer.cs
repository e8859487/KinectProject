﻿using System;
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

        /// 工作進行中產生之訊息
        public static List<string> sMessages = new List<string>();

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
        }
        
        #endregion

        #region Public Method

        public void Start()
        {
            if (!_bgwServer.IsBusy)
                _bgwServer.RunWorkerAsync();
        }

        #endregion

        #region Private Method
        
        private void _bgwServer_DoWork(object sender, DoWorkEventArgs e)
        {
            try
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
            catch (Exception exp)
            {
                //sMessages.Add(exp.ToString());
                log.Error("_bgwServer_DoWork", exp);
            }
        }
        
        #endregion
    }
}
