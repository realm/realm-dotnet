using Realms;

namespace SourceGeneratorPlayground
{
    public partial class OuterClass
    {
        private partial class NestedClass : IRealmObject
        {
            public int Id { get; set; }

            public NestedClass? Link { get; set; }
        }
    }
}

