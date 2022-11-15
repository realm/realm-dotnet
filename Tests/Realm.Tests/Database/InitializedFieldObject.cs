namespace Realms.Tests.Database
{
    public static class Generator
    {
        private static int _currentId;

        public static int GetId() => _currentId++;
    }

    public partial class InitializedFieldObject : Realms.IRealmObject
    {
        public int Id { get; set; } = Generator.GetId();
    }
}
