using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using UniMoveStation.Model;
using UniMoveStation.Service;

namespace UniMoveStation.ViewModel.Flyout
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class AddCameraViewModel : FlyoutBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the AddCameraViewModel class.
        /// </summary>
        public AddCameraViewModel()
        {
            Position = Position.Right;
            Header = "Add Camera";
        }
    }
}