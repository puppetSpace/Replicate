using System;
using System.Collections.Generic;

namespace Pi.Replicate.Schema
{
    public class Folder
    {
        private string _rootFolder;
        public Folder()
        {
            //TODO Get root path from config
            _rootFolder = String.Empty;
        }

        public Folder(string rootFolder)
        {
            _rootFolder = rootFolder;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Host> Receivers { get; set; }

        public FolderType FolderType { get; set; }

        public bool DeleteFilesAfterSend { get; set; }

        public IEnumerable<File> Files { get; set; }

        public string GetPath()
        {
            return System.IO.Path.Combine(_rootFolder, Name); 
        }
    }
}
