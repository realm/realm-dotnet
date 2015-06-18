using System.Collections.Generic;
using RealmIO;

namespace Tests.TestHelpers
{
    public class FakeRow : ICoreRow
    {
        private Dictionary<string, object> _row;

        public FakeRow(Dictionary<string, object> row)
        {
            _row = row;
        }

        public T GetValue<T>(string propertyName)
        {
            return (T) _row[propertyName];
        }

        public void SetValue<T>(string propertyName, T value)
        {
            _row[propertyName] = value;
        }
    }
}
