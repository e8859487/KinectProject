using Microsoft.Kinect;
using SocketPackage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MotionFSM;
using Microsoft.Activities.Extensions.Tracking;
using System.Activities;
using System.ComponentModel;

namespace MultipleKinectMaster3D
{
    class MasterKinectProcessor3D : IWorkflowView, INotifyPropertyChanged
    {

        #region Private Variables
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        private readonly Brush inferredJointBrush = Brushes.Yellow;

        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        private readonly List<Tuple<JointType, JointType>> bones;

        private readonly List<Pen> bodyColors;

        private Body[] bodies = null;

        private MultiSourceFrameReader multiFrameSourceReader = null;


        #region Delegate declare
        //Joints delegate
        private delegate void JointsDelegate(Point3D point3D, Brush brush, float radius);

        //Joints delegate instances
        private JointsDelegate AddPoint;

        //Skeletons delegate
        private delegate void PipeDelegate(Point3D point1, Point3D point2, Brush brush, float radius);

        //Skeletons delegate instances
        private PipeDelegate AddPipe;

        //Joints Container store every joints in it
        public ObservableCollection<Element> ObservableJoints { get; set; }

        //Skeletons Container store every skeleton in it
        public ObservableCollection<Element> ObservablePipe { get; set; }

        //Deleton joints and skeletons
        private delegate void deleteDelegate();

        private deleteDelegate deletePipe;

        private deleteDelegate deleteJoints;

        #endregion


        private SocketServer socketServer = null;

        private const int PORT = 81;

        private Dispatcher dispatcher = null;

        object thisLock = new object();





        #endregion



        public MasterKinectProcessor3D(KinectSensor kinectSensor, Dispatcher dispatcher)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            this.dispatcher = dispatcher;

            //multipleFrameReader 
            this.multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Body);
            this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            socketServer = new SocketServer(PORT);
            socketServer.Start();
            socketServer.changed += new changeEventHandler(SocketServer_changed);



            #region Bone Implement
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            #endregion

            #region BodyColors Implements
            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));
            #endregion

            #region DelegateInit
            ObservableJoints = new ObservableCollection<Element>();

            AddPoint = new JointsDelegate((Point3D point3D, Brush brush, float radius) =>
            {
                this.ObservableJoints.Add(new Element
                {
                    Position = point3D,
                    Brushes = brush,
                    Radius = radius
                });
            });

            ObservablePipe = new ObservableCollection<Element>();

            AddPipe = new PipeDelegate((Point3D point1, Point3D point2, Brush brush, float radius) =>
            {
                this.ObservablePipe.Add(new Element
                {
                    Point1 = point1,
                    Point2 = point2,
                    Radius = radius,
                    Brushes = brush
                });

            });

            deleteJoints = new deleteDelegate(() =>
            {
                this.ObservableJoints.RemoveAt(0);
            });

            deletePipe = new deleteDelegate(() =>
            {
                this.ObservablePipe.RemoveAt(0);
            });
            #endregion

            this.InitialWorkflowInstance();
        }

        private void SocketServer_changed(object sender, EventArgs e)
        {
            lock (thisLock)
            {
                SocketPackage.SocketServer.imgObj.ProcessJointsInfo();
                //清空使用者清單
                //bodyManager.WorkList_client.Clear();
            }

            //for (int i = 0; i < 7; i++)
            //{
            //    MyBody body = SocketPackage.SocketServer.imgObj.Body[i];
            //    //畫出3D骨架

            //    if (body.isTracked)
            //    {
            //        Pen clientPen = new Pen(Brushes.Black, 3);
            //        lock (thisLock)
            //        {
            //            bodyManager.CurrentBodyIdx = body.TrackingId;

            //            if (!bodyManager.BodyState.ContainsKey(bodyManager.CurrentBodyIdx))
            //            {
            //                bodyManager.BodyState.Add(bodyManager.CurrentBodyIdx, DrawStatus.NewUser);
            //            }

            //            //紀錄追蹤清單
            //            bodyManager.WorkList_client.Add(bodyManager.CurrentBodyIdx);

            //        }
            //        //bodyManager.AddClientBody(body.jointsInfo, clientPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx]);
            //          this.DrawBody3D(body.jointsInfo, clientPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx]);
            //    }
            //}

        }

        public void Dispose()
        {
            if (this.multiFrameSourceReader != null)
            {
                this.multiFrameSourceReader.MultiSourceFrameArrived -= Reader_MultiSourceFrameArrived;
                this.multiFrameSourceReader.Dispose();
                this.multiFrameSourceReader = null;
            }
        }

        int frameIndex = 0;
        TimeSpan startTime = new TimeSpan(0, 0, 0);
        TimeSpan tsp;

        private bool IsDeviceHeightReady = false;
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            DepthFrame depthFrame = null;
            BodyFrame bodyFrame = null;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            bool dataReceived = false;

            if (multiSourceFrame == null)
            {
                return;
            }
            try
            {
                bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame();
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                    if (startTime.TotalSeconds != 0)
                    {
                        tsp = (bodyFrame.RelativeTime - startTime);
                        Lbl_TimeStamp = new TimeSpan(0, tsp.Minutes, tsp.Seconds).ToString();
                        motionAnalyzer.timespan = Lbl_TimeStamp;
                    }
                    else
                    {
                        startTime = bodyFrame.RelativeTime;
                    }

                }


                this.frameIndex++;
                if (dataReceived)
                {
                    if ((this.frameIndex % 3) == 0)
                    {
                        this.UpdateBodyFrame(this.bodies);
                    }
                    if (bodyFrame.FloorClipPlane.W != 0 && !IsDeviceHeightReady)
                    {
                        motionAnalyzer.SetFloorClipPlane(bodyFrame.FloorClipPlane);
                        IsDeviceHeightReady = true;
                    }
                }
            }
            finally
            {
                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }
                if (bodyFrame != null)
                {
                    bodyFrame.Dispose();
                }
                if (multiSourceFrame != null)
                {
                    multiSourceFrame = null;
                }
            }
        }

        private BodyManager bodyManager = new BodyManager();

        private ViewPointManager viewPointManager = new ViewPointManager();

        private int IsDeviceRotationMTReady_A = 0;
        private int IsDeviceRotationMTReady_B = 0;
        private int IsDeviceRotationMTReady = 0;
        private void UpdateBodyFrame(Body[] bodies)
        {
            int penIndex = 0;
            int drawIdx = 0;

            bodyManager.CurrentBodyIdx = 0;
            //清空使用者清單
            bodyManager.WorkList_server.Clear();
            foreach (Body body in bodies)
            {
                Pen drawPen = this.bodyColors[penIndex++];

                if (body.IsTracked)
                {
                    IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                    foreach (JointType jointType in joints.Keys)
                    {
                        CameraSpacePoint position = joints[jointType].Position;
                        if (position.Z < 0)
                        {
                            position.Z = 0.1f;
                        }
                    }
                    bodyManager.CurrentBodyIdx = body.TrackingId;

                    if (!bodyManager.BodyState.ContainsKey(bodyManager.CurrentBodyIdx))
                    {
                        bodyManager.BodyState.Add(bodyManager.CurrentBodyIdx, DrawStatus.NewUser);
                    }
                    //紀錄追蹤清單
                    bodyManager.WorkList_server.Add(bodyManager.CurrentBodyIdx);

                    #region 視角轉換
                    //經轉換過的骨架座標點與原始相關資料
                    Dictionary<JointType, Joint> joints_transfered = new Dictionary<JointType, Joint>();


                    //if (!IsDeviceRotationMTReady_A)
                    {

                        //計算RT矩陣
                        viewPointManager.Analyze_RT_Matrix(joints[JointType.ShoulderLeft].Position, joints[JointType.ShoulderRight].Position, joints[JointType.SpineMid].Position, DEVICE_ID.DEVICE_A);
                        IsDeviceRotationMTReady_A += 1;
                    }
                    //視角轉換
                    //foreach (KeyValuePair<JointType, Joint> item in joints)
                    //{
                    //    Joint tempJoint = item.Value;
                    //    tempJoint.Position = viewPointManager.Transform(tempJoint.Position);

                    //    if (joints_transfered.ContainsKey(item.Key))
                    //    {
                    //        joints_transfered[item.Key] = tempJoint;
                    //    }
                    //    else
                    //    {
                    //        joints_transfered.Add(item.Key, tempJoint);
                    //    }
                    //} 
                    #endregion


                    //動作追蹤系統
                    motionAnalyzeManager.AddMotionAnalyzer(bodyManager.CurrentBodyIdx);
                    motionAnalyzeManager.EventAnalyze(bodyManager.CurrentBodyIdx, MyBody.Body2Mybody(body));

                    //畫出3D骨架
                    DrawBody3D(joints, drawPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx], true, drawIdx);

                    drawIdx++;

                }
            }


            #region 3D clientPoints

            //清空使用者清單
            bodyManager.WorkList_client.Clear();
            lock (thisLock)
            {
                for (int i = 0; i < 7; i++)
                {
                    MyBody body = SocketPackage.SocketServer.imgObj.Body[i];
                    //畫出3D骨架

                    if (body.isTracked)
                    {
                        Pen clientPen = new Pen(Brushes.Black, 3);
                        bodyManager.CurrentBodyIdx = body.TrackingId;

                        //client端骨架管理，若是新的使用者則新增進去
                        if (!bodyManager.BodyState.ContainsKey(bodyManager.CurrentBodyIdx))
                        {
                            //骨架ID管理系統
                            bodyManager.BodyState.Add(bodyManager.CurrentBodyIdx, DrawStatus.NewUser);
                            
                            //動作追蹤系統
                            motionAnalyzeManager.AddMotionAnalyzer(bodyManager.CurrentBodyIdx);
                        }

                        //紀錄追蹤清單
                        bodyManager.WorkList_client.Add(bodyManager.CurrentBodyIdx);


                        //經轉換過的骨架座標點與原始相關資料
                        Dictionary<JointType, Joint> joints_transfered = new Dictionary<JointType, Joint>();

                        #region 視角轉換
                        //計算RT矩陣
                        // if (!IsDeviceRotationMTReady_B)
                        {
                            viewPointManager.Analyze_RT_Matrix(body.jointsInfo[JointType.ShoulderLeft].Position, body.jointsInfo[JointType.ShoulderRight].Position, body.jointsInfo[JointType.SpineMid].Position, DEVICE_ID.DEVICE_B);
                            IsDeviceRotationMTReady_B += 1;

                        }

                        if (IsDeviceRotationMTReady_A > 1 && IsDeviceRotationMTReady_B > 1)
                        {
                            //  if (!IsDeviceRotationMTReady)
                            {
                                viewPointManager.Analyze_RT_Matrix_TwoDevice();
                            }
                            //視角轉換
                            foreach (KeyValuePair<JointType, Joint> item in body.jointsInfo)
                            {
                                Joint tempJoint = item.Value;
                                tempJoint.Position = viewPointManager.Transform_B2A(tempJoint.Position);

                                if (joints_transfered.ContainsKey(item.Key))
                                {
                                    joints_transfered[item.Key] = tempJoint;
                                }
                                else
                                {
                                    joints_transfered.Add(item.Key, tempJoint);
                                }
                            } 
                        #endregion
                            this.DrawBody3D(joints_transfered, clientPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx], true, drawIdx);

                        }
                        else
                        {
                            this.DrawBody3D(body.jointsInfo, clientPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx], true, drawIdx);
                        }

                        //bodyManager.AddClientBody(body.jointsInfo, clientPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx]);
                        drawIdx++;


                        //Motion Detect!!
                        if (i == 0 && ClientFrameCounter % 4 == 0)
                        {
                            //motionAnalyzer.EventAnalyze(body);
                            motionAnalyzeManager.motionDict[body.TrackingId].EventAnalyze(body);
                        }
                        //Updata UI motion 
                        //Lbl_MotionState = motionAnalyzer.GetCurrentState();


                    }
                }
                ClientFrameCounter++;

            }

            #endregion


            #region 比對資料
            lock (thisLock)
            {
                bodyManager.WorkList.Clear();
                bodyManager.SynchronousWorkList(bodyManager.WorkList_server);
                bodyManager.SynchronousWorkList(bodyManager.WorkList_client);

                //比對清單，濾出舊資料有，而新資料不復存在的骨架點
                IEnumerable<ulong> exceptOutcome = bodyManager.WorkList_Pre.Except(bodyManager.WorkList);
                if (exceptOutcome.Count() > 0)
                {
                    //清除所有joints
                    Element[] e = new Element[ObservableJoints.Count];
                    ObservableJoints.CopyTo(e, 0);
                    foreach (Element ee in e)
                    {
                        ObservableJoints.Remove(ee);
                    }
                    //清除所有bones
                    Element[] ep = new Element[ObservablePipe.Count];
                    ObservablePipe.CopyTo(ep, 0);
                    foreach (Element ee in ep)
                    {
                        ObservablePipe.Remove(ee);
                    }

                    bodyManager.BodyState.Clear();

                    foreach (ulong l in exceptOutcome)
                    {
                        motionAnalyzeManager.RemoveMotionAnalyzer(l);
                    }

                }

                bodyManager.WorkList_Pre = new List<ulong>(bodyManager.WorkList);
            }
            #endregion

        }
        int ClientFrameCounter = 0;

        /// <summary>
        /// DrawBody3D
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="drawingPen"></param>
        /// <param name="drawStatus"></param>
        /// <param name="IsCheckTrackingState">Check body State</param>
        private void DrawBody3D(IReadOnlyDictionary<JointType, Joint> joints, Pen drawingPen, DrawStatus drawStatus, Boolean IsCheckTrackingState = true, int idx = 0)
        {
            int bonesIndex = 0;
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, bone.Item1, bone.Item2, drawingPen, drawStatus, bonesIndex + 24 * idx, IsCheckTrackingState);
                bonesIndex++;
            }

            //lock (thisLock)
            {
                foreach (JointType jointType in joints.Keys)
                {
                    Brush drawBrush = null;
                    TrackingState trackingState = joints[jointType].TrackingState;
                    if (trackingState == TrackingState.Tracked || !IsCheckTrackingState)
                    {
                        drawBrush = this.trackedJointBrush;
                    }
                    else if (trackingState == TrackingState.Inferred)
                    {
                        drawBrush = this.inferredJointBrush;
                    }

                    if (drawBrush != null)
                    {
                        if (drawStatus == DrawStatus.NewUser)
                        {
                            AddPoint(CameraSpace2Point3D(joints[jointType].Position),
                                    drawBrush,
                                    5);
                        }
                        else
                        {
                            if (drawStatus == DrawStatus.TrackedUser)
                            {
                                lock (thisLock)
                                {
                                    int JointsNumber = (int)jointType + 25 * idx;
                                    if (this.ObservableJoints.Count > JointsNumber && this.ObservableJoints.Count > 0)
                                    {
                                        this.ObservableJoints[JointsNumber].Position = CameraSpace2Point3D(joints[jointType].Position);
                                    }
                                    else
                                    {
                                        Debug.Print("Onfalse total:{0}, actual:{1}", this.ObservableJoints.Count, JointsNumber);
                                    }
                                }
                            }
                        }
                    }
                }

                //畫出joints and bone  後將此user設為Tracked
                bodyManager.BodyState[bodyManager.CurrentBodyIdx] = DrawStatus.TrackedUser;
            }
        }

        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, Pen drawingPen, DrawStatus drawStatus, int boneIndex, Boolean IsCheckTrackingState = true)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if ((joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked) && IsCheckTrackingState)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked)) && IsCheckTrackingState)
            {
                drawPen = drawingPen;
            }

            if (drawStatus == DrawStatus.NewUser)
            {
                AddPipe(CameraSpace2Point3D(joints[jointType0].Position), CameraSpace2Point3D(joints[jointType1].Position), drawingPen.Brush, (float)drawingPen.Thickness);
            }
            else if (drawStatus == DrawStatus.TrackedUser)
            {
                if (boneIndex >= ObservablePipe.Count)
                {
                    Debug.Print(">> error  DrawBone try to read  boneIndex:  {0}/{2} CurrentBodyIdx {1}", boneIndex, bodyManager.CurrentBodyIdx, ObservablePipe.Count);
                }
                else
                {

                    this.ObservablePipe[boneIndex].Point1 = CameraSpace2Point3D(joints[jointType0].Position);
                    this.ObservablePipe[boneIndex].Point2 = CameraSpace2Point3D(joints[jointType1].Position);
                }
            }
        }

        private Point3D CameraSpace2Point3D(CameraSpacePoint cameraSpacePoint)
        {
            return new Point3D(cameraSpacePoint.X * 100, cameraSpacePoint.Y * 100, cameraSpacePoint.Z * 100);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string bodyMotion = string.Empty;

        public string Lbl_MotionState
        {
            get
            {
                return bodyMotion;
            }
            set
            {
                if (bodyMotion != value)
                {
                    bodyMotion = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Lbl_MotionState"));
                    }
                }
            }
        }

        private string timeStamp = string.Empty;

        public string Lbl_TimeStamp
        {
            get
            {
                return timeStamp;
            }
            set
            {
                if (timeStamp != value)
                {
                    timeStamp = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Lbl_TimeStamp"));
                    }
                }
            }
        }


        private MotionsAnalyze motionAnalyzer = null;

        private void InitialWorkflowInstance()
        {
            motionAnalyzer = new MotionsAnalyze(this);

        }


        public void OnIdle(System.Activities.WorkflowApplicationIdleEventArgs args)
        {

            // Lbl_MotionState = motionAnalyzer.GetCurrentState();

        }

        UnhandledExceptionAction IWorkflowView.OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            return UnhandledExceptionAction.Terminate;
        }

        internal void ResetStartTime()
        {
            startTime = new TimeSpan(0, 0, 0);
        }

        internal void SetMotionToUnknown()
        {
            //motionAnalyzer.setToUnknown();
            //Lbl_MotionState = motionAnalyzer.GetCurrentState();
        }

        private MotionAnalyzeManager motionAnalyzeManager;

        internal void SetMotionUI(System.Windows.Controls.ListBox LIB_Motion)
        {
            motionAnalyzeManager = new MotionAnalyzeManager(LIB_Motion);
        }



    }
}
