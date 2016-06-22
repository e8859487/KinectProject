using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketPackage;

namespace MotionFSM
{
    using XmlManager;
    using Microsoft.Kinect;
    using Microsoft.Activities.Extensions.Tracking;
    using System.Diagnostics;
    using System.ComponentModel;

    public class MotionsAnalyze : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal static readonly Dictionary<string, int> THS = new Dictionary<string, int>();

        private WorkflowInstance wfInstance;

        private Vector4 floorClipPlane;

        private float deviceHeight = 0;


        private string currentMotions = string.Empty;
        /// <summary>
        /// 記錄當下動作
        /// </summary>
        public string CurrentMotions
        {
            get
            {
                return currentMotions;
            }
            set
            {
                if (currentMotions != value)
                {
                    currentMotions = value;
                    if (PropertyChanged != null)
                    {
                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("CurrentMotions"));
                    }
                }
            }
        }

        /// <summary>
        /// 讀取初始資料
        /// </summary>
        internal void InitialThreadSheld()
        {
            if (THS.Count < 10)
            {
                XmlReader reader = new XmlReader(@"./Setting/3DSetting.xml");
                THS.Add("T1", int.Parse(reader.getNodeInnerText(@"/Root/T1")));
                THS.Add("T2", int.Parse(reader.getNodeInnerText(@"/Root/T2")));
                THS.Add("T3", int.Parse(reader.getNodeInnerText(@"/Root/T3")));
                THS.Add("T4", int.Parse(reader.getNodeInnerText(@"/Root/T4")));
                THS.Add("FDT", int.Parse(reader.getNodeInnerText(@"/Root/FDT")));
                THS.Add("T6", int.Parse(reader.getNodeInnerText(@"/Root/T6")));
                THS.Add("T7", int.Parse(reader.getNodeInnerText(@"/Root/T7")));
                THS.Add("T8", int.Parse(reader.getNodeInnerText(@"/Root/T8")));
                THS.Add("T9_1", int.Parse(reader.getNodeInnerText(@"/Root/T9_1")));
                THS.Add("T9_2", int.Parse(reader.getNodeInnerText(@"/Root/T9_2")));
                reader.Dispose();
            }
        }

        MyBody initBody = null;
        MyBody preBody = null;
        MyBody nowBody = null;
        // CSV_Writter csv_Writter;

        public MotionsAnalyze(IWorkflowView w)
        {

            this.InitialThreadSheld();

            wfInstance = new WorkflowInstance(w);
            wfInstance.Run();

            nowBody = new MyBody();
            string fileName = DateTime.Now.ToString("MMddyy-HHmmss");
            //csv_Writter = new CSV_Writter(fileName + ".csv");

        }

        public StateMachineStateTracker getStateMachinesStateTracker()
        {
            return wfInstance.StateTracker;
        }

        public string timespan { get; set; }

        private Boolean IsInitBodyHeight = false;
        private Boolean IsInitBodyHeight2 = false;

        float fallDownThreshold = 0.75f;

        public void EventAnalyze(MyBody now)
        {
            //first Execute
            if (initBody == null)
            {
                initBody = new MyBody();

                this.DeepCopyBodyData(now, this.initBody);
            }
            if (preBody == null)
            {
                preBody = new MyBody();

                this.DeepCopyBodyData(now, this.preBody);
            }

            MyBody pre = preBody;
            MyBody Init = initBody;

            MotionTransitions transitionOutcome = MotionTransitions.E_Null;

            #region Skeleton Define
            CameraSpacePoint head_Ini = Init.jointsInfo[JointType.Head].Position;

            CameraSpacePoint head_Pre = pre.jointsInfo[JointType.Head].Position;

            CameraSpacePoint head = now.jointsInfo[JointType.Head].Position;

            CameraSpacePoint torsol_Ini = Init.jointsInfo[JointType.SpineMid].Position;

            CameraSpacePoint torsol_Pre = pre.jointsInfo[JointType.SpineMid].Position;

            CameraSpacePoint torsol = now.jointsInfo[JointType.SpineMid].Position;

            CameraSpacePoint neckkk = now.jointsInfo[JointType.Neck].Position;

            CameraSpacePoint SpineBase_Ini = Init.jointsInfo[JointType.SpineBase].Position;
            CameraSpacePoint SpineBase_Pre = pre.jointsInfo[JointType.SpineBase].Position;
            CameraSpacePoint SpineBase = now.jointsInfo[JointType.SpineBase].Position;

            CameraSpacePoint SpineShoulder_Ini = Init.jointsInfo[JointType.SpineShoulder].Position;
            CameraSpacePoint SpineShoulder_Pre = pre.jointsInfo[JointType.SpineShoulder].Position;
            CameraSpacePoint SpineShoulder = now.jointsInfo[JointType.SpineShoulder].Position;
            #endregion


            if (!IsInitBodyHeight)
            {
                fallDownThreshold = (float)(THS["FDT"] * 0.001);
                IsInitBodyHeight = true;
            }
            else if (D != 0 && !IsInitBodyHeight2)
            {
                fallDownThreshold = getHeightFromPoint(head_Ini) / 2.3f;
                IsInitBodyHeight2 = true;
                // Debug.Print("FDT = " + fallDownThreshold);
            }


            #region Decision Tree
            if (head_Ini.Y - head.Y > 0.05 && torsol_Ini.Y - torsol.Y > 0.05) //第一個分支點  :初始與現在的頭與身 Dy 大於200
            {
                if (head.Y - head_Pre.Y > 0.13 && torsol.Y - torsol_Pre.Y > 0.13) //第三個分支點.20 40//  前後頭身Dy 大於35
                {
                    if (head_Ini.Y - head.Y > 0.3 && torsol_Ini.Y - torsol.Y > 0.3)
                        transitionOutcome = MotionTransitions.E_GetUp; // INCIDENT_SIT_UP; //坐起來
                }
                else if (SpineBase_Ini.Y - SpineBase.Y > fallDownThreshold)//新版跌倒判斷機制
                {
                    if (head.Y - neckkk.Y > 0.06)
                        transitionOutcome = MotionTransitions.E_FallDown;//跌倒
                    //else
                    // Debug.Print("E_FallDown's don't care\n");
                }
                else if (head.Y - SpineBase.Y > 0.28)
                {
                    if (head_Ini.Y - head.Y > 0.3 && torsol_Ini.Y - torsol.Y > 0.3 && head.Y - head_Pre.Y < -0.030 && torsol.Y - torsol_Pre.Y < -0.030)
                    {
                        transitionOutcome = MotionTransitions.E_SitDown; //坐下
                    }
                    else
                    {
                        //Debug.Print("Sitdown's don't care\n");
                    }
                }
                else
                    transitionOutcome = MotionTransitions.E_LieDown; //躺下  
            }
            else//頭與身 Dy 小於200
            {
                if ((head_Pre.X - head.X) * (head_Pre.X - head.X) + (head_Pre.Z - head.Z) * (head_Pre.Z - head.Z) > 0.003 &&
                         (torsol_Pre.X - torsol.X) * (torsol_Pre.X - torsol.X) + (torsol_Pre.Z - torsol.Z) * (torsol_Pre.Z - torsol.Z) > 0.003) //第二個分支點.
                    transitionOutcome = MotionTransitions.E_Walk;//走
                else
                    transitionOutcome = MotionTransitions.E_Stop; //停止
            }
            #endregion

            //Debug.Print(string.Format("TimeSpan:{0}, Transition:{1}, State:{2}", timespan, transitionOutcome, GetCurrentState()));

            //write out to csv
            //csv_Writter.WriteLine(string.Format("{0},{1},{2}", timespan, transitionOutcome, GetCurrentState()));


            if (transitionOutcome == MotionTransitions.E_Null)
            {
                return;
            }

            //Transimit a new event to new State
            this.ResumeBookmark(transitionOutcome);
            //Copy nowbody as pre body.
            this.DeepCopyBodyData(now, preBody);

            this.UpdateCurrentState();

        }

        private float getHeightFromPoint(CameraSpacePoint CSP)
        {
            if (D != 0)
            {
                return (CSP.X * A + CSP.Y * B + CSP.Z * C + D) / denominatore;
            }
            return 0;
        }

        public void ResumeBookmark(MotionTransitions events)
        {
            string eventName = Enum.GetName(typeof(MotionTransitions), events);
            if (wfInstance.IsEventExist(eventName))
            {
                wfInstance.ResumeBookmark(eventName);
            }
            else
            {
                wfInstance.ResumeBookmark("E_Unknow");
            }
        }

        /// <summary>
        /// 取得目前動作狀態
        /// </summary>
        /// <returns></returns>
        public string UpdateCurrentState()
        {
            this.CurrentMotions = wfInstance.StateTracker.CurrentState;
            return wfInstance.StateTracker.CurrentState;
        }

        internal void DeepCopyBodyData(MyBody SourceBody, MyBody TargetBody)
        {
            TargetBody.jointsInfo = new Dictionary<JointType, Joint>(SourceBody.jointsInfo);
        }

        float A;
        float B;
        float C;
        float D;

        float addendo1_d;
        float addendo2_d;
        float addendo3_d;
        float denominatore;
        public void SetFloorClipPlane(Vector4 v4)
        {
            floorClipPlane = v4;
            deviceHeight = v4.W;
            A = floorClipPlane.X;
            B = floorClipPlane.Y;
            C = floorClipPlane.Z;
            D = floorClipPlane.W;

            addendo1_d = A * A;
            addendo2_d = B * B;
            addendo3_d = C * C;
            denominatore = (float)System.Math.Sqrt(addendo1_d + addendo2_d + addendo3_d);
        }

        public void Dispose()
        {
            wfInstance.Disposed();
        }
    }
}
