using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Model
{
    public class AllCamerasModel : ObservableObject
    {
        private List<SingleCameraModel> _allCameras;
        private string _name;

        public AllCamerasModel()
        {
            Name = "All";
        }

        public List<SingleCameraModel> AllCameras
        {
            get
            {
                return _allCameras;
            }
            set
            {
                Set(() => AllCameras, ref _allCameras, value);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                Set(() => Name, ref _name, value);
            }
        }
    }
}
