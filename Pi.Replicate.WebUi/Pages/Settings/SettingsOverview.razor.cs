using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Recipients.Queries.GetRecipients;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettings;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings
{
    public class SettingsOverviewBase : ComponentBase
    {
		[Inject]
		public IMediator Mediator { get; set; }

		protected SystemSettingModel RootFolder { get; set; } = new SystemSettingModel { Key = Constants.ReplicateBasePath, Value = "" };

		protected SystemSettingModel TriggerIntervalFolderCrawl { get; set; } = new SystemSettingModel { Key = Constants.FolderCrawlTriggerInterval, Value = "10" };

		protected SystemSettingModel TriggerIntervalRetry { get; set; } = new SystemSettingModel { Key = Constants.RetryTriggerInterval, Value = "10" };

		protected SystemSettingModel ChunkSize { get; set; } = new SystemSettingModel { Key = Constants.FileSplitSizeOfChunksInBytes, Value = "1000000" };

		protected List<RecipientModel> Recipients { get; set; } = new List<RecipientModel>();

		protected override async Task OnInitializedAsync()
		{
			await LoadSystemSettings();
			await LoadRecipients();
		}

		private async Task LoadSystemSettings()
		{
			var systemSettings = await Mediator.Send(new GetSystemSettingsQuery());
			if (systemSettings.FirstOrDefault(x => x.Key == Constants.ReplicateBasePath) is var rootFolder)
				RootFolder = new SystemSettingModel(rootFolder);

			if (systemSettings.FirstOrDefault(x => x.Key == Constants.FolderCrawlTriggerInterval) is var triggerIntervalFolderCrawl)
				TriggerIntervalFolderCrawl = new SystemSettingModel(triggerIntervalFolderCrawl);

			if (systemSettings.FirstOrDefault(x => x.Key == Constants.RetryTriggerInterval) is var triggerIntervalRetry)
				TriggerIntervalRetry = new SystemSettingModel(triggerIntervalRetry);

			if (systemSettings.FirstOrDefault(x => x.Key == Constants.FileSplitSizeOfChunksInBytes) is var chunkSize)
				ChunkSize = new SystemSettingModel(chunkSize);
		}

		private async Task LoadRecipients()
		{
			var recipients = await Mediator.Send(new GetRecipientsQuery());
			Recipients = recipients.Select(x => new RecipientModel(x)).ToList();
		}

		
		protected class SystemSettingModel
		{
			private string _key;
			private string _value;

			public SystemSettingModel()
			{

			}

			public SystemSettingModel(SystemSetting systemSetting):this()
			{
				Key = systemSetting.Key;
				Value = systemSetting.Value;
				IsChanged = false;
			}

			public string Key { get => _key; set => Set(ref _key,value); }

			public string Value { get => _value; set => Set(ref _value,value); }

			public bool IsChanged { get; set; }

			private void Set<TE>(ref TE oldValue, TE newValue)
			{
				if (EqualityComparer<TE>.Default.Equals(oldValue, newValue))
					return;

				oldValue = newValue;
				IsChanged = true;
			}
		}

		protected class RecipientModel
		{
			private string _name;
			private string _address;
			private bool _verified;

			public RecipientModel(Recipient recipient)
			{
				Name = recipient.Name;
				Address = recipient.Address;
				Verified = recipient.Verified;
				IsChanged = false;
			}

			public string Name 
			{ 
				get => _name; 
				set => Set(ref _name,value); 
			}

			public string Address 
			{ 
				get => _address; 
				set => Set(ref _address,value); 
			}

			public bool Verified 
			{ 
				get => _verified; 
				set => Set(ref _verified,value); 
			}

			public bool IsChanged { get; set; }

			private void Set<TE>(ref TE oldValue, TE newValue)
			{
				if (EqualityComparer<TE>.Default.Equals(oldValue, newValue))
					return;

				oldValue = newValue;
				IsChanged = true;
			}
		}
	}
}
