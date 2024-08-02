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
    StageData stage = new StageData();      //ステージのブロックデータ
    StageData stage_flag = new StageData(); //ステージの旗データ

    List<GameObject> blocks = new List<GameObject>();

    CreateStage createStage;//ステージ作成スクリプト
    PlayerMove playerMove;  //プレイヤースクリプト
    SaveData saveData;      //セーブデータスクリプト

    DebugMan debugMan;//デバッグ用フラグ

    //ブロックの親オブジェクト
    [SerializeField]
    GameObject block_parent;

    //タイルマップ
    [SerializeField]//旗のタイルマップ
    Tilemap flag_tilemap;
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
    [SerializeField]//旗ブロック
    TileBase tile_flag;
    [SerializeField]//数字
    TileBase[] tile_num = new TileBase[9];

    [SerializeField]//誘爆の待ち時間
    float chain_explo_delay = 0.3f;

    int current_stage = 1;//現在のステージ
    public int deepest_stage = 1;//たどり着いたことのある最奥のステージ

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
    //周囲4方向の座標
    readonly Vector2Int[] surround4_pos =
    {
        new Vector2Int( 0, 1),//上
        new Vector2Int( 1, 0),//右
        new Vector2Int( 0,-1),
        new Vector2Int(-1, 0),
    };

    private void Awake()
    {
        //スクリプト取得
        createStage = gameObject.GetComponent<CreateStage>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();

        debugMan = GameObject.Find("DebugMan").GetComponent<DebugMan>();
    }

    private void Start()
    {
        ChangeStage(1);//ステージ1を呼び出し
    }

    //ステージ変更
    public void ChangeNextStage()
    {
        ChangeStage(current_stage + 1);
    }
    public void ChangeStage(int _num)
    {
        current_stage = _num;//現在のステージの更新
        if (deepest_stage < current_stage) 
            deepest_stage = current_stage;//最奥の更新
        stage = createStage.GetStageFileData(current_stage);
        createStage.SetAllBlockData(stage);

        playerMove.SetDirection(Vector2Int.down);//プレイヤーの向き変更
        playerMove.UpdateAttackTarget();

        FindAllBlock();

        //全空白の数字を更新
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//空白部分のみ数字を出す

            UpdateTileNum(data.Key);//更新
        }
        
        saveData.ResetMemento();//リセット
        saveData.CreateMemento();//データ保存
    }

    //クリア処理
    public void Clear()
    {
        ChangeNextStage();
    }


    // 一定時間後に処理を呼び出すコルーチン
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    //ブロックを開ける(座標, 旗をこじ開けるか)
    public bool OpenBlock(Vector2Int _pos, bool _flag_open=false)
    {
        ObjId block_id = stage.GetData(_pos);

        Debug.Log(nameof(OpenBlock)  + "id:" + block_id + " : "+ _pos);
        //ブロックと地雷のみ実行
        if (block_id != ObjId.BLOCK && 
            block_id != ObjId.MINE) return false;
        //旗が設置していれば
        //if (!_flag_open && 
        //    stage_flag.GetData(_pos) == ObjId.FLAG) return false;

        DeleteFlag(_pos);//旗削除

        stage.SetData(_pos, ObjId.EMPTY);//空白に変える
        UpdateTileNum(_pos);//数字の画像更新
      

        //地雷なら
        if (block_id == ObjId.MINE)
        {
            Explosion(_pos);//爆発処理
        }

        //block_tilemap.SetTile((Vector3Int)_pos, null);//ブロックの削除
        GetBlock(_pos).Broken();//ブロック削除

        //連続して開ける
        if (debugMan.on_areaopen && block_id == ObjId.BLOCK && GetMineCount(_pos) == 0)
        {
            //周囲4マスを探索
            for (int i = 0; i < surround4_pos.Length; i++)
            {
                Vector2Int pos = _pos + surround4_pos[i];
                if (stage.GetData(pos) == ObjId.NULL) continue; //データがあるか

                OpenBlock(pos, true);//旗ごと開ける
            }
        }
        return true;
    }


    //爆発
    public void Explosion(Vector2Int _pos)
    {
        //周囲8マスのプレイヤーを探索
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //周囲の座標
            Vector2Int pos = _pos + surround_pos[i];
            //プレイヤーの座標
            Vector2Int p_pos = playerMove.GetIntPos();
            Debug.Log("player座標:" + p_pos);
            //周囲にプレイヤーがいた場合
            if (pos == p_pos)
            {
                int num = GetMineCount(p_pos) + 1;//足元の数字を取得
                playerMove.StartLeap(_pos, num);//その分ふっとばし
            }
        }
        //周囲8マスのブロックを探索
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
                //StartCoroutine(DelayCoroutine(chain_explo_delay, () =>
                //{
                    Debug.Log("誘爆");
                    // 3秒後にここの処理が実行される
                    OpenBlock(pos);//爆発処理
                //}));
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

    //旗設置切り替え
    public void SwitchFlag(Vector2Int _pos)
    {
        if (stage.GetData(_pos) != ObjId.BLOCK &&
            stage.GetData(_pos) != ObjId.MINE) return;

        //未設置なら設置
        if (stage_flag.GetData(_pos) != ObjId.FLAG)
        {
            stage_flag.SetData(_pos, ObjId.FLAG);
            flag_tilemap.SetTile((Vector3Int)_pos, tile_flag);
        }
        else//設置済なら削除
        {
            stage_flag.Delete(_pos);
            flag_tilemap.SetTile((Vector3Int)_pos, null);
        }
    }
    //旗削除
    public void DeleteFlag(Vector2Int _pos)
    {
        if (stage_flag.GetData(_pos) != ObjId.FLAG) return;

        stage_flag.Delete(_pos);
        flag_tilemap.SetTile((Vector3Int)_pos, null);
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
    public int GetMineCount(Vector2Int _pos)
    {
        int mine_count = 0;

        //周囲8マスを探索
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (stage.GetData(pos) != ObjId.MINE) continue;//地雷のみ計算

            mine_count++;
        }

        return mine_count;
    }

    public ObjId GetTileId(Vector2Int _pos)
    {
        return stage.GetData(_pos);
    }

    public Block GetBlock(Vector2Int _pos)
    {
        foreach(var obj in blocks)
        {
            if (obj == null) continue;

            Block block = obj.GetComponent<Block>();
            if (block.GetIntPos() == _pos) 
                return block;
        }
        return null;
    }

    private void FindAllBlock()
    {
        blocks.Clear();
        foreach(Transform obj in block_parent.transform)
            blocks.Add(obj.gameObject);
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
        if(next_id != ObjId.EMPTY && next_id != ObjId.HOLE)
        {
            //移動先のブロックを押す
            if (!PushBlock(next_pos, _vec)) return false;
        }

        //移動先が穴
        if (next_id == ObjId.HOLE)
        {
            DeleteFlag(_pos);
            stage.SetData(_pos, ObjId.EMPTY);

            stage.SetData(next_pos, ObjId.EMPTY);//穴を地面に変える
        }
        else if (stage_flag.GetData(_pos) == ObjId.FLAG)//旗の移動
        {
            SwitchFlag(_pos);//旗の情報変更

            stage.Move(_pos, next_pos);

            SwitchFlag(next_pos);//旗の情報変更
        }
        else
        {
            stage.Move(_pos, next_pos);
        }


        Block block = GetBlock(_pos);//ブロック取得
        block.Move(next_pos);//移動
       
        if (next_id != ObjId.HOLE) //穴に落ちる処理
        {
            block.Fall();
        }


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

    //ステージデータ記録 変更点のみ保存してもいいかも
    public StageMemento CreateMemento()
    {
        var memento = new StageMemento();
        memento.stageData = new StageData();
        memento.stageData.data = new Dictionary<Vector2Int, ObjId>(stage.data);

        return memento;
    }

    //ステージデータのロード
    public void SetMemento(StageMemento memento)
    {
        stage = memento.stageData;

        createStage.SetAllBlockData(stage);

        foreach(var data in stage_flag.data)
        {
            if(data.Value == ObjId.FLAG)
            {
                flag_tilemap.SetTile((Vector3Int)data.Key, tile_flag);
            }
        }

        //全空白の数字を更新
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//空白部分のみ数字を出す

            UpdateTileNum(data.Key);//更新
        }

        FindAllBlock();

    }
}
