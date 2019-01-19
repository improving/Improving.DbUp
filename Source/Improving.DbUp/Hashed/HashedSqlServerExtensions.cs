namespace Improving.DbUp.Hashed
{
    using System;
    using System.Reflection;
    using System.Text;
    using global::DbUp;
    using global::DbUp.Builder;
    using global::DbUp.Engine;
    using global::DbUp.ScriptProviders;
    using global::DbUp.SqlServer;
    
    public static class HashedSqlServerExtensions
    {
        public const string VersionTableName = "SchemaVersions";

        private static Func<UpgradeConfiguration, IJournal> JournalFactory = (UpgradeConfiguration upgradeConfiguration) => new HashedSqlTableJournal(() => upgradeConfiguration.ConnectionManager, () => upgradeConfiguration.Log, null, VersionTableName);

        public static UpgradeEngineBuilder HashedSqlDatabase(this SupportedDatabases supported, SqlConnectionManager connectionManager)
        {
            var builder = new UpgradeEngineBuilder();
            builder.Configure(c => c.ConnectionManager = connectionManager);
            builder.Configure(c => c.ScriptExecutor = new SqlScriptExecutor(() => c.ConnectionManager, () => c.Log, null, () => c.VariablesEnabled, c.ScriptPreprocessors, () => JournalFactory(c)));
            builder.Configure(c => c.Journal = JournalFactory(c));
            return builder;
        }
        
        public static UpgradeEngineBuilder WithHashedScripts(this UpgradeEngineBuilder builder, IScriptProvider scriptProvider, IHashedJournal journal)
        {
            var hashedScriptsProvider = new HashedScriptsProvider(journal, scriptProvider);
            return WithScripts(builder, hashedScriptsProvider);
        }

        /// <summary>
        /// Adds a custom script provider to the upgrader.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="scriptProvider">The script provider.</param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static UpgradeEngineBuilder WithScripts(this UpgradeEngineBuilder builder, IScriptProvider scriptProvider)
        {
            builder.Configure(c => c.ScriptProviders.Add(scriptProvider));
            return builder;
        }
    }
}
