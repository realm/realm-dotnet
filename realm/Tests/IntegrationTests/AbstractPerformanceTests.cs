using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealmNet;

namespace IntegrationTests
{
    public abstract class AbstractPerformanceTests
    {
        private string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void BaseSetup()
        {
            Setup();
            _databasePath = GetTempDatabasePath();
            _realm = Realm.GetInstance(_databasePath);
        }

        protected virtual void Setup () { }
        protected abstract string GetTempDatabasePath();

        [Test]
        public void SimplePerformanceTest()
        {
            
        }
    }
}
