using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace UniMoveStation.Business.Model
{
    public class MotionControllersModel : ObservableObject
    {
        private ObservableCollection<MotionControllerModel> _controllers;

        public ObservableCollection<MotionControllerModel> Controllers
        {
            get { return _controllers ?? (_controllers = new ObservableCollection<MotionControllerModel>()); }
            set { Set(() => Controllers, ref _controllers, value); }
        }
    }
}
