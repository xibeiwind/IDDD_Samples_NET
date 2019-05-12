// Copyright 2012,2013 Vaughn Vernon
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

using System;

namespace SaaSOvation.Common.Domain.Model
{
    public abstract class Identity : IEquatable<Identity>, IIdentity
    {
        public Identity()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Identity(string id)
        {
            Id = id;
        }

        public bool Equals(Identity id)
        {
            if (ReferenceEquals(this, id)) return true;
            if (ReferenceEquals(null, id)) return false;
            return Id.Equals(id.Id);
        }

        // currently for Entity Framework, set must be protected, not private.
        // will be fixed in EF 6.
        public string Id { get; protected set; }

        public override bool Equals(object anotherObject)
        {
            return Equals(anotherObject as Identity);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() * 907 + Id.GetHashCode();
        }

        public override string ToString()
        {
            return GetType().Name + " [Id=" + Id + "]";
        }
    }
}