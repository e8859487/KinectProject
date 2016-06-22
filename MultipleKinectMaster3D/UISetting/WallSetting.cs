
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MultipleKinectMaster3D.UISetting
{
    class WallSetting
    {
        public double WallWidth { get; set; }
        public double WallHeight { get; set; }
        public double WallLength { get; set; }
        public Point3D WallCenter { get; set; }
        public Brush WallBrush { get; set; }
        public WallSetting(double Length_X, double width_Y, double height_Z, Point3D Center, Brush brush)
        {
            this.WallWidth = width_Y;
            this.WallHeight = height_Z;
            this.WallLength = Length_X;
            this.WallCenter = Center;
            this.WallBrush = brush;
        }
    }
}