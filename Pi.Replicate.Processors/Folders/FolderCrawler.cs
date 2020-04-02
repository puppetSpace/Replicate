using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Folders
{
    internal sealed class FolderCrawler
    {

        public IList<System.IO.FileInfo> GetFiles(string path)
        {
            var files = new List<System.IO.FileInfo>();
            if (!System.IO.Directory.Exists(path))
            {
                Log.Warning($"Given path, '{path}' is not a directory. Returning empty list");
                return files;
            }

            Log.Verbose($"Traversing '{path}'");

            foreach (var file in System.IO.Directory.GetFiles(path))
                files.Add(new System.IO.FileInfo(file));

            foreach (var dir in System.IO.Directory.GetDirectories(path))
                files.AddRange(GetFiles(path));

            return files;
        }
    }
}
