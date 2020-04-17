using FluentValidation;
using FluentValidation.Validators;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .MustAsync(async (x,y) => 
                {
                    using (database)
                    {
                        var folderid = await database.QuerySingle<Guid>("SELECT Id FROM dbo.Folders WHERE Name = @Name", new { Name = x });
                        return folderid != Guid.Empty;
                    }
                })
                .WithMessage(x => $"Folder '{x.Name}' already exists.");

            RuleFor(x => x.Recipients)
                .NotNull()
                .NotEmpty()
                .WithMessage("I need a place to sync too, Atleast 1 recipient must be selected.");
        }
    }
}
