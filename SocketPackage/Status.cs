using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPackage
{
    public delegate void changedEventHandler(object sender, EventArgs e);

    public  enum TRANSMIT_STATUS{
        Received,
        StartRecord,
        Recording,
        StopRecord,
        StartPlaybackClip,
        PlaybackCliping,
        StopPlaybackClip
    }

   public  class Status
    {
        public event changedEventHandler changed;

        private TRANSMIT_STATUS _socketStatus;

        public TRANSMIT_STATUS SocketStatus
        {
            get
            {
                return _socketStatus;
            }
            set
            {
                if (_socketStatus != value)
                {
                    _socketStatus = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        public Status()
        {

        }

        protected virtual void OnChanged(EventArgs e){
            if(changed != null){
                changed(this,e);
            }
        }

    }


    /// <summary>
    /// 紀錄目前封包狀態
    /// </summary>
    public class StatusEventArgs:EventArgs
    {
        private TRANSMIT_STATUS status;

        public StatusEventArgs(TRANSMIT_STATUS s)
        {
            Status = s;
        }


        public TRANSMIT_STATUS Status
        {
            get { return status; }
            set { 
                status = value;
            }
        }
            

    }
}
