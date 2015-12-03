using Mono.Cecil;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class MethodReferenceExtensions
{
    public static MethodReference MakeHostInstanceGeneric(this MethodReference @this, params TypeReference[] genericArguments)
    {
        var genericDeclaringType = new GenericInstanceType(@this.DeclaringType);
        foreach (var genericArgument in genericArguments)
        {
            genericDeclaringType.GenericArguments.Add(genericArgument);
        }

        var reference = new MethodReference(@this.Name, @this.ReturnType, genericDeclaringType)
        {
            HasThis = @this.HasThis,
            ExplicitThis = @this.ExplicitThis,
            CallingConvention = @this.CallingConvention
        };

        foreach (var parameter in @this.Parameters)
        {
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        }

        foreach (var genericParam in @this.GenericParameters)
        {
            reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
        }

        return reference;
    }
}
