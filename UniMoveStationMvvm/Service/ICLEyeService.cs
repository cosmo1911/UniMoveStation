using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Service
{
    public interface ICLEyeService
    {
        bool Start(int id);

        bool Stop();

        bool Enabled
        {
            get;
            set;
        }
    }
}
