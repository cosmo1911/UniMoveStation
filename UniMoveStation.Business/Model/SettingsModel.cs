using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace UniMoveStation.Business.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsModel : ObservableObject, IDataErrorInfo
    {
        private double? _width;
        private double? _height;
        private double? _x;
        private double? _y;
        private bool _debug;
        private bool _loadCamerasOnStartUp;
        private bool _loadControllersOnStartUp;
        private List<string> _movedHosts;

        public SettingsModel()
        {
            Width = 1280;
            Height = 720;
            X = 0;
            Y = 0;
            LoadCamerasOnStartUp = true;
            LoadControllersOnStartUp = true;
            Debug = false;
            _movedHosts = new List<string>();
        }

        [JsonProperty]
        public bool Debug
        {
            get { return _debug; }
            set { Set(() => Debug, ref _debug, value); }
        }

        public List<string> MovedHosts
        {
            get { return _movedHosts; }
            set { Set(() => MovedHosts, ref _movedHosts, value); }
        }

        [JsonProperty]
        public double? Width
        {
            get { return _width; }
            set { Set(() => Width, ref _width, value); }
        }

        [JsonProperty]
        public double? Height
        {
            get { return _height; }
            set { Set(() => Height, ref _height, value); }
        }

        [JsonProperty]
        public double? X
        {
            get { return _x; }
            set { Set(() => X, ref _x, value); }
        }

        [JsonProperty]
        public double? Y
        {
            get { return _y; }
            set { Set(() => Y, ref _y, value); }
        }

        [JsonProperty]
        public bool LoadCamerasOnStartUp
        {
            get { return _loadCamerasOnStartUp; }
            set { Set(() => LoadCamerasOnStartUp, ref _loadCamerasOnStartUp, value); }
        }

        [JsonProperty]
        public bool LoadControllersOnStartUp
        {
            get { return _loadControllersOnStartUp; }
            set { Set(() => LoadControllersOnStartUp, ref _loadControllersOnStartUp, value); }
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
                    case "X":
                        break;
                    case "Y":
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
