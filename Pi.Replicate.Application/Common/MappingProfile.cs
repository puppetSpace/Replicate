using AutoMapper;
using Pi.Replicate.Application.FileChanges.Models;
using Pi.Replicate.Application.Files.Models;
using System;
using System.Security.Cryptography.X509Certificates;

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
            CreateMap<Domain.File, FileTransmissionModel>()
				.ForMember(x=>x.Signature,opt=> opt.MapFrom((x,y)=> x.Signature.ToArray()));
			CreateMap<Domain.FileChange, FileChangeTransmissionModel>();
			CreateMap<FileDto, Domain.File>()
                .ForMember(x => x.Signature, opt => opt.MapFrom((x, y) => x.Signature.AsMemory()));
        }
    }
}
