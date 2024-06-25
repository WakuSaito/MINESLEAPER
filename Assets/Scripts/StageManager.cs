using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class StageManager : MonoBehaviour
{
    [SerializeField]//ステージのタイルマップ
    Tilemap stage_tilemap;
    //ブロックのTileBase
    [SerializeField]//block
    TileBase tile_block;
    [SerializeField]//爆弾
    TileBase tile_mine;
    [SerializeField]//爆弾
    Sprite sprite_mine;

    Dictionary<Vector2Int, int> map_data = new Dictionary<Vector2Int, int>();


    private void Start()
    {
        GetBlockData();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_pos = GetMousePos();
            //無理やり0以下でも問題が起きない
            if (mouse_pos.x < 0) mouse_pos.x -= 1.0f;
            if (mouse_pos.y < 0) mouse_pos.y -= 1.0f;
            Vector2Int pos = new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y);
            Debug.Log("マウス座標f:" + mouse_pos);
            Debug.Log("マウス座標i:" + pos);
            if (map_data.ContainsKey(pos))
                stage_tilemap.SetTile((Vector3Int)pos, null);
        }
    }

    //タイルのブロック情報を取得
    public void GetBlockData()
    {
        int max_x = stage_tilemap.cellBounds.xMax;
        int max_y = stage_tilemap.cellBounds.yMax;

        //タイルマップの情報取得
        foreach(var pos in stage_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!stage_tilemap.HasTile(pos)) continue;

            int obj_id;

            if (stage_tilemap.GetTile(pos) == tile_mine)
            {
                Debug.Log("設置");
                stage_tilemap.SetTile(pos, tile_block);
                obj_id = 2;
            }
            else
            {
                obj_id = 1;
            }

            map_data.Add( (Vector2Int)pos ,obj_id);

        }
    }

    //マウスの座標取得
    public Vector3 GetMousePos()
    {
        Vector3 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.z = 10.0f;

        return pos;
    }
}
