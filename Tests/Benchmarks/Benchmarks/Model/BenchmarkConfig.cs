using Realms;

namespace Benchmarks.Model
{
    [Explicit]
    public partial class BenchmarkConfig : IRealmObject
    {
        public string SelectedJob { get; set; } = null!;

        public string? Filters { get; set; }
    }
}
