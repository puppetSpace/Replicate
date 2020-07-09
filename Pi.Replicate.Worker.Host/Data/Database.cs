﻿using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Data
{
	public class Database : IDatabase, IDatabaseInitializer
	{
		private const string _exceptionString = "Error occured during execution of query";

		public Database(IConfiguration configuration)
		{
			Connection = new SqlConnection(configuration.GetConnectionString("ReplicateDatabase"));
		}

		public IDbConnection Connection { get; }


		public async Task<Result> Execute(string query, object parameters)
		{
			try
			{
				await Connection.ExecuteAsync(query, parameters).ConfigureAwait(false);
				return Result.Success();
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, _exceptionString);
				return Result.Failure();
			}
		}

		public async Task<Result<TE>> Execute<TE>(string query, object parameters)
		{
			try
			{
				var result = await Connection.ExecuteScalarAsync<TE>(query, parameters).ConfigureAwait(false);
				return Result<TE>.Success(result);
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, _exceptionString);
				return Result<TE>.Failure();
			}
		}

		public async Task<Result<ICollection<TE>>> Query<TE>(string query, object parameters)
		{
			try
			{
				var result = await Connection.QueryAsync<TE>(query, parameters).ConfigureAwait(false);
				return Result<ICollection<TE>>.Success(result.ToList());
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, _exceptionString);
				return Result<ICollection<TE>>.Failure();
			}
		}

		public async Task<Result<ICollection<TResult>>> Query<TFirst, TSecond, TResult>(string query, object parameters, Func<TFirst, TSecond, TResult> map, string splitOn = "Id")
		{
			try
			{
				var result = await Connection.QueryAsync(query, map, parameters, splitOn: splitOn).ConfigureAwait(false);
				return Result<ICollection<TResult>>.Success(result.ToList());
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, _exceptionString);
				return Result<ICollection<TResult>>.Failure();
			}
		}

		public async Task<Result<ICollection<TResult>>> Query<TFirst, TSecond, TThird, TResult>(string query, object parameters, Func<TFirst, TSecond, TThird, TResult> map, string splitOn = "Id")
		{
			try
			{
				var result = await Connection.QueryAsync(query, map, parameters, splitOn: splitOn).ConfigureAwait(false);
				return Result<ICollection<TResult>>.Success(result.ToList());
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, _exceptionString);
				return Result<ICollection<TResult>>.Failure();
			}
		}

		public async Task<Result<TE>> QuerySingle<TE>(string query, object parameters)
		{
			try
			{
				var result = await Connection.QueryFirstOrDefaultAsync<TE>(query, parameters).ConfigureAwait(false);
				return Result<TE>.Success(result);
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, _exceptionString);
				return Result<TE>.Failure();
			}
		}

		public void Dispose()
		{
			Connection?.Close();
		}

		void IDatabaseInitializer.Initialize()
		{
			using (Connection)
			{
				var doesDatabaseExist = Connection.ExecuteScalar<bool>("select 1 from sys.databases where name = 'ReplicateDb';");
				if (!doesDatabaseExist)
				{
					//todo get password from environment variable
					Connection.Execute("CREATE DATABASE ReplicateDb");
					Connection.Execute("USE ReplicateDB; CREATE LOGIN Replicator WITH PASSWORD = 'Y9w@*bdnPhM*6AQXbjdzD^33Z^j8B'; CREATE USER Replicator FOR LOGIN Replicator;");
					var server = new Server(new ServerConnection(new Microsoft.Data.SqlClient.SqlConnection(Connection.ConnectionString)));
					server.ConnectionContext.ExecuteNonQuery(System.IO.File.ReadAllText("Created_Db_Structure.sql"));
				}
			}
		}
	}

	public class DatabaseFactory : IDatabaseFactory
	{
		private readonly IConfiguration _configuration;

		public DatabaseFactory(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IDatabase Get()
		{
			return new Database(_configuration);
		}
	}
}
