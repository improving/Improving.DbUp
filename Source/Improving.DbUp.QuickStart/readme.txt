
************************IMPORTANT:****************************************
*   Find & replace all instances of "YourDbName" in	App.config &         *
*   0000_MigrateMetaTableToSchemaVersions.sql with the name of your db.  *
**************************************************************************

Program.cs Quick-Start:
-------------------------------------------------------------------------------
using System.Configuration;
using System.Linq;
using System.Reflection;
using Improving.DbUp;

internal class Program
{
    private static readonly string[] ConfigurationVariables =
    {
        "DbName","DatabaseLocation", "LogLocation", "Env", "AppUser", "TestUser"
    };

    private static int Main(string[] args)
    {
        const string connectionStringName = "connectionStringName";
        var scriptVariables = ConfigurationVariables.ToDictionary(s => s, s => ConfigurationManager.AppSettings[s]);
        var env = EnvParser.Parse(scriptVariables["Env"]);
        var shouldSeedData = env == Env.LOCAL;

        var dbName = ConfigurationManager.AppSettings["DbName"];
        var dbUpdater = new DbUpdater(Assembly.GetExecutingAssembly(), "Scripts", dbName, connectionStringName, scriptVariables, shouldSeedData, env);
        return dbUpdater.Run() ? 0 : -1;
    }
}

-------------------------------------------------------------------------------

**************************************************************************
*   Improving.DbUp.QuickStart should not be updated after first installed *
**************************************************************************

See http://dbup.readthedocs.org/en/latest/ for dbup info.

