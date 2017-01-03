
************************IMPORTANT:****************************************
*   Find & replace all instances of "YourDbName" in	App.config &         *
*   0000_MigrateMetaTableToSchemaVersions.sql with the name of your db.  *
**************************************************************************

Program.cs Quick-Start:
-------------------------------------------------------------------------------
	using Improving.DbUp;
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Reflection;

    internal class Program
    {
        private static readonly string[] _configurationVariables =
        {
            "DbName","DatabaseLocation", "LogLocation", "Env", "AppUser", "TestUser"
        };

        private static int Main(string[] args)
        {
            Dictionary<string, string> scriptVariables = _configurationVariables.ToDictionary(s => s, s => ConfigurationManager.AppSettings[s]);
            var env = (Env)Enum.Parse(typeof(Env), scriptVariables["Env"], true);
            var shouldSeedData = env == Env.LOCAL;

            var dbName = ConfigurationManager.AppSettings["DbName"];
            var dbUpdater = new DbUpdater(Assembly.GetExecutingAssembly(), "Scripts", dbName, scriptVariables, shouldSeedData, dbName, env);
            return dbUpdater.Run() ? 0 : -1;
        }
    }
-------------------------------------------------------------------------------

**************************************************************************
*   Improving.DbUp.QuickStart should not be updated after first installed *
**************************************************************************

See http://dbup.readthedocs.org/en/latest/ for dbup info.

