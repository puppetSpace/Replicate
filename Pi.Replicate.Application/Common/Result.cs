using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common
{

	public class Result
	{
		private static Result _success = new Result { WasSuccessful = true };
		private static Result _failure = new Result { WasSuccessful = false };

		public bool WasSuccessful { get; protected set; }

		public static Result Success() => _success;
		public static Result Failure() => _failure;
	}

	public class Result<TE> : Result
	{
		private static Result<TE> _failure = new Result<TE> { WasSuccessful = false, Data = default };
		public TE Data { get; private set; }

		public static Result<TE> Success(TE data)
		{
			return new Result<TE> { WasSuccessful = true, Data = data };
		}

		public static new Result<TE> Failure() => _failure;
	}

}
