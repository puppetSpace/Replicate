using FluentValidation;
using Pi.Replicate.Application.Common.Interfaces;
using System;

namespace Pi.Replicate.Application.Recipients.Commands.AddRecipient
{
	public class AddRecipientValidator : AbstractValidator<AddRecipientCommand>
	{

		public AddRecipientValidator(IDatabase database)
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage("Name cannot be empty");

			RuleFor(x => x.Name)
				.MustAsync(async (x, c) =>
				{
					using (database)
					{
						var result = await database.QuerySingle<Guid>("SELECT Id FROM dbo.Recipients where name = @Name", new { Name = x });
						return result != Guid.Empty;
					}
				})
				.WithMessage(x => $"The name '{x}' is not unique");

			RuleFor(x => x.Address)
				.NotEmpty()
				.WithMessage("Address canot be empty");

			RuleFor(x => x.Address)
				.MustAsync(async (x, c) =>
				{
					using (database)
					{
						var result = await database.QuerySingle<Guid>("SELECT Id FROM dbo.Recipients where address = @Address", new { Address = x });
						return result != Guid.Empty;
					}
				})
				.WithMessage(x => $"There is already a recipient with the address '{x}'");
		}
	}
}
