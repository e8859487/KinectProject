using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Media.Imaging;

namespace Img_Serializable
{
    [Serializable]
    public class ImageInfo_Serializable
    {
        private int _imageIdx = 0;
        public byte[] _ImgBuffer = null  ;
       // public WriteableBitmap _depthBitmap = null;

        public ImageInfo_Serializable() { }

        public ImageInfo_Serializable(byte[] ImgBuffer, int imageIndex)
        {
            _ImgBuffer = ImgBuffer;
            _imageIdx = imageIndex;
        }


    }
}
