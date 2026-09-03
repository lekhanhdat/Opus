using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemConcurrentHashSet<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new ConcurrentDictionary<T, byte>();

        public void Add(T item) => _dictionary.TryAdd(item, 0);

        public bool Contains(T item) => _dictionary.ContainsKey(item);

        public void Remove(T item) => _dictionary.TryRemove(item, out _);

        public List<T> ToList() => _dictionary.Keys.ToList();

        public int Count => _dictionary.Count;
    }
}
