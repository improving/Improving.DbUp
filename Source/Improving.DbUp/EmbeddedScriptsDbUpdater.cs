using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.ScriptProviders;

namespace Improving.DbUp
{
    public class EmbeddedScriptsDbUpdater : DbUpdater
    {
        private readonly string _namespacePrefix;
        private readonly Assembly _migrationAssembly;

        public EmbeddedScriptsDbUpdater(Assembly migrationAssembly,
            string folderName,
            string databaseName,
            string connectionStringName,
            IDictionary<string, string> scriptVariables,
            bool seedData = false,
            Env env = Env.Undefined,
            IUpgradeLog logger = null)
            : base(folderName, databaseName, connectionStringName, scriptVariables, seedData, env, logger)
        {
            _namespacePrefix = migrationAssembly.GetName().Name;
            _migrationAssembly = migrationAssembly;
        }

        protected override string RootPrefix => _namespacePrefix;

        protected override string PathSeparator => ".";
        
        protected override IScriptProvider UnderlyingScriptProvider(string scriptPrefix)
        {
            return new EmbeddedScriptProvider(_migrationAssembly, name => name.StartsWith(scriptPrefix));
        }
    }
}
