using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace UniMoveStation.Utilities
{
    class Utils
    {
        /// <summary>
        /// http://www.cnblogs.com/xrwang/archive/2010/01/26/TheInteractionOfOpenCv-EmguCvANDDotNet.html
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static Bitmap MIplImagePointerToBitmap(MIplImage mi)
        {
            PixelFormat pixelFormat; // pixel format
            string unsupportedDepth = "Unsupported pixel bit depth IPL_DEPTH";
            string unsupportedChannels = "The number of channels is not supported (only 1,2,4 channels)";
            switch (mi.nChannels)
            {
                case 1:
                    switch (mi.depth)
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
                    switch (mi.depth)
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
                    switch (mi.depth)
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
            Bitmap bitmap = new Bitmap(mi.width, mi.height, mi.widthStep, pixelFormat, mi.imageData);
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
    }
}
