using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Realms;

namespace PurePCLViewModel
{

    class MePersist : RealmObject
    {
        public string MyName { get; set; }
        public int MyAwesomeness { get; set; }
    }


    public class Model : INotifyPropertyChanged
    {
        public string TheAnswer { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Model()
        {
        }

        public void TestRealm()
        {
            var _realm = Realm.GetInstance();
            for (int i=0; i<10; ++i)
                using (var trans = _realm.BeginWrite())
                {
                    var MeNess = _realm.CreateObject<MePersist>();
                    MeNess.MyName = $"Thor {i}";
                    MeNess.MyAwesomeness = 10 * i;
                    trans.Commit();
                }
            var numAwe = _realm.All<MePersist>().Count();

            var timeStamp = DateTimeOffset.Now.ToString();
            var sb = new StringBuilder();
            foreach (var aThor in _realm.All<MePersist>())
                sb.AppendLine(aThor.MyName);  // get the names back out to prove that data saved

            // finished with Realm now
            _realm.Close();

            TheAnswer = $"{timeStamp}\n{numAwe} realm objects created:\n{sb.ToString()}";
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("TheAnswer"));  // normally woudl be setter
        }
    }
}
