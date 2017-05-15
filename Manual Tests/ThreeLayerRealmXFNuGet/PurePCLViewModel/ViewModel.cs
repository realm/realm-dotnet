////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.ComponentModel;
using Realms;
using System.Linq;

namespace PurePCLViewModel
{

    class Hero : RealmObject
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
            using (var _realm = Realm.GetInstance())
            {
                _realm.Write(() =>
                {
					for (int i = 0; i < 10; ++i)  // quick loop to add a few objects
					{
                        _realm.Add(new Hero
                        {
                            SuperName = $"Thor {i}",
                            SuperScore = 10 * i
                        });
					}
				});

                var numAwe = _realm.All<Hero>().Count();

                var timeStamp = DateTimeOffset.Now.ToString();
                var sb = new StringBuilder();
                foreach (var aThor in _realm.All<Hero>())
                {
					sb.AppendLine(aThor.SuperName);  // get the names back out to prove that data saved
				}

                TheAnswer = $"{timeStamp}\n{numAwe} realm objects created:\n{sb.ToString()}";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TheAnswer)));  // normally would be setter
            }
        }
    }
}
