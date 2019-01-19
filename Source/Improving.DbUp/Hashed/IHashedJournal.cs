using DbUp.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Improving.DbUp.Hashed
{
    public interface IHashedJournal : IJournal
    {
        Dictionary<string, string> GetExecutedScriptDictionary();
    }
}
