using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class FolderOption
    {
        public static FolderOption Empty { get; } = new FolderOption();
        public bool DeleteAfterSent { get; set; }
    }
}
