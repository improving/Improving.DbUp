namespace Improving.DbUp.Hashed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using global::DbUp.Engine;
    using global::DbUp.Engine.Transactions;

    public class HashedEmbeddedScriptsProvider : IScriptProvider
    {
        private readonly Assembly _assembly;
        private readonly Encoding _encoding;
        private readonly Func<string, bool> _filter;
        private readonly HashedSqlTableJournal _journal;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashedEmbeddedScriptsProvider" /> class.
        /// </summary>
        /// <param name="assembly">The assemblies to search.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="journal">The Journal</param>
        public HashedEmbeddedScriptsProvider(Assembly assembly, Func<string, bool> filter, Encoding encoding,
            IJournal journal)
        {
            _assembly = assembly;
            _filter = filter;
            _encoding = encoding;
            _journal = (HashedSqlTableJournal)journal;
        }

        /// <summary>
        ///     Gets all scripts that should be executed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var executedScriptInfo = _journal.GetExecutedScriptDictionary();
            var allScripts = GetAssemblyScripts();

            return allScripts
                .Where(script =>
                !executedScriptInfo.ContainsKey(script.Name)
                    || (executedScriptInfo.ContainsKey(script.Name) && executedScriptInfo[script.Name] != Md5Utils.Md5EncodeString(script.Contents)))
                .ToList();
        }

        private IEnumerable<SqlScript> GetAssemblyScripts()
        {
            var allScripts = _assembly.GetManifestResourceNames().Where(_filter).ToArray()
                .Select(resourceName => SqlScript.FromStream(resourceName, _assembly.GetManifestResourceStream(resourceName), _encoding))
                .OrderBy(sqlScript => sqlScript.Name)
                .ToList();

            return allScripts;
        }
    }
}