using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class StartupCleanup
    {
        public void Clean()
        {
            //todo delete files that are New, Changed and their Chunks from database if teh file is not processed yet, it should be deleted
        }
    }
}
