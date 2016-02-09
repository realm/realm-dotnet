using System;
using System.Collections.Generic;
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
            using (var trans = _realm.BeginWrite())
            {
                var MeNess = _realm.CreateObject<MePersist>();
                MeNess.MyName = "Thor";
                MeNess.MyAwesomeness = 100;
                trans.Commit();
            }
            var numAwe = _realm.All<MePersist>().Count();
            _realm.Close();

            var timeStamp = DateTimeOffset.Now.ToString();
            TheAnswer = $"{timeStamp} {numAwe} realm objects created";
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("TheAnswer"));  // normally woudl be setter
        }
    }
}
