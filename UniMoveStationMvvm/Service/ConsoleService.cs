using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UniMoveStation.Service
{
    public class ConsoleService : IConsoleService
    {
        public TextBox Console
        {
            get;
            set;
        }

        public ConsoleService()
        {
            Console = new TextBox();
        }

        public void WriteLine(String text)
        {
            Write(text + Environment.NewLine);
        }

        public void Write(String text)
        {
            string hours = DateTime.Now.TimeOfDay.Hours < 10 
                ? "0" + DateTime.Now.TimeOfDay.Hours
                : DateTime.Now.TimeOfDay.Hours.ToString();
            string minutes = DateTime.Now.TimeOfDay.Minutes < 10 
                ? "0" + DateTime.Now.TimeOfDay.Minutes 
                : DateTime.Now.TimeOfDay.Minutes.ToString();
            string seconds = DateTime.Now.TimeOfDay.Seconds < 10
                ? "0" + DateTime.Now.TimeOfDay.Seconds
                : DateTime.Now.TimeOfDay.Seconds.ToString();
            string time = hours + ":" + minutes + ":" + seconds;

            DispatcherHelper.RunAsync(() =>
                {
                    Console.AppendText("[" + time + "] " + text);
                });
        }
    } // ConsoleService
} // namespace
