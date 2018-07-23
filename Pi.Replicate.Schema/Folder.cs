using System;
using System.Collections.Generic;

namespace Pi.Replicate.Schema
{
    public class Folder
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Host> Receivers { get; set; }

        public FolderType FolderType { get; set; }

        public bool DeleteFilesAfterSend { get; set; }

        public IEnumerable<File> Files { get; set; }

        public string GetPath()
        {
            return System.IO.Path.Combine("", Name); //TODO Get root path from config
        }
    }
}
