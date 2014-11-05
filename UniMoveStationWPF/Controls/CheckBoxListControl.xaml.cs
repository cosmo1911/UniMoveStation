using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using UniMove;

namespace UniMoveStation
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CheckBoxListControl : UserControl
    {
        public ObservableCollection<CheckedListItem> checkBoxListBoxItems { get; set; }

        public UniMoveController[] moves;

        public CheckBoxListControl()
        {
            InitializeComponent();

            DataContext = this;

            checkBoxListBoxItems = new ObservableCollection<CheckedListItem>();
        }

        public void init(int count)
        {
            if(count == 0)
            {
                checkBoxListBoxItems.Add(new CheckedListItem()
                    {
                        Id = 0,
                        Name = "No Controller Found",
                        IsChecked = false
                    });
                IsEnabled = false;
            }
            for (int i = 0; i < count; i++)
            {
                checkBoxListBoxItems.Add(new CheckedListItem()
                {
                    Id = i,
                    Name = "UMC",
                    IsChecked = false
                });
                checkBoxListBoxItems[i].OnControllerToggle += HandleOnControllerToggle;
            }

            moves = new UniMoveController[count];
        }


        public void HandleOnControllerToggle(object sender, ControllerToggleEventArgs e)
        {
            if(e.enable)
            {
                moves[e.id] = initMove(e.id);
            }
            else
            {
                moves[e.id].Disconnect();
                moves[e.id] = null;
            }
        }

        public class ControllerToggleEventArgs : EventArgs
        {
            public int id;
            public bool enable;

            public ControllerToggleEventArgs(int id, bool enable)
            {
                this.id = id;
                this.enable = enable;
            }
        }

        private void toggleController(bool enable)
        {

        }

        public UniMoveController initMove(int index)
        {
            UniMoveController move = new UniMoveController();
            PSMoveConnectStatus status = move.Init(index);
            if (status != PSMoveConnectStatus.OK)
            {
                //console.AppendText(string.Format("Move {0} Status: {1}" + Environment.NewLine, index, status));
                return null;
            }

            // This example program only uses Bluetooth-connected controllers
            PSMoveConnectionType conn = move.ConnectionType;
            if (conn == PSMoveConnectionType.Unknown || conn == PSMoveConnectionType.USB)
            {
                //console.AppendText(string.Format("Move {0} Status: {1}" + Environment.NewLine, index, status));
                return null;
            }
            else
            {
                //move.OnControllerDisconnected += HandleControllerDisconnected;
                move.SetLED(UnityEngine.Color.blue);
                //console.AppendText(string.Format("Move {0} Status: {1}" + Environment.NewLine, index, status));
            }
            return move;
        } //initMove

        public class CheckedListItem : INotifyPropertyChanged
        {
            private int id;
            private string name;
            private bool isChecked;

            public event EventHandler<ControllerToggleEventArgs> OnControllerToggle;
            public event PropertyChangedEventHandler PropertyChanged;

            public CheckedListItem() { }

            protected void NotifyPropertyChanged(string strPropertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(strPropertyName));
                }
            }

            public int Id
            {
                get { return id; }
                set
                {
                    id = value;
                    NotifyPropertyChanged("Id");
                }
            }

            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }

            public bool IsChecked
            {
                get { return isChecked; }
                set
                {
                    isChecked = value;
                    if(OnControllerToggle != null)
                    {
                        OnControllerToggle(this, new ControllerToggleEventArgs(Id, value));
                    }
                    NotifyPropertyChanged("IsChecked");
                }
            }

            public string Content
            {
                get { return "#" + id + ": " + name; }
            }
        } //CheckListItem
    } //CheckBoxList
} //namespace