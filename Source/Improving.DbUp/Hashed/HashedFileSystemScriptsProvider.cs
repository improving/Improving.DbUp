using DbUp.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.ScriptProviders;
using System.IO;

namespace Improving.DbUp.Hashed
{
    public class HashedFileSystemScriptsProvider : HashedScriptsProvider, IScriptProvider
    {
        private readonly string _directoryPath;
        private readonly FileSystemScriptOptions _fileSystemScriptOptions;
        private readonly Func<string, bool> _filter;

        public HashedFileSystemScriptsProvider(string directoryPath, FileSystemScriptOptions fileSystemScriptOptions, Func<string, bool> filter, IHashedJournal journal)
            : base(journal)
        {
            _directoryPath = directoryPath;
            _fileSystemScriptOptions = fileSystemScriptOptions;
            this._filter = filter;
        }

        public override char PathSeparator => Path.PathSeparator;

        protected override IEnumerable<SqlScript> GetAllScripts()
        {
            var files = GetAllFiles(_directoryPath, _fileSystemScriptOptions)
                            .Select(file => file.FullName)
                            .Where(_filter)
                            .Select(file => SqlScript.FromFile(file))
                            .OrderBy(script => script.Name)
                            .ToList();

            return files;
        }

        private IEnumerable<FileInfo> GetAllFiles(string directoryPath, FileSystemScriptOptions fileSystemScriptOptions)
        {
            var files = new List<FileInfo>();

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"No directory found at {directoryPath}.");
            }

            var directory = new DirectoryInfo(directoryPath);

            if (fileSystemScriptOptions.IncludeSubDirectories)
            {
                foreach (var subDirectory in directory.EnumerateDirectories())
                {
                    files.AddRange(GetAllFiles(subDirectory.FullName, fileSystemScriptOptions));
                }
            }

            files.AddRange(directory.GetFiles());

            return files;
        }
    }
}
