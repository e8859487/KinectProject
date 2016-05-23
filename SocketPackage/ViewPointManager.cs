using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using MathNet;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
namespace SocketPackage
{
    public class ViewPointManager
    {
        /// <summary>
        /// Rotational Matrix for decice A
        /// </summary>
        private float[] RMatrix_A = new float[9];

        /// <summary>
        /// Rotational Matrix for decice B
        /// </summary>
        private float[] RMatrix_B = new float[9];

        /// <summary>
        /// Translate Matrix decice A
        /// </summary>
        private float[] TMatrix_A = new float[3];

        /// <summary>
        /// Translate Matrix decice B
        /// </summary>
        private float[] TMatrix_B = new float[3];


        public void Analyze_RT_Matrix(CameraSpacePoint left, CameraSpacePoint right, CameraSpacePoint torsolCenter, DEVICE_ID deviceID)
        {
            // 存放左右兩肩的中心點
            myCameraSpacePoint shoulderCenter = new myCameraSpacePoint(0, 0, 0);

            // (左右肩中心點到右肩的距離),
            float D_SCenter2RShoulder;

            // (左右肩中心點到身體質心的距離)
            float D_SCenter2BodyCenter;

            float A, B, C, D, E, F, G, H, I;  //旋轉矩陣R

            myCameraSpacePoint transitionMatrix = new myCameraSpacePoint(0, 0, 0);

            //計算左右兩肩的中心點  b 
            shoulderCenter.X = (left.X + right.X) / 2;
            shoulderCenter.Y = (left.Y + right.Y) / 2;
            shoulderCenter.Z = (left.Z + right.Z) / 2;

            //計算長度
            D_SCenter2RShoulder = (float)Math.Pow(Math.Pow(left.X - right.X, 2) + Math.Pow(left.Y - right.Y, 2) + Math.Pow(left.Z - right.Z, 2), 0.5) / 2;

            D_SCenter2BodyCenter = (float)Math.Pow(Math.Pow(torsolCenter.X - shoulderCenter.X, 2) + Math.Pow(torsolCenter.Y - shoulderCenter.Y, 2) + Math.Pow(torsolCenter.Z - shoulderCenter.Z, 2), 0.5);


            #region Rotation Matrix:
            /* 
             A  D   G 
             B  E   H
             C  F   I         
             */
            A = (right.X - shoulderCenter.X) / D_SCenter2RShoulder;  //右肩位移到(t,0,0)並且除以t
            B = (right.Y - shoulderCenter.Y) / D_SCenter2RShoulder;  //同上
            C = (right.Z - shoulderCenter.Z) / D_SCenter2RShoulder;  //同上 

            D = (torsolCenter.X - shoulderCenter.X) / (-D_SCenter2BodyCenter);  //身體中心點位移到(0,-s,0)並且除以s
            E = (torsolCenter.Y - shoulderCenter.Y) / (-D_SCenter2BodyCenter);  //同上
            F = (torsolCenter.Z - shoulderCenter.Z) / (-D_SCenter2BodyCenter);  //同上

            G = B * F - E * C;  //Z軸外積
            H = C * D - F * A;  //同上
            I = A * E - D * B;  //同上 
            #endregion

            #region Translate Matrix:
            //以新座標系為基準的平移向量
            transitionMatrix.X = -(A * shoulderCenter.X + B * shoulderCenter.Y + C * shoulderCenter.Z);
            transitionMatrix.Y = -(D * shoulderCenter.X + E * shoulderCenter.Y + F * shoulderCenter.Z);
            transitionMatrix.Z = -(G * shoulderCenter.X + H * shoulderCenter.Y + I * shoulderCenter.Z);
            #endregion


            if (deviceID == DEVICE_ID.DEVICE_A)
            {
                RMatrix_A[0] = A;
                RMatrix_A[1] = B;
                RMatrix_A[2] = C;
                RMatrix_A[3] = D;
                RMatrix_A[4] = E;
                RMatrix_A[5] = F;
                RMatrix_A[6] = G;
                RMatrix_A[7] = H;
                RMatrix_A[8] = I;

                TMatrix_A[0] = transitionMatrix.X;
                TMatrix_A[1] = transitionMatrix.Y;
                TMatrix_A[2] = transitionMatrix.Z;
            }
            else
            {
                if (deviceID == DEVICE_ID.DEVICE_B)
                {
                    RMatrix_B[0] = A;
                    RMatrix_B[1] = B;
                    RMatrix_B[2] = C;
                    RMatrix_B[3] = D;
                    RMatrix_B[4] = E;
                    RMatrix_B[5] = F;
                    RMatrix_B[6] = G;
                    RMatrix_B[7] = H;
                    RMatrix_B[8] = I;

                    TMatrix_B[0] = transitionMatrix.X;
                    TMatrix_B[1] = transitionMatrix.Y;
                    TMatrix_B[2] = transitionMatrix.Z;
                }
            }
        }

        /// <summary>
        /// Transform point.
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        public CameraSpacePoint Transform(CameraSpacePoint Point)
        {

            CameraSpacePoint _Point = new CameraSpacePoint();

            _Point.X = RMatrix_A[0] * Point.X + RMatrix_A[1] * Point.Y + RMatrix_A[2] * Point.Z + TMatrix_A[0];
            _Point.Y = RMatrix_A[3] * Point.X + RMatrix_A[4] * Point.Y + RMatrix_A[5] * Point.Z + TMatrix_A[1];
            _Point.Z = RMatrix_A[6] * Point.X + RMatrix_A[7] * Point.Y + RMatrix_A[8] * Point.Z + TMatrix_A[2];

            return _Point;
        }

        private float[,] RMatrix_B2A;

        private float[,] TMatrix_B2A;

        Matrix<float> RMatrix_A2S_Inversed;
        Matrix<float> RMatrix_B2S;
        Matrix<float> TMatrix_BminusA;
        public void Analyze_RT_Matrix_TwoDevice()
        {
            var M = Matrix<float>.Build;

            RMatrix_A2S_Inversed = M.DenseOfArray(new float[,]{
                {RMatrix_A[0],RMatrix_A[1],RMatrix_A[2]},
                {RMatrix_A[3],RMatrix_A[4],RMatrix_A[5]}, 
                {RMatrix_A[6],RMatrix_A[7],RMatrix_A[8]},
            }).Inverse();

            RMatrix_B2S = M.DenseOfArray(new float[,]{
                {RMatrix_B[0],RMatrix_B[1],RMatrix_B[2]},
                {RMatrix_B[3],RMatrix_B[4],RMatrix_B[5]}, 
                {RMatrix_B[6],RMatrix_B[7],RMatrix_B[8]},
            });

            RMatrix_B2A = (RMatrix_A2S_Inversed * RMatrix_B2S).ToArray();

            TMatrix_BminusA = M.DenseOfArray(new float[,]{
                {TMatrix_B[0] - TMatrix_A[0]},
                {TMatrix_B[1] - TMatrix_A[1]},
                {TMatrix_B[2] - TMatrix_A[2]}
            });

            TMatrix_B2A = (RMatrix_A2S_Inversed * TMatrix_BminusA).ToArray();



            //float[,] x = { { 2, 2, 5 }, { -2, 1, 2 }, { 6, 3, 9 } };

            //var M = Matrix<float>.Build;
            //Matrix<float> matrix = M.DenseOfArray(x);
            //float a = matrix[0, 1];
            //Console.WriteLine(matrix.Inverse());
        }

        public CameraSpacePoint Transform_B2A(CameraSpacePoint Point)
        {

            CameraSpacePoint _Point = new CameraSpacePoint();

            ////假資料
           // RMatrix_B2A = new float[,] {{1,0,0},{0,1,0},{0,0,1}};

            ////假資料
           // TMatrix_B2A = new float[,] { { 0.004f }, { 0.03f }, { 0.05f } };

            _Point.X = RMatrix_B2A[0, 0] * Point.X + RMatrix_B2A[0, 1] * Point.Y + RMatrix_B2A[0, 2] * Point.Z + TMatrix_B2A[0, 0];
            _Point.Y = RMatrix_B2A[1, 0] * Point.X + RMatrix_B2A[1, 1] * Point.Y + RMatrix_B2A[1, 2] * Point.Z + TMatrix_B2A[1, 0];
            _Point.Z = RMatrix_B2A[2, 0] * Point.X + RMatrix_B2A[2, 1] * Point.Y + RMatrix_B2A[2, 2] * Point.Z + TMatrix_B2A[2, 0];

            return _Point;
        }

        public void displayMatrix()
        {
            if (false)
            {

                Debug.Print("-----RMatrix_A-----");

                float[] mat = RMatrix_A;
                for (int i = 0; i < mat.Length; i += 3)
                {
                    Debug.Print("{0} {1} {2}", mat[i], mat[i + 1], mat[i + 2]);
                }

                Debug.Print("TMatrix_A");
                mat = TMatrix_A;
                for (int i = 0; i < mat.Length; i += 3)
                {
                    Debug.Print("{0} {1} {2}", mat[i], mat[i + 1], mat[i + 2]);
                }

                Debug.Print("-------RMatrix_B-------");

                mat = RMatrix_B;
                for (int i = 0; i < mat.Length; i += 3)
                {
                    Debug.Print("{0} {1} {2}", mat[i], mat[i + 1], mat[i + 2]);
                }

                Debug.Print("TMatrix_B");
                mat = TMatrix_B;
                for (int i = 0; i < mat.Length; i += 3)
                {
                    Debug.Print("{0} {1} {2}", mat[i], mat[i + 1], mat[i + 2]);
                }






                Debug.Print("-------RMatrix_A2S_Inversed-------");

                Matrix<float> matrix = RMatrix_A2S_Inversed;
                for (int i = 0; i < mat.Length; i += 3)
                {
                    Debug.Print("{0} {1} {2}", matrix[i, 0], matrix[i, 1], matrix[i, 2]);
                }

                Debug.Print("-------TMatrix_BminusA-------");

                matrix = TMatrix_BminusA;
                Debug.Print("{0} {1} {2}", matrix[0, 0], matrix[1, 0], matrix[2, 0]);

                //RMatrix_A2S_Inversed * TMatrix_BminusA


                float[,] mat2;




                Debug.Print("-------RMatrix_B2A-------");
                mat2 = RMatrix_B2A;
                for (int j = 0; j < mat2.GetLength(0); j++)
                {
                    Debug.Print("{0} {1} {2}", mat2[j, 0], mat2[j, 1], mat2[j, 2]);
                }

                Debug.Print("TMatrix_B2A");
                mat2 = TMatrix_B2A;
                for (int j = 0; j < mat2.GetLength(0); j++)
                {
                    Debug.Print("{0}  ", mat2[j, 0]);
                }
                
            }

           

        }
    }



    /// <summary>
    /// 裝置辨認編號
    /// </summary>
    public enum DEVICE_ID
    {
        DEVICE_A,
        DEVICE_B
    }
}
