using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Observr;
using Pi.Replicate.Application.SystemSettings.Commands.UpdateSystemSettings;
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

		public List<SystemSettingModel> SystemSettings { get; set; } = new List<SystemSettingModel>();


		protected List<string> ValidationMessages { get; set; } = new List<string>();

		public async Task Handle(SettingsSaveNotificationMessage value, CancellationToken cancellationToken)
		{
			var saveTasks = new List<Task>();
			foreach(var ss in SystemSettings.Where(x=>x.IsChanged))
				saveTasks.Add(Mediator.Send(new UpdateSystemSettingsCommand { Key = ss.Key, Value = ss.Value, DateType = ss.DataType }));
			

			try
			{
				await Task.WhenAll(saveTasks);
				
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
			SystemSettings = systemSettings.Select(x => new SystemSettingModel(x)).ToList();
		}
	}

	public class SystemSettingModel
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
			DataType = systemSetting.DataType;
			Info = systemSetting.Info;
			IsChanged = false;
		}

		public string Key { get => _key; set => Set(ref _key, value); }

		public string Value { get => _value; set => Set(ref _value, value); }

		public string DataType { get; set; }

		public string Info { get; set; }

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
