using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Kinect;

namespace SocketPackage
{
    public delegate void changeEventHandler(object sender, EventArgs e);

    public class SocketData
    {
        public event changeEventHandler changed;

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
                OnChange(EventArgs.Empty);
            }
        }

        //骨架座標
        private string[] _SBodyJoints = new string[7];
        public string[] SBodyJoints
        {
            get { return _SBodyJoints; }
            set { _SBodyJoints = value; }
        }

        //是否追蹤到
        private Boolean[] _IsTracked = new Boolean[7];
        public Boolean[] IsTracked
        {
            get { return _IsTracked; }
            set { _IsTracked = value; }
        }


        //constructor
        public SocketData() {

            //初始化人體物件陣列
            for (int i = 0; i < body.Length; ++i)
            {
                body[i] = new MyBody();
            }
        
        }

        public void processJoints( )
        {
            int index = 0;
            foreach (MyBody b in body)
            {

                if (b.isTracked)
                {

                    string[] pieces = SBodyJoints[index].Split(':');

                    string skeletonIndex = pieces[0];

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


        protected virtual void OnChange(EventArgs e)
        {
            if (changed != null)
            {
                changed(this, e);
            }
        }

    }

    public class MyBody
    {
        public Boolean isTracked ;

        public Dictionary<JointType, myCameraSpacePoint> joints;

        public string userId;
 
        public MyBody(){

            joints = new Dictionary<JointType, myCameraSpacePoint>();

            isTracked = false;

            userId = string.Empty;
        }
    }

    public class myCameraSpacePoint
    {
       public float X;
       public float Y;
       public float Z;

        public myCameraSpacePoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public myCameraSpacePoint(string joints_XYZ)
        {
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
