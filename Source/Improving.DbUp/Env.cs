using System;

namespace Improving.DbUp
{
    public enum Env
    {
        Undefined = 0,
        LOCAL     = 1,
        DEV       = 2,
        QA        = 3,
        UAT       = 4,
        Prod      = 5
    }

    public static class EnvParser
    {
        public static Env Parse(string value)
        {
            if (value == null)         return Env.Undefined;
            if (value == string.Empty) return Env.Prod;

            Env returnValue;
            return Enum.TryParse(value, true, out returnValue)
               ? returnValue
               : Env.Undefined;
        }
    }
}