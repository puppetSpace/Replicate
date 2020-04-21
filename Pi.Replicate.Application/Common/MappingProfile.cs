using AutoMapper;
using Pi.Replicate.Application.FileChanges.Models;
using Pi.Replicate.Application.Files.Commands.UpdateChangedFiles;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Application.Files.Queries.GetFilesForFolder;
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
            CreateMap<Domain.FileChange, FileChangeTransmissionModel>();
            CreateMap<GetFilesForFolderDto, Domain.File>()
                .ForMember(x => x.Signature, opt => opt.MapFrom((x, y) => x.Signature.AsMemory()));
            CreateMap<FoundToUpdateFileDto, Domain.File>()
                .ForMember(x => x.Signature, opt => opt.MapFrom((x, y) => x.Signature.AsMemory()));
        }
    }
}
