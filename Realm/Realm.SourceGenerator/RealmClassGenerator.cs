using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realms.SourceGenerator
{
    [Generator]
    public class RealmClassGenerator : ISourceGenerator
    {
        /* General notes:
         * - We can repurpose the Weaver Tests once we have an initial generation so that we can have a feeling if it works or not
         * The tests allow us to see if the correct methods are getting called or not
         * - Given that we need to use the weaver anyways, maybe we can simplify the weaving process by emitting something useful from the SG.
         * For example the schema, the list of classes to weave and so on. In case we could also add properties on the generated class, as those
         * can be easily retrieved by the weaver (in that case we can add those to the Content of the nuget package, so it will be included in the compilation
         * of the final project)
         * - Check VSCode if it solves the caching issue
         * 
         * - The idea about testing is correct, just need to set it up
         */
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxContextReceiver scr)
            {
                return;
            }

            foreach (var (classSyntax, classSymbol) in scr.RealmClasses)
            {
                try
                {
                    if (classSymbol.HasAttribute("IgnoredAttribute"))
                    {
                        continue;
                    }

                    var classInfo = new ClassInfo();

                    if (!classSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ClassNotPartial(classSymbol.Name, classSyntax.GetIdentifierLocation()));
                    }

                    if (classSymbol.BaseType.SpecialType != SpecialType.System_Object)
                    {
                        // We will expand this later
                        classInfo.Diagnostics.Add(Diagnostics.ClassWithBaseType(classSymbol.Name, classSyntax.GetIdentifierLocation()));
                    }

                    var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

                    var isEmbedded = classSymbol.IsEmbeddedObject();

                    if (isEmbedded && classSymbol.IsRealmObject())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ClassUnclearDefinition(classSymbol.Name, classSyntax.GetIdentifierLocation()));
                    }

                    //General info
                    classInfo.Namespace = classSymbol.ContainingNamespace.ToDisplayString();
                    classInfo.Name = classSymbol.Name;
                    classInfo.MapTo = (string)classSymbol.GetAttributeArgument("MapToAttribute");
                    classInfo.Accessibility = classSymbol.DeclaredAccessibility;
                    classInfo.TypeSymbol = classSymbol;
                    classInfo.IsEmbedded = isEmbedded;

                    //Properties
                    var propertiesSyntax = classSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                    //By using the property syntax, what happens if the class is partial? What happens if it derives from another class?

                    FillPropertyInfo(semanticModel, classInfo, propertiesSyntax);

                    var props = string.Join(Environment.NewLine, classInfo.Properties.Select(t => t.Name + " " + t.TypeInfo.ToString()));  //TODO For testing

                    if (!classInfo.Properties.Any())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ObjectWithNoProperties(classInfo.Name, classSyntax.GetIdentifierLocation()));
                    }

                    if (classInfo.Properties.Count(p => p.IsPrimaryKey) > 1)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.MultiplePrimaryKeys(classInfo.Name, classSyntax.GetIdentifierLocation()));
                    }

                    SerializeDiagnostics(context, classInfo);

                    classInfo.Diagnostics.ForEach(context.ReportDiagnostic);

                    if (classInfo.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        continue;
                    }

                    var generator = new Generator(classInfo);
                    var generatedFile = generator.GenerateSource();
                    context.AddSource($"{classInfo.Name}_generated.cs", generatedFile);
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostics.UnexpectedError(classSymbol.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        private void FillPropertyInfo(SemanticModel model, ClassInfo classInfo, IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes)
        {
            foreach (var propSyntax in propertyDeclarationSyntaxes)
            {
                var propSymbol = model.GetDeclaredSymbol(propSyntax);

                if (propSymbol.HasAttribute("IgnoredAttribute"))
                {
                    continue;
                }

                var info = new PropertyInfo();

                info.Name = propSymbol.Name;
                info.Accessibility = propSymbol.DeclaredAccessibility;
                info.IsIndexed = propSymbol.HasAttribute("IndexedAttribute");
                info.IsRequired = propSymbol.HasAttribute("RequiredAttribute");
                info.IsPrimaryKey = propSymbol.HasAttribute("PrimaryKeyAttribute");
                info.MapTo = (string)propSymbol.GetAttributeArgument("MapToAttribute");
                info.Backlink = (string)propSymbol.GetAttributeArgument("BacklinkAttribute");
                info.TypeInfo = GetPropertyTypeInfo(classInfo, propSymbol, propSyntax);

                if (info.TypeInfo.IsUnsupported)
                {
                    continue;
                }

                if (!propSyntax.IsAutomaticProperty() && info.TypeInfo.SimpleType == SimpleTypeEnum.Object)
                {
                    classInfo.Diagnostics.Add(Diagnostics.RealmObjectWithoutAutomaticProperty(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    continue;
                }

                if (!propSyntax.HasSetter() && !info.TypeInfo.IsCollection && !info.TypeInfo.IsIQueryable)
                {
                    continue;
                }

                if (info.Backlink != null)
                {
                    if (!info.TypeInfo.IsIQueryable)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.BacklinkNotQueryable(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    }

                    var backlinkType = info.TypeInfo.InternalType.TypeSymbol;
                    var inversePropertyName = info.Backlink;
                    var inverseProperty = backlinkType.GetMembers(inversePropertyName).FirstOrDefault() as IPropertySymbol;
                    var inversePropertyTypeInfo = inverseProperty == null ? null : GetSingleLevelPropertyTypeInfo(inverseProperty.Type);

                    var isSameType = SymbolEqualityComparer.Default.Equals(inversePropertyTypeInfo?.TypeSymbol, classInfo.TypeSymbol);
                    var isCollectionOfSameType = inversePropertyTypeInfo?.IsListOrSet == true
                        && SymbolEqualityComparer.Default.Equals(inversePropertyTypeInfo?.InternalType.TypeSymbol, classInfo.TypeSymbol);

                    if (inversePropertyTypeInfo == null || (!isSameType && !isCollectionOfSameType))
                    {
                        classInfo.Diagnostics.Add(Diagnostics.BacklinkWrongRelationship(classInfo.Name, info.Name, backlinkType.Name, inversePropertyName, propSyntax.GetLocation()));
                    }
                }

                if (info.IsPrimaryKey)
                {
                    if (classInfo.IsEmbedded)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.EmbeddedObjectWithPrimaryKey(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    }

                    if (!info.TypeInfo.IsSupportedPrimaryKeyType())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.PrimaryKeyWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                }

                if (info.IsIndexed && !info.TypeInfo.IsSupportedIndexType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.IndexedWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                }

                if (info.IsRequired)
                {
                    if (info.TypeInfo.NullableAnnotation != NullableAnnotation.None)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.RequiredWithNullability(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                    else if (!info.TypeInfo.IsSupportedRequiredType())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.RequiredWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                }

                classInfo.Properties.Add(info);
            }
        }

        private PropertyTypeInfo GetPropertyTypeInfo(ClassInfo classInfo, IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertySyntax)
        {
            var propertyLocation = propertySyntax.GetLocation();
            var typeSymbol = propertySymbol.Type;
            var typeString = propertySyntax.Type.ToString();

            var propertyType = GetSingleLevelPropertyTypeInfo(typeSymbol);

            if (propertyType.IsUnsupported)
            {
                if (propertySymbol is INamedTypeSymbol namedSymbol && namedSymbol.SpecialType == SpecialType.System_DateTime)
                {
                    classInfo.Diagnostics.Add(Diagnostics.DateTimeNotSupported(classInfo.Name, propertySymbol.Name, propertyLocation));
                }
                else if (propertySymbol.Type.Name == "List")
                {
                    classInfo.Diagnostics.Add(Diagnostics.ListWithoutInterface(classInfo.Name, propertySymbol.Name, propertyLocation));
                }
                else
                {
                    classInfo.Diagnostics.Add(Diagnostics.TypeNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
                }

                return propertyType;  //We are sure we can't produce more diagnostics
            }

            if (!propertyType.SupportsNullability())
            {
                classInfo.Diagnostics.Add(Diagnostics.NullabilityNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
            }

            if (propertyType.IsRealmInteger)
            {
                var argument = (propertyType.TypeSymbol as INamedTypeSymbol).TypeArguments.Single();

                if (!argument.IsValidRealmIntgerType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.RealmIntegerTypeUnsupported(classInfo.Name, propertySymbol.Name,
                        argument.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            else if (propertyType.IsIQueryable)
            {
                var argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();

                var internalType = GetSingleLevelPropertyTypeInfo(argument);

                if (internalType.SimpleType != SimpleTypeEnum.Object)
                {
                    classInfo.Diagnostics.Add(Diagnostics.IQueryableUnsupportedType(classInfo.Name, propertySymbol.Name, propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                if (propertySyntax.HasSetter())
                {
                    classInfo.Diagnostics.Add(Diagnostics.BacklinkWithSetter(classInfo.Name, propertySymbol.Name, propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            else if (propertyType.IsCollection)
            {
                PropertyTypeInfo internalPropertyType;
                ITypeSymbol argument;
                bool isUnsupported = false;

                var collectionTypeString = propertyType.CollectionType.ToString();

                if (propertyType.IsDictionary)
                {
                    var dictionaryArguments = (typeSymbol as INamedTypeSymbol).TypeArguments;
                    var keyArgument = dictionaryArguments[0];
                    var valueArgument = dictionaryArguments[1];

                    if (keyArgument.SpecialType != SpecialType.System_String)
                    {
                        classInfo.Diagnostics.Add(
                            Diagnostics.DictionaryWithNonStringKeys(classInfo.Name, propertySymbol.Name,
                            keyArgument.ToReadableName(), valueArgument.ToReadableName(), propertyLocation));
                        isUnsupported = true;
                    }

                    internalPropertyType = GetSingleLevelPropertyTypeInfo(valueArgument);
                    propertyType.InternalType = internalPropertyType;
                    argument = valueArgument;
                }
                else
                {
                    //List or Set
                    argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();
                    internalPropertyType = GetSingleLevelPropertyTypeInfo(argument);

                    if (propertyType.IsSet && internalPropertyType.SimpleType == SimpleTypeEnum.Object && argument.IsEmbeddedObject())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.SetWithEmbedded(classInfo.Name, propertySymbol.Name, propertyLocation));
                        isUnsupported = true;
                    }

                    propertyType.InternalType = internalPropertyType;
                }

                if (argument.IsRealmInteger())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionRealmInteger(classInfo.Name, propertySymbol.Name, collectionTypeString, propertyLocation));
                    isUnsupported = true;
                }
                else if (internalPropertyType.IsUnsupported)
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionUnsupportedType(classInfo.Name, propertySymbol.Name, collectionTypeString, argument.ToReadableName(), propertyLocation));
                    isUnsupported = true;
                }

                if (propertySyntax.HasSetter())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionWithSetter(classInfo.Name, propertySymbol.Name, collectionTypeString, propertyLocation));
                    isUnsupported = true;
                }

                if (isUnsupported)
                {
                    return PropertyTypeInfo.Unsupported;
                }
            }

            return propertyType;
        }

        private PropertyTypeInfo GetSingleLevelPropertyTypeInfo(ITypeSymbol typeSymbol)
        {
            var nullableAnnotation = typeSymbol.NullableAnnotation;
            if (nullableAnnotation == NullableAnnotation.Annotated)
            {
                if (typeSymbol.Name == "Nullable")
                {
                    typeSymbol = (typeSymbol as INamedTypeSymbol).TypeArguments.First();
                }
                else
                {
                    //This happens only when nullability annotations are enabled
                    typeSymbol = typeSymbol.OriginalDefinition;
                }
            }

            PropertyTypeInfo propInfo = typeSymbol switch
            {
                INamedTypeSymbol when typeSymbol.IsRealmInteger() => PropertyTypeInfo.RealmInteger,
                INamedTypeSymbol when typeSymbol.IsValidIntegerType() => PropertyTypeInfo.Int,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Boolean => PropertyTypeInfo.Bool,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Single => PropertyTypeInfo.Float,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Double => PropertyTypeInfo.Double,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_String => PropertyTypeInfo.String,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Decimal || typeSymbol.Name == "Decimal128" => PropertyTypeInfo.Decimal,
                ITypeSymbol when typeSymbol.ToDisplayString() == "byte[]" => PropertyTypeInfo.Data,
                INamedTypeSymbol when typeSymbol.Name == "ObjectId" => PropertyTypeInfo.ObjectId,
                INamedTypeSymbol when typeSymbol.Name == "Guid" => PropertyTypeInfo.Guid,
                INamedTypeSymbol when typeSymbol.Name == "DateTimeOffset" => PropertyTypeInfo.Date,
                INamedTypeSymbol when typeSymbol.Name == "RealmValue" => PropertyTypeInfo.RealmValue,
                INamedTypeSymbol when typeSymbol.IsRealmObjectBase() => PropertyTypeInfo.Object,
                INamedTypeSymbol when typeSymbol.Name == "IList" => PropertyTypeInfo.List,
                INamedTypeSymbol when typeSymbol.Name == "ISet" => PropertyTypeInfo.Set,
                INamedTypeSymbol when typeSymbol.Name == "IDictionary" => PropertyTypeInfo.Dictionary,
                INamedTypeSymbol when typeSymbol.Name == "IQueryable" => PropertyTypeInfo.IQueryable,
                _ => PropertyTypeInfo.Unsupported
            };

            propInfo.TypeSymbol = typeSymbol;
            propInfo.NullableAnnotation = nullableAnnotation;

            return propInfo;
        }

        private void SerializeDiagnostics(GeneratorExecutionContext context, ClassInfo classInfo)
        {
            if (!classInfo.Diagnostics.Any())
            {
                return;
            }

            var serializedJson = Diagnostics.GetSerializedDiagnostics(classInfo.Diagnostics);
            if (!string.IsNullOrEmpty(serializedJson))
            {
                context.AddSource($"{classInfo.Name}.diagnostics", serializedJson);
            }
        }
    }
}
