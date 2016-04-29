using System;

namespace Realms
{
    public class RealmInvalidTransactionException : RealmException
    {
        public RealmInvalidTransactionException(string message) : base(message)
        {
        }
    }
}

