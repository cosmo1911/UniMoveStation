using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;
using UnityEngine;


namespace UniMoveStation.Business.Service.Design
{
    public class DesignMotionControllerService : IMotionControllerService
    {
        private MotionControllerModel _motionController;

        public MotionControllerModel MotionController
        {
            get { return _motionController; }
            set { _motionController = value; }
        }

        public bool Enabled { get; set; }

        public MotionControllerModel Initialize(MotionControllerModel motionController)
        {
            return MotionController = motionController;
        }

        public MotionControllerModel Initialize(int id)
        {
            return MotionController = new MotionControllerModel();
        }

        public void Start() { }

        public void Stop() { }

        public void SetColor(Color color) { }

        public async void CalibrateMagnetometer(MetroWindow window)
        {
            var controller = await window.ShowProgressAsync("Please wait...", "Setting up Magnetometer Calibration.", true);

            await Task.Delay(3000);
            controller.SetTitle("Magnetometer Calibration");
            for (int i = 0; i < 101; i++)
            {
                controller.SetProgress(i / 100.0);
                controller.SetMessage(string.Format("Rotate the controller in all directions: {0}%", i));

                if (controller.IsCanceled) break;
                await Task.Delay(100);
            }
            await controller.CloseAsync();

            if (controller.IsCanceled)
            {
                await window.ShowMessageAsync("Magnetometer Calibration", "Calibration has been cancelled.");
            }
            else
            {
                await window.ShowMessageAsync("Magnetometer Calibration", "Calibration finished successfully.");
            }
        }
    } // DesignMotionControllerService
} // namespace
