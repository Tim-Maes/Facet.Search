using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Facet.Search.Generators;

/// <summary>
/// An immutable, equatable array wrapper for incremental source generators.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;

    public EquatableArray(T[] array) => _array = array;

    public EquatableArray(ImmutableArray<T> array) =>
        _array = array.IsDefault ? null : Enumerable.ToArray(array);

    public int Length => _array?.Length ?? 0;
    public bool IsEmpty => _array is null || _array.Length == 0;
    public T this[int index] => _array![index];

    public ImmutableArray<T> AsImmutableArray() =>
        _array is null ? ImmutableArray<T>.Empty : ImmutableArray.Create(_array);

    public bool Equals(EquatableArray<T> other)
    {
        if (_array is null && other._array is null) return true;
        if (_array is null || other._array is null) return false;
        if (_array.Length != other._array.Length) return false;

        for (int i = 0; i < _array.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_array[i], other._array[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (_array is null) return 0;
        unchecked
        {
            var hash = 17;
            foreach (var item in _array)
                hash = hash * 31 + (item?.GetHashCode() ?? 0);
            return hash;
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();

    public Enumerator GetEnumerator() => new(_array ?? Array.Empty<T>());

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    public struct Enumerator
    {
        private readonly T[] _array;
        private int _index;

        internal Enumerator(T[] array)
        {
            _array = array;
            _index = -1;
        }

        public bool MoveNext() => ++_index < _array.Length;
        public readonly T Current => _array[_index];
    }
}

/// <summary>
/// Extension methods for <see cref="EquatableArray{T}"/>.
/// </summary>
internal static class EquatableArrayExtensions
{
    public static EquatableArray<T> ToEquatableArray<T>(this ImmutableArray<T> array)
        where T : IEquatable<T> => new(array);
}
