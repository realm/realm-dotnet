////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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

using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Realms.Server
{
    /// <summary>
    /// A <see cref="INotificationHandler"/> implementation that handles Realm changes based
    /// on a <see cref="Regex"/>.
    /// </summary>
    public abstract class RegexNotificationHandler : INotificationHandler
    {
        private readonly Regex _pathRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexNotificationHandler"/>.
        /// </summary>
        /// <param name="regex">
        /// A regular expression that will be used to match Realm paths against.
        /// </param>
        protected RegexNotificationHandler(string regex)
        {
            _pathRegex = new Regex(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// A method invoked by the <see cref="INotifier"/> when a Realm has changed and
        /// <see cref="ShouldHandle"/> has returned <c>true</c>.
        /// </summary>
        /// <param name="details">
        /// An instance of <see cref="IChangeDetails"/>, containing detailed information
        /// about the changes that have occurred in the Realm.
        /// </param>
        /// <returns>
        /// An awaitable task that, upon completion, signifies that the changes have been processed.
        /// </returns>
        /// <remarks>
        /// Handlers will be invoked sequentially in the order in which they have been supplied
        /// in the <see cref="NotifierConfiguration.Handlers"/>.
        ///
        /// This method will be invoked sequentially for Realms with the same path and in parallel
        /// for different Realms. This means that if the processing takes a lot of time, it will
        /// build up a queue of changes for that Realm path but will not affect notifications from
        /// other Realms.
        /// </remarks>
        public abstract Task HandleChangeAsync(IChangeDetails details);

        /// <summary>
        /// A method, invoked by the <see cref="INotifier"/> when a Realm has changed. If the handler returns
        /// <c>true</c>, <see cref="HandleChangeAsync"/> will then be invoked with information
        /// about the change.
        /// </summary>
        /// <remarks>
        /// If the handler returns <c>false</c> and no other handler wants to be notified about
        /// the Realm at this path, then this method will no longer will be called for Realms
        /// with that path. It is recommended that you always return the same value for a path
        /// and perform any additional handling in the <see cref="HandleChangeAsync"/> method.
        /// </remarks>
        /// <param name="path">
        /// The path to the Realm that has changed. It will be a path relative to the root
        /// of your server.
        /// </param>
        /// <returns>
        /// <c>true</c> if the handler wants to handle the change, <c>false</c> otherwise.
        /// </returns>
        public bool ShouldHandle(string path) => _pathRegex.IsMatch(path);
    }
}
