using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public interface ISettings
    {
        object this[string key] { get; set; }
    }
}
