using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data.Db
{
    public class SystemContext : DbContext,ISystemContext
    {
        public SystemContext(DbContextOptions<SystemContext> options) : base(options)
        {

        }

        public DbSet<SystemSetting> SystemSettings { get; set; }
    }
}
