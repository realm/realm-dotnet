using System.Linq;
using NUnit.Framework;
using RealmNet;

namespace UnitTests
{
    [TestFixture]
    public class RealmTests
    {
        [SetUp]
        public void Setup()
        {
            Logger.Instance = new Logger();
        }

        [Test]
        public void GetInstanceShouldCallGetTable()
        {
            // Act
            var r = Realm.GetInstance("");

            // Assert
            Assert.That(Logger.Instance.LogList[0], Is.EqualTo("NativeSharedRealm.get_table(tableName = \"class_Person\")"));
        }

        [Test]
        public void CreateObjectShouldAddEmptyRow()
        {
            // Arrange
            var r = Realm.GetInstance("");

            // Act
            r.CreateObject<Person>();

            // Assert
            Assert.That(Logger.Instance.LogList[1], Is.EqualTo("NativeTable.add_empty_row()"));
        }

        [Test]
        public void AllShouldCreateQuery()
        {
            // Arrange
            var r = Realm.GetInstance("");
            var q = r.All<Person>();

            // Act
            q.ToList();

            // Assert
            Assert.That(Logger.Instance.LogList[1], Is.EqualTo("NativeTable.where()"));
            Assert.That(Logger.Instance.LogList[2], Is.EqualTo("NativeQuery.find()"));
        }
    }
}
