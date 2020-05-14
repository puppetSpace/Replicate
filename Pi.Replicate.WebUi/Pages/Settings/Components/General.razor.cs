using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Observr;
using Pi.Replicate.Application.SystemSettings.Commands.UpdateSystemSettings;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingOverview;
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
	public class GeneralBase : ComponentBase
	{

		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		public List<SystemSettingViewModel> SystemSettings { get; set; } = new List<SystemSettingViewModel>();

		protected List<string> ValidationMessages { get; set; } = new List<string>();

		public async Task<bool> Save()
		{
			ValidationMessages.Clear();
			var saveTasks = new List<Task>();
			foreach(var ss in SystemSettings.Where(x=>x.IsChanged))
				saveTasks.Add(Mediator.Send(new UpdateSystemSettingsCommand { Key = ss.Key, Value = ss.Value, DateType = ss.DataType }));

			try
			{
				await Task.WhenAll(saveTasks);
				SystemSettings.ForEach(x => x.ResetState());
				return true;
			}
			catch (ValidationException ex)
			{
				ValidationMessages = ex.Errors.Select(x => x.ErrorMessage).ToList();
				StateHasChanged();
				await JSRuntime.InvokeVoidAsync("scrollIntoView", "settings-general-top");
				return false;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			var systemSettingResult = await Mediator.Send(new GetSystemSettingOverviewQuery());
			if (systemSettingResult.WasSuccessful)
				SystemSettings = systemSettingResult.Data.ToList();
		}
	}
}
