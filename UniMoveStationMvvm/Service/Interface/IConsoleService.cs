using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
