using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using UnityEngine;

namespace UniMoveStation.Utilities
{
    public class Utilities
    {
        public static Matrix<double> GetTranslationVector(double x, double y, double z)
        {
            double[,] data = new double[3, 1];

            data[0, 0] = x;
            data[1, 0] = y;
            data[2, 0] = z;

            return new Matrix<double>(data);
        }

        public static Matrix<double> GetYRotationMatrix(float angle)
        {
            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = Mathf.Cos(angle);
            rotationMatrix[0, 1] = 0;
            rotationMatrix[0, 2] = Mathf.Sin(angle);

            rotationMatrix[1, 0] = 0;
            rotationMatrix[1, 1] = 1;
            rotationMatrix[1, 2] = 0;

            rotationMatrix[2, 0] = -Mathf.Sin(angle);
            rotationMatrix[2, 1] = 0;
            rotationMatrix[2, 2] = Mathf.Cos(angle);

            return rotationMatrix;
        }
    }
}
