using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace UniMoveStation.Business.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsModel : ObservableObject, IDataErrorInfo
    {
        private double _width;
        private double _height;
        private double _left;
        private double _top;
        private bool _debug;
        private bool _loadCamerasOnStartup;
        private bool _loadControllersOnStartup;
        private string _movedHostsFile;
        private string _movedHostsFileWaterMark;

        public SettingsModel()
        {
            Width = 1280;
            Height = 720;
            Left = 0;
            Top = 0;
            LoadCamerasOnStartup = true;
            LoadControllersOnStartup = true;
            Debug = false;
        }

        [JsonProperty]
        public bool Debug
        {
            get { return _debug; }
            set { Set(() => Debug, ref _debug, value); }
        }

        public string MovedHostsFile
        {
            get { return _movedHostsFile; }
            set { Set(() => MovedHostsFile, ref _movedHostsFile, value); }
        }

        public string MovedHostsWaterMark
        {
            get { return _movedHostsFileWaterMark; }
            set { Set(() => MovedHostsWaterMark, ref _movedHostsFileWaterMark, value); }
        }

        [JsonProperty]
        public double Width
        {
            get { return _width; }
            set { Set(() => Width, ref _width, value); }
        }

        [JsonProperty]
        public double Height
        {
            get { return _height; }
            set { Set(() => Height, ref _height, value); }
        }

        [JsonProperty]
        public double Left
        {
            get { return _left; }
            set { Set(() => Left, ref _left, value); }
        }

        [JsonProperty]
        public double Top
        {
            get { return _top; }
            set { Set(() => Top, ref _top, value); }
        }

        [JsonProperty]
        public bool LoadCamerasOnStartup
        {
            get { return _loadCamerasOnStartup; }
            set { Set(() => LoadCamerasOnStartup, ref _loadCamerasOnStartup, value); }
        }

        [JsonProperty]
        public bool LoadControllersOnStartup
        {
            get { return _loadControllersOnStartup; }
            set { Set(() => LoadControllersOnStartup, ref _loadControllersOnStartup, value); }
        }

        // TODO or custom validation rule
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Width":
                        if (Width == null)
                        {
                            return "Enter a valid value.";
                        }
                        if (Width > SystemParameters.VirtualScreenWidth)
                        {
                            return "Given width is too large.";
                        }
                        if (Width < 480)
                        {
                            return "Width needs to be at least 480px";
                        }
                        break;
                    case "Height":
                        break;
                    case "Left":
                        break;
                    case "Top":
                        break;
                }
                return null;
            }
        }

        public string Error
        {
            get { return string.Empty; }
        }
    } // SettingsModel
} // namespace
