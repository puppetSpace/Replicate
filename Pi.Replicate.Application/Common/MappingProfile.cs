using AutoMapper;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingOverview;
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
			CreateMap<Domain.File, FileTransmissionModel>();
			//.ForMember(x=>x.Signature,opt=> opt.MapFrom((x,y)=> x.Signature.ToArray()));
			CreateMap<Domain.FileChunk, FileChunkTransmissionModel>()
				.ForMember(x => x.Value, opt => opt.MapFrom((x, y) => x.Value.ToArray()));
			CreateMap<FileDao, Domain.File>();
			//.ForMember(x => x.Signature, opt => opt.MapFrom((x, y) => x.Signature.AsMemory())).ReverseMap();
			CreateMap<Domain.EofMessage, EofMessageTransmissionModel>();
			CreateMap<Domain.SystemSetting, SystemSettingViewModel>()
				.AfterMap((s,d)=> d.ResetState());
		}
	}
}
