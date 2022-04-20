using Realms;

namespace Benchmarks.Model
{
    [Explicit]
    public class BenchmarkConfig : RealmObject
    {
        public string SelectedJob { get; set; }

        public string Filters { get; set; }
    }
}
