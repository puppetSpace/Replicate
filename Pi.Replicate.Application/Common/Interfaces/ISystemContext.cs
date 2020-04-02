using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface ISystemContext
    {
        DbSet<SystemSetting> SystemSettings { get; set; }
    }
}
