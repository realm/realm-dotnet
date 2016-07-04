using System;
using Realms.Weaving;

namespace Realms.Dynamic
{
    internal class DynamicRealmObjectHelper : IRealmObjectHelper
    {
        internal static readonly DynamicRealmObjectHelper Instance = new DynamicRealmObjectHelper();

        public RealmObject CreateInstance()
        {
            return new DynamicRealmObject();
        }
    }
}

