using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Media.Imaging;

namespace Img_Serializable
{
    [Serializable]
    public class ImageInfo_Serializable
    {
        public int _imageIdx = 0;
        public byte[] _ImgBuffer = null  ;
       // public WriteableBitmap _depthBitmap = null;
        public Body[] _bodies = null;

        //人體串列 (  是否追蹤到, 2:Tracking ,骨架座標 ) 最多只能追蹤6人 所以Tuple大小設為六
        //public List<Tuple<int, Boolean, Dictionary<JointType, Point>>> _BodyList = new List<Tuple<int, Boolean, Dictionary<JointType, Point>>>();
        //Dictionary<int,Tuple< Boolean, Dictionary<JointType, Point>>> _BodyDictTp =null;
        public Tuple< Boolean, Dictionary<JointType, Point>>[] _BodyTpArray = new Tuple< bool, Dictionary<JointType, Point>>[6];
 

        public ImageInfo_Serializable() { }

        public ImageInfo_Serializable(byte[] ImgBuffer, int imageIndex)
        {
            _ImgBuffer = ImgBuffer;
            _imageIdx = imageIndex;
            //_BodyDictTp = new Dictionary<int,Tuple<bool,Dictionary<JointType,Point>>>();
        }

 
    }

    //public class BodysInfo : ISerializable
    //{

    //    //人體串列 (編號 ,狀態 0 not Exit 1unTracking 2:Tracking ,骨架座標 )
    //    private List<Tuple<int, int, Dictionary<JointType, Point>>> BodyList = new List<Tuple<int, int, Dictionary<JointType, Point>>>();
    //    public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

 
}
