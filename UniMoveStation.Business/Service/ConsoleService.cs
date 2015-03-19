using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Threading;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Business.Service
{
    public class ConsoleService : IConsoleService
    {
        public ObservableCollection<ConsoleEntry> Entries { get; set; }

        public ConsoleService()
        {
            Entries = new ObservableCollection<ConsoleEntry>();
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
                Entries.Add(new ConsoleEntry
                {
                    Text = text,
                    Time = time
                });
            });

        }
    } // ConsoleService
} // namespace
