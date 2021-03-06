﻿using System;
using System.Collections.Generic;

namespace SaaSOvation.Common.Domain.Model
{
    public abstract class ComparableValueObject : ValueObject, IComparable
    {
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj)) return 0;
            if (ReferenceEquals(null, obj)) return 1;

            if (GetType() != obj.GetType())
                throw new InvalidOperationException();

            return CompareTo(obj as ComparableValueObject);
        }

        protected abstract IEnumerable<IComparable> GetComparableComponents();

        protected IComparable AsNonGenericComparable<T>(IComparable<T> comparable)
        {
            return new NonGenericComparable<T>(comparable);
        }

        protected int CompareTo(ComparableValueObject other)
        {
            using (var thisComponents = GetComparableComponents().GetEnumerator())
            using (var otherComponents = other.GetComparableComponents().GetEnumerator())
            {
                while (true)
                {
                    var x = thisComponents.MoveNext();
                    var y = otherComponents.MoveNext();
                    if (x != y)
                        throw new InvalidOperationException();
                    if (x)
                    {
                        var c = thisComponents.Current.CompareTo(otherComponents.Current);
                        if (c != 0)
                            return c;
                    }
                    else
                    {
                        break;
                    }
                }

                return 0;
            }
        }

        private class NonGenericComparable<T> : IComparable
        {
            private readonly IComparable<T> comparable;

            public NonGenericComparable(IComparable<T> comparable)
            {
                this.comparable = comparable;
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(comparable, obj)) return 0;
                if (ReferenceEquals(null, obj))
                    throw new ArgumentNullException();
                return comparable.CompareTo((T) obj);
            }
        }
    }
}