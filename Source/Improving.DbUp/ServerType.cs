using System;

namespace Improving.DbUp
{
    public enum ServerType
    {
        Undefined    = 0,
        Developer    = 1
    }

    public static class ServerTypeParser
    {
        public static ServerType Parse(string value)
        {
            if (value == null) return ServerType.Undefined;

            ServerType returnValue;
            return Enum.TryParse(value, true, out returnValue)
               ? returnValue
               : ServerType.Undefined;
        }
    }
}
