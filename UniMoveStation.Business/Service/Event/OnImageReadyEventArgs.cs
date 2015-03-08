using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace UniMoveStation.Business.Service.Event
{
    public class OnImageReadyEventArgs : EventArgs
    {
        public Image<Bgr, Byte> Image { get; set; }

        public OnImageReadyEventArgs(Image<Bgr, Byte> image)
        {
            Image = image;
        }
    }
}
