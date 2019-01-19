using DbUp.Engine;
using DbUp.Engine.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Improving.DbUp.Hashed
{
    public class HashedScriptsProvider : IScriptProvider
    {
        private readonly IHashedJournal _journal;
        private readonly IScriptProvider _underlyingScriptProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashedScriptsProvider" /> class.
        /// </summary>
        /// <param name="journal">The Journal</param>
        public HashedScriptsProvider(IHashedJournal journal)
        {
            _journal = journal;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashedScriptsProvider" /> class.
        /// </summary>
        /// <param name="journal">The Journal</param>
        /// <param name="underlyingScriptsProvider">Native script provider. EmbeddedScriptsProvider, FileSystemScriptsProvider, etc</param>
        public HashedScriptsProvider(IHashedJournal journal, IScriptProvider underlyingScriptsProvider)
        {
            _journal = journal;
            this._underlyingScriptProvider = underlyingScriptsProvider;
        }

        /// <summary>
        /// Gets all scripts. <see cref="GetScripts(IConnectionManager)"/> filters already executeds scripts from the list returned by this method.
        /// </summary>
        /// <returns>Sql scripts</returns>
        protected virtual IEnumerable<SqlScript> GetAllScripts(IConnectionManager connectionManager)
        {
            return this._underlyingScriptProvider.GetScripts(connectionManager);
        }

        /// <summary>
        ///     Gets all scripts that should be executed.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var executedScriptInfo = _journal.GetExecutedScriptDictionary();
            var allScripts = this.GetAllScripts(connectionManager);

            return allScripts
                .Where(script =>
                !executedScriptInfo.ContainsKey(script.Name)
                    || (executedScriptInfo.ContainsKey(script.Name) && executedScriptInfo[script.Name] != Md5Utils.Md5EncodeString(script.Contents)))
                .ToList();
        }
    }
}
