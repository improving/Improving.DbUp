namespace Improving.DbUp.Hashed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using global::DbUp.Engine;
    using global::DbUp.Engine.Transactions;

    public class HashedEmbeddedScriptsProvider : HashedScriptsProvider, IScriptProvider
    {
        private readonly Assembly _assembly;
        private readonly Encoding _encoding;
        private readonly Func<string, bool> _filter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashedEmbeddedScriptsProvider" /> class.
        /// </summary>
        /// <param name="assembly">The assemblies to search.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="encoding">The encoding.</param>
        public HashedEmbeddedScriptsProvider(Assembly assembly, Func<string, bool> filter, Encoding encoding,
            IHashedJournal journal)
            : base(journal)
        {
            _assembly = assembly;
            _filter = filter;
            _encoding = encoding;
        }

        protected override IEnumerable<SqlScript> GetAllScripts()
        {
            return this.GetAssemblyScripts();
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