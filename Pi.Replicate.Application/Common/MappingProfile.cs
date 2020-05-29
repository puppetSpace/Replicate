using AutoMapper;
using Pi.Replicate.Application.Folders.Queries.GetFolderSettings;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForSettings;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingOverview;

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
			CreateMap<Domain.SystemSetting, SystemSettingViewModel>()
				.AfterMap((s, d) => d.ResetState());
			CreateMap<Domain.Recipient, RecipientViewModel>()
				.AfterMap((s, d) => d.ResetState());
			CreateMap<Domain.FolderWebhook, FolderWebhookViewModel>()
				.AfterMap((s, d) => d.ResetState());
		}
	}
}
