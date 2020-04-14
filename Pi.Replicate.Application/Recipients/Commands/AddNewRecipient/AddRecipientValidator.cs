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
				.Must(x => !workerContext.Recipients.ToList().Any(y => string.Equals(y.Name, x, StringComparison.OrdinalIgnoreCase)))
				.WithMessage(x => $"The name '{x}' is not unique");

			RuleFor(x => x.Address)
				.NotEmpty()
				.WithMessage("Address canot be empty");

			RuleFor(x => x.Address)
				.Must(x => !workerContext.Recipients.ToList().Any(y => string.Equals(y.Address, x, StringComparison.OrdinalIgnoreCase)))
				.WithMessage(x => $"There is already a recipient with the address '{x}'");
		}
	}
}
