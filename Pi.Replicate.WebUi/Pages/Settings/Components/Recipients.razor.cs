using MediatR;
using Microsoft.AspNetCore.Components;
using Observr;
using Pi.Replicate.Application.Recipients.Queries.GetRecipients;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.WebUi.Components;
using Pi.Replicate.WebUi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings.Components
{
    public class RecipientsBase : ComponentBase, Observr.IObserver<SettingsSaveNotificationMessage>, IDisposable
    {
		private List<RecipientModel> _toDeleteRecipients = new List<RecipientModel>();
		private IDisposable _saveNotificationSubscription;

		[Inject]
		public ProbeService ProbeService { get; set; }

		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		protected IBroker SaveNotifier { get; set; }

		protected Dialog Dialog { get; set; }

		protected List<string> ValidationMessages { get; set; } = new List<string>();

		protected List<RecipientModel> Recipients { get; set; } = new List<RecipientModel>();

		protected RecipientModel NewRecipient { get; set; }

		public Task Handle(SettingsSaveNotificationMessage value, CancellationToken cancellationToken)
		{
			Log.Information("Save engaged");
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			Log.Information("Subscription destroyed");
			_saveNotificationSubscription?.Dispose();
		}

		protected override async Task OnInitializedAsync()
		{
			Log.Information("Subscription created");
			_saveNotificationSubscription = SaveNotifier.Subscribe(this);
			var recipients = await Mediator.Send(new GetRecipientsQuery());
			Recipients = recipients.Select(x => new RecipientModel(x)).ToList();
		}

		protected void ShowAddRecipientDialog()
		{
			ValidationMessages.Clear();
			NewRecipient = new RecipientModel();
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

		protected async Task VerifyRecipient(RecipientModel recipientModel)
		{
			recipientModel.IsVerifying = true;
			var result = await ProbeService.Probe(recipientModel.Address);
			recipientModel.Verified = result.IsSuccessful;
			recipientModel.VerifyResult = result.IsSuccessful ? string.Empty : result.Message;
			recipientModel.IsVerifying = false;
		}

		protected void DeleteRecipient(RecipientModel recipientModel)
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

		protected class RecipientModel
		{
			private string _name;
			private string _address;
			private bool _verified;

			public RecipientModel()
			{
			}

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
				set => Set(ref _name, value);
			}

			public string Address
			{
				get => _address;
				set => Set(ref _address, value);
			}

			public bool Verified
			{
				get => _verified;
				set => Set(ref _verified, value);
			}

			public bool IsVerifying { get; set; }

			public string VerifyResult { get; set; }

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
