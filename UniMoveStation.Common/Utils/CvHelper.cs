using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using UniMoveStation.NitoMessages;

namespace UniMoveStation.Common.Utils
{
    public static class CvHelper
    {
        public static MCvPoint3D64f GetCubeCenter(MCvPoint3D64f[] cubeObjectPoints)
        {
            double mx = 0, my = 0, mz = 0;
            for (int i = 0; i < cubeObjectPoints.Length; i++)
            {
                mx += cubeObjectPoints[i].x / cubeObjectPoints.Length;
                my += cubeObjectPoints[i].y / cubeObjectPoints.Length;
                mz += cubeObjectPoints[i].z / cubeObjectPoints.Length;
            }
            return new MCvPoint3D64f(mx, my, mz);
        }

        public static double GetDistanceToPoint(MCvPoint3D64f origin, MCvPoint3D64f destination)
        {
            return GetPoint3dMagnitude(destination - origin);
        }

        public static double GetPoint3dMagnitude(MCvPoint3D64f point)
        {
            return Math.Sqrt(point.x * point.x + point.y * point.y + point.z * point.z);
        }

        public static Matrix<double> GetTranslationVector(double x, double y, double z)
        {
            double[,] data = new double[3, 1];

            data[0, 0] = x;
            data[1, 0] = y;
            data[2, 0] = z;

            return new Matrix<double>(data);
        }

        public static PointF PointToPointF(Point p)
        {
            return new PointF(p.X, p.Y);
        }

        public static Point PointFtoPoint(PointF p)
        {
            return new Point((int)(p.X + 0.5), (int)(p.Y + 0.5));
        }

        public static Matrix<double> Get3DTranslationMatrix(double x, double y, double z)
        {
            Matrix<double> m = new Matrix<double>(4, 4);
            m.SetIdentity();
            m[0, 3] = x;
            m[1, 3] = y;
            m[2, 3] = z;

            return m;
        }

        public static Matrix<double> ConvertToHomogenous(Matrix<double> matrix)
        {
            if (matrix.Rows < 2 || matrix.Cols < 2 || (matrix.Rows != matrix.Cols))
            {
                throw new ArgumentException("symmetric matrix > 1x1 expected");
            }

            Matrix<double> m = new Matrix<double>(matrix.Rows + 1, matrix.Cols + 1);
            m.SetIdentity();
            for (int row = 0; row < matrix.Rows; row++)
            {
                for (int col = 0; col < matrix.Cols; col++)
                {
                    m[row, col] = matrix[row, col];
                }
            }
            return m;
        }

        public static Matrix<double> Get2DTranslationMatrix(double x, double y)
        {
            Matrix<double> m = new Matrix<double>(3, 3);
            m.SetIdentity();
            m[0, 3] = x;
            m[1, 3] = y;

            return m;
        }

        public static Matrix<double> Get3DTranslationMatrix(Matrix<double> translation)
        {
            if (translation.Cols == 1 && translation.Rows == 3)
            {
                return Get3DTranslationMatrix(translation[0, 0], translation[1, 0], translation[2, 0]);
            }
            if (translation.Cols == 3 && translation.Rows == 1)
            {
                return Get3DTranslationMatrix(translation[0, 0], translation[0, 1], translation[0, 2]);
            }
            throw new ArgumentException("3x1 or 1x3 matrix expected");
        }

        public static Matrix<double> GetXRotationMatrix(float degrees)
        {
            float rad = (float)(Math.PI / 180) * degrees;

            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = 1;
            rotationMatrix[0, 1] = 0;
            rotationMatrix[0, 2] = 0;

            rotationMatrix[1, 0] = 0;
            rotationMatrix[1, 1] = Math.Cos(rad);
            rotationMatrix[1, 2] = -Math.Sin(rad);

            rotationMatrix[2, 0] = 0;
            rotationMatrix[2, 1] = Math.Sin(rad);
            rotationMatrix[2, 2] = Math.Cos(rad);

            return rotationMatrix;
        }

        public static Matrix<double> GetYRotationMatrix(float degrees)
        {
            float rad = (float)(Math.PI / 180) * degrees;

            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = Math.Cos(rad);
            rotationMatrix[0, 1] = 0;
            rotationMatrix[0, 2] = Math.Sin(rad);

            rotationMatrix[1, 0] = 0;
            rotationMatrix[1, 1] = 1;
            rotationMatrix[1, 2] = 0;

            rotationMatrix[2, 0] = -Math.Sin(rad);
            rotationMatrix[2, 1] = 0;
            rotationMatrix[2, 2] = Math.Cos(rad);

            return rotationMatrix;
        }

        public static Matrix<double> GetZRotationMatrix(float degrees)
        {
            float rad = (float)(Math.PI / 180) * degrees;

            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = Math.Cos(rad);
            rotationMatrix[0, 1] = -Math.Sin(rad);
            rotationMatrix[0, 2] = 0;

            rotationMatrix[1, 0] = Math.Sin(rad);
            rotationMatrix[1, 1] = Math.Cos(rad);
            rotationMatrix[1, 2] = 0;

            rotationMatrix[2, 0] = 0;
            rotationMatrix[2, 1] = 0;
            rotationMatrix[2, 2] = 1;

            return rotationMatrix;
        }

        public static Point[] PointFToPoint(PointF[] points)
        {
            Point[] result = new Point[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                int x = (int)(points[i].X + 0.5);
                int y = (int)(points[i].Y + 0.5);

                result[i] = new Point(x, y);
            }
            return result;
        }

        public static MCvPoint2D64f[] GetImagePoints(Float3 rawPosition)
        {
            MCvPoint2D64f[] imgPts = new MCvPoint2D64f[4];

            float radiusPx = rawPosition.Z;

            imgPts[0] = new MCvPoint2D64f(rawPosition.X - radiusPx, rawPosition.Y - radiusPx);
            imgPts[1] = new MCvPoint2D64f(rawPosition.X + radiusPx, rawPosition.Y - radiusPx);
            imgPts[2] = new MCvPoint2D64f(rawPosition.X + radiusPx, rawPosition.Y + radiusPx);
            imgPts[3] = new MCvPoint2D64f(rawPosition.X - radiusPx, rawPosition.Y + radiusPx);

            return imgPts;
        }

        public static PointF[] GetImagePointsF(Float3 rawPosition)
        {
            PointF[] imgPts = new PointF[4];
            float radiusPx = rawPosition.Z;

            imgPts[0] = new PointF(rawPosition.X - radiusPx, rawPosition.Y - radiusPx);
            imgPts[1] = new PointF(rawPosition.X + radiusPx, rawPosition.Y - radiusPx);
            imgPts[2] = new PointF(rawPosition.X + radiusPx, rawPosition.Y + radiusPx);
            imgPts[3] = new PointF(rawPosition.X - radiusPx, rawPosition.Y + radiusPx);

            return imgPts;
        }

        public static MCvPoint2D64f PointFtoPoint2D(PointF point)
        {
            return new MCvPoint2D64f(point.X, point.Y);
        }

        public static IList<PointF> NormalizePoints(IList<PointF> points, IntrinsicCameraParameters icp)
        {
            float fx = (float)icp.IntrinsicMatrix[0, 0];
            float fy = (float)icp.IntrinsicMatrix[1, 1];

            IList<PointF> list = new List<PointF>();
            foreach (PointF point in points)
            {
                list.Add(new PointF(point.X / fx, point.Y / fy));
            }
            return list;
        }

        public static IntPtr CreatePointListPointer(IList<PointF> points)
        {
            IntPtr result = CvInvoke.cvCreateMat(points.Count, 2, MAT_DEPTH.CV_32F);

            for (int i = 0; i < points.Count; i++)
            {
                double currentX = points[i].X;
                double currentY = points[i].Y;
                CvInvoke.cvSet2D(result, i, 0, new MCvScalar(currentX));
                CvInvoke.cvSet2D(result, i, 1, new MCvScalar(currentY));
            }

            return result;
        }

        public static IntPtr CreatePointListPointer(Matrix<double> matrix)
        {
            IntPtr result = CvInvoke.cvCreateMat(matrix.Rows, matrix.Cols, MAT_DEPTH.CV_64F);

            for (int row = 0; row < matrix.Rows; row++)
            {
                for (int col = 0; col < matrix.Cols; col++)
                {
                    CvInvoke.cvSet2D(result, row, col, new MCvScalar(matrix[row, col]));
                }
            }

            return result;
        }

        public static IntPtr CreatePointListPointer(IList<MCvPoint3D64f> points)
        {
            IntPtr result = CvInvoke.cvCreateMat(points.Count, 3, MAT_DEPTH.CV_64F);

            for (int i = 0; i < points.Count; i++)
            {
                double currentX = points[i].x;
                double currentY = points[i].y;
                double currentZ = points[i].z;
                CvInvoke.cvSet2D(result, i, 0, new MCvScalar(currentX));
                CvInvoke.cvSet2D(result, i, 1, new MCvScalar(currentY));
                CvInvoke.cvSet2D(result, i, 2, new MCvScalar(currentZ));
            }

            return result;
        }



        /** this conversion uses NASA standard aeroplane conventions as described on page:
        *   http://www.euclideanspace.com/maths/geometry/rotations/euler/index.htm
        *   Coordinate System: right hand
        *   Positive angle: right hand
        *   Order of euler angles: heading first, then attitude, then bank
        *   matrix row column ordering:
        *   [m00 m01 m02]
        *   [m10 m11 m12]
        *   [m20 m21 m22]*/
        public static Matrix<double> Rotate(double x, double y, double z)
        {
            double heading = (Math.PI / 180) * y;
            double attitude = (Math.PI / 180) * z;
            double bank = (Math.PI / 180) * x;

            // Assuming the angles are in radians.
            double ch = Math.Cos(heading);
            double sh = Math.Sin(heading);
            double ca = Math.Cos(attitude);
            double sa = Math.Sin(attitude);
            double cb = Math.Cos(bank);
            double sb = Math.Sin(bank);

            Matrix<double> r = new Matrix<double>(3, 3);
            r[0, 0] = ch * ca;
            r[0, 1] = sh * sb - ch * sa * cb;
            r[0, 2] = ch * sa * sb + sh * cb;
            r[1, 0] = sa;
            r[1, 1] = ca * cb;
            r[1, 2] = -ca * sb;
            r[2, 0] = -sh * ca;
            r[2, 1] = sh * sa * cb + ch * sb;
            r[2, 2] = -sh * sa * sb + ch * cb;

            return r;
        }
    } // CvHelper
} // namespace
