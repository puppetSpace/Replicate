using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Commands.UpsertSystemSettings
{
	public class UpsertSystemSettingsCommand : IRequest
	{
		public string Key { get; set; }
		public string Value { get; set; }

	}

	public class UpsertSystemSettingsCommandHandler : IRequestHandler<UpsertSystemSettingsCommand>
	{
		private readonly IDatabase _database;
		private const string _insertStatement = @"
			IF EXISTS(SELECT 1 FROM dbo.SystemSetting where [Key] = @Key)
				BEGIN
				UPDATE dbo.SystemSetting Set [Value] = @Value WHERE [Key] = @Key;
				END
			ELSE
				BEGIN
					INSERT INTO dbo.SystemSetting(Id,[Key],[Value]) VALUES (@Id,@Key,@Value);
				END";

		public UpsertSystemSettingsCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(UpsertSystemSettingsCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var systemSetting = SystemSetting.Build(request.Key, request.Value);
				await _database.Execute(_insertStatement, new { systemSetting.Id, systemSetting.Key, systemSetting.Value });
			}

			return Unit.Value;
		}
	}
}
