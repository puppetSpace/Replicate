using AutoMapper;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Application.Folders.Queries.GetFolderSettings;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForSettings;
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
			CreateMap<Domain.File, Domain.RequestFile>();
			CreateMap<Domain.File, FileTransmissionModel>();
			CreateMap<Domain.FileChunk, FileChunkTransmissionModel>()
				.ForMember(x => x.Value, opt => opt.MapFrom((x, y) => x.Value.ToArray()));
			CreateMap<FileDao, Domain.File>();
			CreateMap<Domain.EofMessage, EofMessageTransmissionModel>();
			CreateMap<Domain.SystemSetting, SystemSettingViewModel>()
				.AfterMap((s,d)=> d.ResetState());
			CreateMap<Domain.Recipient, RecipientViewModel>()
				.AfterMap((s, d) => d.ResetState());
			CreateMap<Domain.FolderWebhook, FolderWebhookViewModel>()
				.AfterMap((s, d) => d.ResetState());
		}
	}
}
