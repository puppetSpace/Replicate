using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class FileChange
    {
        public Guid Id { get; set; }

        public Guid FileId { get; set; }

        public File File { get; set; }

        public int VersionNo { get; set; }

        public int AmountOfChunks { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public static FileChange Build(File file, int versionNo, int amountOfChunks, DateTime lastModifiedDate)
        {
            return new FileChange
            {
                Id = Guid.NewGuid(),
                FileId = file.Id,
                File = file,
                VersionNo = versionNo,
                AmountOfChunks = amountOfChunks,
                LastModifiedDate = lastModifiedDate
            };
        }
    }
}
