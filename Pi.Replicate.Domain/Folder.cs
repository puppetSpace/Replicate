using System;
using System.Collections.Generic;

namespace Pi.Replicate.Domain
{
    public class Folder
    {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public FolderOption FolderOptions { get; set; }

        public List<FolderRecipient> Recipients { get; set; }

    }

    public class FolderRecipient
    {
        public Guid FolderId { get; set; }
        public Folder Folder { get; set; }
        public Guid RecipientId { get; set; }
        public Recipient Recipient { get; set; }
    }
}
