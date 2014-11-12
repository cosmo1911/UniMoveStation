using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Service
{
    public class CLEyeService : ICLEyeService
    {
        public bool Start(int id)
        {
            return Enabled = true;
        }

        public bool Stop()
        {

            return Enabled = false;
        }

        public bool Enabled
        {
            get;
            set;
        }
    }
}
