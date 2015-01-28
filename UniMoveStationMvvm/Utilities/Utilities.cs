using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using UnityEngine;

namespace UniMoveStation.Utilities
{
    public static class Utils
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
            return new PointF((float)p.X, (float)p.Y);
        }

        public static System.Drawing.Point PointFtoPoint(PointF p)
        {
            return new System.Drawing.Point((int)(p.X + 0.5), (int)(p.Y + 0.5));
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
                int x = (int) (points[i].X + 0.5);
                int y = (int) (points[i].Y + 0.5);

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
            float fx = (float) icp.IntrinsicMatrix[0, 0];
            float fy = (float) icp.IntrinsicMatrix[1, 1];

            IList<PointF> list = new List<PointF>();
            foreach (PointF point in points)
            {
                list.Add(new PointF( point.X / fx, point.Y / fy));
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

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// http://stackoverflow.com/a/1118557
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ip,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        /// <summary>
        /// http://www.cnblogs.com/xrwang/archive/2010/01/26/TheInteractionOfOpenCv-EmguCvANDDotNet.html
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static Bitmap MIplImagePointerToBitmap(MIplImage image)
        {

            PixelFormat pixelFormat; // pixel format
            string unsupportedDepth = "Unsupported pixel bit depth IPL_DEPTH";
            string unsupportedChannels = "The number of channels is not supported (only 1,2,4 channels)";
            switch (image.nChannels)
            {
                case 1:
                    switch (image.depth)
                    {
                        case IPL_DEPTH.IPL_DEPTH_8U:
                            pixelFormat = PixelFormat.Format8bppIndexed;
                            break;
                        case IPL_DEPTH.IPL_DEPTH_16U:
                            pixelFormat = PixelFormat.Format16bppGrayScale;
                            break;
                        default:
                            throw new NotImplementedException(unsupportedDepth);
                    }
                    break;
                case 3:
                    switch (image.depth)
                    {
                        case IPL_DEPTH.IPL_DEPTH_8U:
                            pixelFormat = PixelFormat.Format24bppRgb;
                            break;
                        case IPL_DEPTH.IPL_DEPTH_16U:
                            pixelFormat = PixelFormat.Format48bppRgb;
                            break;
                        default:
                            throw new NotImplementedException(unsupportedDepth);
                    }
                    break;
                case 4:
                    switch (image.depth)
                    {
                        case IPL_DEPTH.IPL_DEPTH_8U:
                            pixelFormat = PixelFormat.Format32bppArgb;
                            break;
                        case IPL_DEPTH.IPL_DEPTH_16U:
                            pixelFormat = PixelFormat.Format64bppArgb;
                            break;
                        default:
                            throw new NotImplementedException(unsupportedDepth);
                    }
                    break;
                default:
                    throw new NotImplementedException(unsupportedChannels);

            }
            Bitmap bitmap = new Bitmap(image.width, image.height, image.widthStep, pixelFormat, image.imageData);
            // For grayscale images, but also to modify the color palette
            if (pixelFormat == PixelFormat.Format8bppIndexed)
                SetColorPaletteOfGrayscaleBitmap(bitmap);
            return bitmap;
        }

        public static void SetColorPaletteOfGrayscaleBitmap(Bitmap bitmap)
        {
            PixelFormat pixelFormat = bitmap.PixelFormat;
            if (pixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = bitmap.Palette;
                for (int i = 0; i < palette.Entries.Length; i++)
                    palette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                bitmap.Palette = palette;
            }
        }
    } // Utils
} // namespace
