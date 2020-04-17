using AutoMapper;
using Pi.Replicate.Application.Files.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            InitializeMappings();
        }

        private void InitializeMappings()
        {
            CreateMap<Domain.File, FileTransmissionModel>();
        }
    }
}
