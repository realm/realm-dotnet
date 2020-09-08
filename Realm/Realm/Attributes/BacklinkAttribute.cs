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

namespace Realms
{
    /// <summary>
    /// An attribute that indicates that the property it decorates is the inverse end of a relationship.
    /// </summary>
    /// <example>
    /// <code>
    /// class Dog : RealmObjectB
    /// {
    ///     // One to many relationship with Person.Dogs
    ///     public Person Owner { get; set; }
    /// }
    ///
    /// class Person : RealmObject
    /// {
    ///     [Backlink(nameof(Dog.Owner))]
    ///     public IQueryable&lt;Dog&gt; Dogs { get; }
    ///
    ///     // Many to many relationship with Hobby.PeopleWithThatHobby
    ///     public IList&lt;Hobby&gt; Hobbies { get; }
    /// }
    ///
    /// class Hobby : RealmObject
    /// {
    ///     [Backlink(nameof(Person.Hobbies))]
    ///     public IQueryable&lt;Person&gt; PeopleWithThatHobby { get; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BacklinkAttribute : Attribute
    {
        internal string Property { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BacklinkAttribute"/> class.
        /// </summary>
        /// <param name="property">The property that is on the other end of the relationship.</param>
        public BacklinkAttribute(string property)
        {
            Property = property;
        }
    }
}
