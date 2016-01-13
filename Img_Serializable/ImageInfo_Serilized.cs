using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
//using System.Windows.Media.Imaging;

namespace Img_Serializable
{
    [Serializable]
    public class ImageInfo_Serializable
    {
        public int _imageIdx = 0;
        public byte[] _ImgBuffer = null  ;

        //人體串列 (  是否追蹤到, 2:Tracking ,骨架座標 ) 最多只能追蹤6人 所以Tuple大小設為六
        //public List<Tuple<int, Boolean, Dictionary<JointType, Point>>> _BodyList = new List<Tuple<int, Boolean, Dictionary<JointType, Point>>>();
        public Dictionary<int, Tuple<Boolean, Dictionary<JointType, Point>>> _BodyDictTp = new Dictionary<int, Tuple<bool, Dictionary<JointType, Point>>>();
        //public Tuple< Boolean, Dictionary<JointType, Point>>[] _BodyTpArray = new Tuple< bool, Dictionary<JointType, Point>>[6];
 
        public ImageInfo_Serializable() { }

        public ImageInfo_Serializable(byte[] ImgBuffer, int imageIndex)
        {
            _ImgBuffer = ImgBuffer;
            _imageIdx = imageIndex;
        }

    }
 
}
