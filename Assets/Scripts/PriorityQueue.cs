using System;
using System.Collections.Generic;

class PriorityQueue<T> where T : IComparable<T> {
    private List<T> _buf = new List<T>();
    private IComparer<T> _comp;

    private void Swap(int a, int b) {
        T t = _buf[a];
        _buf[a] = _buf[b];
        _buf[b] = t;
    }

    private int Compare(T a, T b) {
        if (_comp == null) return a.CompareTo(b);
        return _comp.Compare(a, b);
    }

    public int Count { get { return _buf.Count; } }

    public PriorityQueue() {
    }

    public PriorityQueue(IComparer<T> comp) {
        _comp = comp;
    }

    public void Enqueue(T x) {
        _buf.Add(x);

        // shiftup
        int p = Count-1;
        while (p > 0) {
            int parent = (p - 1) / 2;
            if (Compare(_buf[parent], _buf[p]) <= 0)
                break;
            Swap(parent, p);
            p = parent;
        }
    }

    public T Dequeue() {
        if (Count == 0)
            throw new IndexOutOfRangeException();

        T ret = _buf[0];
        _buf[0] = _buf[Count-1];
        _buf.RemoveAt(Count-1);

        // shiftdown
        int p = 0;
        while (p < Count) {
            int c = p * 2 + 1; // 左の子
            if (c >= Count) break;

            if (c+1 < Count) { // c+1 は右の子
                if (Compare(_buf[c+1], _buf[c]) < 0) { // 右の子のほうが小さい
                    c++;
                }
            }

            if (Compare(_buf[p], _buf[c]) <= 0)
                break;
            Swap(p, c);
            p = c;
        }

        return ret;
    }
}
