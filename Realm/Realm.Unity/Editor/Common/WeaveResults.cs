using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace RealmWeaver
{
    internal class WeaveModuleResult
    {
        public static WeaveModuleResult Success(IEnumerable<WeaveTypeResult> types)
        {
            return new WeaveModuleResult(types.ToArray(), skipReason: null);
        }

        public static WeaveModuleResult Skipped(string reason)
        {
            return new WeaveModuleResult(types: null, skipReason: reason);
        }

        public WeaveTypeResult[] Types { get; }

        public string SkipReason { get; }

        private WeaveModuleResult(WeaveTypeResult[] types, string skipReason)
        {
            Types = types;
            SkipReason = skipReason;
        }

        public override string ToString()
        {
            if (SkipReason != null)
            {
                return SkipReason;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{Types.Length} types were woven:");
            foreach (var type in Types)
            {
                sb.AppendLine(type.ToString());
            }

            return sb.ToString();
        }
    }

    internal class WeaveTypeResult
    {
        public static WeaveTypeResult Success(string type, IEnumerable<WeavePropertyResult> properties)
        {
            return new WeaveTypeResult(type, properties.ToArray());
        }

        public string Type { get; }

        public WeavePropertyResult[] Properties { get; }

        private WeaveTypeResult(string type, WeavePropertyResult[] properties)
        {
            Properties = properties;
            Type = type;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>{Type}</b>");
            foreach (var prop in Properties)
            {
                sb.AppendLine($"  {prop}");
            }

            return sb.ToString();
        }
    }

    internal class WeavePropertyResult
    {
        public static WeavePropertyResult Success(PropertyDefinition property, FieldReference field, bool isPrimaryKey, bool isIndexed)
        {
            return new WeavePropertyResult(property, field, isPrimaryKey, isIndexed);
        }

        public static WeavePropertyResult Warning(string warning)
        {
            return new WeavePropertyResult(warning: warning);
        }

        public static WeavePropertyResult Error(string error)
        {
            return new WeavePropertyResult(error: error);
        }

        public static WeavePropertyResult Skipped()
        {
            return new WeavePropertyResult();
        }

        public string ErrorMessage { get; }

        public string WarningMessage { get; }

        public bool Woven { get; }

        public PropertyDefinition Property { get; }

        public FieldReference Field { get; }

        public bool IsPrimaryKey { get; }

        public bool IsIndexed { get; }

        private WeavePropertyResult(PropertyDefinition property, FieldReference field, bool isPrimaryKey, bool isIndexed)
        {
            Property = property;
            Field = field;
            IsPrimaryKey = isPrimaryKey;
            IsIndexed = isIndexed;
            Woven = true;
        }

        private WeavePropertyResult(string error = null, string warning = null)
        {
            ErrorMessage = error;
            WarningMessage = warning;
        }

        public override string ToString()
        {
            return $"    <i>{Property.Name}</i>: {Property.PropertyType.ToFriendlyString()}{(IsPrimaryKey ? " [PrimaryKey]" : string.Empty)}{(IsIndexed ? " [Indexed]" : string.Empty)}";
        }
    }
}
