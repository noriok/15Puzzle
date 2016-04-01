using System;
using System.Collections.Generic;
using System.Linq;

public class Solver {
    private int[] _board;
    private readonly int _space; // 空き
    private readonly int _size;
    private readonly List<int[]> _move; // 各位置から移動可能な方向

/*
    public Solver(int size, int shuffleCount) {
        _size = size;
        _space = size * size;

        _board = new int[size * size];
        for (int i = 0; i < _board.Length; i++)
            _board[i] = i+1;

        // 各セルの移動できる方向を求める
        _move = new List<int[]>();
        for (int i = 0; i < size * size; i++) {
            var xs = new List<int>();
            if (i % _size != _size-1) xs.Add(1);     // 右
            if (i % _size != 0)       xs.Add(-1);    // 左
            if (i / _size != _size-1) xs.Add(size);  // 下
            if (i / _size != 0)       xs.Add(-size); // 上
            _move.Add(xs.ToArray());
        }

        // シャッフルする
        Shuffle(shuffleCount);
    }
*/

    public Solver(int[] board) {
        _size = 4;
        _space = 4 * 4;

        _board = (int[])board.Clone();
        // 各セルの移動できる方向を求める
        _move = new List<int[]>();
        for (int i = 0; i < _size * _size; i++) {
            var xs = new List<int>();
            if (i % _size != _size-1) xs.Add(1);      // 右
            if (i % _size != 0)       xs.Add(-1);     // 左
            if (i / _size != _size-1) xs.Add(_size);  // 下
            if (i / _size != 0)       xs.Add(-_size); // 上
            _move.Add(xs.ToArray());
        }
    }

    public void Display() {
        for (int i = 0; i < _size; i++) {
            for (int j = 0; j < _size; j++) {
                int x = _board[i * _size + j];
                if (x == _space) Console.Write("  *");
                else Console.Write(" {0,2}", x);
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

/*
    public void Shuffle(int n) {
        int cnt = 0;
        var rand = new Random();
        int pos = Array.IndexOf(_board, _space);
        int prevMove = 0; // 一つ前のセルの移動方向
        while (cnt < n) {
            int p = rand.Next(_move[pos].Length);
            int m = _move[pos][p];
            if (prevMove != -m) { // 一つ前の移動を元に戻さない
                Swap(ref _board[pos], ref _board[pos + m]);
                pos += m;
                prevMove = m;
                cnt++;
            }
        }
    }
*/

    private struct D : IComparable<D> {
        public double cost_est;
        public int md;
        public int step; // 移動回数
        public int[] board;
        public int pos; // 空きセルの位置
        public int last;
        public List<int> hist;

        public D(double cost_est, int md, int step, int[] board, int pos, int last, List<int> hist) {
            this.cost_est = cost_est;
            this.md = md;
            this.step = step;
            this.board = board;
            this.pos = pos;
            this.last = last;
            this.hist = hist;
        }

        public int CompareTo(D o) {
            return cost_est.CompareTo(o.cost_est);
        }
    }

    // セルの現在位置と最終位置とのマンハッタン距離を計算する
    // pos : セルの現在位置
    // no : セルの番号
    private int ManhattanDistance(int pos, int no) {
        int px = pos % _size;
        int py = pos / _size;
        int ox = (no - 1) % _size;
        int oy = (no - 1) / _size;
        return Math.Abs(px - ox) + Math.Abs(py - oy);
    }

    private void Swap<T>(ref T a, ref T b) {
        var t = a; a = b; b = t;
    }

    private bool IsGoal(int[] board) {
        for (int i = 0; i < board.Length; i++) {
            if (board[i] != i+1)
                return false;
        }
        return true;
    }

    private string Hash(int[] board) {
        var xs = board.Select(e => string.Format("{0,2}", e));
        return string.Join("", xs.ToArray());
    }

    public int[] Solve() {
        return Solve(_board);
    }

    private int[] Solve(int[] board) {
        if (IsGoal(board)) return new int[0];

        int md = Enumerable.Range(0, board.Length).Select(e => ManhattanDistance(e, board[e])).Sum();

        int pos = Array.IndexOf(board, _space);
        var q = new PriorityQueue<D>();
        q.Enqueue(new D(2.5 * md, md, 0, (int[])board.Clone(), pos, 0, new List<int>()));

        var used = new HashSet<string>();
        while (q.Count > 0) {
            D d = q.Dequeue();

            foreach (int dir in _move[d.pos]) {
                if (dir == -d.last) continue; // ひとつ前の状態には戻らない

                int[] bd = (int[])d.board.Clone();
                Swap(ref bd[d.pos], ref bd[d.pos + dir]); // 空きセルの移動

                if (IsGoal(bd)) {
                    return new List<int>(d.hist) { bd[d.pos] }.ToArray();
                }

                // マンハッタン距離の差分を計算
                int mdDiff = 0;
                mdDiff += ManhattanDistance(d.pos,       bd[d.pos]);
                mdDiff += ManhattanDistance(d.pos + dir, bd[d.pos + dir]);
                mdDiff -= ManhattanDistance(d.pos,       bd[d.pos + dir]);
                mdDiff -= ManhattanDistance(d.pos + dir, bd[d.pos]);

                var key = Hash(bd);
                if (used.Contains(key) || d.step + 1 + d.md + mdDiff > 1000) {
                    continue;
                }
                used.Add(key);

                var hist = new List<int>(d.hist) { bd[d.pos] };
                q.Enqueue(new D(d.step + 1 + 2.5 * (d.md + mdDiff), d.md + mdDiff, d.step + 1, bd, d.pos + dir, dir, hist));
            }
        }

        return null; // solution not exists
    }
}

