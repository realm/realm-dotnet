////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

using Realms;
using Realms.Exceptions;
using Xunit;

namespace Tests.XUnit
{
    public partial class SmokeTests : IDisposable
    {
        private Realm? _realm;

        public void Dispose()
        {
            _realm?.Dispose();
            if (_realm != null)
            {
                Realm.DeleteRealm(_realm.Config);
            }
        }

        [Fact]
        public void SynchronousTest()
        {
            _realm = Realm.GetInstance(Guid.NewGuid().ToString());

            _realm.Write(() =>
            {
                _realm.Add(new DummyObject
                {
                    StringProp = "foo",
                });
            });

            Assert.Equal("foo", _realm.All<DummyObject>().Single().StringProp);
        }

        [Fact]
        public async Task AsynchronousTest()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            _realm = await Realm.GetInstanceAsync(config);

            var tcs = new TaskCompletionSource();
            var initialNotification = false;
            using var token = _realm.All<DummyObject>().SubscribeForNotifications((sender, changes) =>
            {
                try
                {
                    if (changes == null)
                    {
                        initialNotification = true;
                    }
                    else
                    {
                        Assert.Empty(changes.ModifiedIndices);
                        Assert.Empty(changes.DeletedIndices);
                        var value = Assert.Single(changes.InsertedIndices);
                        Assert.Equal("foo", sender[value].StringProp);

                        tcs.TrySetResult();
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await _realm.WriteAsync(() =>
            {
                _realm.Add(new DummyObject
                {
                    StringProp = "foo"
                });
            });

            await tcs.Task;

            Assert.True(initialNotification);
        }

        [Fact]
        public async Task WrongThreadTest()
        {
            _realm = Realm.GetInstance(Guid.NewGuid().ToString());

            await Task.Run(() =>
            {
                var ex = Assert.Throws<RealmException>(() => _realm.All<DummyObject>());
                Assert.Contains("incorrect thread", ex.Message);
            });
        }

        public partial class DummyObject : IRealmObject
        {
            public string? StringProp { get; set; }
        }
    }
}
