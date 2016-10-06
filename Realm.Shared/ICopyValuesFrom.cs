using System.Collections.Generic;

namespace Realms
{

    // Interface allows us to cast a generic RealmList to this and invoke CopyValuesFrom
	public interface ICopyValuesFrom
    {
        void CopyValuesFrom(IEnumerable<RealmObject> values);
    }
}