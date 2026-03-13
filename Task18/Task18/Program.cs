using System;
using System.Collections.Generic;

public class MyTreeMap<K, V> where K : IComparable<K>
{
    private class Node
    {
        public K Key;
        public V Value;
        public Node Left;
        public Node Right;

        public Node(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }

    private Node root;
    private int size;
    private IComparer<K> comparator;

    public MyTreeMap()
    {
        comparator = Comparer<K>.Default;
        size = 0;
    }

    public MyTreeMap(IComparer<K> comp)
    {
        if (comp != null)
        {
            comparator = comp;
        }
        else
        {
            comparator = Comparer<K>.Default;
        }
        size = 0;
    }

    public void Clear()
    {
        root = null;
        size = 0;
    }

    //Проверка ключа
    public bool ContainsKey(object key)
    {
        if (key is K k)
        {
            Node node = FindNode(k);
            if (node != null)
            {
                return true;
            }
        }
        return false;
    }

    // 5 Проверка значения
    public bool ContainsValue(object value)
    {
        return CheckValue(root, value);
    }

    private bool CheckValue(Node node, object value)
    {
        if (node == null)
        {
            return false;
        }

        if ((node.Value == null && value == null) || (node.Value != null && node.Value.Equals(value)))
        {
            return true;
        }

        bool leftHas = CheckValue(node.Left, value);
        bool rightHas = CheckValue(node.Right, value);

        return leftHas || rightHas;
    }

    //6 Получить все пары
    public ISet<KeyValuePair<K, V>> EntrySet()
    {
        HashSet<KeyValuePair<K, V>> set = new HashSet<KeyValuePair<K, V>>();
        FillEntrySet(root, set);
        return set;
    }

    private void FillEntrySet(Node node, HashSet<KeyValuePair<K, V>> set)
    {
        if (node == null)
        {
            return;
        }

        FillEntrySet(node.Left, set);
        set.Add(new KeyValuePair<K, V>(node.Key, node.Value));
        FillEntrySet(node.Right, set);
    }

    //7) Получить значение 
    public V Get(object key)
    {
        if (key is K k)
        {
            Node node = FindNode(k);
            if (node != null)
            {
                return node.Value;
            }
        }
        return default(V);
    }

    // 8) Пусто 
    public bool IsEmpty()
    {
        if (size == 0)
        {
            return true;
        }
        return false;
    }

    //9) Получить только ключи
    public ISet<K> KeySet()
    {
        HashSet<K> set = new HashSet<K>();
        FillKeySet(root, set);
        return set;
    }

    private void FillKeySet(Node node, HashSet<K> set)
    {
        if (node == null)
        {
            return;
        }

        FillKeySet(node.Left, set);
        set.Add(node.Key);
        FillKeySet(node.Right, set);
    }

    // 10) Добавить пару
    public void Put(K key, V value)
    {
        if (key == null)
        {
            throw new ArgumentNullException("Ключ не может быть null");
        }
        root = AddNode(root, key, value);
    }

    private Node AddNode(Node node, K key, V value)
    {
        if (node == null)
        {
            size++;
            return new Node(key, value);
        }

        int res = comparator.Compare(key, node.Key);
        if (res < 0)
        {
            node.Left = AddNode(node.Left, key, value);
        }
        else if (res > 0)
        {
            node.Right = AddNode(node.Right, key, value);
        }
        else
        {
            node.Value = value;
        }
        return node;
    }

    // 11) Удалить по ключу
    public bool Remove(object key)
    {
        if (!(key is K k))
        {
            return false;
        }

        int oldSize = size;
        root = DeleteNode(root, k);

        if (size != oldSize)
        {
            return true;
        }
        return false;
    }

    private Node DeleteNode(Node node, K key)
    {
        if (node == null)
        {
            return null;
        }

        int res = comparator.Compare(key, node.Key);
        if (res < 0)
        {
            node.Left = DeleteNode(node.Left, key);
        }
        else if (res > 0)
        {
            node.Right = DeleteNode(node.Right, key);
        }
        else
        {
            if (node.Left == null)
            {
                size--;
                return node.Right;
            }
            if (node.Right == null)
            {
                size--;
                return node.Left;
            }

            Node minNode = GetMinNode(node.Right);
            node.Key = minNode.Key;
            node.Value = minNode.Value;
            node.Right = DeleteNode(node.Right, minNode.Key);
        }
        return node;
    }

    // 12) Размер
    public int Size()
    {
        return size;
    }

    // 13) Первый (мин)
    public K FirstKey()
    {
        if (root == null)
        {
            throw new InvalidOperationException("Map пустой");
        }
        return GetMinNode(root).Key;
    }

    // 14) Последний (макс)
    public K LastKey()
    {
        if (root == null)
        {
            throw new InvalidOperationException("Map пустой");
        }
        return GetMaxNode(root).Key;
    }

    // 15) Все, что строго меньше end
    public MyTreeMap<K, V> HeadMap(K end)
    {
        if (end == null)
        {
            throw new ArgumentNullException();
        }
        MyTreeMap<K, V> map = new MyTreeMap<K, V>(comparator);
        FillSubMap(root, map, default(K), end, false, true, false);
        return map;
    }

    //16) От start (вкл) до end (не вкл)
    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        if (start == null || end == null)
        {
            throw new ArgumentNullException();
        }
        MyTreeMap<K, V> map = new MyTreeMap<K, V>(comparator);
        FillSubMap(root, map, start, end, true, true, false);
        return map;
    }

    //17) Строго больше start 
    public MyTreeMap<K, V> TailMap(K start)
    {
        if (start == null)
        {
            throw new ArgumentNullException();
        }
        MyTreeMap<K, V> map = new MyTreeMap<K, V>(comparator);

        FillSubMap(root, map, start, default(K), true, false, true);
        return map;
    }

    //Метод, который бегает по дереву и кидает подходящие элементы в новую карту
    private void FillSubMap(Node node, MyTreeMap<K, V> map, K start, K end, bool useStart, bool useEnd, bool strictStart)
    {
        if (node == null)
        {
            return;
        }

        FillSubMap(node.Left, map, start, end, useStart, useEnd, strictStart);

        // Проверяем нижнюю границу
        bool goodLow = false;
        if (!useStart)
        {
            goodLow = true;
        }
        else
        {
            int cmp = comparator.Compare(node.Key, start);
            if (strictStart)
            {
                if (cmp > 0)
                {
                    goodLow = true;
                }
            }
            else
            {
                if (cmp >= 0)
                {
                    goodLow = true;
                }
            }
        }

        // Проверяем верхнюю границу
        bool goodHigh = false;
        if (!useEnd)
        {
            goodHigh = true;
        }
        else
        {
            if (comparator.Compare(node.Key, end) < 0)
            {
                goodHigh = true;
            }
        }

        if (goodLow && goodHigh)
        {
            map.Put(node.Key, node.Value);
        }

        FillSubMap(node.Right, map, start, end, useStart, useEnd, strictStart);
    }

    //18-21 Методы возвращают пару Ключ-Значение
    public KeyValuePair<K, V>? LowerEntry(K key)
    {
        Node n = GetLowerNode(root, key);
        return NodeToPair(n);
    }

    public KeyValuePair<K, V>? FloorEntry(K key)
    {
        Node n = GetFloorNode(root, key);
        return NodeToPair(n);
    }

    public KeyValuePair<K, V>? HigherEntry(K key)
    {
        Node n = GetHigherNode(root, key);
        return NodeToPair(n);
    }

    public KeyValuePair<K, V>? CeilingEntry(K key)
    {
        Node n = GetCeilingNode(root, key);
        return NodeToPair(n);
    }

    //22-25 возвращают только Ключик
    public K LowerKey(K key)
    {
        Node n = GetLowerNode(root, key);
        return NodeToKey(n);
    }

    public K FloorKey(K key)
    {
        Node n = GetFloorNode(root, key);
        return NodeToKey(n);
    }

    public K HigherKey(K key)
    {
        Node n = GetHigherNode(root, key);
        return NodeToKey(n);
    }

    public K CeilingKey(K key)
    {
        Node n = GetCeilingNode(root, key);
        return NodeToKey(n);
    }

    //Вспомогательные методы
    private KeyValuePair<K, V>? NodeToPair(Node n)
    {
        if (n == null)
        {
            return null;
        }
        return new KeyValuePair<K, V>(n.Key, n.Value);
    }

    private K NodeToKey(Node n)
    {
        if (n == null)
        {
            return default(K);
        }
        return n.Key;
    }

    //26-29 Методы Poll (удалить и вернуть) и просто вернуть края
    public KeyValuePair<K, V>? PollFirstEntry()
    {
        if (root == null)
        {
            return null;
        }
        Node min = GetMinNode(root);
        Remove(min.Key);
        return new KeyValuePair<K, V>(min.Key, min.Value);
    }

    public KeyValuePair<K, V>? PollLastEntry()
    {
        if (root == null)
        {
            return null;
        }
        Node max = GetMaxNode(root);
        Remove(max.Key);
        return new KeyValuePair<K, V>(max.Key, max.Value);
    }

    public KeyValuePair<K, V>? FirstEntry()
    {
        Node min = GetMinNode(root);
        return NodeToPair(min);
    }

    public KeyValuePair<K, V>? LastEntry()
    {
        Node max = GetMaxNode(root);
        return NodeToPair(max);
    }

    private Node FindNode(K key)
    {
        Node current = root;
        while (current != null)
        {
            int res = comparator.Compare(key, current.Key);
            if (res < 0)
            {
                current = current.Left;
            }
            else if (res > 0)
            {
                current = current.Right;
            }
            else
            {
                return current;
            }
        }
        return null;
    }

    //Самый левый узел
    private Node GetMinNode(Node node)
    {
        while (node.Left != null)
        {
            node = node.Left;
        }
        return node;
    }

    //Самый правый узел
    private Node GetMaxNode(Node node)
    {
        while (node.Right != null)
        {
            node = node.Right;
        }
        return node;
    }

    //Ищем наибольший, который строго меньше ключика
    private Node GetLowerNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            if (comparator.Compare(node.Key, key) < 0)
            {
                result = node;
                node = node.Right;
            }
            else
            {
                node = node.Left;
            }
        }
        return result;
    }

    private Node GetFloorNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            int res = comparator.Compare(node.Key, key);
            if (res <= 0)
            {
                result = node;
                if (res == 0)
                {
                    break;
                }
                node = node.Right;
            }
            else
            {
                node = node.Left;
            }
        }
        return result;
    }

    // Ищем наименьший, который > key
    private Node GetHigherNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            if (comparator.Compare(node.Key, key) > 0)
            {
                result = node; // Подходит
                node = node.Left; // Ищем поменьше
            }
            else
            {
                node = node.Right;
            }
        }
        return result;
    }

    //Ищем наименьший, который >= key
    private Node GetCeilingNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            int res = comparator.Compare(node.Key, key);
            if (res >= 0)
            {
                result = node;
                if (res == 0)
                {
                    break;
                }
                node = node.Left;
            }
            else
            {
                node = node.Right;
            }
        }
        return result;
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            MyTreeMap<int, string> map = new MyTreeMap<int, string>();

            map.Put(10, "Маша");
            map.Put(5, "Никита");
            map.Put(20, "Дима");
            map.Put(3, "Катя");
            map.Put(8, "Борис");
            map.Put(15, "Ева");
            map.Put(25, "Егор");

            Console.WriteLine("Дерево заполнено (7 элементов).");
            Console.WriteLine();


            // 1) Метод 12
            Console.WriteLine($"12) Количество элементов (size): {map.Size()}");

            // 2) Метод 4
            bool hasKey = map.ContainsKey(5);
            Console.WriteLine($"4) Содержит ли ключ 5? {hasKey}");

            // 3) Метод 7
            string val = map.Get(20);
            Console.WriteLine($"7) Значение по ключу 20: {val}");

            // 4) Метод 13
            Console.WriteLine($"13) Первый (минимальный) ключ: {map.FirstKey()}");

            // 5) Метод 22

            Console.WriteLine($"22) Ключ строго меньше 10: {map.LowerKey(10)}");

            // 6) Метод 16:

            Console.WriteLine("16) subMap [5, 15):");
            var sub = map.SubMap(5, 15);
            foreach (var key in sub.KeySet())
            {
                Console.Write(key + " ");
            }
            Console.WriteLine();

            // 7) Метод 11
            Console.WriteLine("\n11) Удаляем ключ 5");
            map.Remove(5);
            Console.WriteLine($"   Теперь содержит ключ 5? {map.ContainsKey(5)}");
            Console.WriteLine($"   Новый размер: {map.Size()}");

        }
        catch (Exception ex)
        {
            Console.WriteLine("ошибка: " + ex.Message);
        }

        Console.ReadLine();
    }
}
