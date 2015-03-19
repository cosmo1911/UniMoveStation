using System;
using System.Collections.ObjectModel;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.Service.Interfaces
{
    public interface IConsoleService
    {
        ObservableCollection<ConsoleEntry> Entries { get; set; }

        void Write(String text);
    }
}
