using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SpaceDepolyment
{
    public class CameraPosMapper
    {
        Dictionary<int,CameraCoordinate> deviceDeplymentDict = new Dictionary<int,CameraCoordinate>();
        int deviceNumbers = 0;
        
        public void AddDevice(Point3D point,double angle){
            deviceNumbers++;
            deviceDeplymentDict.Add(deviceNumbers,new CameraCoordinate(point,angle));
        }

        public CameraCoordinate GetCameraSetting(int index)
        {
            return deviceDeplymentDict[index];
        }
    }

    public class CameraCoordinate
    {
        /// <summary>
        /// 相對於虛擬空間的XYZ座標
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// 相對於虛擬空間座標軸旋轉角度
        /// </summary>
        public double Angle { get; set; }

        public CameraCoordinate(Point3D point,double angle){
            this.Position = point;
            this.Angle = angle;
        }
    }
}
