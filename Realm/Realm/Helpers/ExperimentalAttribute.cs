#if !NET8_0_OR_GREATER

using System.ComponentModel;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that an API is experimental and it may change in the future.
/// </summary>
/// <remarks>
/// This attribute allows call sites to be flagged with a diagnostic that indicates that an experimental
/// feature is used. Authors can use this attribute to ship preview features in their assemblies.
/// <br/>
/// This is a polyfill of the ExperimentalAttribute added in .NET 8.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(
    AttributeTargets.Assembly
    | AttributeTargets.Module
    | AttributeTargets.Class
    | AttributeTargets.Struct
    | AttributeTargets.Enum
    | AttributeTargets.Constructor
    | AttributeTargets.Method
    | AttributeTargets.Property
    | AttributeTargets.Field
    | AttributeTargets.Event
    | AttributeTargets.Interface
    | AttributeTargets.Delegate,
    Inherited = false)]
public sealed class ExperimentalAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExperimentalAttribute"/> class, specifying the ID that the compiler will use
    /// when reporting a use of the API the attribute applies to.
    /// </summary>
    /// <param name="diagnosticId">The ID that the compiler will use when reporting a use of the API the attribute applies to.</param>
    public ExperimentalAttribute(string diagnosticId)
    {
        DiagnosticId = diagnosticId;
    }

    /// <summary>
    /// Gets the ID that the compiler will use when reporting a use of the API the attribute applies to.
    /// </summary>
    /// <value>The unique diagnostic ID.</value>
    /// <remarks>
    /// <para>The diagnostic ID is shown in build output for warnings and errors.</para>
    /// <para>This property represents the unique ID that can be used to suppress the warnings or errors, if needed.</para>
    /// </remarks>
    public string DiagnosticId { get; }

    /// <summary>
    /// <para>Gets or sets the URL for corresponding documentation.</para>
    /// <para>The API accepts a format string instead of an actual URL, creating a generic URL that includes the diagnostic ID.</para>
    /// </summary>
    /// <value>The format string that represents a URL to corresponding documentation.</value>
    /// <remarks>
    /// <para>An example format string is <c>https://contoso.com/obsoletion-warnings/{0}</c>.</para>
    /// </remarks>
    public string? UrlFormat { get; set; }
}

#endif
