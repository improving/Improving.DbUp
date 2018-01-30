namespace Improving.DbUp.Hashed
{
    using System;
    using System.Reflection;
    using System.Text;
    using global::DbUp.Builder;
    using global::DbUp.Engine;
    using global::DbUp.Support.SqlServer;

    public static class HashedSqlServerExtensions
    {
        public const string VersionTableName = "SchemaVersions";

        public static UpgradeEngineBuilder HashedSqlDatabase(this SupportedDatabases supported, SqlConnectionManager connectionManager, string versionTableSchema, string versionTableName)
        {
            var builder = new UpgradeEngineBuilder();
            builder.Configure(c => c.ConnectionManager = connectionManager);
            builder.Configure(c => c.ScriptExecutor = new SqlScriptExecutor(() => c.ConnectionManager, () => c.Log, versionTableSchema, () => c.VariablesEnabled, c.ScriptPreprocessors));
            builder.Configure(c => c.Journal = new HashedSqlTableJournal(() => c.ConnectionManager, () => c.Log, versionTableSchema, versionTableName));
            return builder;
        }

        /// <summary>
        /// Adds all scripts found as embedded resources in the given assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="journal">The journal.</param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static UpgradeEngineBuilder WithHashedScriptsEmbeddedInAssembly(this UpgradeEngineBuilder builder, Assembly assembly, Func<string, bool> filter, IJournal journal)
        {
            return WithScripts(builder, new HashedEmbeddedScriptsProvider(assembly, filter, Encoding.Default, journal));
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
