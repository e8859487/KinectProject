﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RBG_D_MOST_Project_K4Wv2_WPF_Depth_BodyIX_TwoDevice
{
    public sealed class KinectBodyView : IDisposable
    {
        private BodyFrameReader bodyFrameReader = null;

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

        //RenderTargetBitmap bitmap = null;

        private const int MapDepthToByte = 8000 / 256;

        FrameDescription frameDescription = null;
        private MultiSourceFrameReader multiFrameSourceReader = null;

        private bool ListenerState = true;

        KinectSensor myKinectSensor = null;
        
        public KinectBodyView(KinectSensor kinectSensor)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
            //set Machine Index
            myKinectSensor = kinectSensor;

            //this.bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
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
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            //Check if this frame belong to me.
            DepthFrame depthFrame = null;
            BodyFrame bodyFrame = null;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            bool isBitmapLocked = false;
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
                if (isBitmapLocked)
                {
                    //this.bitmap.Unlock();
                }
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

        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
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

        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }
            if (dataReceived)
            {
                this.UpdateBodyFrame(this.bodies);
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
                    foreach (Body body in bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];
                        if (body.IsTracked)
                        {
                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

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

                            }
                            this.DrawBody(joints, jointPoints, dc, drawPen);

                        }
                    }

                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    //transfer into bitmap 
                    //1.transfer into image
                    Image drawingImage = new Image { Source = new DrawingImage(drawingGroup) };
                    //2.render into RenderTargetBitmap
                    //   bitmap.Render(drawingImage);
                    //3. 


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
        /// <summary>
        /// Remove MultiSourceFrameArrived Listener
        /// </summary>
        public void CloseSourceListener()
        {
            if (ListenerState == true)
            {
                this.multiFrameSourceReader.MultiSourceFrameArrived -= this.Reader_MultiSourceFrameArrived;
                this.multiFrameSourceReader.Dispose();
                //this.multiFrameSourceReader = null;
                ListenerState = false;

            }
        }
        /// <summary>
        /// Add MultiSourceFrameArrived Listener
        /// </summary>
        public void OpenSourceListener()
        {
            if (ListenerState == false)
            {
                this.multiFrameSourceReader = myKinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Body);
                this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;
                ListenerState = true;
            }
        }

    }
}
