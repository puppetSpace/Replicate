using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class FailedFileChange
    {
        public FileChange FileChange { get; set; }

        public Recipient Recipient { get; set; }

        public static FailedFileChange Build(FileChange fileChange, Recipient recipient)
        {
            return new FailedFileChange { FileChange = fileChange, Recipient = recipient };
        }
    }
}
