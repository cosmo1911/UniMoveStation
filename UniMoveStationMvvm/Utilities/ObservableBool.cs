using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace UniMoveStation.Utilities
{
    public class ObservableNullableBool : INotifyPropertyChanged
    {
        private bool isChecked;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string strPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(strPropertyName));
            }
        }

        public Nullable<bool> IsChecked
        {
            get
            {
                return new Nullable<bool>(isChecked);
            }
            set
            {
                if(value == null)
                {
                    isChecked = false;
                }
                else
                {
                    isChecked = value.Value;
                }
                NotifyPropertyChanged("IsChecked");
            }
        }
    }
}
