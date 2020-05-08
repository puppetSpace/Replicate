using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Commands.UpdateSystemSettings
{
	public class UpdateSystemSettingsCommand : IRequest<Result>
	{
		public string Key { get; set; }
		public string Value { get; set; }
		//pure for validation
		public string DateType { get; set; }
	}

	public class UpdateSystemSettingsCommandHandler : IRequestHandler<UpdateSystemSettingsCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _updateStatement = "UPDATE dbo.SystemSetting Set [Value] = @Value WHERE [Key] = @Key";

		public UpdateSystemSettingsCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				await _database.Execute(_updateStatement, new { request.Key, request.Value });
			}

			return Result.Success();
		}
	}
}
