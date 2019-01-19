using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Improving.DbUp.Hashed
{
    public class HashedSqlTableJournal : SqlTableJournal, IHashedJournal
    {
        private const string ScriptHashParameter = "@scriptHash";

        private readonly ISqlObjectParser _sqlObjectParser;

        public HashedSqlTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string table)
            : base(connectionManager, logger, schema, table)
        {
            this._sqlObjectParser = new SqlServerObjectParser();
        }

        protected override string GetInsertJournalEntrySql(string scriptName, string applied)
        {
            return this.GetInsertJournalEntrySql(scriptName, applied, ScriptHashParameter);
        }

        private string GetInsertJournalEntrySql(string @scriptName, string @applied, string @scriptHash)
        {

            return $@"MERGE {FqSchemaTableName} AS [Target] 
                    USING(SELECT {@scriptName} as scriptName, {@applied} as applied, {@scriptHash} as scriptHash) AS [Source] 
                    ON [Target].scriptName = [Source].scriptName 
                    WHEN MATCHED THEN 
                        UPDATE SET[Target].applied = [Source].applied, [Target].scriptHash = [Source].scriptHash 
                    WHEN NOT MATCHED THEN 
                        INSERT(ScriptName, Applied, ScriptHash) VALUES([Source].scriptName, [Source].applied, [Source].scriptHash); ";
        }

        public override void StoreExecutedScript(SqlScript script, Func<IDbCommand> dbCommandFactory)
        {
            EnsureTableExistsAndIsLatestVersion(dbCommandFactory);
            using (var command = this.GetInsertScriptCommand(dbCommandFactory, script))
            {
                command.ExecuteNonQuery();
            }
        }

        private new IDbCommand GetInsertScriptCommand(Func<IDbCommand> dbCommandFactory, SqlScript script)
        {
            var command = base.GetInsertScriptCommand(dbCommandFactory, script);

            var scriptHashParameter = command.CreateParameter();
            scriptHashParameter.ParameterName = "scriptHash";
            scriptHashParameter.Value = Md5Utils.Md5EncodeString(script.Contents);
            command.Parameters.Add(scriptHashParameter);

            return command;
        }

        protected override string GetJournalEntriesSql()
        {
            // note: the HashedEmbeddedScriptsProvider implementation will deal with "already/should" run determination so we don't want any "executed scripts" in the pipeline
            return $"SELECT * FROM {FqSchemaTableName} WHERE 1 = 2";
        }

        protected override string CreateSchemaTableSql(string quotedPrimaryKeyName)
        {
            return $@"create table {FqSchemaTableName} (
	            [Id] int identity(1,1) not null constraint {quotedPrimaryKeyName} primary key,
	            [ScriptName] nvarchar(255) not null,
	            [Applied] datetime not null,
                [ScriptHash] nvarchar(255) null
            )";
        }

        public override void EnsureTableExistsAndIsLatestVersion(Func<IDbCommand> dbCommandFactory)
        {
            base.EnsureTableExistsAndIsLatestVersion(dbCommandFactory);

            var createHashColumnSql = $"if not exists (select column_name from INFORMATION_SCHEMA.columns where table_name = '{UnquotedSchemaTableName}' " +
                $"  and column_name = 'ScriptHash')" +
                $"  alter table {FqSchemaTableName} add ScriptHash nvarchar(255)";

            Log().WriteInformation(string.Format($"Adding ScriptHash column to ${FqSchemaTableName} table"));
            // We will never change the schema of the initial table create.
            using (var command = dbCommandFactory())
            {
                command.CommandText = createHashColumnSql;
                command.CommandType = CommandType.Text;

                command.ExecuteNonQuery();
            }
        }


        // ----------------------

        public Dictionary<string, string> GetExecutedScriptDictionary()
        {
            Log().WriteInformation("Fetching list of already executed scripts with their known hash.");
            ConnectionManager().ExecuteCommandsWithManagedConnection(this.EnsureTableExistsAndIsLatestVersion);

            var scripts = new Dictionary<string, string>();
            ConnectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                using (var command = dbCommandFactory())
                {
                    command.CommandText = GetExecutedScriptsSql();
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
        protected virtual string GetExecutedScriptsSql()
        {
            return $"select [ScriptName], [ScriptHash] from {FqSchemaTableName} order by [ScriptName]";
        }
    }
}
