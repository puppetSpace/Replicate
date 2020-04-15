using FluentValidation;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Commands.AddRecipient
{
	public class AddRecipientValidator : AbstractValidator<AddRecipientCommand>
	{

		public AddRecipientValidator(IWorkerContext workerContext)
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage("Name cannot be empty");

			RuleFor(x => x.Name)
				.MustAsync((x,c) => workerContext.RecipientRepository.IsNameUnique(x))
				.WithMessage(x => $"The name '{x}' is not unique");

			RuleFor(x => x.Address)
				.NotEmpty()
				.WithMessage("Address canot be empty");

			RuleFor(x => x.Address)
				.MustAsync((x,c) => workerContext.RecipientRepository.IsAddressUnique(x))
				.WithMessage(x => $"There is already a recipient with the address '{x}'");
		}
	}
}
