using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Kinect;
using System.Diagnostics;

namespace SocketPackage
{
    public delegate void changeEventHandler(object sender, EventArgs e);

    public class SocketData
    {
        private static int DeviceNumberAssigner = 1;
        
        public event changeEventHandler changed;

        public PlayBackStatus playbackStatus;



        protected void OnChange(EventArgs e)
        {
            if (changed != null)
            {
                changed(this, e);
            }
        }

        #region Properties and getter,setter
        //裝置辨識ID
        private int socketClientId = 0;
        public int SocketClientId
        {
            get { return socketClientId; }
            set { socketClientId = value; }
        }

        //frameNumbers
        private int framenumbers = 0;
        public int Framenumbers
        {
            get { return framenumbers; }
            set { framenumbers = value; }
        }

        MyBody[] body = new MyBody[7];
        public MyBody[] Body
        {
            get { return body; }
            set { body = value; }
        }

        private int _imageIdx = 0;
        public int ImageIdx
        {
            get { return _imageIdx; }
            set { _imageIdx = value; }
        }

        private byte[] _ImgBuffer = null;
        public byte[] ImgBuffer
        {
            get { return _ImgBuffer; }
            set
            {
                _ImgBuffer = value;
                //OnChange(EventArgs.Empty);
            }
        }

        //骨架座標
        private string[] _SBodyJoints = new string[7];
        public string[] SBodyJoints
        {
            get
            {
                return _SBodyJoints;
            }
            set
            {
                _SBodyJoints = value;
               // OnChange(EventArgs.Empty);
            }
        }

        //是否追蹤到
        private Boolean[] _IsTracked = new Boolean[7];
        public Boolean[] IsTracked
        {
            get { return _IsTracked; }
            set { _IsTracked = value; }
        }


        /// <summary>
        /// 裝置辨認編號，依照連線順序自動給以裝置編號，起始值為1。
        /// </summary>
        public int DeviceSerialNum { get;private set; }
        #endregion


        //Constructor
        public SocketData()
        {
            //初始化人體物件陣列 預設7個  [0-6]
            for (int i = 0; i < body.Length; ++i)
            {
                body[i] = new MyBody();
            }
            playbackStatus = new PlayBackStatus();

            //自動依照連線順序給以裝置編號
            this.DeviceSerialNum = DeviceNumberAssigner;
            DeviceNumberAssigner++;
        }

        /// <summary>
        ///  將骨架座標字串轉換成 Mybody 物件儲存，存入jointsInfo Dictionrys內
        /// </summary>
        public void ProcessJointsInfo()
        {
            int index = 0;
            foreach (MyBody b in body)
            {
                if (b.isTracked)
                {
                    string[] pieces = SBodyJoints[index].Split(':');

                    string skeletonIndex = pieces[0];

                    //設定使用者ID
                    b.TrackingId = (ulong)Int64.Parse(skeletonIndex);

                    if (pieces.Length > 1)
                    {

                        string[] jointsPieces = pieces[1].Split('|');

                        //將所有骨架座標轉成 CameraSpacePoint 存到 joints 裡面
                        foreach (string JointKey in Enum.GetNames(typeof(JointType)))
                        {
                            JointType jointType = (JointType)Enum.Parse(typeof(JointType), JointKey);

                            int jointTypeValue = (int)jointType;

                            Joint tempJoint = new Joint();

                            tempJoint.TrackingState = TrackingState.Tracked;
                            tempJoint.JointType = jointType;

                            tempJoint.Position = jointsStr2CameraSpacePoint(jointsPieces[jointTypeValue]);
                            if (b.jointsInfo.ContainsKey(jointType))
                            {
                                b.jointsInfo[jointType] = tempJoint;
                            }
                            else
                            {
                                b.jointsInfo.Add(jointType, tempJoint);
                            }

                        }
                    }
                    else
                    {
                        Debug.Print(">>SocketData.ProcessJointsInfo Data Format Error");
                    }
                }
                index++;
            }
        }

        /// <summary>
        ///  [Not Suggest] 如果要使用jointsInfo 請改用 ProcessJointsInfo()
        ///  將骨架座標轉換成Mybody物件儲存，存入joints Dictionrys內
        /// </summary>
        public void processJoints()
        {
            int index = 0;
            foreach (MyBody b in body)
            {
                if (b.isTracked)
                {
                    string[] pieces = SBodyJoints[index].Split(':');

                    string skeletonIndex = pieces[0];

                    //設定使用者ID
                    b.userId = skeletonIndex;

                    if (pieces.Length > 1)
                    {

                        string[] jointsPieces = pieces[1].Split('|');

                        //將所有骨架座標轉成 CameraSpacePoint 存到 joints 裡面
                        foreach (string JointKey in Enum.GetNames(typeof(JointType)))
                        {

                            JointType jointType = (JointType)Enum.Parse(typeof(JointType), JointKey);

                            int jointTypeValue = (int)jointType;

                            if (b.joints.ContainsKey(jointType))
                            {
                                b.joints[jointType] = new myCameraSpacePoint(jointsPieces[jointTypeValue]);
                            }
                            else
                            {
                                b.joints.Add(jointType, new myCameraSpacePoint(jointsPieces[jointTypeValue]));
                            }
                        }
                    }
                }
                index++;

            }
        }

        private CameraSpacePoint jointsStr2CameraSpacePoint(string joints)
        {
            CameraSpacePoint csp = new CameraSpacePoint();
            string[] pieces = joints.Split(',');
            if (pieces.Length == 3)
            {
                csp.X = float.Parse(pieces[0]);
                csp.Y = float.Parse(pieces[1]);
                csp.Z = float.Parse(pieces[2]);
            }
            else
            {
                csp.X = 0;
                csp.Y = 0;
                csp.Z = 0;
            }
            return csp;
        }

    }

    public class MyBody
    {
        public static MyBody Body2Mybody(Body body)
        {
            MyBody newBody = new MyBody();


            foreach (KeyValuePair<JointType, Joint> items in body.Joints)
            {
                newBody.jointsInfo.Add(items.Key, items.Value);
            }

            newBody.isTracked = body.IsTracked;

            newBody.TrackingId = body.TrackingId;



            return newBody;
        }


        public Boolean isTracked;

        /// <summary>
        /// [Not Suggest] please use jointsInfo intead.
        /// </summary>
        public Dictionary<JointType, myCameraSpacePoint> joints;

        /// <summary>
        /// 儲存詳細骨架資料
        /// </summary>
        public Dictionary<JointType, Joint> jointsInfo;
        public Dictionary<JointType, Joint> jointsInfo_VPTransfored = new Dictionary<JointType, Joint>();

        /// <summary>
        /// [not supported] please use trackingId instead.
        /// </summary>
        public string userId;

        /// <summary>
        /// UserID
        /// </summary>
        public ulong TrackingId { get; set; }

        public MyBody()
        {

            joints = new Dictionary<JointType, myCameraSpacePoint>();

            jointsInfo = new Dictionary<JointType, Joint>();

            isTracked = false;

            userId = string.Empty;
        }

        public void init()
        {
            foreach (string js in Enum.GetNames(typeof(JointType)))
            {
                joints.Add((JointType)Enum.Parse(typeof(JointType), js), null);
            }
        }
    }

    public class myCameraSpacePoint
    {
        public float X;
        public float Y;
        public float Z;
        public string joints_XYZ = string.Empty;

        /// <summary>
        /// Set x,y,z.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public myCameraSpacePoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Transform string into float.
        /// </summary>
        /// <param name="joints_XYZ">Format : X,Y,Z</param>
        /// <param name="isStoreString"></param>
        public myCameraSpacePoint(string joints_XYZ, Boolean isStoreString = false)
        {
            if (isStoreString)
            {
                this.joints_XYZ = joints_XYZ;
            }

            string[] pieces = joints_XYZ.Split(',');

            //check data type length
            if (pieces.Length == 3)
            {
                X = float.Parse(pieces[0]);
                Y = float.Parse(pieces[1]);
                Z = float.Parse(pieces[2]);
            }
            else
            {
                //format wrong !!!, set (0,0,0)
                X = 0; Y = 0; Z = 0;
            }
        }
    }

}
