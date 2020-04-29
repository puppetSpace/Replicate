using FluentValidation;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Commands.UpsertRecipient
{
    public class UpsertRecipientCommandValidator : AbstractValidator<UpsertRecipientCommand>
    {
		public UpsertRecipientCommandValidator(IDatabase database)
		{
			RuleFor(x => x.Address)
				.NotEmpty()
				.WithMessage("Address of a recipient cannot be empty");

			RuleFor(x => x.Address)
				.MustAsync(async (x, c) =>
				{
					using(database)
					{
						var exists = await database.QuerySingle<bool>("SELECT 1 from dbo.Recipient where Address = @Address", new { Address = x });
						return !exists;
					}
				}).WithMessage(x=>$"Address '{x.Address}' already exists");
		}
    }
}
