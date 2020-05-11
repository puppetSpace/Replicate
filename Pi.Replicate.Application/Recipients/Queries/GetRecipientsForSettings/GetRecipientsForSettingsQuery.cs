using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipientsForSettings
{
	public class GetRecipientsForSettingsQuery : IRequest<Result<ICollection<RecipientViewModel>>>
	{

	}

	public class GetRecipientsForSettingsQueryHandler : IRequestHandler<GetRecipientsForSettingsQuery, Result<ICollection<RecipientViewModel>>>
	{
		private readonly IDatabase _database;
		private readonly IMapper _mapper;
		private const string _selectStatement = "SELECT Id,Name,Address,Verified FROM dbo.Recipient";

		public GetRecipientsForSettingsQueryHandler(IDatabase database, IMapper mapper)
		{
			_database = database;
			_mapper = mapper;
		}

		public async Task<Result<ICollection<RecipientViewModel>>> Handle(GetRecipientsForSettingsQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var queryResult = await _database.Query<Recipient>(_selectStatement, null);
				return Result<ICollection<RecipientViewModel>>.Success(_mapper.Map<ICollection<RecipientViewModel>>(queryResult));
			}

		}
	}
}
