using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Realms;

namespace QuickJournal
{
    public partial class JournalEntriesPage : ContentPage
    {
        public JournalEntriesPage()
        {
            InitializeComponent();

            var realm = Realm.GetInstance();
            this.JournalEntries.BindingContext = realm.All<JournalEntry>();
        }
    }
}

