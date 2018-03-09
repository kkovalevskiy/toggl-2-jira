using EnsureThat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Toggl2Jira.Core.Model
{
    public static class UnorderedCollectionComparer<T>
    {
        public static UnorderedCollectionComparer<T, TKey> WithKey<TKey>(Func<T, TKey> keySelector, IEqualityComparer<T> itemComparer = null)
        {
            return new UnorderedCollectionComparer<T, TKey>(keySelector, itemComparer);
        }
    }

    public class UnorderedCollectionComparer<T, TKey> : NullableEqualityComparer<IEnumerable<T>>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly IEqualityComparer<T> _itemComparer;

        public UnorderedCollectionComparer(Func<T, TKey> keySelector, IEqualityComparer<T> itemComparer = null)
        {
            EnsureArg.IsNotNull(keySelector);
            _keySelector = keySelector;
            if (itemComparer == null)
            {
                itemComparer = EqualityComparer<T>.Default;
            }

            _itemComparer = itemComparer;
        }

        protected override bool EqualsImpl(IEnumerable<T> x, IEnumerable<T> y)
        {
            var orderedX = x.OrderBy(_keySelector);
            var orderedY = y.OrderBy(_keySelector);
            return orderedX.SequenceEqual(orderedY, _itemComparer);
        }
    }
}