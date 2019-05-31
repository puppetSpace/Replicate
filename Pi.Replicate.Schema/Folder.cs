using System;
using System.Collections.Generic;

namespace Pi.Replicate.Schema
{
    public class Folder
    {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public FolderType FolderType { get; set; }

        public bool DeleteFilesAfterSend { get; set; }

		//todo don't use foreign key mapping. Not performant when retrieving data

        //public IEnumerable<File> Files { get; set; }

        //public IEnumerable<Host> Hosts { get; set; }

        public string Path { get; set; }

        public string GetPath()
        {
            return Pi.Replicate.Shared.System.PathBuilder.Build(Name); 
        }
    }
}
