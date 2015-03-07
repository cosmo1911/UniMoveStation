﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using UnityEngine;

namespace UniMoveStation.Utils
{
    public static class CvHelper
    {
        public static Matrix<double> GetTranslationVector(double x, double y, double z)
        {
            double[,] data = new double[3, 1];

            data[0, 0] = x;
            data[1, 0] = y;
            data[2, 0] = z;

            return new Matrix<double>(data);
        }

        public static PointF PointToPointF(System.Drawing.Point p)
        {
            return new PointF(p.X, p.Y);
        }

        public static System.Drawing.Point PointFtoPoint(PointF p)
        {
            return new System.Drawing.Point((int)(p.X + 0.5), (int)(p.Y + 0.5));
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
            rotationMatrix[1, 1] = Mathf.Cos(rad);
            rotationMatrix[1, 2] = -Mathf.Sin(rad);

            rotationMatrix[2, 0] = 0;
            rotationMatrix[2, 1] = Mathf.Sin(rad);
            rotationMatrix[2, 2] = Mathf.Cos(rad);

            return rotationMatrix;
        }

        public static Matrix<double> GetYRotationMatrix(float degrees)
        {
            float rad = (float)(Math.PI / 180) * degrees;

            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = Mathf.Cos(rad);
            rotationMatrix[0, 1] = 0;
            rotationMatrix[0, 2] = Mathf.Sin(rad);

            rotationMatrix[1, 0] = 0;
            rotationMatrix[1, 1] = 1;
            rotationMatrix[1, 2] = 0;

            rotationMatrix[2, 0] = -Mathf.Sin(rad);
            rotationMatrix[2, 1] = 0;
            rotationMatrix[2, 2] = Mathf.Cos(rad);

            return rotationMatrix;
        }

        public static Matrix<double> GetZRotationMatrix(float degrees)
        {
            float rad = (float)(Math.PI / 180) * degrees;

            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = Mathf.Cos(rad);
            rotationMatrix[0, 1] = -Mathf.Sin(rad);
            rotationMatrix[0, 2] = 0;

            rotationMatrix[1, 0] = Mathf.Sin(rad);
            rotationMatrix[1, 1] = Mathf.Cos(rad);
            rotationMatrix[1, 2] = 0;

            rotationMatrix[2, 0] = 0;
            rotationMatrix[2, 1] = 0;
            rotationMatrix[2, 2] = 1;

            return rotationMatrix;
        }

        public static System.Drawing.Point[] PointFToPoint(PointF[] points)
        {
            System.Drawing.Point[] result = new System.Drawing.Point[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                int x = (int)(points[i].X + 0.5);
                int y = (int)(points[i].Y + 0.5);

                result[i] = new System.Drawing.Point(x, y);
            }
            return result;
        }

        public static MCvPoint2D64f[] GetImagePoints(Vector3 rawPosition)
        {
            MCvPoint2D64f[] imgPts = new MCvPoint2D64f[4];

            float radiusPx = rawPosition.z;

            imgPts[0] = new MCvPoint2D64f(rawPosition.x - radiusPx, rawPosition.y - radiusPx);
            imgPts[1] = new MCvPoint2D64f(rawPosition.x + radiusPx, rawPosition.y - radiusPx);
            imgPts[2] = new MCvPoint2D64f(rawPosition.x + radiusPx, rawPosition.y + radiusPx);
            imgPts[3] = new MCvPoint2D64f(rawPosition.x - radiusPx, rawPosition.y + radiusPx);

            return imgPts;
        }

        public static PointF[] GetImagePointsF(Vector3 rawPosition)
        {
            PointF[] imgPts = new PointF[4];
            float radiusPx = rawPosition.z;

            imgPts[0] = new PointF(rawPosition.x - radiusPx, rawPosition.y - radiusPx);
            imgPts[1] = new PointF(rawPosition.x + radiusPx, rawPosition.y - radiusPx);
            imgPts[2] = new PointF(rawPosition.x + radiusPx, rawPosition.y + radiusPx);
            imgPts[3] = new PointF(rawPosition.x - radiusPx, rawPosition.y + radiusPx);

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
    } // CvHelper
} // namespace