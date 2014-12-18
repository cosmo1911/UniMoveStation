using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UniMoveStation.Model;
using UniMoveStation.SharpMove;

namespace UniMoveStation.Service
{
    public interface IMotionControllerService
    {
        bool Enabled
        {
            get;
            set;
        }

        MotionControllerModel Initialize(int id);

        MotionControllerModel Initialize(MotionControllerModel motionController);

        void Start();

        void Stop();

        void SetColor(UnityEngine.Color color);

        void CalibrateMagnetometer(MetroWindow window);
    }
}
