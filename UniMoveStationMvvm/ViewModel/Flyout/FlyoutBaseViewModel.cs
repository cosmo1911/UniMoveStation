using GalaSoft.MvvmLight;
using MahApps.Metro.Controls;

namespace UniMoveStation.ViewModel.Flyout
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class FlyoutBaseViewModel : ViewModelBase
    {

        private bool _isOpen = false;
        private string _header = "None";
        private Position _position = Position.Right;

        #region Properties
        /// <summary>
        /// The <see cref="Header" /> property's name.
        /// </summary>
        public const string HeaderPropertyName = "Header";

        /// <summary>
        /// Sets and gets the Header property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                Set(() => Header, ref _header, value, true);
            }
        }

        /// <summary>
        /// The <see cref="Position" /> property's name.
        /// </summary>
        public const string PositionPropertyName = "Position";

        /// <summary>
        /// Sets and gets the Position property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public Position Position
        {
            get
            {
                return _position;
            }
            set
            {
                Set(() => Position, ref _position, value, true);
            }
        }

        /// <summary>
        /// The <see cref="IsOpen" /> property's name.
        /// </summary>
        public const string IsOpenPropertyName = "IsOpen";

        /// <summary>
        /// Sets and gets the IsOpen property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                Set(() => IsOpen, ref _isOpen, value, true);
            }
        }
        #endregion

    }
}