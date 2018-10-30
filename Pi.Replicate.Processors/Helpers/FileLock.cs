using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Helpers
{
    internal static class FileLock
    {
        public static bool IsLocked(string filePath)
        {
            var isLocked = false;


            try
            {
                using (var fs = System.IO.File.Open(filePath, System.IO.FileMode.OpenOrCreate)) { }
            }
            catch (System.IO.IOException)
            {
                isLocked = true;
            }

            return isLocked;
        }
    }
}
