using System;
using System.Collections.Generic;

namespace Toggl2Jira.Core.Model
{
    public abstract class NullableEqualityComparer<T> : EqualityComparer<T> where T:class
    {
        public override bool Equals(T x, T y)
        {
            if(ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if(y == null)
            {
                return false;
            }

            return EqualsImpl(x, y);
        }

        public override int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }

        protected abstract bool EqualsImpl(T x, T y);
    }
}