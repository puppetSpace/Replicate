using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Helpers
{
    internal class FolderCrawler
    {
        //private static Logger _logger = LogManager.GetCurrentClassLogger();


        public IList<string> GetFiles(string path)
        {
            var files = new List<string>();
            if (!System.IO.Directory.Exists(path))
            {
                //_logger.Warn($"Given path, '{path}' is not a directory. Returning empty list");
                return files;
            }

            //_logger.Trace($"Traversing '{path}'");

            foreach (var file in System.IO.Directory.GetFiles(path))
                files.Add(file);

            foreach (var dir in System.IO.Directory.GetDirectories(path))
                files.AddRange(GetFiles(path));

            return files;
        }
    }
}
