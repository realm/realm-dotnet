using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Realms.DataBinding
{
    class RealmObjectCustomPropertyProvider : Windows.UI.Xaml.Data.ICustomPropertyProvider
    {
        public ICustomProperty GetCustomProperty(string name)
        {
            throw new NotImplementedException();
        }

        public ICustomProperty GetIndexedProperty(string name, Type type)
        {
            throw new NotImplementedException();
        }

        public string GetStringRepresentation()
        {
            throw new NotImplementedException();
        }

        public Type Type => throw new NotImplementedException();
    }
}
