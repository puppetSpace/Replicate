using FluentValidation;
using FluentValidation.Validators;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.AddNewFolder
{
    public class AddNewFolderCommandValidator : AbstractValidator<AddNewFolderCommand>
    {
        public AddNewFolderCommandValidator(IWorkerContext workerContext)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("A folder or name for a folder must be provided");

            RuleFor(x => x.Name)
                .Must(x => !workerContext.Folders.ToList().Any(y => string.Equals(y.Name, x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage(x => $"Folder '{x.Name}' already exists.");

            RuleFor(x => x.Recipients)
                .NotNull()
                .NotEmpty()
                .WithMessage("I need a place to sync too, Atleast 1 recipient must be selected.");
        }
    }
}
