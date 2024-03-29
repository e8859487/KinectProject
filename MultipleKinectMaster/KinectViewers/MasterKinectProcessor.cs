﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SocketPackage;
using System.Windows.Threading;
using System.IO;

using Microsoft.Kinect;

using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

using log4net;
using log4net.Config;

namespace MultipleKinectMaster
{
    public sealed class MasterKinectProcessor : IDisposable, INotifyPropertyChanged
    {


        #region Private Region Parameters

        private Body[] bodies = null;

        private const double JointThickness = 3;

        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        private readonly Brush inferredJointBrush = Brushes.Yellow;

        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);


        private DrawingGroup drawingGroup;

        private DrawingImage imageSource;

        private CoordinateMapper coordinateMapper = null;

        private List<Tuple<JointType, JointType>> bones;

        private int displayWidth;

        private int displayHeight;

        private List<Pen> bodyColors;

        private WriteableBitmap depthBitmap = null;
        private byte[] depthPixels = null;

        private WriteableBitmap depthBitmapClient = null;
        private byte[] depthPixelsClient = null;

        private const int MapDepthToByte = 8000 / 256;

        FrameDescription frameDescription = null;
        private MultiSourceFrameReader multiFrameSourceReader = null;

        //Socket Server declare:
        private SocketServer _socketServer = null;

        private const int _Port = 81;


        //skeleton joints information
        public string B_Head1 = string.Empty;

        //Total Frame
        public int B_FrameNumbers1 = 0;
        //Frame Rate
        public int B_FrameNumbers1_average = 0;
        //Timer to calculate the Frame Rate
        System.Diagnostics.Stopwatch swTimer = new System.Diagnostics.Stopwatch();
        

        public string txb2_skeletonInfo = string.Empty;

        //Log Declare
        private ILog log = null;

        Dispatcher _dispatcher;
        #endregion


        public MasterKinectProcessor(KinectSensor kinectSensor,Dispatcher guiDispatcher)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            _dispatcher = guiDispatcher;

            //Socket Server Initial
            _socketServer = new SocketServer(_Port);
            _socketServer.Start();
            _socketServer.changed += new changeEventHandler(_socketServer_changed);
         
            //multipleFrameReader 
            this.multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Body);
            this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            this.coordinateMapper = kinectSensor.CoordinateMapper;

            frameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            //window size : 512*424
            this.displayHeight = frameDescription.Height;   //424
            this.displayWidth = frameDescription.Width;     //512

            // allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.displayWidth * this.displayHeight];
            //create bitmap to display
            this.depthBitmap = new WriteableBitmap(this.displayWidth, this.displayHeight, 96.0, 96.0, PixelFormats.Gray8, null);


            // allocate space to put the pixels being received and converted        [Client]
            this.depthPixelsClient = new byte[this.displayWidth * this.displayHeight];
            //create bitmap to display
            this.depthBitmapClient = new WriteableBitmap(this.displayWidth, this.displayHeight, 96.0, 96.0, PixelFormats.Gray8, null);


            this.bones = new List<Tuple<JointType, JointType>>();
            #region bone implement

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

            this.bodyColors = new List<Pen>();
            #region bodyColors implements

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));
            #endregion

            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);

            //Log Setting
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Motion Detector Server Start!!");
            //Timer start 
            swTimer.Start();


        }

        void _socketServer_changed(object sender, EventArgs e)
        {
            if (SocketPackage.SocketServer.imgObj != null && SocketPackage.SocketServer.imgObj.ImgBuffer != null)
            {
                depthPixelsClient = SocketPackage.SocketServer.imgObj.ImgBuffer;
                Action GUIDelegateDepthImg = delegate()
                {
                    this.depthBitmapClient.WritePixels(
                        new Int32Rect(0, 0, this.depthBitmapClient.PixelWidth, this.depthBitmapClient.PixelHeight),
                        this.depthPixelsClient,
                        this.depthBitmapClient.PixelWidth,
                        0);
                };
                this._dispatcher.BeginInvoke(GUIDelegateDepthImg);

                //預處理骨架資料
                SocketPackage.SocketServer.imgObj.processJoints();

                //
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < 7; i++)
                {
                    MyBody _body = SocketPackage.SocketServer.imgObj.Body[i];

                    if (_body.isTracked)
                    {
                        sb.Append(string.Format("User : {0} {1}:({2},{3},{4})\n", 
                            _body.userId, 
                            JointType.Head.ToString(), 
                            (int)(_body.joints[JointType.Head].X * 100),
                            (int)(_body.joints[JointType.Head].Y * 100),
                            (int)(_body.joints[JointType.Head].Z * 100)
                            ));
                    }
                }

                //顯示到UI上
                Action GUIjointInfoDelegate = delegate()
                {
                    Txb2SkeletonInfo = sb.ToString();
                };
                this._dispatcher.BeginInvoke(GUIjointInfoDelegate);
            }
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

        #region GUI Commands
        public void StartAsychronousRecord()
        {
            _socketServer.SendStartAsychronousRecord();
        }

        public void StopAsychronousRecord()
        {
            _socketServer.SendStopAsychronousRecord();
        }

        public void StartAsychronousPlay()
        {
            SocketServer.SendStartAsychronousPlay();
            //_socketServer.SendStartAsychronousPlay();
        }

        public void StopAsychronousPlay()
        {
            _socketServer.SendStopAsychronousPlay();
        }

        internal void testMotion()
        {



        }
        #endregion

        #region GUI Getter and Setter

        public ImageSource ImageSourceMaster
        {
            get
            {
                return this.imageSource;
                //return this.depthBitmap;
            }
        }

        public ImageSource ImageSourceClient
        {
            get
            {
                //  return this.imageSource;
                return this.depthBitmapClient;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public string Txb_SkeletonInfo
        {
            get
            {
                return this.B_Head1;
            }
            set
            {
                if (B_Head1 != value)
                {
                    B_Head1 = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Txb_SkeletonInfo"));
                    }
                }
            }
        }

        public string FrameNumbers1
        {
            get
            {
                return this.B_FrameNumbers1_average.ToString();
            }
            set
            {
                if (B_FrameNumbers1_average != int.Parse(value))
                {
                    B_FrameNumbers1_average = int.Parse(value);
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this,new PropertyChangedEventArgs("FrameNumbers1"));
                    }
                }
            }
        }

        public string Txb2SkeletonInfo 
        {
            get
            {
                return this.txb2_skeletonInfo;
            }
            set
            {
                if (txb2_skeletonInfo != value)
                {
                    txb2_skeletonInfo = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Txb2SkeletonInfo"));
                    }
                }
            }
        }

        private string txb_Motions = string.Empty;

        public string Txb_Motions{
            get{
                return this.txb_Motions;
            }
            set{
                if(txb_Motions != value){
                    txb_Motions = value;
                    if(PropertyChanged !=null){
                        this.PropertyChanged(this,new PropertyChangedEventArgs("Txb_Motions"));
                    }
                }
            }
        }

        #endregion

        #region private Method

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            DepthFrame depthFrame = null;
            BodyFrame bodyFrame = null;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            //bool isBitmapLocked = false;
            bool dataReceived = false;
            bool depthFrameProcessed = false;
            // If the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }
            try
            {
                #region Process Depth Frame
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();

                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.frameDescription.Width * this.frameDescription.Height) == (depthBuffer.Size / this.frameDescription.BytesPerPixel)) &&
                            (this.frameDescription.Width == this.depthBitmap.PixelWidth) && (this.frameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance

                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
                if (depthFrameProcessed)
                {
                    this.RenderDepthPixels();
                }
                #endregion

                #region Process BodyFrame
                //Record skeleton Relative timespan
                bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame();
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
                if (dataReceived)
                {
                    this.UpdateBodyFrame(this.bodies);
                }

                #endregion

                //Update GUI Frame numbers, count total Frames and display it.
                B_FrameNumbers1++;
                if (B_FrameNumbers1 > 30)
                    FrameNumbers1 = string.Format("{0}", (int)(B_FrameNumbers1 / swTimer.Elapsed.TotalSeconds));


            }
            finally
            {
                //if (isBitmapLocked)
                //{
                //    //this.bitmap.Unlock();
                //}
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

        private void UpdateBodyFrame(Body[] bodies)
        {
            if (bodies != null)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    dc.DrawImage(depthBitmap, new System.Windows.Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight));
                    int penIndex = 0;

                    //Skeleton Joints Information  (Device - master )
                    StringBuilder sb_SkeletonJointInfo1 = new StringBuilder();
                    foreach (Body body in bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];
                        if (body.IsTracked)
                        {
                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            sb_SkeletonJointInfo1.Append(string.Format("User : {0} ",body.TrackingId));

                            foreach (JointType jointType in joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = 0.1f;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);

                                if (jointType == JointType.Head)
                                {
                                    sb_SkeletonJointInfo1.Append(string.Format("{0}:({1},{2},{3})", jointType, (int)(position.X * 100), (int)(position.Y * 100), (int)(position.Z * 100)));
                                }
                            }
                            //區別不同人
                            sb_SkeletonJointInfo1.Append("\n");
                            //Dispaly to GUI
                            Txb_SkeletonInfo = sb_SkeletonJointInfo1.ToString();
                    
                            this.DrawBody(joints, jointPoints, dc, drawPen);
                        }
                    }


                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;
                TrackingState trackingState = joints[jointType].TrackingState;
                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }

        }

        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.frameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
        }



        
        #endregion

    }
}
