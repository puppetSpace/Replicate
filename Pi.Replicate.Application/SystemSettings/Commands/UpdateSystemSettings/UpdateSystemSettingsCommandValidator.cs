using FluentValidation;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Commands.UpdateSystemSettings
{
	public class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
	{
		public UpdateSystemSettingsCommandValidator()
		{
			RuleFor(x => x.Value)
				.NotEmpty()
				.WithMessage(x => $"Value for {x.Key} cannot be empty");

			RuleFor(x => x.Value)
				.Must(x => int.TryParse(x, out var _))
				.WithMessage(x => $"value of {x.Key} should be a number")
				.When(x => string.Equals(x.DateType,"number",StringComparison.OrdinalIgnoreCase));

			RuleFor(x => x.Value)
				.Must(x => int.TryParse(x, out var _))
				.WithMessage(x => $"value of {x.Key} should be greater than 0")
				.When(x => int.TryParse(x.Value, out var _) && string.Equals(x.DateType,"number",StringComparison.OrdinalIgnoreCase));

			RuleFor(x => x.Value)
				.Must(x => System.IO.Directory.Exists(x))
				.WithMessage(x => $"Folder {x.Value} does not exists")
				.When(x => !string.IsNullOrEmpty(x.Value) && x.Key == Constants.ReplicateBasePath);
		}
	}
}
