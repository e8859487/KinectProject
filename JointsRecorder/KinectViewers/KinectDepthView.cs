//------------------------------------------------------------------------------
// <copyright file="KinectDepthView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace JointsRecorder
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.ComponentModel;

    using SocketPackage;

    /// <summary>
    /// Visualizes the Kinect Depth stream for display in the UI
    /// </summary>
    public sealed class KinectDepthView : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private MultiSourceFrameReader multiFrameSourceReader = null;

        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] depthPixels = null;

        private DrawingGroup drawingGroup;

        private DrawingImage imageSource;

        private List<Tuple<JointType, JointType>> bones;

        private List<Pen> bodyColors;

        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        private Body[] bodies = null;

        private const double JointThickness = 3;

        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        private readonly Brush inferredJointBrush = Brushes.Yellow;

        FrameDescription frameDescription = null;

        private CoordinateMapper coordinateMapper = null;

        //Display window width
        private int displayWidth;

        //Display window Height
        private int displayHeight;

        /// <summary>
        /// 使用者編號 / 骨架座標
        /// </summary>
        Dictionary<int, MyBody> _JointsPosDict = new Dictionary<int, MyBody>();

        CSV_Writter csv_Writter = null;


        /// <summary>
        /// 紀錄是否需要刷新骨架資料
        /// </summary>
        Boolean FrameRecordFlag = true;

        /// <summary>
        /// 上一幀骨架資料
        /// </summary>
        MyBody[] preFrameBody = null;

        /// <summary>
        /// 當下要輸出的骨架資料
        /// </summary>
        MyBody[] nowFrameBody = null;

        /// <summary>
        /// 目前幀數
        /// </summary>
        int frames = 0;




        /// <summary>
        /// Initializes a new instance of the KinectDepthView class
        /// </summary>
        /// <param name="kinectSensor">Active instance of the Kinect sensor</param>
        public KinectDepthView(KinectSensor kinectSensor)
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

            // get FrameDescription from DepthFrameSource
            this.depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;

            // allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            // create the bitmap to display
            this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);



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

            csv_Writter = new CSV_Writter(@".\skeletons.csv");

            preFrameBody = new MyBody[6];
            nowFrameBody = new MyBody[6];
            for (int i = 0; i < preFrameBody.Length; i++)
            {
                preFrameBody[i] = new MyBody();
                preFrameBody[i].init();

                nowFrameBody[i] = new MyBody();
                nowFrameBody[i].init();
            }
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;

                //return this.depthBitmap;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private string frameTimeSpan = string.Empty;
        /// <summary>
        /// 顯示於GUI上
        /// </summary>
        public string FrameTimeSpan
        {
            get
            {
                return this.frameTimeSpan;
            }
            set
            {
                if (frameTimeSpan != value)
                {
                    frameTimeSpan = value;
                    if (PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("FrameTimeSpan"));
                    }
                }
            }
        }



        /// <summary>
        /// Disposes the DepthFrameReader
        /// </summary>
        public void Dispose()
        {
            if (this.multiFrameSourceReader != null)
            {
                this.multiFrameSourceReader.MultiSourceFrameArrived -= Reader_MultiSourceFrameArrived;
                this.multiFrameSourceReader.Dispose();
                this.multiFrameSourceReader = null;
            }
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
                    frames++;

                    FrameTimeSpan = depthFrame.RelativeTime.ToString();
                    //display timespan to screen
                    //write in datas into csv
                    if (frames % 30 == 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        int i = 0;
                        foreach (MyBody mb in nowFrameBody)
                        {
                            sb.Clear();
                            if (mb.isTracked == true)
                            {
                                sb.Append(FrameTimeSpan + ",");
                                sb.Append(mb.userId + ",,");
                                foreach (myCameraSpacePoint csp in mb.joints.Values)
                                {
                                    sb.Append(string.Format("{0},", csp.joints_XYZ));
                                }

                                sb.Append(",");
                                //計算兩frame之差
                                if (preFrameBody[i].isTracked == true)
                                {
                                    foreach (JointType jt in preFrameBody[i].joints.Keys)
                                    {
                                        float distance = (float)Math.Pow((float)
                                            (
                                            Math.Pow(preFrameBody[i].joints[jt].X - mb.joints[jt].X, 2) +
                                            Math.Pow(preFrameBody[i].joints[jt].Y - mb.joints[jt].Y, 2) +
                                            Math.Pow(preFrameBody[i].joints[jt].Z - mb.joints[jt].Z, 2)
                                            ), 0.5);
                                        sb.Append(string.Format("{0},", distance.ToString()));    
                                    }
                                }


                                //write into csv file
                                csv_Writter.WriteLine(sb.ToString());

                                //deepcopy data as pre frame
                                foreach (JointType jp in mb.joints.Keys)
                                {

                                    preFrameBody[i].joints[jp] = new myCameraSpacePoint(mb.joints[jp].joints_XYZ);
                                }
                                preFrameBody[i].userId = mb.userId;
                            }
                            preFrameBody[i].isTracked = mb.isTracked;

                            i++;

                            //代表資料已寫出
                            FrameRecordFlag = true;
                        }


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

                    //foreach counter(LoopIndex)
                    int BodyIndex = 0;

                    Dictionary<JointType, Point> jointPoints = null;

                    foreach (Body body in bodies)
                    {
                        Pen drawPen = this.bodyColors[BodyIndex++];
                        if (body.IsTracked)
                        {
                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            // convert the joint points to depth (display) space
                            jointPoints = new Dictionary<JointType, Point>();

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

                                if (FrameRecordFlag == true)
                                {
                                    //全部預設為false;
                                    foreach (MyBody mb in nowFrameBody)
                                    {
                                        mb.isTracked = false;
                                    }

                                    nowFrameBody[BodyIndex].isTracked = true;
                                    //紀錄人體編號
                                    nowFrameBody[BodyIndex].userId = body.TrackingId.ToString();
                                    nowFrameBody[BodyIndex].joints[jointType] = new myCameraSpacePoint(string.Format("{0},{1},{2}", position.X.ToString("0.0000"), position.Y.ToString("0.0000"), position.Z.ToString("0.0000")), true);
                                }
                            }
                            FrameRecordFlag = false;

                            this.DrawBody(joints, jointPoints, dc, drawPen);
                        }//if (body.IsTracked) End

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
