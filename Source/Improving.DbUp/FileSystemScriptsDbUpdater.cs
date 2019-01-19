using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.ScriptProviders;

namespace Improving.DbUp
{
    public class FileSystemScriptsDbUpdater : DbUpdater
    {
        private readonly FileSystemScriptOptions _fileSystemScriptOptions;

        public FileSystemScriptsDbUpdater(string folderName, 
            FileSystemScriptOptions fileSystemScriptOptions,
            string databaseName, 
            string connectionStringName, 
            IDictionary<string, string> scriptVariables, 
            bool seedData = false, 
            Env env = Env.Undefined, 
            IUpgradeLog upgradeLog = null) 
            : base(folderName, databaseName, connectionStringName, scriptVariables, seedData, env, upgradeLog)
        {
            this._fileSystemScriptOptions = fileSystemScriptOptions;
        }

        protected override string RootPrefix => String.Empty;

        protected override string PathSeparator => Path.PathSeparator.ToString();

        protected override IScriptProvider UnderlyingScriptProvider(string scriptPrefix)
        {
            return new FileSystemScriptProvider(FolderName, _fileSystemScriptOptions);
        }
    }
}
