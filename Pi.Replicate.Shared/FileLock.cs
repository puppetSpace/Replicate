using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public static class FileLock
    {
        public static bool IsLocked(string filePath)
        {
            return IsLocked(filePath,false);
        }

        public static bool IsLocked(string filePath, bool checkWriteAccess)
        {
            var isLocked = false;
            try
            {
                if (checkWriteAccess)
                    using (var fs = System.IO.File.OpenWrite(filePath)) { }
                else
					using (var fs = System.IO.File.OpenRead(filePath)) { }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "unable to open file for read");
                isLocked = true;
            }

            return isLocked;
        }
    }
}
