using System;
using System.Collections.Generic;

namespace Pi.Replicate.Schema
{
    public class Folder
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IList<Host> Receivers { get; set; }

        public FolderType FolderType { get; set; }
    }
}
