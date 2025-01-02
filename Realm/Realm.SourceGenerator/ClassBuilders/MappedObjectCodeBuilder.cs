////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using Microsoft.CodeAnalysis.CSharp;

namespace Realms.SourceGenerator;

internal class MappedObjectCodeBuilder : ClassCodeBuilderBase
{
    public MappedObjectCodeBuilder(ClassInfo classInfo, GeneratorConfig generatorConfig) : base(classInfo, generatorConfig)
    {
    }

    protected override string GeneratePartialClass()
    {
        var contents = @"private IDictionary<string, RealmValue> _backingStorage = null!;

public void SetBackingStorage(IDictionary<string, RealmValue> dictionary)
{
    _backingStorage = dictionary;
}";

        return $@"[Generated]
[Realms.Preserve(AllMembers = true)]
{SyntaxFacts.GetText(_classInfo.Accessibility)} partial class {_classInfo.Name} : {_baseInterface}, INotifyPropertyChanged
{{
{contents.Indent()}

{GetPropertyChanged().Indent()}
}}";
    }

    private string GetPropertyChanged()
    {
        return _classInfo.HasPropertyChangedEvent ? string.Empty :
            @"#region INotifyPropertyChanged

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

private void RaisePropertyChanged([CallerMemberName] string propertyName = """")
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

#endregion";
    }
}
