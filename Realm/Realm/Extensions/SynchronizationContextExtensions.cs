////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Reflection;
using System.Threading;

namespace Realms
{
    internal static class SynchronizationContextExtensions
    {
        private static readonly Lazy<Type> _wpfSyncContext = new Lazy<Type>(() => Type.GetType("System.Windows.Threading.DispatcherSynchronizationContext, WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
        private static readonly Lazy<FieldInfo> _wpfDispatcherFI = new Lazy<FieldInfo>(() => _wpfSyncContext.Value?.GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<Type> _uwpSyncContext = new Lazy<Type>(() => Type.GetType("System.Threading.WinRTCoreDispatcherBasedSynchronizationContext, System.Runtime.WindowsRuntime, Version=4.0.14.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
        private static readonly Lazy<FieldInfo> _uwpRTDispatcherFI = new Lazy<FieldInfo>(() => _uwpSyncContext.Value?.GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Instance));

        public static bool IsSameAs(this SynchronizationContext first, SynchronizationContext second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            return first == second ||
                first.IsSameDispatcherContext(second, _wpfSyncContext, _wpfDispatcherFI) ||
                first.IsSameDispatcherContext(second, _uwpSyncContext, _uwpRTDispatcherFI);
        }

        private static bool IsSameDispatcherContext(this SynchronizationContext first, SynchronizationContext second, Lazy<Type> contextType, Lazy<FieldInfo> dispatcherFI)
        {
            try
            {
                if (contextType.Value == null || contextType.Value == null)
                {
                    return false;
                }

                var firstType = first.GetType();
                var secondType = second.GetType();

                return firstType == contextType.Value &&
                    secondType == contextType.Value &&
                    dispatcherFI.Value.GetValue(first) == dispatcherFI.Value.GetValue(second);
            }
            catch
            {
                return false;
            }
        }
    }
}
