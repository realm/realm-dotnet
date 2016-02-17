using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Realms;

namespace PurePCLViewModel
{

    class HeroPersist : RealmObject
    {
        public string SuperName { get; set; }
        public int SuperScore { get; set; }
    }


    public class ViewModel : INotifyPropertyChanged
    {
        public string TheAnswer { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
        }

        public void TestRealm()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);  // cleanup previous test run
            var _realm = Realm.GetInstance();
            using (var trans = _realm.BeginWrite()) {
                for (int i = 0; i < 10; ++i)  // quick loop to add a few objects
                {
                    var pclPerson = _realm.CreateObject<HeroPersist>();
                    pclPerson.SuperName = $"Thor {i}";
                    pclPerson.SuperScore = 10 * i;
                }
                trans.Commit();
            }  // transaction wrapping add loop
            var numAwe = _realm.All<HeroPersist>().Count();

            var timeStamp = DateTimeOffset.Now.ToString();
            var sb = new StringBuilder();
            foreach (var aThor in _realm.All<HeroPersist>())
                sb.AppendLine(aThor.SuperName);  // get the names back out to prove that data saved

            // finished with Realm now
            _realm.Close();

            TheAnswer = $"{timeStamp}\n{numAwe} realm objects created:\n{sb.ToString()}";
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("TheAnswer"));  // normally woudl be setter
        }
    }
}
