using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace SocketPackage
{
    class ViewPointManager
    {
        /// <summary>
        /// Rotational Matrix
        /// </summary>
        private float[] RMatrix = new float[9];

        /// <summary>
        /// Translate Matrix
        /// </summary>
        private float[] TMatrix = new float[3];


        public  void Analyze_RT_Matrix(CameraSpacePoint left, CameraSpacePoint right, CameraSpacePoint center )
        {
            // 存放左右兩肩的中心點
            //double lrx = 0.0, lry = 0.0, lrz = 0.0;// 存放左右兩肩的中心點
            myCameraSpacePoint shoulderCenter = new myCameraSpacePoint(0, 0, 0);


            //double a, b;                //a為t(左右肩中心點到右肩的距離),b為s(左右肩中心點到身體質心的距離)
            // (左右肩中心點到右肩的距離),
            float D_SCenter2RShoulder;
            
            // (左右肩中心點到身體質心的距離)
            float D_SCenter2BodyCenter;

            float A, B, C, D, E, F, G, H, I;  //旋轉矩陣R

            //double Tx, Ty, Tz;
            myCameraSpacePoint transitionMatrix = new myCameraSpacePoint(0, 0, 0);

            //計算左右兩肩的中心點  b 
            shoulderCenter.X = (left.X + right.X) / 2;
            shoulderCenter.Y = (left.Y + right.Y) / 2;
            shoulderCenter.Z = (left.Z + right.Z) / 2;

            //a = sqrt((left.X - right.X) * (left.X - right.X) + (left.Y - right.Y) * (left.Y - right.Y) + (left.Z - right.Z) * (left.z - right.z)) / 2;  //計算一邊肩膀寬的距離
            D_SCenter2RShoulder = (float)Math.Pow(Math.Pow(left.X - right.X, 2) + Math.Pow(left.Y - right.Y, 2) + Math.Pow(left.Z - right.Z, 2), 0.5) / 2;

            //b = sqrt((center.x - lrx) * (center.x - lrx) + (center.y - lry) * (center.y - lry) + (center.z - lrz) * (center.z - lrz));   //計算左右肩中心點到身體質心的距離
            D_SCenter2BodyCenter = (float)Math.Pow(Math.Pow(center.X - shoulderCenter.X, 2) + Math.Pow(center.Y - shoulderCenter.Y, 2) + Math.Pow(center.Z - shoulderCenter.Z, 2), 0.5);


            /* Rotation Matrix: 
             A  D   G 
             B  E   H
             C  F   I         
             */
            A = (right.X - shoulderCenter.X) / D_SCenter2RShoulder;  //右肩位移到(t,0,0)並且除以t
            B = (right.Y - shoulderCenter.Y) / D_SCenter2RShoulder;  //同上
            C = (right.Z - shoulderCenter.Z) / D_SCenter2RShoulder;  //同上 

            D = (center.X - shoulderCenter.X) / (-D_SCenter2BodyCenter);  //身體中心點位移到(0,-s,0)並且除以s
            E = (center.Y - shoulderCenter.Y) / (-D_SCenter2BodyCenter);  //同上
            F = (center.Z - shoulderCenter.Z) / (-D_SCenter2BodyCenter);  //同上

            G = B * F - E * C;  //Z軸外積
            H = C * D - F * A;  //同上
            I = A * E - D * B;  //同上

            //以新座標系為基準的平移向量

            RMatrix[0] = A;
            RMatrix[1] = B;
            RMatrix[2] = C;
            RMatrix[3] = D;
            RMatrix[4] = E;
            RMatrix[5] = F;
            RMatrix[6] = G;
            RMatrix[7] = H;
            RMatrix[8] = I;

            transitionMatrix.X = -(A * shoulderCenter.X + B * shoulderCenter.Y + C * shoulderCenter.Z);
            transitionMatrix.Y = -(D * shoulderCenter.X + E * shoulderCenter.Y + F * shoulderCenter.Z);
            transitionMatrix.Z = -(G * shoulderCenter.X + H * shoulderCenter.Y + I * shoulderCenter.Z);

            TMatrix[0] = transitionMatrix.X;
            TMatrix[1] = transitionMatrix.Y;
            TMatrix[2] = transitionMatrix.Z;
        }

        /// <summary>
        /// Transform point.
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        public  CameraSpacePoint Transform(CameraSpacePoint Point ){
            CameraSpacePoint _Point = new CameraSpacePoint();

            _Point.X = RMatrix[0] * Point.X + RMatrix[1] * Point.Y + RMatrix[2] * Point.Z + TMatrix[0];
            _Point.Y = RMatrix[3] * Point.X + RMatrix[4] * Point.Y + RMatrix[5] * Point.Z + TMatrix[1];
            _Point.Z = RMatrix[6] * Point.X + RMatrix[7] * Point.Y + RMatrix[8] * Point.Z + TMatrix[2];

            return _Point;
        }


    }
}
