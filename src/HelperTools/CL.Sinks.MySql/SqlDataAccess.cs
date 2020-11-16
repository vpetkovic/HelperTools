using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL.Sinks.MySql
{
    public interface ISqlDataAccess
    {
        IDbConnection MySqlConn(string connName = "Default");
        IDbConnection MSSqlConn(string connName = "Default");
        Task<List<T>> LoadFromSqlAsync<T, T1>(string sql, T1 parameters, string connName = "Default");
        Task<List<T>> LoadFromStoredProcedureAsync<T, T1>(string storedProcedure, T1 parameters, string connName = "Default");
        void SaveFromSql<T>(string sql, T parameters, string connName = "Default");
        void SaveFromStoredProcedure<T>(string storedProcedure, T parameters, string connName = "Default");
    }
    public class MySqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;

        public MySqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection MySqlConn(string connName = "Default")
        {
            return new MySqlConnection(_config.GetConnectionString(connName));
        }

        public async Task<List<T>> LoadFromSqlAsync<T, T1>(string sql, T1 parameters, string connName = "Default")
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString(connName)))
            {
                await conn.OpenAsync();

                var data = await conn.QueryAsync<T>(sql, parameters).ConfigureAwait(true);

                return data.ToList();
            }
        }

        public async Task<List<T>> LoadFromStoredProcedureAsync<T, T1>(string storedProcedure, T1 parameters, string connName = "Default")
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString(connName)))
            {
                var data = await conn.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(true);

                return data.ToList();
            }
        }

        public void SaveFromSql<T>(string sql, T parameters, string connName = "Default")
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString(connName)))
            {
                conn.Execute(sql, parameters);
            }
        }

        public void SaveFromStoredProcedure<T>(string storedProcedure, T parameters, string connName = "Default")
        {
            using (var conn = new MySqlConnection(_config.GetConnectionString(connName)))
            {
                conn.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public IDbConnection MSSqlConn(string connName = "Default")
        {
            throw new NotImplementedException();
        }
    }

    public class MySqlDb : IDisposable
    {
        public MySqlConnection Connection { get; }

        public MySqlDb(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }

        public void Dispose() => Connection.Dispose();
    }
}
