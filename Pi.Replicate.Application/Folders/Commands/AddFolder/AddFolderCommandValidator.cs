using FluentValidation;
using Pi.Replicate.Application.Common.Interfaces;
using System;

namespace Pi.Replicate.Application.Folders.Commands.AddFolder
{
	public class AddFolderCommandValidator : AbstractValidator<AddFolderCommand>
	{
		public AddFolderCommandValidator(IDatabase database)
		{

			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage("A folder or name for a folder must be provided");

			RuleFor(x => x.Name)
				.MustAsync(async (x, y) =>
				{
					using (database)
					{
						var folderid = await database.QuerySingle<Guid>("SELECT Id FROM dbo.Folder WHERE Name = @Name", new { Name = x });
						return folderid == Guid.Empty;
					}
				})
				.WithMessage(x => $"Folder '{x.Name}' already exists.");

			RuleFor(x => x.Recipients)
				.NotNull()
				.NotEmpty()
				.WithMessage("I need a place to sync too, atleast 1 recipient must be selected.");
		}
	}
}
