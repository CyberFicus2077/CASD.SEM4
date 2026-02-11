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

    public bool ContainsKey(K key)
    {
        Node found = FindNode(key);
        if (found != null)
        {
            return true;
        }
        return false;
    }

    public bool ContainsValue(V value)
    {
        return CheckValue(root, value);
    }

    private bool CheckValue(Node node, V value)
    {
        if (node == null)
        {
            return false;
        }
        if (Equals(node.Value, value))
        {
            return true;
        }
        bool leftHas = CheckValue(node.Left, value);
        bool rightHas = CheckValue(node.Right, value);

        return leftHas || rightHas;
    }

    public ISet<KeyValuePair<K, V>> EntrySet()
    {
        HashSet<KeyValuePair<K, V>> set = new HashSet<KeyValuePair<K, V>>();
        FillEntrySet(root, set);
        return set;
    }

    private void FillEntrySet(Node node, HashSet<KeyValuePair<K, V>> set)
    {
        if (node == null) return;
        FillEntrySet(node.Left, set);
        set.Add(new KeyValuePair<K, V>(node.Key, node.Value));
        FillEntrySet(node.Right, set);
    }

    public V Get(K key)
    {
        Node node = FindNode(key);
        if (node != null)
        {
            return node.Value;
        }
        return default(V);
    }

    public bool IsEmpty()
    {
        if (size == 0)
        {
            return true;
        }
        return false;
    }

    public ISet<K> KeySet()
    {
        HashSet<K> set = new HashSet<K>();
        FillKeySet(root, set);
        return set;
    }

    private void FillKeySet(Node node, HashSet<K> set)
    {
        if (node == null) return;
        FillKeySet(node.Left, set);
        set.Add(node.Key);
        FillKeySet(node.Right, set);
    }

    public void Put(K key, V value)
    {
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

    public bool Remove(K key)
    {
        int oldSize = size;
        root = DeleteNode(root, key);
        if (size != oldSize)
        {
            return true;
        }
        return false;
    }

    private Node DeleteNode(Node node, K key)
    {
        if (node == null) return null;

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

    public int Size()
    {
        return size;
    }

    public K FirstKey()
    {
        if (root == null) throw new Exception("Map is empty");
        return GetMinNode(root).Key;
    }

    public K LastKey()
    {
        if (root == null) throw new Exception("Map is empty");
        return GetMaxNode(root).Key;
    }

    public MyTreeMap<K, V> HeadMap(K end)
    {
        MyTreeMap<K, V> map = new MyTreeMap<K, V>(comparator);
        FillSubMap(root, map, default(K), end, false, true);
        return map;
    }

    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        MyTreeMap<K, V> map = new MyTreeMap<K, V>(comparator);
        FillSubMap(root, map, start, end, true, true);
        return map;
    }

    public MyTreeMap<K, V> TailMap(K start)
    {
        MyTreeMap<K, V> map = new MyTreeMap<K, V>(comparator);
        FillSubMap(root, map, start, default(K), true, false);
        return map;
    }

    private void FillSubMap(Node node, MyTreeMap<K, V> map, K start, K end, bool useStart, bool useEnd)
    {
        if (node == null) return;

        FillSubMap(node.Left, map, start, end, useStart, useEnd);

        bool goodLow = false;
        if (!useStart)
        {
            goodLow = true;
        }
        else if (comparator.Compare(node.Key, start) >= 0)
        {
            goodLow = true;
        }

        bool goodHigh = false;
        if (!useEnd)
        {
            goodHigh = true;
        }
        else if (comparator.Compare(node.Key, end) < 0)
        {
            goodHigh = true;
        }

        if (goodLow && goodHigh)
        {
            map.Put(node.Key, node.Value);
        }

        FillSubMap(node.Right, map, start, end, useStart, useEnd);
    }

    public KeyValuePair<K, V>? LowerEntry(K key)
    {
        Node n = GetLowerNode(root, key);
        if (n != null)
        {
            return new KeyValuePair<K, V>(n.Key, n.Value);
        }
        return null;
    }

    public KeyValuePair<K, V>? FloorEntry(K key)
    {
        Node n = GetFloorNode(root, key);
        if (n != null)
        {
            return new KeyValuePair<K, V>(n.Key, n.Value);
        }
        return null;
    }

    public KeyValuePair<K, V>? HigherEntry(K key)
    {
        Node n = GetHigherNode(root, key);
        if (n != null)
        {
            return new KeyValuePair<K, V>(n.Key, n.Value);
        }
        return null;
    }

    public KeyValuePair<K, V>? CeilingEntry(K key)
    {
        Node n = GetCeilingNode(root, key);
        if (n != null)
        {
            return new KeyValuePair<K, V>(n.Key, n.Value);
        }
        return null;
    }

    public K LowerKey(K key)
    {
        Node n = GetLowerNode(root, key);
        if (n != null) return n.Key;
        return default(K);
    }

    public K FloorKey(K key)
    {
        Node n = GetFloorNode(root, key);
        if (n != null) return n.Key;
        return default(K);
    }

    public K HigherKey(K key)
    {
        Node n = GetHigherNode(root, key);
        if (n != null) return n.Key;
        return default(K);
    }

    public K CeilingKey(K key)
    {
        Node n = GetCeilingNode(root, key);
        if (n != null) return n.Key;
        return default(K);
    }

    public KeyValuePair<K, V>? PollFirstEntry()
    {
        if (root == null) return null;
        Node min = GetMinNode(root);
        Remove(min.Key);
        return new KeyValuePair<K, V>(min.Key, min.Value);
    }

    public KeyValuePair<K, V>? PollLastEntry()
    {
        if (root == null) return null;
        Node max = GetMaxNode(root);
        Remove(max.Key);
        return new KeyValuePair<K, V>(max.Key, max.Value);
    }

    public KeyValuePair<K, V>? FirstEntry()
    {
        if (root == null) return null;
        Node min = GetMinNode(root);
        return new KeyValuePair<K, V>(min.Key, min.Value);
    }

    public KeyValuePair<K, V>? LastEntry()
    {
        if (root == null) return null;
        Node max = GetMaxNode(root);
        return new KeyValuePair<K, V>(max.Key, max.Value);
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

    private Node GetMinNode(Node node)
    {
        while (node.Left != null)
        {
            node = node.Left;
        }
        return node;
    }

    private Node GetMaxNode(Node node)
    {
        while (node.Right != null)
        {
            node = node.Right;
        }
        return node;
    }

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
                if (res == 0) break;
                node = node.Right;
            }
            else
            {
                node = node.Left;
            }
        }
        return result;
    }

    private Node GetHigherNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            if (comparator.Compare(node.Key, key) > 0)
            {
                result = node;
                node = node.Left;
            }
            else
            {
                node = node.Right;
            }
        }
        return result;
    }

    private Node GetCeilingNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            int res = comparator.Compare(node.Key, key);
            if (res >= 0)
            {
                result = node;
                if (res == 0) break;
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