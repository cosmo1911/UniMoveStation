using System;
using System.Windows.Controls;

namespace UniMoveStation.Service
{
    public interface IConsoleService
    {
        TextBox Console
        {
            get;
            set;
        }

        void Write(String text);

        void WriteLine(String text);
    }
}
