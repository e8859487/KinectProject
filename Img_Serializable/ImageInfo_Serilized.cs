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

        public byte[] _ImgBuffer = null;

        public byte[] _JointsPosBuffer = null;

        //骨架座標
        public string[] _SBodyJoints = new string[7];
        //是否追蹤到
        public Boolean[] _IsTracked = new Boolean[7];
 

        public ImageInfo_Serializable(int bufferSize) {
            _ImgBuffer = new byte[bufferSize];

        }

        public ImageInfo_Serializable(byte[] ImgBuffer, int imageIndex)
        {
            _ImgBuffer = ImgBuffer;
            _imageIdx = imageIndex;
        }

    }
}
