using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Observr;
using Pi.Replicate.Application.Recipients.Commands.DeleteRecipient;
using Pi.Replicate.Application.Recipients.Commands.UpsertRecipient;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForSettings;
using Pi.Replicate.Infrastructure.Services;
using Pi.Replicate.WebUi.Components;
using Pi.Replicate.WebUi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings.Components
{
	public class RecipientsBase : ComponentBase, Observr.IObserver<SettingsSaveNotificationMessage>, IDisposable
	{
		private List<RecipientViewModel> _toDeleteRecipients = new List<RecipientViewModel>();
		private IDisposable _saveNotificationSubscription;

		[Inject]
		public ProbeService ProbeService { get; set; }

		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		protected IBroker SaveNotifier { get; set; }

		protected Dialog Dialog { get; set; }

		protected List<string> ValidationMessages { get; set; } = new List<string>();

		protected List<RecipientViewModel> Recipients { get; set; } = new List<RecipientViewModel>();

		protected RecipientViewModel NewRecipient { get; set; }

		public async Task Handle(SettingsSaveNotificationMessage value, CancellationToken cancellationToken)
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
			}
			catch (ValidationException ex)
			{
				ValidationMessages = ex.Errors.Select(x => x.ErrorMessage).ToList();
			}
		}

		public void Dispose()
		{
			_saveNotificationSubscription?.Dispose();
		}

		protected override async Task OnInitializedAsync()
		{
			_saveNotificationSubscription = SaveNotifier.Subscribe(this);
			var recipientsResult = await Mediator.Send(new GetRecipientsForSettingsQuery());
			if (recipientsResult.WasSuccessful)
				Recipients = recipientsResult.Data.ToList();
		}

		protected void ShowAddRecipientDialog()
		{
			ValidationMessages.Clear();
			NewRecipient = new RecipientViewModel();
			NewRecipient.SetAsNew();
			Dialog.Show();
		}

		protected void AddRecipient()
		{
			ValidateRecipient();
			if (!ValidationMessages.Any())
			{
				Recipients.Insert(0, NewRecipient);
				Dialog.Close();
				_ = VerifyRecipient(NewRecipient);
			}
		}

		protected async Task VerifyRecipient(RecipientViewModel recipientModel)
		{
			recipientModel.IsVerifying = true;
			var result = await ProbeService.Probe($"{recipientModel.Address}/api/probe");
			recipientModel.Verified = result.IsSuccessful;
			recipientModel.VerifyResult = result.IsSuccessful ? string.Empty : result.Message;
			recipientModel.IsVerifying = false;
			StateHasChanged();
		}

		protected void DeleteRecipient(RecipientViewModel recipientModel)
		{
			_toDeleteRecipients.Add(recipientModel);
			Recipients.Remove(recipientModel);
		}

		private void ValidateRecipient()
		{
			ValidationMessages.Clear();
			if (string.IsNullOrWhiteSpace(NewRecipient.Name))
				ValidationMessages.Add("Name cannot be empty");
			if (string.IsNullOrWhiteSpace(NewRecipient.Address))
				ValidationMessages.Add("Address cannot be empty");
			if (Recipients.Any(x => string.Equals(x.Name, NewRecipient.Name, StringComparison.OrdinalIgnoreCase)))
				ValidationMessages.Add("Name already exists");
			if (Recipients.Any(x => string.Equals(x.Address, NewRecipient.Address, StringComparison.OrdinalIgnoreCase)))
				ValidationMessages.Add("Address already exists for a recipient");
		}

	}
}
