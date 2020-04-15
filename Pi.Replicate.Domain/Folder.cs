using System;
using System.Collections.Generic;

namespace Pi.Replicate.Domain
{
    public class Folder
    {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public FolderOption FolderOptions { get; set; }

        public List<Recipient> Recipients { get; set; }

    }
}
