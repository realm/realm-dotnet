﻿// <auto-generated />
#nullable enable

using MongoDB.Bson.Serialization;
using NUnit.Framework;
using Realms;
using Realms.Helpers;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Realms.Tests.Database
{
    public partial class FlexibleSchemaPocTests
    {
        [Generated]
        [Realms.Preserve(AllMembers = true)]
        public partial class Dog : IMappedObject, INotifyPropertyChanged
        {
            private IDictionary<string, RealmValue> _backingStorage = null!;

            public void SetBackingStorage(IDictionary<string, RealmValue> dictionary)
            {
                _backingStorage = dictionary;
            }

            #region INotifyPropertyChanged

            private IDisposable? _notificationToken;

            private event PropertyChangedEventHandler? _propertyChanged;

            /// <inheritdoc />
            public event PropertyChangedEventHandler? PropertyChanged
            {
                add
                {
                    if (_propertyChanged == null)
                    {
                        SubscribeForNotifications();
                    }

                    _propertyChanged += value;
                }

                remove
                {
                    _propertyChanged -= value;

                    if (_propertyChanged == null)
                    {
                        UnsubscribeFromNotifications();
                    }
                }
            }

            partial void OnPropertyChanged(string? propertyName);

            private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                OnPropertyChanged(propertyName);
            }

            private void SubscribeForNotifications()
            {
                _notificationToken = _backingStorage.SubscribeForKeyNotifications((sender, changes) =>
                {
                    if (changes == null)
                    {
                        return;
                    }

                    foreach (var key in changes.ModifiedKeys)
                    {
                        RaisePropertyChanged(key);
                    }

                    // TODO: what do we do with deleted/inserted keys
                });
            }

            private void UnsubscribeFromNotifications()
            {
                _notificationToken?.Dispose();
            }

            #endregion
        }
    }
}
