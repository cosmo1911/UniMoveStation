using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniMoveStation.Model
{
    public class SettingsModel : ObservableObject
    {
        private double _width;
        private double _height;
        private double _x;
        private double _y;

        public SettingsModel()
        {
            Width = 1280;
            Height = 720;
            X = 0;
            Y = 0;
        }

        public double Width
        {
            get { return _width; }
            set { Set(() => Width, ref _width, value); }
        }

        public double Height
        {
            get { return _height; }
            set { Set(() => Height, ref _height, value); }
        }

        public double X
        {
            get { return _x; }
            set { Set(() => X, ref _x, value); }
        }

        public double Y
        {
            get { return _y; }
            set { Set(() => Y, ref _y, value); }
        }
    }
}
