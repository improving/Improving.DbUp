namespace Improving.DbUp.Hashed
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using global::DbUp.Engine;
    using global::DbUp.Engine.Output;
    using global::DbUp.Engine.Transactions;
    using global::DbUp.Support.SqlServer;

    public class HashedSqlTableJournalOld : IJournal
    {
        private readonly Func<IConnectionManager> _connectionManager;
        private readonly Func<IUpgradeLog> _log;
        private readonly string _schema;
        private readonly string _table;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashedSqlTableJournalOld" /> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        /// <param name="logger">The log.</param>
        /// <param name="schema">The schema that contains the table.</param>
        /// <param name="table">The table name.</param>
        public HashedSqlTableJournalOld(
            Func<IConnectionManager> connectionManager,
            Func<IUpgradeLog> logger,
            string schema,
            string table)
        {
            _schema = schema;
            _table = table;

            _connectionManager = connectionManager;

            _log = logger;
        }

        /// <summary>
        ///     Recalls the version number of the database.
        /// </summary>
        /// <returns>All executed scripts.</returns>
        public string[] GetExecutedScripts()
        {
            // note: the HashedEmbeddedScriptsProvider implementation will deal with "already/should" run determination so we don't want any "executed scripts" in the pipeline
            return new string[0];
        }

        /// <summary>
        ///     Records a database upgrade for a database specified in a given connection string.
        /// </summary>
        /// <param name="script">The script.</param>
        public void StoreExecutedScript(SqlScript script)
        {
            var exists = DoesTableExist();
            if (!exists)
            {
                _log().WriteInformation($"Creating the {CreateTableName(_schema, _table)} table");

                _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                {
                    using (var command = dbCommandFactory())
                    {
                        command.CommandText = CreateTableSql(_schema, _table);

                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }

                    _log()
                        .WriteInformation($"The {CreateTableName(_schema, _table)} table has been created");
                });
            }
            EnsureHashColumnExists();

            _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText =
                        $@"
        MERGE {CreateTableName(_schema, _table)} AS [Target] 
        USING(SELECT @scriptName as scriptName, @applied as applied, @scriptHash as scriptHash) AS [Source] 
        ON [Target].scriptName = [Source].scriptName 
        WHEN MATCHED THEN 
            UPDATE SET[Target].applied = [Source].applied, [Target].scriptHash = [Source].scriptHash 
        WHEN NOT MATCHED THEN 
            INSERT(ScriptName, Applied, ScriptHash) VALUES([Source].scriptName, [Source].applied, [Source].scriptHash); ";

                    // todo: strip off the |HASH" from Name:
                    var scriptNameParam = command.CreateParameter();
                    scriptNameParam.ParameterName = "scriptName";
                    scriptNameParam.Value = script.Name;
                    command.Parameters.Add(scriptNameParam);

                    var appliedParam = command.CreateParameter();
                    appliedParam.ParameterName = "applied";
                    appliedParam.Value = DateTime.Now;
                    command.Parameters.Add(appliedParam);

                    var hashParam = command.CreateParameter();
                    hashParam.ParameterName = "scriptHash";
                    hashParam.Value = Md5Utils.Md5EncodeString(script.Contents);
                    command.Parameters.Add(hashParam);

                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            });
        }

        public Dictionary<string, string> GetExecutedScriptDictionary()
        {
            _log().WriteInformation("Fetching list of already executed scripts with their known hash.");
            var exists = DoesTableExist();
            if (!exists)
            {
                _log()
                    .WriteInformation(
                        $"The {CreateTableName(_schema, _table)} table could not be found. The database is assumed to be at version 0.");
                return new Dictionary<string, string>();
            }
            EnsureHashColumnExists();

            var scripts = new Dictionary<string, string>();
            _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = GetExecutedScriptsSql(_schema, _table);
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            scripts.Add((string)reader[0], reader[1] == DBNull.Value ? string.Empty : (string)reader[1]);
                    }
                }
            });

            return scripts;
        }

        /// <summary>
        ///     Create an SQL statement which will retrieve all executed scripts in order.
        /// </summary>
        protected virtual string GetExecutedScriptsSql(string schema, string table)
        {
            return $"select [ScriptName], [ScriptHash] from {CreateTableName(schema, table)} order by [ScriptName]";
        }

        /// <summary>Generates an SQL statement that, when exectuted, will create the journal database table.</summary>
        /// <param name="schema">Desired schema name supplied by configuration or <c>NULL</c></param>
        /// <param name="table">Desired table name</param>
        /// <returns>A <c>CREATE TABLE</c> SQL statement</returns>
        protected virtual string CreateTableSql(string schema, string table)
        {
            var tableName = CreateTableName(schema, table);
            var primaryKeyConstraintName = CreatePrimaryKeyName(table);

            return $@"create table {tableName} (
	[Id] int identity(1,1) not null constraint {primaryKeyConstraintName} primary key,
	[ScriptName] nvarchar(255) not null,
	[Applied] datetime not null,
    [ScriptHash] nvarchar(255) null
)";
        }

        /// <summary>
        ///     Combine the <c>schema</c> and <c>table</c> values into an appropriately-quoted identifier for the journal
        ///     table.
        /// </summary>
        /// <param name="schema">Desired schema name supplied by configuration or <c>NULL</c></param>
        /// <param name="table">Desired table name</param>
        /// <returns>Quoted journal table identifier</returns>
        protected virtual string CreateTableName(string schema, string table)
        {
            return string.IsNullOrEmpty(schema)
                ? SqlObjectParser.QuoteSqlObjectName(table)
                : SqlObjectParser.QuoteSqlObjectName(schema) + "." + SqlObjectParser.QuoteSqlObjectName(table);
        }

        /// <summary>
        ///     Convert the <c>table</c> value into an appropriately-quoted identifier for the journal table's unique primary
        ///     key.
        /// </summary>
        /// <param name="table">Desired table name</param>
        /// <returns>Quoted journal table primary key identifier</returns>
        protected virtual string CreatePrimaryKeyName(string table)
        {
            return SqlObjectParser.QuoteSqlObjectName("PK_" + table + "_Id");
        }

        private bool DoesTableExist()
        {
            return _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                try
                {
                    using (var command = dbCommandFactory())
                    {
                        return VerifyTableExistsCommand(command, _table, _schema);
                    }
                }
                catch (SqlException)
                {
                    return false;
                }
                catch (DbException)
                {
                    return false;
                }
            });
        }
        protected void EnsureHashColumnExists()
        {
            _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText =
                        $"if not exists (select column_name from INFORMATION_SCHEMA.columns where table_name = '{_table}' and column_name = 'ScriptHash') alter table {_table} add ScriptHash nvarchar(255)";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            });
        }

        /// <summary>Verify, using database-specific queries, if the table exists in the database.</summary>
        /// <param name="command">The <c>IDbCommand</c> to be used for the query</param>
        /// <param name="tableName">The name of the table</param>
        /// <param name="schemaName">The schema for the table</param>
        /// <returns>True if table exists, false otherwise</returns>
        protected virtual bool VerifyTableExistsCommand(IDbCommand command, string tableName, string schemaName)
        {
            command.CommandText = string.IsNullOrEmpty(_schema)
                ? $"select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{tableName}'"
                : $"select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{tableName}' and TABLE_SCHEMA = '{schemaName}'";
            command.CommandType = CommandType.Text;
            var result = command.ExecuteScalar() as int?;
            return result == 1;
        }

    }
}