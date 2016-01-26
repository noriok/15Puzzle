using UnityEngine;
using System;

public class MainSystem : MonoBehaviour {
    private const float PIECE_SIZE = 1.35f;
    private const int N = 4;
    private GameObject[,] _grid = new GameObject[N, N];

    private Vector3 GetPiecePosition(int row, int col) {
        var x = PIECE_SIZE * col;
        var y = -PIECE_SIZE * row;
        return new Vector3(x, y, 0);
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
        if (Input.GetMouseButtonDown(0)) {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D c = Physics2D.OverlapPoint(pos);
            if (c != null) {
                int id = c.gameObject.GetComponent<PieceBase>().Id;
                // Debug.Log("clicked piece id: " + id);
                MovePiece(id);
            }
        }
	}

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
}
