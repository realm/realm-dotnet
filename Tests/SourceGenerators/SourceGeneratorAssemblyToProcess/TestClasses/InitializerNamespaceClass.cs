using System;
using Realms;

namespace SourceGeneratorAssemblyToProcess
{
    public partial class InitializerNamespaceClass : IRealmObject
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}

