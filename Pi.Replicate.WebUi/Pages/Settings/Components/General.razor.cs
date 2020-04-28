using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Observr;
using Pi.Replicate.Application.SystemSettings.Commands.UpsertSystemSettings;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettings;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Pi.Replicate.WebUi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings.Components
{
    public class GeneralBase : ComponentBase, Observr.IObserver<SettingsSaveNotificationMessage>, IDisposable
	{
		private IDisposable _saveNotificationSubscription;

		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		protected IBroker SaveNotifier { get; set; }

		protected SystemSettingModel RootFolder { get; set; } = new SystemSettingModel { Key = Constants.ReplicateBasePath, Value = "" };

		protected SystemSettingModel TriggerIntervalFolderCrawl { get; set; } = new SystemSettingModel { Key = Constants.FolderCrawlTriggerInterval, Value = "10" };

		protected SystemSettingModel TriggerIntervalRetry { get; set; } = new SystemSettingModel { Key = Constants.RetryTriggerInterval, Value = "10" };

		protected SystemSettingModel ChunkSize { get; set; } = new SystemSettingModel { Key = Constants.FileSplitSizeOfChunksInBytes, Value = "1000000" };

		protected List<string> ValidationMessages { get; set; } = new List<string>();

		public async Task Handle(SettingsSaveNotificationMessage value, CancellationToken cancellationToken)
		{
			var saveTasks = new List<Task>();
			if(RootFolder.IsChanged)
				saveTasks.Add(Mediator.Send(new UpsertSystemSettingsCommand { Key = RootFolder.Key, Value = RootFolder.Value }));
			if(TriggerIntervalFolderCrawl.IsChanged)
				saveTasks.Add(Mediator.Send(new UpsertSystemSettingsCommand { Key = TriggerIntervalFolderCrawl.Key, Value = TriggerIntervalFolderCrawl.Value }));
			if(TriggerIntervalRetry.IsChanged)
				saveTasks.Add(Mediator.Send(new UpsertSystemSettingsCommand { Key = TriggerIntervalRetry.Key, Value = TriggerIntervalRetry.Value }));
			if(ChunkSize.IsChanged)
				saveTasks.Add(Mediator.Send(new UpsertSystemSettingsCommand { Key = ChunkSize.Key, Value = ChunkSize.Value }));

			try
			{
				await Task.WhenAll(saveTasks);
				RootFolder.IsChanged = false;
				TriggerIntervalFolderCrawl.IsChanged = false;
				TriggerIntervalRetry.IsChanged = false;
				ChunkSize.IsChanged = false;
			}
			catch (ValidationException ex)
			{
				ValidationMessages = ex.Errors.Select(x => x.ErrorMessage).ToList();
				StateHasChanged();
			}
		}

		public void Dispose()
		{
			_saveNotificationSubscription?.Dispose();
		}

		protected override async Task OnInitializedAsync()
		{
			_saveNotificationSubscription = SaveNotifier.Subscribe(this);
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

		protected class SystemSettingModel
		{
			private string _key;
			private string _value;

			public SystemSettingModel()
			{

			}

			public SystemSettingModel(SystemSetting systemSetting) : this()
			{
				Key = systemSetting.Key;
				Value = systemSetting.Value;
				IsChanged = false;
			}

			public string Key { get => _key; set => Set(ref _key, value); }

			public string Value { get => _value; set => Set(ref _value, value); }

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
