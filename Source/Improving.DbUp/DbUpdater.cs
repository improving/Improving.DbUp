using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Helpers;
using DbUp.ScriptProviders;
using DbUp.SqlServer;
using Improving.DbUp.Hashed;

namespace Improving.DbUp
{
    public abstract class DbUpdater
    {
        private readonly bool _seedData;
        private readonly Env _env;
        private readonly IUpgradeLog _upgradeLog;

        public virtual string FolderName { get; }
        public virtual string DatabaseName { get; }
        public virtual string ConnectionString { get; }
        public virtual bool UseTransactions { get; set; }
        public virtual IDictionary<string, string> ScriptVariables { get; }

        protected abstract string RootPrefix { get; }
        protected abstract string PathSeparator { get; }

        public DbUpdater(
            string folderName,
            string databaseName,
            string connectionStringName,
            IDictionary<string, string> scriptVariables,
            bool seedData = false,
            Env env = Env.Undefined,
            IUpgradeLog upgradeLog = null)
        {
            _seedData = seedData;
            _env = env;

            _upgradeLog = upgradeLog;
            if (_upgradeLog == null)
            {
                _upgradeLog = new ConsoleUpgradeLog();
            }

            ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString + ";App=" + databaseName + "Migrations";
            UseTransactions = true;
            DatabaseName = databaseName;
            FolderName = folderName;
            ScriptVariables = scriptVariables;
        }

        public bool Run()
        {
            try
            {
                if (IsNew())
                {
                    _upgradeLog.WriteInformation("No '{0}' database found. One will be created.", DatabaseName);
                    CreateDatabase();
                }

                _upgradeLog.WriteInformation("Performing beforeMigrations for '{0}' database.", DatabaseName);
                BeforeMigration();

                _upgradeLog.WriteInformation("Performing migrations for '{0}' database.", DatabaseName);
                MigrateDatabase();

                _upgradeLog.WriteInformation("Executing hashed scripts for '{0}' database.", DatabaseName);
                HashedScripts();

                _upgradeLog.WriteInformation("Executing always run scripts for '{0}' database.", DatabaseName);
                AlwaysRun();

                _upgradeLog.WriteInformation("Executing test scripts for '{0}' database. These are run every time.", DatabaseName);
                UpdateTests();

                if (_seedData)
                {
                    _upgradeLog.WriteInformation("Executing seed scripts for '{0}' database.", DatabaseName);
                    SeedDatabase();
                }
            }
            catch (Exception e)
            {
                _upgradeLog.WriteError(e.Message);

                return false;
            }

            return true;
        }

        private void HashedScripts()
        {
            ExecuteHashedDatabaseActions(
                JoinPath(RootPrefix, FolderName, "Hash"),
                ConnectionString,
                builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
        }

        private void AlwaysRun()
        {
            ExecuteDatabaseActions(
                JoinPath(RootPrefix, FolderName, "AlwaysRun"),
                ConnectionString,
                builder => builder.JournalTo(new NullJournal()));
        }

        private void BeforeMigration()
        {
            ExecuteDatabaseActions(
                JoinPath(RootPrefix, FolderName, "BeforeMigration"),
                ConnectionString,
                builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
        }

        private void MigrateDatabase()
        {
            ExecuteDatabaseActions(
                JoinPath(RootPrefix, FolderName, "Migration"),
                ConnectionString,
                builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
        }

        private void UpdateTests()
        {
            if (_env != Env.LOCAL && _env != Env.DEV && _env != Env.QA)
            {
                _upgradeLog.WriteInformation("Scripts in the test folder are only executed in LOCAL, DEV and QA");
                return;
            }
            ExecuteDatabaseActions(
                JoinPath(RootPrefix, FolderName, "Test"),
                ConnectionString,
                builder => builder.JournalTo(new NullJournal()));
        }

        private void SeedDatabase()
        {
            ExecuteDatabaseActions(
                JoinPath(RootPrefix, FolderName, "Seed"),
                ConnectionString,
                builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
        }

        private void CreateDatabase()
        {
            var masterDbSqlConnection = new SqlConnectionStringBuilder(ConnectionString)
            {
                InitialCatalog = "master"
            };

            ExecuteDatabaseActions(
                JoinPath(RootPrefix, FolderName, "FirstRun"),
                masterDbSqlConnection.ConnectionString,
                builder => builder.JournalTo(new NullJournal()));
        }

        private void ExecuteDatabaseActions(string scriptPrefix, string connectionString,
            Func<UpgradeEngineBuilder, UpgradeEngineBuilder> customBuilderTransform)
        {
            var builder =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithExecutionTimeout(TimeSpan.FromSeconds(2147483647))
                    .WithScripts(UnderlyingScriptProvider(scriptPrefix))
                    .WithVariables(ScriptVariables)
                    .LogToConsole();
            
            PerformUpgrade(customBuilderTransform, builder);
        }

        protected abstract IScriptProvider UnderlyingScriptProvider(string scriptPrefix);

        private void PerformUpgrade(Func<UpgradeEngineBuilder, UpgradeEngineBuilder> customBuilderTransform, UpgradeEngineBuilder builder)
        {
            var upgrader = customBuilderTransform(builder).Build();
            var createResult = upgrader.PerformUpgrade();

            if (!createResult.Successful)
            {
                _upgradeLog.WriteError(createResult.Error.Message);
                throw createResult.Error;
            }
            else
            {
                _upgradeLog.WriteInformation("Success!");
            }
        }

        private void ExecuteHashedDatabaseActions(string scriptPrefix, string connectionString,
            Func<UpgradeEngineBuilder, UpgradeEngineBuilder> customBuilderTransform)
        {
            var sqlConnectionManager = new SqlConnectionManager(connectionString);
            var journal = new HashedSqlTableJournal(() => sqlConnectionManager, () => _upgradeLog, null, HashedSqlServerExtensions.VersionTableName);

            var builder =
                DeployChanges.To
                    .HashedSqlDatabase(sqlConnectionManager)
                    .WithExecutionTimeout(TimeSpan.FromSeconds(2147483647))
                    .WithHashedScripts(UnderlyingScriptProvider(scriptPrefix), journal)
                    .WithVariables(ScriptVariables)
                    .LogToConsole();

            PerformUpgrade(customBuilderTransform, builder);
        }

        private bool IsNew()
        {
            var masterDbSqlConnection = new SqlConnectionStringBuilder(ConnectionString)
            {
                InitialCatalog = "master"
            };
            using (var connection = new SqlConnection(masterDbSqlConnection.ConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        $"SELECT name FROM sys.databases WHERE name = N'{DatabaseName}'";
                    command.CommandType = CommandType.Text;

                    connection.Open();
                    var db = command.ExecuteScalar();
                    connection.Close();

                    return db == null;
                }
            }
        }

        private string JoinPath(params string[] values)
        {
            return string.Join(PathSeparator, values.Where(p => !String.IsNullOrWhiteSpace(p?.Trim())));
        }

        #region Factory Methods
        public static DbUpdater UsingEmbeddedScripts(Assembly migrationAssembly,
            string folderName,
            string databaseName,
            string connectionStringName,
            IDictionary<string, string> scriptVariables,
            bool seedData = false,
            Env env = Env.Undefined,
            IUpgradeLog logger = null)
        {
            return new EmbeddedScriptsDbUpdater(migrationAssembly, folderName, databaseName, connectionStringName, scriptVariables, seedData, env, logger);
        }

        public static DbUpdater UsingScriptsInFileSystem(string folderName,
            string databaseName,
            string connectionStringName,
            IDictionary<string, string> scriptVariables,
            FileSystemScriptOptions fileSystemScriptOptions = null,
            bool seedData = false,
            Env env = Env.Undefined,
            IUpgradeLog logger = null)
        {
            if (fileSystemScriptOptions == null)
            {
                fileSystemScriptOptions = new FileSystemScriptOptions()
                {
                    IncludeSubDirectories = true,
                    Filter = name => name.EndsWith("*.sql")
                };
            }

            return new FileSystemScriptsDbUpdater(folderName, fileSystemScriptOptions, databaseName, connectionStringName, scriptVariables, seedData, env, logger);
        }
        #endregion
    }
}
