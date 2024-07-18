using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BlockId
{
    EMPTY=0,
    NUM1,
    NUM2,
    NUM3,
    NUM4,
    NUM5,
    NUM6,
    NUM7,
    NUM8,
    MINE,
}


public class StageManager : MonoBehaviour
{
    StageData stage = new StageData();

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
    [SerializeField]//壁のタイルマップ
    Tilemap wall_tilemap;
    [SerializeField]//地面のタイルマップ
    Tilemap ground_tilemap;

    //ブロックのTileBase
    [SerializeField]//ブロック
    TileBase tile_block;
    [SerializeField]//爆弾
    TileBase tile_mine;
    [SerializeField]//数字
    TileBase[] tile_num = new TileBase[9];

    [SerializeField]//誘爆の待ち時間
    float chain_explo_delay = 0.3f;


    //周囲の座標
    readonly Vector2Int[] surround_pos = 
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
        //ステージのブロックデータ取得
        GetBlockData();

        //全空白の数字を更新
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//空白部分のみ数字を出す

            UpdateTileNum(data.Key);//更新
        }
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
        ObjId block_id = stage.GetData(_pos);

        Debug.Log(nameof(OpenBlock)  + "id:" + block_id + " : "+ _pos);
        //ブロックと地雷のみ実行
        if (block_id != ObjId.BLOCK && 
            block_id != ObjId.MINE) return;

        stage.SetData(_pos, ObjId.EMPTY);//空白に変える
        UpdateTileNum(_pos);//数字の画像更新
      
        Debug.Log("ブロックのID" + block_id);

        //地雷なら
        if (block_id == ObjId.MINE)
        {
            Explosion(_pos);//爆発処理
        }

        block_tilemap.SetTile((Vector3Int)_pos, null);//ブロックの削除

        //連続して開ける
        if (block_id == ObjId.BLOCK && GetMineCount(_pos) == 0 && on_areaopen)
        {
            //周囲8マスを探索
            for (int i = 0; i < surround_pos.Length; i++)
            {
                Vector2Int pos = _pos + surround_pos[i];
                if (stage.GetData(pos) == ObjId.NULL) continue; //データがあるか

                OpenBlock(pos);
            }
        }
    }


    //爆発
    public void Explosion(Vector2Int _pos)
    {
        //全オブジェクトに爆発の情報を渡す
        //GameObject[] all_obj = GameObject.FindGameObjectsWithTag("Object");
        //foreach(GameObject obj in all_obj)
        //{
        //    obj.GetComponent<ObjectManager>().HitExplosion(_pos);
        //}

        //周囲8マスを探索
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //周囲の座標
            Vector2Int pos = _pos + surround_pos[i];

            ObjId id = stage.GetData(pos);

            //誘爆（ワンテンポ遅らせるようにする）
            if(id == ObjId.MINE)
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
            //ブロックは開く
            else if (id == ObjId.BLOCK)
            {
                OpenBlock(pos);
            }
            //空白なら数字更新
            else if(id == ObjId.EMPTY)
            {
                UpdateTileNum(pos);
            }
        }
    }

    //タイルの数字の見た目情報更新
    private void UpdateTileNum(Vector2Int _pos)
    {       
        if (stage.GetData(_pos) != ObjId.EMPTY) return;//空白のみ更新

        int num = GetMineCount(_pos);

        TileBase tile = tile_num[num];//変更するタイル

        //タイルの置き換え
        under_tilemap.SetTile((Vector3Int)_pos, tile);
    }

    //周囲8マスの地雷の数を調べる関数(座標)
    private int GetMineCount(Vector2Int _pos)
    {
        int mine_count = 0;

        //周囲8マスを探索
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (stage.GetData(pos) != ObjId.MINE) continue;//地雷のみ計算

            mine_count++;
        }

        Debug.Log("周囲の地雷の数:" + mine_count);
        return mine_count;
    }

    //全タイルのブロック情報を取得
    private void GetBlockData()
    {
        //ブロックの情報取得
        foreach (var pos in block_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!block_tilemap.HasTile(pos)) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.BLOCK);
        }
        //地雷の位置情報取得
        foreach (var pos in under_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!under_tilemap.HasTile(pos)) continue;

            if (on_changemine)
                //ブロックタイルを重ねるように設置
                block_tilemap.SetTile(pos, tile_block);

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.MINE);
        }
        //壁の情報取得
        foreach (var pos in wall_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!wall_tilemap.HasTile(pos)) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.WALL);
        }
        //壁の情報取得
        foreach (var pos in ground_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!ground_tilemap.HasTile(pos)) continue;
            //すでに他のデータがあるなら処理しない
            if (stage.GetData((Vector2Int)pos) != ObjId.NULL) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.EMPTY);
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

    public ObjId GetTileId(Vector2Int _pos)
    {
        return stage.GetData(_pos);
    }

    //ブロックを押す関数
    public bool PushBlock(Vector2Int _pos, Vector2Int _vec)
    {
        ObjId id = GetTileId(_pos);
        //ブロック、地雷のみ押せる
        if (id != ObjId.BLOCK && id != ObjId.MINE) return false;
       
        //移動先座標
        Vector2Int next_pos = _pos + _vec;
        //移動先のidを取得
        ObjId next_id = GetTileId(next_pos);
        
        //移動先が空白以外
        if(next_id != ObjId.EMPTY)
        {
            //移動先のブロックを押す
            if (!PushBlock(next_pos, _vec)) return false;
        }
      
        stage.Move(_pos, next_pos);

        //移動先画像変更
        block_tilemap.SetTile((Vector3Int)next_pos, tile_block);
        //現座標画像変更
        block_tilemap.SetTile((Vector3Int)_pos, null);


        //数字の更新
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //周囲の座標
            Vector2Int pos = _pos + surround_pos[i];

            if (stage.GetData(pos) == ObjId.EMPTY)
                UpdateTileNum(pos);
        }
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //周囲の座標
            Vector2Int pos = next_pos + surround_pos[i];

            if (stage.GetData(pos) == ObjId.EMPTY)
                UpdateTileNum(pos);
        }

        Debug.Log("押す");

        return true;
    }


}
