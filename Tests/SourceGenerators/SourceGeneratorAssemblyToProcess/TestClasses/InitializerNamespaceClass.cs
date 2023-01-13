using System;
using Realms;
using MongoDB.Bson;

namespace SourceGeneratorAssemblyToProcess
{
    public partial class InitializerNamespaceClass : IRealmObject
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}

