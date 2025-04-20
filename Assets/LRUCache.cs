using System.Collections.Generic;

public class LRUCache<K, V>
{
    private readonly int capacity;
    private Dictionary<K, LinkedListNode<(K key, V value)>> cache;
    private LinkedList<(K key, V value)> lruList;

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        cache = new Dictionary<K, LinkedListNode<(K key, V value)>>();
        lruList = new LinkedList<(K key, V value)>();
    }

    public bool TryGet(K key, out V value)
    {
        if (cache.TryGetValue(key, out var node))
        {
            value = node.Value.value;

            // перемещаем в начало списка
            lruList.Remove(node);
            lruList.AddFirst(node);

            return true;
        }

        value = default;
        return false;
    }

    public void Add(K key, V value)
    {
        if (cache.ContainsKey(key))
        {
            // Обновляем значение
            var node = cache[key];
            lruList.Remove(node);
        }
        else if (cache.Count >= capacity)
        {
            // Удаляем наименее использованный элемент
            var last = lruList.Last;
            if (last != null)
            {
                cache.Remove(last.Value.key);
                lruList.RemoveLast();
            }
        }

        var newNode = new LinkedListNode<(K key, V value)>((key, value));
        lruList.AddFirst(newNode);
        cache[key] = newNode;
    }
}