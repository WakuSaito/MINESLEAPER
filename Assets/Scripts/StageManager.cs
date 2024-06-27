using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//public struct TileData
//{
//    public int id;
//    public bool is_open;
//}

public class StageManager : MonoBehaviour
{
    const int BLOCK_EMPTY = 0;//空のID
    const int BLOCK_MINE = 9; //地雷のID

    [SerializeField, Header("爆弾を置き換える(デバッグ用)")]
    bool on_changemine = true;

    [SerializeField]//ステージのタイルマップ(現状地雷、数字、ブロックを同じタイルマップで管理しているが、数字にも接触してしまうため要変更)
    Tilemap stage_tilemap;
    //ブロックのTileBase
    [SerializeField]//ブロック
    TileBase tile_block;
    [SerializeField]//爆弾
    TileBase tile_mine;
    [SerializeField]//数字
    TileBase[] tile_num = new TileBase[9];

    //マップデータ
    Dictionary<Vector2Int, int> map_data = new Dictionary<Vector2Int, int>();

    //周囲の座標
    Vector2Int[] surround_pos = new Vector2Int[8]
    {
        new Vector2Int( 0, 1),//上
        new Vector2Int( 1, 1),//右上
        new Vector2Int( 1, 0),//右
        new Vector2Int( 1,-1),
        new Vector2Int( 0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1)
    };


    private void Start()
    {
        GetBlockData();

        SetMineCount();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouse_pos = GetMousePos();
            //無理やり0以下でも問題が起きないようにする
            if (mouse_pos.x < 0) mouse_pos.x -= 1.0f;
            if (mouse_pos.y < 0) mouse_pos.y -= 1.0f;
            //intに変換
            Vector2Int int_mouse_pos = new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y);

            //その座標にブロックデータがあれば
            if (map_data.ContainsKey(int_mouse_pos))
            {
                OpenBlock(int_mouse_pos);
            }
        }
    }

    //ブロックを開ける
    public void OpenBlock(Vector2Int _pos)
    {
        //すでに開いているなら実行しない
        if (stage_tilemap.GetTile((Vector3Int)_pos) == null) return;

        int block_id = map_data[_pos];
        TileBase tile = null;
        Debug.Log("ブロックのID" + block_id);

        //地雷なら
        if (block_id == BLOCK_MINE)
        {
            Explosion(_pos);
        }
        //通常のブロックなら
        else
        {
            //0以外は数字を出す
            if (block_id > BLOCK_EMPTY)
                tile = tile_num[block_id];
        }

        stage_tilemap.SetTile((Vector3Int)_pos, tile);//タイルの置き換え
    }

    //爆発
    public void Explosion(Vector2Int _pos)
    {
        map_data[_pos] = BLOCK_EMPTY;//この位置のidを空白に変更

        //全オブジェクトに爆発の情報を渡す
        GameObject[] all_obj = GameObject.FindGameObjectsWithTag("Object");
        foreach(GameObject obj in all_obj)
        {
            obj.GetComponent<ObjectManager>().HitExplosion(_pos);
        }

        //周囲8マスを探索
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (!map_data.ContainsKey(pos)) continue;

            //地雷以外ならカウントを減らす
            if (map_data[pos] != BLOCK_MINE)
                map_data[pos]--;


            OpenBlock(pos);
        }
    }

    //全タイルのブロック情報を取得
    public void GetBlockData()
    {
        //タイルマップの情報取得
        foreach(var pos in stage_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!stage_tilemap.HasTile(pos)) continue;

            int obj_id;

            if (stage_tilemap.GetTile(pos) == tile_mine)
            {
                Debug.Log("設置");
                if(on_changemine)
                    //地雷の見た目を通常のブロックに変更
                    stage_tilemap.SetTile(pos, tile_block);

                obj_id = BLOCK_MINE;
            }
            else
            {
                obj_id = BLOCK_EMPTY;
            }

            //位置情報とオブジェクト情報を保存
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

    //地雷の周囲8マスのカウントを変更
    public void SetMineCount()
    {
        Debug.Log("サイズ : " + map_data.Count);
        Dictionary<Vector2Int, int> mine_count = new Dictionary<Vector2Int, int>();

        //地雷を探す
        foreach (KeyValuePair<Vector2Int, int> data in map_data)
        {
            if (data.Value != BLOCK_MINE) continue;

            //周囲8マスを探索
            for(int i=0; i<surround_pos.Length; i++)
            {
                Vector2Int pos = data.Key + surround_pos[i];
                if (!map_data.ContainsKey(pos)) continue;
                if (map_data[pos] == BLOCK_MINE) continue;//地雷は計算しない

                //カウント追加
                if (mine_count.ContainsKey(pos))
                    mine_count[pos]++;
                else
                    mine_count.Add(pos, 1);
                
            }
        }
        //マップの値を変更（直にmap_dataを変えるとエラーが起きる）
        foreach (KeyValuePair<Vector2Int, int> data in mine_count)
        {
            map_data[data.Key] += data.Value;
        }
    }
}
