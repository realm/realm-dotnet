using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public static class TestHelpers
    {
        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propInfo = null;
            do
            {
                propInfo = type.GetTypeInfo().GetDeclaredProperty(propertyName);
                type = type.GetTypeInfo().BaseType;
            }
            while (propInfo == null && type != null);
            return propInfo;
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            var propInfo = GetPropertyInfo(obj.GetType(), propertyName);
            return (T)propInfo.GetValue(obj, null);
        }

    }
}
