using System;
using Realms;

namespace SourceGeneratorPlayground
{
    public class OuterClass
    {
        public partial class NestedClass : IRealmObject
        {
            public int Id { get; set; }
        }
    }
}

