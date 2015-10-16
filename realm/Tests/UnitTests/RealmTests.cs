using System.Collections.Generic;
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
        public void GetInstanceShouldCreateSchemaAndCallGetTable()
        {
            // Act
            var r = Realm.GetInstance("");

            // Assert
            Assert.That(Logger.Instance.LogList, Is.EqualTo(new List<string> {
                "NativeObjectSchema.create(name = \"Person\")",
                "NativeObjectSchema.add_property(name = \"FirstName\", type = 2)",
                "NativeObjectSchema.add_property(name = \"LastName\", type = 2)",
                "NativeObjectSchema.add_property(name = \"Email\", type = 2)",
                "NativeObjectSchema.add_property(name = \"IsInteresting\", type = 1)",
                "NativeSharedRealm.get_table(tableName = \"class_Person\")"
            }));
        }

        [Test]
        public void CreateObjectShouldAddEmptyRow()
        {
            // Arrange
            var r = Realm.GetInstance("");
            Logger.Clear();

            // Act
            r.CreateObject<Person>();

            // Assert
            Assert.That(Logger.Instance.LogList[0], Is.EqualTo("NativeTable.add_empty_row()"));
        }

        [Test]
        public void AllShouldCreateQuery()
        {
            // Arrange
            var r = Realm.GetInstance("");
            var q = r.All<Person>();
            Logger.Clear();

            // Act
            q.ToList();

            // Assert
            Assert.That(Logger.Instance.LogList[0], Is.EqualTo("NativeTable.where()"));
            Assert.That(Logger.Instance.LogList[2], Is.EqualTo("NativeQuery.find()"));
        }
    }
}
