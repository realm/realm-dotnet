using System;

namespace Realms
{
    public class RealmMismatchedConfigException : RealmException
    {
        public RealmMismatchedConfigException(string message) : base(message)
        {
        }
    }
}

