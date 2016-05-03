using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleKinectMaster3D
{
    public enum DrawStatus
    {
        NewUser,
        TrackedUser,
    }

    class BodyManager
    {

        private int totalBody = 0;
    
        /// <summary>
        /// 目前偵測總人數
        /// </summary>
        public int TotalBody
        {
            get { return totalBody; }
            set { totalBody = value; }
        }

        private Dictionary<ulong, DrawStatus> bodyState;
        /// <summary>
        /// 偵測狀態
        /// </summary>
        public Dictionary<ulong, DrawStatus> BodyState
        {
            get { return bodyState; }
            set { bodyState = value; }
        }

        private ulong currentBodyIdx = 0;
        /// <summary>
        /// 當下繪圖目標
        /// </summary>
        public ulong CurrentBodyIdx
        {
            get { return currentBodyIdx; }
            set { currentBodyIdx = value; }
        }


        private List<ulong> workList;

        /// <summary>
        /// 紀錄當前追蹤中的使用者ID
        /// </summary>
        public List<ulong> WorkList
        {
            get { return workList; }
            set { workList = value; }
        }

        private List<ulong> workList_Pre;



        /// <summary>
        /// 記錄前一刻追蹤中的使用者ID
        /// </summary>
        public List<ulong> WorkList_Pre
        {
            get { return workList_Pre; }
            set { workList_Pre = value; }
        }

        private List<ulong> workList_server;

        public List<ulong> WorkList_server
        {
            get
            {
                return workList_server;
            }
            set { workList_server = value; }
        }

        private List<ulong> workList_client;

        public List<ulong> WorkList_client
        {
            get
            {
                return workList_client;
            }
            set { workList_client = value; }
        }



        public BodyManager()
        {
            bodyState = new Dictionary<ulong, DrawStatus>();
            workList = new List<ulong>();
            workList_Pre = new List<ulong>();
            workList_client = new List<ulong>();
            workList_server = new List<ulong>();
        }

        public void SynchronousWorkList(List<ulong> list)
        {
            foreach (ulong l in list)
            {
                if (!WorkList.Contains(l))
                {
                    WorkList.Add(l);
                }
            }
        }

 
    }
}
