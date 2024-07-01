using System;
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
    [SerializeField, Header("空白ブロックが連続で開く(デバッグ用)")]
    bool on_areaopen = true;

    [SerializeField]//ブロックのタイルマップ
    Tilemap block_tilemap;
    [SerializeField]//ブロック下のタイルマップ（地雷、数字）
    Tilemap under_tilemap;
    //ブロックのTileBase
    [SerializeField]//ブロック
    TileBase tile_block;
    [SerializeField]//爆弾
    TileBase tile_mine;
    [SerializeField]//数字
    TileBase[] tile_num = new TileBase[9];

    [SerializeField]//誘爆の待ち時間
    float chain_explo_delay = 0.3f;

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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_pos = GetMousePos();
            //無理やり0以下でも問題が起きないようにする
            if (mouse_pos.x < 0) mouse_pos.x -= 1.0f;
            if (mouse_pos.y < 0) mouse_pos.y -= 1.0f;
            //intに変換
            Vector2Int int_mouse_pos = new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y);
      
            OpenBlock(int_mouse_pos);//マウス位置のブロックを開く
            
        }
    }

    // 一定時間後に処理を呼び出すコルーチン
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    //ブロックを開ける
    public void OpenBlock(Vector2Int _pos)
    {
        Debug.Log(nameof(OpenBlock)  + "id:" + map_data[_pos] + " : "+ _pos);
        //その座標にブロックデータが無ければ実行しない
        if (!map_data.ContainsKey(_pos)) return;
        //何もないマスなら実行しない
        if (block_tilemap.GetTile((Vector3Int)_pos) == null &&
            under_tilemap.GetTile((Vector3Int)_pos) == null) return;
        

        int block_id = map_data[_pos];

        UpdateTileNum(_pos);//数字の画像更新
      
        Debug.Log("ブロックのID" + block_id);

        //地雷なら
        if (block_id == BLOCK_MINE)
        {
            map_data[_pos] = SearchMine(_pos);//この位置のidを空白に変更
            UpdateTileNum(_pos);//画像更新

            Explosion(_pos);//爆発処理
        }

        block_tilemap.SetTile((Vector3Int)_pos, null);//ブロックの削除

        if (block_id == BLOCK_EMPTY && on_areaopen)
        {
            //周囲8マスを探索
            for (int i = 0; i < surround_pos.Length; i++)
            {
                Vector2Int pos = _pos + surround_pos[i];
                if (!map_data.ContainsKey(pos)) continue; //データがあるか

                OpenBlock(pos);
            }
        }
    }


    //爆発
    public void Explosion(Vector2Int _pos)
    {
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
            //マップ情報無しなら何もしない
            if (!map_data.ContainsKey(pos)) continue;

            //誘爆（ワンテンポ遅らせるようにする）
            if(map_data[pos] == BLOCK_MINE)
            {
                block_tilemap.SetTile((Vector3Int)pos, null);//ブロックの削除 位置を変更する可能性アリ

                // コルーチンの起動
                StartCoroutine(DelayCoroutine(chain_explo_delay, () =>
                {
                    Debug.Log("誘爆");
                    // 3秒後にここの処理が実行される
                    OpenBlock(pos);//爆発処理
                }));
                continue;
            }
            //地雷と空白以外ならカウントを減らす
            else if (map_data[pos] != BLOCK_EMPTY)
            {
                map_data[pos]--;

                OpenBlock(pos);
            }
        }
    }

    //タイルの数字の見た目情報更新
    public void UpdateTileNum(Vector2Int _pos)
    {       
        int block_id = map_data[_pos];
        if (block_id >= BLOCK_MINE) return;//地雷なら更新しない

        TileBase tile = null;//変更するタイル

        //0以外は数字を出す
        if (block_id > BLOCK_EMPTY)
        {
            tile = tile_num[block_id];//数字タイル
        }

        //タイルの置き換え
        under_tilemap.SetTile((Vector3Int)_pos, tile);
    }

    //全タイルのブロック情報を取得
    public void GetBlockData()
    {
        //ブロックタイルマップの情報取得
        foreach (var pos in block_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!block_tilemap.HasTile(pos)) continue;

            //位置情報とオブジェクト情報を保存
            map_data.Add((Vector2Int)pos, BLOCK_EMPTY);
        }

        //地雷の位置情報取得
        foreach (var pos in under_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!under_tilemap.HasTile(pos)) continue;

            Debug.Log("設置");
            if (on_changemine)
                //ブロックタイルを重ねるように設置
                block_tilemap.SetTile(pos, tile_block);

            //位置情報とオブジェクト情報を保存
            map_data.Add((Vector2Int)pos, BLOCK_MINE);
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

    //周囲8マスの地雷の数を調べる関数(座標)
    public int SearchMine(Vector2Int _pos)
    {
        int mine_count = 0;

        //周囲8マスを探索
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (!map_data.ContainsKey(pos)) continue; //データがあるか
            if (map_data[pos] != BLOCK_MINE) continue;//地雷のみ計算
        
            mine_count++;
        }

        return mine_count;
    }

    //地雷の周囲8マスのカウントを変更
    public void SetMineCount()
    {
        Debug.Log("サイズ : " + map_data.Count);
        Dictionary<Vector2Int, int> mine_count = new Dictionary<Vector2Int, int>();

        //地雷を探す
        foreach (KeyValuePair<Vector2Int, int> data in map_data)
        {
            if (data.Value == BLOCK_MINE) continue;//地雷マスには数値を設定しない

            int id = SearchMine(data.Key);//周囲の地雷の数を調べる
            mine_count.Add(data.Key, id);

        }
        //マップの値を変更（直にmap_dataを変えるとエラーが起きる）
        foreach (KeyValuePair<Vector2Int, int> data in mine_count)
        {
            map_data[data.Key] += data.Value;
        }
    }

    
}
