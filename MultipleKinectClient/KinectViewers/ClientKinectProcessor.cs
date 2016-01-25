using Microsoft.Kinect;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SocketPackage;
using Img_Serializable;
using XmlManager;

using System.Windows.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using System.ComponentModel;

using log4net;
using log4net.Config;


namespace MultipleKinectClient
{
    public delegate void clientRecordEventHandler(object sender, StatusEventArgs e);

    public   class ClientKinectProcessor : IDisposable,INotifyPropertyChanged
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

        //Display window width
        private int displayWidth;

        //Display window Height
        private int displayHeight;

        private List<Pen> bodyColors;

        private WriteableBitmap depthBitmap = null;
        private byte[] depthPixels = null;

        private const int MapDepthToByte = 8000 / 256;

        FrameDescription frameDescription = null;
        private MultiSourceFrameReader multiFrameSourceReader = null;

        //定義客戶端傳輸socket
        SocketClient socketClient = null;
        private  string IpAddr = null;
        private int Port;

        int stride = 0;
        byte[] imgBuffer = null;
        
       // double depthTimeStamp = 0;
        int framesNumber = 0;

        /// <summary>
        /// 記錄當下偵測到的人體骨架數量
        /// </summary>
        int bodyNumbers = 0;

        //記錄當下偵測到的人體骨架編號
        string bodyIndex = string.Empty;

        ImageInfo_Serializable ImgObj = null;
        Dictionary<int, string> _JointsPosDict = new Dictionary<int, string>(); 

        //用來評估程式碼計算的時間
        System.Diagnostics.Stopwatch swTimer = new System.Diagnostics.Stopwatch();

        //Log Declare
        private ILog log = null;

 

        #endregion

        public ClientKinectProcessor(KinectSensor kinectSensor)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            //multipleFrameReader Read Depth and Body infomation.
            this.multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Body);

            this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            this.coordinateMapper = kinectSensor.CoordinateMapper;

            frameDescription = kinectSensor.DepthFrameSource.FrameDescription;

            //window size : 512*424
            this.displayHeight = kinectSensor.DepthFrameSource.FrameDescription.Height;   //424
            this.displayWidth = kinectSensor.DepthFrameSource.FrameDescription.Width;     //512


            // Allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.displayWidth * this.displayHeight];

            //create bitmap to display
            this.depthBitmap = new WriteableBitmap(this.displayWidth, this.displayHeight, 96.0, 96.0, PixelFormats.Gray8, null);

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


            //network processes
            //= "140.118.170.215";              //127.0.0.1  me 140.118.170.215  wai 140.118.170.214
            XmlReader reader = new XmlReader(@"./Setting.xml");
            IpAddr = reader.getNodeInnerText(@"/Root/IPAddress");
            Port = int.Parse(reader.getNodeInnerText(@"/Root/Port"));
            this.socketClient = new SocketClient(IpAddr, Port);
            this.socketClient.Connect();
            this.socketClient.clientDataChanged += new clientDataChangeEventHandler(StatusChange);



            // Stride = (Width) * (bytes per pixel)
            this.stride = (int)depthBitmap.PixelWidth * ((depthBitmap.Format.BitsPerPixel + 7) / 8);//每列影像列的位元長度(bytes)
            this.imgBuffer = new byte[(int)depthBitmap.PixelHeight * stride];

            //serializable Image object . Including depth and skeleton joints. 
            //ImgObj   size :　depthBitmap.PixelHeight * stride
            ImgObj = new ImageInfo_Serializable(1);

            //Log Setting
            XmlConfigurator.Configure(new System.IO.FileInfo(@"./config.xml"));
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Motion Detector Client Start!!");


        }

        #region CallBack Event and Event handler

        private void StatusChange(object sender, StatusEventArgs e)
        {
            //Status changed
            OnChanged(e);
        }

        public event clientRecordEventHandler changed;

        protected virtual void OnChanged(StatusEventArgs e)
        {
            if (changed != null)
            {
                changed(this, e);
            }
        }

        #endregion

        private byte[] CombomBinaryArray(byte[] srcArray1, byte[] srcArray2)
        {
            //根據要合併的兩個byte陣列長度建立一個新的byte[]
            byte[] newArray = new byte[srcArray1.Length + srcArray2.Length];

            //把第一個陣列複製到新的陣列
            Array.Copy(srcArray1, 0, newArray, 0, srcArray1.Length);

            //把第二個陣列貼到第一個陣列之後
            Array.Copy(srcArray2, 0, newArray, srcArray1.Length, srcArray2.Length);

            return newArray;
        }

        /// <summary>
        /// Send All Data to Server to server va network
        /// </summary>
        public void SendDataToServer()
        {
            ImgObj._imageIdx = framesNumber;

            byte[] bodyNumbers_byte = System.Text.Encoding.UTF8.GetBytes(bodyNumbers.ToString());
            
            //深度 + 人體骨架數量
            byte[] Depth_BodyNum_byte = CombomBinaryArray(depthPixels, bodyNumbers_byte);

            //深度 + 人體骨架數量 + 骨架點
            byte[] Depth_BodyNum_Skeleton_byte = Depth_BodyNum_byte;

            byte[] bodySkeletons_byteArray =null;

            if (bodyNumbers > 0)
            {
                //找出人體的index
                string[] dictKey = bodyIndex.Split(',');

                if (dictKey != null && dictKey.Length > 1)
                {
                    for (int i = 0; i < dictKey.Length - 1; i++)
                    {
                        //骨架位置字串
                        byte[] skeleton_Joints_byte =  System.Text.Encoding.UTF8.GetBytes(_JointsPosDict[int.Parse(dictKey[i])]);

                        //骨架字串的長度
                        byte[] str_SkeletonJoints_Len_byte = System.Text.Encoding.UTF8.GetBytes(_JointsPosDict[int.Parse(dictKey[i])].Length.ToString());

                        //
                        if (bodySkeletons_byteArray == null) {
                            bodySkeletons_byteArray = CombomBinaryArray(str_SkeletonJoints_Len_byte, skeleton_Joints_byte);
                        }
                        else {
                            bodySkeletons_byteArray = CombomBinaryArray(bodySkeletons_byteArray, CombomBinaryArray(str_SkeletonJoints_Len_byte, skeleton_Joints_byte));
                        }
                    }
                }
            }
            //沒有任何人體骨架
            if (bodySkeletons_byteArray == null)
            {
                //送出深度影像 + 骨架數量(0)
                socketClient.SendBytes(Depth_BodyNum_byte);
            }
            else  //一個以上的人體骨架
            {
                Depth_BodyNum_Skeleton_byte = CombomBinaryArray(Depth_BodyNum_Skeleton_byte, bodySkeletons_byteArray);
                //送出 深度影像 + 骨架數量(n) + (骨架字串長度 +  骨架字串) * n
                socketClient.SendBytes(Depth_BodyNum_Skeleton_byte);

            }
        }

        public void CatchImgBitmap()
        {
            //測試程式碼消耗時間
            swTimer.Reset();
            swTimer.Start();
            SendDataToServer();
       
            swTimer.Stop();
            string consumeTime = swTimer.Elapsed.TotalMilliseconds.ToString();
            MessageBox.Show("消耗時間為:" + consumeTime);
        }

        public void RenderBitmap()
        {
 
        }

        public ImageSource ImageSource
        {
            get
            {
                 return this.imageSource;
                //return this.depthBitmap;
            }
        }

        #region GUI Getter and Setter
        public event PropertyChangedEventHandler PropertyChanged;


        public string Bodyindex
        {
            get
            {
                return this.bodyIndex;
            }
            set
            {
                if (bodyIndex != value)
                {
                    bodyIndex = value;
                    if (PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Bodyindex"));
                    }
                }
            }
        } 
        #endregion


        public void Dispose()
        {
            if (this.multiFrameSourceReader != null)
            {
                this.multiFrameSourceReader.MultiSourceFrameArrived -= Reader_MultiSourceFrameArrived;
                this.multiFrameSourceReader.Dispose();
                this.multiFrameSourceReader = null;
            }

            socketClient.DisCounect();
        }

        #region Private Method
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            DepthFrame depthFrame = null;
            BodyFrame bodyFrame = null;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            bool dataReceived = false;
            bool depthFrameProcessed = false;

            // If the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }

            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        //if (((this.frameDescription.Width * this.frameDescription.Height) == (depthBuffer.Size / this.frameDescription.BytesPerPixel)) &&
                        // (this.frameDescription.Width == this.depthBitmap.PixelWidth) && (this.frameDescription.Height == this.depthBitmap.PixelHeight))
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

            }

            //Send image Obj to master at 15 FPS
           //  if (framesNumber % 2  == 0)
            {
                //Send Image to Server
                this.SendDataToServer();
            }
            framesNumber++;
        }

        private void UpdateBodyFrame(Body[] bodies)
        {
            if (bodies != null)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    dc.DrawImage(depthBitmap, new System.Windows.Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight));


                    //顯示當下使用者編號於視窗中。
                    string _bodyIndex = string.Empty;

                    //foreach counter(LoopIndex)
                    int BodyIndex = 0;

                    bodyNumbers = 0;

                    StringBuilder sb_skeletonJoints ;
                    Dictionary<JointType, Point> jointPoints = null;
                    foreach (Body body in bodies)
                    {
                        Pen drawPen = this.bodyColors[BodyIndex++];
                        ImgObj._SBodyJoints[BodyIndex] = "";
                        sb_skeletonJoints = new StringBuilder();
                        int testnumber = 0;
                        if (body.IsTracked)
                        {
                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            // convert the joint points to depth (display) space
                             jointPoints = new Dictionary<JointType, Point>();
                           
                            bodyNumbers++;
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

                                //紀錄骨架座標於string中  jointType == JointType.ShoulderRight || jointType == JointType.ShoulderLeft || jointType == JointType.Head
                               // if(testnumber < 12)
                                {sb_skeletonJoints.Append(string.Format("({0},{1},{2})|", position.X.ToString("0.0000"), position.Y.ToString("0.0000"), position.Z.ToString("0.0000")));
                                
                                }
                                testnumber++;
                            }

                            //將joints點存入dictionary中準備傳到server
                            if (!_JointsPosDict.ContainsKey(BodyIndex))
                            {
                                _JointsPosDict.Add(BodyIndex, sb_skeletonJoints.ToString());
                            }
                            else
                            {
                                _JointsPosDict[BodyIndex] = sb_skeletonJoints.ToString();
                               
                            }
                            sb_skeletonJoints.Clear();
                            _bodyIndex = _bodyIndex + BodyIndex.ToString()+",";

                            this.DrawBody(joints, jointPoints, dc, drawPen);
                        }//if (body.IsTracked) End

                    }

                    //更新使用者視窗tracked人數textbox
                    Bodyindex = _bodyIndex;
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
