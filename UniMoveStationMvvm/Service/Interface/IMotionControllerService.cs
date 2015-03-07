using MahApps.Metro.Controls;
using UniMoveStation.Model;

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
