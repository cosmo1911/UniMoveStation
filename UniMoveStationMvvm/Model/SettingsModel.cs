﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UniMoveStation.ViewModel;

namespace UniMoveStation.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsModel : ObservableObject, IDataErrorInfo
    {
        private double? _width;
        private double? _height;
        private double? _x;
        private double? _y;
        private bool _loadCamerasOnStartUp;

        public SettingsModel()
        {
            Width = 1280;
            Height = 720;
            X = 0;
            Y = 0;
            LoadCamerasOnStartUp = true;
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

        // TODO
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
                        else if (Width > SystemParameters.VirtualScreenWidth)
                        {
                            return "Given width is too large.";
                        }
                        else if (Width < 480)
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
