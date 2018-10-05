using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Schema
{
    public class TempFile
    {
        public string Path { get; set; }

        public string Hash { get; set; }

        public Guid FileId { get; set; }
    }
}
