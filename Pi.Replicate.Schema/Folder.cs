using System;
using System.Collections.Generic;

namespace Pi.Replicate.Schema
{
    public class Folder
    {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public FolderType FolderType { get; set; }

		//todo remove when FolderOptions are implemented
        public bool DeleteFilesAfterSend { get; set; }

        public string Path { get; set; }

        public string GetPath()
        {
            return Pi.Replicate.Shared.System.PathBuilder.Build(Name); 
        }
    }
}
