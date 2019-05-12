using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SaaSOvation.Common.Port.Adapters.Persistence
{
    /// <summary>
    ///     TODO: consider refactoring away from abstract class in favor of composition
    /// </summary>
    public abstract class AbstractQueryService
    {
        private readonly string connectionString;

        private readonly DbProviderFactory providerFactory;

        public AbstractQueryService(string connectionString, string providerName)
        {
            providerFactory = DbProviderFactories.GetFactory(providerName);
            this.connectionString = connectionString;
        }

        protected T QueryObject<T>(string query, JoinOn joinOn, params object[] arguments)
        {
            using (var conn = CreateOpenConnection())
            using (var selectStatement = CreateCommand(conn, query, arguments))
            using (var dataReader = selectStatement.ExecuteReader())
            {
                if (dataReader.Read())
                    return new ResultSetObjectMapper<T>(dataReader, joinOn).MapResultToType();
                return default;
            }
        }

        protected IList<T> QueryObjects<T>(string query, JoinOn joinOn, params object[] arguments)
        {
            using (var conn = CreateOpenConnection())
            using (var selectStatement = CreateCommand(conn, query, arguments))
            using (var dataReader = selectStatement.ExecuteReader())
            {
                var objects = new List<T>();
                while (dataReader.Read())
                {
                    var obj = new ResultSetObjectMapper<T>(dataReader, joinOn).MapResultToType();
                    objects.Add(obj);
                }

                return objects;
            }
        }

        protected string QueryString(string query, params object[] arguments)
        {
            using (var conn = CreateOpenConnection())
            using (var selectStatement = CreateCommand(conn, query, arguments))
            using (var dataReader = selectStatement.ExecuteReader())
            {
                if (dataReader.Read())
                    return dataReader.GetString(0);
                return null;
            }
        }

        private DbCommand CreateCommand(DbConnection conn, string query, object[] args)
        {
            var command = conn.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;

            for (var i = 0; i < args.Length; i++)
            {
                var argument = args[i];
                var argumentType = argument.GetType();

                var parameter = command.CreateParameter();
                parameter.Value = argument;

                command.Parameters.Add(parameter);
            }

            return command;
        }

        private DbConnection CreateOpenConnection()
        {
            var conn = providerFactory.CreateConnection();
            conn.ConnectionString = connectionString;
            conn.Open();
            return conn;
        }
    }
}