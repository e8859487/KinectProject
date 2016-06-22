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
using SpaceDepolyment;
using DatabaseService;
using System.Globalization;
namespace MultipleKinectMaster3D
{
    class MasterKinectProcessor3D : INotifyPropertyChanged
    {

        #region Private Variables
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        private readonly Brush inferredJointBrush = Brushes.Yellow;

        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        private readonly List<Tuple<JointType, JointType>> bones;

        private readonly List<Pen> bodyColors;

        private Body[] bodies = null;

        private MultiSourceFrameReader multiFrameSourceReader = null;

        double floorHeight = 145;

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

        private List<SocketData> socketDataList = new List<SocketData>();

        //相機位置
        private CameraPosMapper cameraPosMapper;

        const string USERMOTIONSID = "1gDJnT4RCKXzFBHu-S60FSF_cCd_tSaa_sdPtN5FC";
        private FusionTableServices fusionTableServices;
        private int MotionId = 0;
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

            socketServer = new SocketServer(PORT, socketDataList);
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

            //DataBase
            fusionTableServices = new FusionTableServices();


        }
        private void SocketServer_changed(object sender, EventArgs e)
        {
            //lock (thisLock)
            //{
            //    //SocketPackage.SocketServer.socketData.ProcessJointsInfo();
            //    //清空使用者清單
            //    //bodyManager.WorkList_client.Clear();
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
                        // motionAnalyzer.timespan = Lbl_TimeStamp;
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
                    //設定地板平面
                    //if (bodyFrame.FloorClipPlane.W != 0)//&& !IsDeviceHeightReady
                    //{
                    //    //motionAnalyzer.SetFloorClipPlane(bodyFrame.FloorClipPlane);
                    //    Debug.Print(">> MasterKinectProcessor3D.Reader_MultiSourceFrameArrived  W : " + bodyFrame.FloorClipPlane.W.ToString());
                    //    IsDeviceHeightReady = true;
                    //}
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

        //private int IsDeviceRotationMTReady_A = 0;testg g
        //private int IsDeviceRotationMTReady_B = 0;test gg
        //private int IsDeviceRotationMTReady = 0;test gg


        int Serverframenumbers = 0;
        private void UpdateBodyFrame(Body[] bodies)
        {
            int penIndex = 0;
            int drawIdx = 0;

            bodyManager.CurrentBodyIdx = 0;

            #region 3D Server Drawer
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


                    if (!viewPointManager.IsDeviceACalculated)
                    {
                        //計算RT矩陣
                        viewPointManager.Analyze_RT_Matrix(joints[JointType.ShoulderLeft].Position, joints[JointType.ShoulderRight].Position, joints[JointType.SpineMid].Position, DEVICE_ID.DEVICE_A);
                    }
                    //IsDeviceRotationMTReady_A += 1;test gg
                    #endregion

                    if (Serverframenumbers % 4 == 0)
                    {
                        //動作追蹤系統
                        motionAnalyzeManager.AddMotionAnalyzer(bodyManager.CurrentBodyIdx);
                        motionAnalyzeManager.EventAnalyze(bodyManager.CurrentBodyIdx, MyBody.Body2Mybody(body));

                        //塞資料進資料庫
                        string startTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        Point3D center = CameraSpace2Point3D(body.Joints[JointType.SpineShoulder].Position, DEVICE_ID.DEVICE_A);
                        string XYZ = string.Format("({0},{1},{2})", center.X, center.Y, center.Z);
                        string Motion = motionAnalyzeManager.motionDict[bodyManager.CurrentBodyIdx].CurrentMotions;
                        string newData = string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", MotionId, startTime, "", Motion, XYZ, "RoomA", bodyManager.CurrentBodyIdx);
                        fusionTableServices.Insert(USERMOTIONSID, @"(MotionId,StartTime,EndTime,Motion,Coordinate,RoomId,UserInfo)", newData);
                        MotionId++;
                    }

                    //開始畫3D骨架
                    DrawBody3D(joints, drawPen, bodyManager.BodyState[bodyManager.CurrentBodyIdx], DEVICE_ID.DEVICE_A, true, drawIdx);

                    drawIdx++;
                }

                //?更新人體動作 it seems no need to perform
                foreach (ulong l in motionAnalyzeManager.motionDict.Keys)
                {
                    motionAnalyzeManager.motionDict[l].UpdateCurrentState();
                }

            }
            Serverframenumbers++;
            #endregion

            #region 3D Client Drawer

            //清空使用者清單
            bodyManager.WorkList_client.Clear();
            lock (thisLock)
            {
                //處理不同的 client 資料
                for (int socketClientIdx = 0; socketClientIdx < socketDataList.Count; socketClientIdx++)
                {

                    for (int i = 0; i < 7; i++)
                    {
                        MyBody body = socketDataList[socketClientIdx].Body[i];
                        //畫出3D骨架

                        //處理骨架字串
                        socketDataList[socketClientIdx].ProcessJointsInfo();
                        //SocketPackage.SocketServer.socketData.ProcessJointsInfo();

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
                            if (!viewPointManager.IsDeviceBCalculated)
                            {
                                viewPointManager.Analyze_RT_Matrix(body.jointsInfo[JointType.ShoulderLeft].Position, body.jointsInfo[JointType.ShoulderRight].Position, body.jointsInfo[JointType.SpineMid].Position, DEVICE_ID.DEVICE_B);
                            }

                            if (viewPointManager.IsDeviceACalculated && viewPointManager.IsDeviceBCalculated)
                            {
                                if (!viewPointManager.IsRTMatrixCalculated)
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
                                //viewPointManager.displayMatrix();
                                //轉換後的座標點
                                this.DrawBody3D(joints_transfered,
                                    clientPen,
                                    bodyManager.BodyState[bodyManager.CurrentBodyIdx],
                                    (DEVICE_ID)socketDataList[socketClientIdx].DeviceSerialNum,
                                    true,
                                    drawIdx);
                                //Debug.Print("hello he;lkdsfj;ksafdj;ldsakfjlsa;kdfjlakdsfjlksdfjlskdfjslkdfjlskdfjlskdfj");
                            }
                            else
                            {
                                //未轉換的座標點
                                this.DrawBody3D(body.jointsInfo,
                                    clientPen,
                                    bodyManager.BodyState[bodyManager.CurrentBodyIdx],
                                    (DEVICE_ID)socketDataList[socketClientIdx].DeviceSerialNum,
                                    true,
                                    drawIdx);
                            }
                            #endregion

                            drawIdx++;

                            //Motion Detect!!
                            if (socketDataList[socketClientIdx].Framenumbers % 4 == 0)
                            {
                                motionAnalyzeManager.motionDict[body.TrackingId].EventAnalyze(body);

                                if (joints_transfered.Count > 0)
                                {
                                    //塞資料進資料庫
                                    string startTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    Point3D center = CameraSpace2Point3D(joints_transfered[JointType.SpineShoulder].Position, (DEVICE_ID)socketDataList[socketClientIdx].DeviceSerialNum);

                                    string XYZ = string.Format("({0},{1},{2})", center.X, center.Y, center.Z);
                                    string Motion = motionAnalyzeManager.motionDict[bodyManager.CurrentBodyIdx].CurrentMotions;
                                    string newData = string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", MotionId, startTime, "", Motion, XYZ, Enum.GetName(typeof(DEVICE_ID), socketDataList[socketClientIdx].DeviceSerialNum), bodyManager.CurrentBodyIdx);
                                    fusionTableServices.Insert(USERMOTIONSID, @"(MotionId,StartTime,EndTime,Motion,Coordinate,RoomId,UserInfo)", newData);
                                    MotionId++;
                                }
                                else
                                {
                                    string startTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    Point3D center = CameraSpace2Point3D(body.jointsInfo[JointType.SpineShoulder].Position, (DEVICE_ID)socketDataList[socketClientIdx].DeviceSerialNum);
                                    string XYZ = string.Format("({0},{1},{2})", center.X, center.Y, center.Z);
                                    string Motion = motionAnalyzeManager.motionDict[bodyManager.CurrentBodyIdx].CurrentMotions;
                                    string newData = string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", MotionId, startTime, "", Motion, XYZ, Enum.GetName(typeof(DEVICE_ID), socketDataList[socketClientIdx].DeviceSerialNum), bodyManager.CurrentBodyIdx);
                                    fusionTableServices.Insert(USERMOTIONSID, @"(MotionId,StartTime,EndTime,Motion,Coordinate,RoomId,UserInfo)", newData);
                                    MotionId++;
                                }
                            }
                        }
                    }
                    socketDataList[socketClientIdx].Framenumbers++;
                }
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

                    //移除舊資料有，而新資料不復存在的ID
                    foreach (ulong l in exceptOutcome)
                    {
                        motionAnalyzeManager.RemoveMotionAnalyzer(l);
                    }

                }

                bodyManager.WorkList_Pre = new List<ulong>(bodyManager.WorkList);
            }
            #endregion
        }

        /// <summary>
        /// DrawBody3D
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="drawingPen"></param>
        /// <param name="drawStatus"></param>
        /// <param name="IsCheckTrackingState">Check body State</param>
        private void DrawBody3D(IReadOnlyDictionary<JointType, Joint> joints, Pen drawingPen, DrawStatus drawStatus, DEVICE_ID device_id, Boolean IsCheckTrackingState = true, int idx = 0)
        {
            int bonesIndex = 0;
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, bone.Item1, bone.Item2, drawingPen, drawStatus, bonesIndex + 24 * idx, device_id, IsCheckTrackingState);
                bonesIndex++;
            }

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
                        AddPoint(CameraSpace2Point3D(joints[jointType].Position, device_id), drawBrush, 5);
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
                                    this.ObservableJoints[JointsNumber].Position = CameraSpace2Point3D(joints[jointType].Position, device_id);
                                }
                                else
                                {
                                    Debug.Print("Onfalse total:{0}, actual:{1}", this.ObservableJoints.Count, JointsNumber);
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
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, Pen drawingPen, DrawStatus drawStatus, int boneIndex, DEVICE_ID device_id, Boolean IsCheckTrackingState = true)
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
                AddPipe(CameraSpace2Point3D(joints[jointType0].Position, device_id), CameraSpace2Point3D(joints[jointType1].Position, device_id), drawingPen.Brush, (float)drawingPen.Thickness);
            }
            else if (drawStatus == DrawStatus.TrackedUser)
            {
                if (boneIndex >= ObservablePipe.Count)
                {
                    Debug.Print(">> error  DrawBone try to read  boneIndex:  {0}/{2} CurrentBodyIdx {1}", boneIndex, bodyManager.CurrentBodyIdx, ObservablePipe.Count);
                }
                else
                {
                    this.ObservablePipe[boneIndex].Point1 = CameraSpace2Point3D(joints[jointType0].Position, device_id);
                    this.ObservablePipe[boneIndex].Point2 = CameraSpace2Point3D(joints[jointType1].Position, device_id);
                }
            }
        }

        /// <summary>
        /// 轉換 CameraSpacePoint to Point3D, 做相對應的floor mapping
        /// </summary>
        /// <param name="cameraSpacePoint"></param>
        /// <returns></returns>
        private Point3D CameraSpace2Point3D(CameraSpacePoint cameraSpacePoint, DEVICE_ID device_id)
        {

            if (device_id == DEVICE_ID.DEVICE_A )
            {
                Matrix3D m = Matrix3D.Identity;

                Quaternion q = new Quaternion(new Vector3D(0, 1, 0), cameraPosMapper.GetCameraSetting(1).Angle);

                m.Rotate(q);

                double x = cameraSpacePoint.X * 100;
                double y = cameraSpacePoint.Y * 100;
                double z = cameraSpacePoint.Z * 100;

                Vector3D v = new Vector3D(x, y, z);

                Vector3D result = Vector3D.Multiply(v, m);

                return new Point3D(result.X + cameraPosMapper.GetCameraSetting(1).Position.X
                    , result.Y + cameraPosMapper.GetCameraSetting(1).Position.Y
                    , result.Z + cameraPosMapper.GetCameraSetting(1).Position.Z);

            }
            else if( device_id == DEVICE_ID.DEVICE_B){
                Matrix3D m = Matrix3D.Identity;

                Quaternion q = new Quaternion(new Vector3D(0, 1, 0), cameraPosMapper.GetCameraSetting(2).Angle);

                m.Rotate(q);

                double x = cameraSpacePoint.X * 100;
                double y = cameraSpacePoint.Y * 100;
                double z = cameraSpacePoint.Z * 100;

                Vector3D v = new Vector3D(x, y, z);

                Vector3D result = Vector3D.Multiply(v, m);

                return new Point3D(result.X + cameraPosMapper.GetCameraSetting(2).Position.X
                    , result.Y + cameraPosMapper.GetCameraSetting(2).Position.Y
                    , result.Z + cameraPosMapper.GetCameraSetting(2).Position.Z);
            }
            else if (device_id == DEVICE_ID.DEVICE_C)
            {
                Matrix3D m = Matrix3D.Identity;

                Quaternion q = new Quaternion(new Vector3D(0, 1, 0), cameraPosMapper.GetCameraSetting(3).Angle);

                m.Rotate(q);

                double x = cameraSpacePoint.X * 100;
                double y = cameraSpacePoint.Y * 100;
                double z = cameraSpacePoint.Z * 100;

                Vector3D v = new Vector3D(x, y, z);

                Vector3D result = Vector3D.Multiply(v, m);

                //return new Point3D(result.X + cameraPosMapper.GetCameraSetting(3).Position.X
                //    , result.Y + cameraPosMapper.GetCameraSetting(3).Position.Y
                //    , result.Z + cameraPosMapper.GetCameraSetting(3).Position.Z
                //    );
                return new Point3D(result.X + cameraPosMapper.GetCameraSetting(3).Position.X
                , result.Y + cameraPosMapper.GetCameraSetting(3).Position.Y
                , result.Z + cameraPosMapper.GetCameraSetting(3).Position.Z
                );
            }

            Debug.Print("MasterKinectProcessor3D Cameraspace2Point3D occur error");
            return new Point3D(cameraSpacePoint.X * 100 + this.floorHeight, cameraSpacePoint.Y * 100 + this.floorHeight, cameraSpacePoint.Z * 100 + this.floorHeight);

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


        //綁定UI動作的listbox
        internal void SetMotionUI(System.Windows.Controls.ListBox LIB_Motion)
        {
            motionAnalyzeManager = new MotionAnalyzeManager(LIB_Motion);
        }



        /// <summary>
        /// Asychronous play
        /// </summary>
        internal void SendStartAsychronousPlay()
        {
            foreach (var socketdata in this.socketDataList)
            {
                socketdata.playbackStatus.SocketStatus = TRANSMIT_STATUS.StartPlaybackClip;
            }
        }


        internal void SetCameraDeplyment(CameraPosMapper cameraPosMapper)
        {
            this.cameraPosMapper = cameraPosMapper;

        }
    }
}
