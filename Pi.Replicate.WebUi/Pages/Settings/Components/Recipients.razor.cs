using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pi.Replicate.Application.Recipients.Commands.DeleteRecipient;
using Pi.Replicate.Application.Recipients.Commands.UpsertRecipient;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForSettings;
using Pi.Replicate.Shared;
using Pi.Replicate.WebUi.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings.Components
{
	public class RecipientsBase : ComponentBase
	{
		private readonly List<RecipientViewModel> _toDeleteRecipients = new List<RecipientViewModel>();

		[Inject]
		public ProbeService ProbeService { get; set; }

		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		protected Dialog Dialog { get; set; }

		protected List<string> ValidationMessages { get; set; } = new List<string>();

		protected List<RecipientViewModel> Recipients { get; set; } = new List<RecipientViewModel>();

		public async Task<bool> Save()
		{
			ValidationMessages.Clear();
			var saveTasks = new List<Task>();
			foreach (var recipient in Recipients.Where(x => x.IsChanged || x.IsNew))
				saveTasks.Add(Mediator.Send(new UpsertRecipientCommand { Address = recipient.Address, Name = recipient.Name, Verified = recipient.Verified }));

			foreach (var deleted in _toDeleteRecipients)
				saveTasks.Add(Mediator.Send(new DeleteRecipientCommand { Name = deleted.Name }));
			try
			{
				await Task.WhenAll(saveTasks);
				_toDeleteRecipients.Clear();
				Recipients.ForEach(x => x.ResetState());
				return true;
			}
			catch (ValidationException ex)
			{
				ValidationMessages = ex.Errors.Select(x => x.ErrorMessage).ToList();
				StateHasChanged();
				await JSRuntime.InvokeVoidAsync("scrollIntoView", "settings-recipient-top");
				return false;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			var recipientsResult = await Mediator.Send(new GetRecipientsForSettingsQuery());
			if (recipientsResult.WasSuccessful)
				Recipients = recipientsResult.Data.ToList();
		}

		protected void AddRecipient()
		{
			var newRecipient = new RecipientViewModel();
			newRecipient.SetAsNew();
			Recipients.Insert(0, newRecipient);
		}

		protected async Task VerifyRecipient(RecipientViewModel recipientModel)
		{
			recipientModel.IsVerifying = true;
			var result = await ProbeService.ProbeGet<string>($"{recipientModel.Address}/api/probe");
			recipientModel.Verified = result.IsSuccessful;
			recipientModel.VerifyResult = result.IsSuccessful ? string.Empty : result.Message;
			recipientModel.Name = result.ResponseData;
			recipientModel.IsVerifying = false;
			StateHasChanged();
		}

		protected void DeleteRecipient(RecipientViewModel recipientModel)
		{
			_toDeleteRecipients.Add(recipientModel);
			Recipients.Remove(recipientModel);
		}

	}
}
