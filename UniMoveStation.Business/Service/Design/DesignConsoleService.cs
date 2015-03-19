using System;
using System.Collections.ObjectModel;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Business.Service.Design
{
    public class DesignConsoleService : IConsoleService
    {
        public ObservableCollection<ConsoleEntry> Entries { get; set; }

        public DesignConsoleService()
        {
            Entries = new ObservableCollection<ConsoleEntry>();
        }

        public void Write(string text)
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

            Entries.Add(new ConsoleEntry
            {
                Text = text,
                Time = time
            });
        }

    } // DesignConsoleService
} // namespace
