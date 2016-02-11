using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class MainSystem : MonoBehaviour {
    private const float PIECE_SIZE = 1.35f;
    private const int N = 4;
    private GameObject[,] _grid = new GameObject[N, N];

    private bool _isAnimationRunning = false;

    private Vector3 GetPiecePosition(int row, int col) {
        float offsetX = -PIECE_SIZE * 1.5f;
        float offsetY =  PIECE_SIZE * 1.5f;

        var x = PIECE_SIZE * col;
        var y = -PIECE_SIZE * row;
        return new Vector3(x + offsetX, y + offsetY, 0);
    }

	void Start () {
        for (int i = 1; i <= 15; i++) {
            var prefabPath = "Prefabs/piece" + i;

            int row = (i - 1) / 4;
            int col = (i - 1) % 4;
            var pos = GetPiecePosition(row, col);
            var prefab = Resources.Load(prefabPath);
            var piece = Util.Bless<PieceBase>(prefab, pos);
            piece.Id = i;
            _grid[row, col] = piece.gameObject;
        }
	}

	void Update () {
        // アニメーション中なら入力操作は受け付けない
        if (_isAnimationRunning) return;

        if (Input.GetMouseButtonDown(0)) {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D c = Physics2D.OverlapPoint(pos);
            if (c != null) {
                // int id = c.gameObject.GetComponent<PieceBase>().Id;
                // Debug.Log("clicked piece id: " + id);

                int row = -1, col = -1;
                for (int i = 0; i < N; i++) {
                    for (int j = 0; j < N; j++) {
                        if (c.gameObject == _grid[i, j]) {
                            row = i;
                            col = j;
                            break;
                        }
                    }
                }
                Assert.IsTrue(row != -1 && col != -1);
                StartCoroutine(Slide(row, col));
            }
        }
	}

    void OnGUI() {
        Func<string, bool> button = (caption) => {
            return GUILayout.Button(caption, GUILayout.Width(110), GUILayout.Height(50));
        };

        if (GUILayout.Button("Shuffle 10 times", GUILayout.Width(110), GUILayout.Height(50))) {
            if (_isAnimationRunning) return;
            StartCoroutine(Shuffle(10));
        }
        else if (GUILayout.Button("Shuffle 30 times", GUILayout.Width(110), GUILayout.Height(50))) {
            if (_isAnimationRunning) return;
            StartCoroutine(Shuffle(30));
        }
        else if (button("Shuffle 50 times")) {
            if (_isAnimationRunning) return;
            StartCoroutine(Shuffle(50));
        }
        else if (button("Shuffle Cancel")) {
        }
    }

    private void Swap<T>(ref T a, ref T b) {
        var t = a; a = b; b = t;
    }

    // (row, col) のピースをスライドさせる
    IEnumerator Slide(int row, int col) {
        if (_grid[row, col] == null) {
            Debug.Log(string.Format("ピースがありません。row:{0} col:{1}", row, col));
            yield break;
        }

        // 空きピースの位置
        var pos = GetEmptyPieceLoc();
        if (!IsNeighbor(row, col, pos[0], pos[1])) {
            Debug.Log(string.Format("空きセルが隣接していません。row:{0} col:{1}", row, col));
            yield break;
        }

        _isAnimationRunning = true;
        int dstRow = row + (pos[0] - row);
        int dstCol = col + (pos[1] - col);
        var target = _grid[row, col];
        var src = GetPiecePosition(row, col);
        var dst = GetPiecePosition(dstRow, dstCol);
        float duration = 0.2f;
        float elapsed = 0;
        while (elapsed <= duration) {
            float x = Mathf.Lerp(src.x, dst.x, elapsed / duration);
            float y = Mathf.Lerp(src.y, dst.y, elapsed / duration);
            target.transform.position = new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Swap(ref _grid[row, col], ref _grid[dstRow, dstCol]);

        // 各ピースの位置を揃える(ずれ補正)
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                if (_grid[i, j] == null) continue;
                _grid[i, j].transform.position = GetPiecePosition(i, j);
            }
        }
        _isAnimationRunning = false;
    }

    IEnumerator Shuffle(int count) {
        var delta = new List<int[]>() {
            new[] { -1, 0 },
            new[] { 1, 0 },
            new[] { 0, -1 },
            new[] { 0, 1},
        };

        var rand = new System.Random();
        int drow = 0;
        int dcol = 0;

        int cnt = 0;
        while (cnt < count) {
            var pos = GetEmptyPieceLoc();

            int p = rand.Next(delta.Count);
            // 前の状態に後戻りさせない
            if (drow == -delta[p][0] && dcol == -delta[p][1]) continue;

            int r = pos[0] + delta[p][0];
            int c = pos[1] + delta[p][1];
            if (0 <= r && r < N && 0 <= c && c < N) {
                drow = delta[p][0];
                dcol = delta[p][1];
                yield return StartCoroutine(Slide(r, c));
                cnt++;
            }
        }
    }

    // 上下左右で隣接しているか
    private bool IsNeighbor(int r1, int c1, int r2, int c2) {
        int dr = Math.Abs(r1 - r2);
        int dc = Math.Abs(c1 - c2);
        return dr + dc == 1;
    }

    private int[] GetEmptyPieceLoc() {
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                if (_grid[i, j] == null) {
                    return new[] { i,  j };
                }
            }
        }
        Assert.IsTrue(false, "空きピースがありません");
        return null;
    }

    private GameObject GetPiece(int id) {
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                if (_grid[i, j] == null) continue;
                if (id == _grid[i, j].GetComponent<PieceBase>().Id) {
                    return _grid[i, j];
                }
            }
        }
        Assert.IsTrue(false, "不正なIDです: " + id);
        return null;
    }

/*
    private void MovePiece(int id) {
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                if (_grid[i, j] == null) continue;
                if (id != _grid[i, j].GetComponent<PieceBase>().Id) continue;

                var dxy = new[] { 0, 1, 0, -1 };
                for (int k = 0; k < dxy.Length; k++) {
                    int r = i + dxy[k];
                    int c = j + dxy[3-k];
                    if (0 <= r && r < N && 0 <= c && c < N) {
                        if (_grid[r, c] == null) {
                            // swap (i, j) -- (r, c)
                            Debug.Log(string.Format("swap ({0}, {1}) - ({2}, {3})", i, j, r, c));
                            _grid[r, c] = _grid[i, j];
                            _grid[i, j] = null;

                            _grid[r, c].transform.position = GetPiecePosition(r, c);
                            return;
                        }
                    }
                }
            }
        }
        Debug.Log("移動できません: id:" + id);
    }
*/
}
