using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class CreateStage : MonoBehaviour
{
    GameObject player;//プレイヤーオブジェクト

    //グリッド（タイルマップの親）
    Grid grid;
    //ブロックのタイルマップ
    Tilemap tilemap_block;
    //ブロック下のタイルマップ（地雷、数字）
    Tilemap tilemap_under;
    //壁のタイルマップ
    Tilemap tilemap_wall;
    //地面のタイルマップ
    Tilemap tilemap_ground;

    //タイル
    [SerializeField]//床
    TileBase tile_floor;
    [SerializeField]//壁
    TileBase tile_wall;
    [SerializeField]//ゴール
    TileBase tile_goal;
    [SerializeField]//ブロック 削除予定
    TileBase tile_block;
    [SerializeField]//爆弾　削除予定
    TileBase tile_mine;
    [SerializeField]//旗ブロック
    TileBase tile_flag;

    //ブロックのTileBase
    [SerializeField]//ブロック
    GameObject obj_block;
    [SerializeField]//爆弾
    GameObject obj_mine;

    //ブロックの親オブジェクト
    [SerializeField]
    GameObject block_parent;

    const int MAP_WIDTH = 32;
    const int MAP_HEIGHT = 18;
    const int MAP_MIN_X = -16;
    const int MAP_MAX_Y = 8;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        tilemap_block = GameObject.Find("BlockTilemap").GetComponent<Tilemap>();
        tilemap_under = GameObject.Find("UnderBlockTilemap").GetComponent<Tilemap>();
        tilemap_wall  = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        tilemap_ground =GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
    }

    //ファイルからステージ情報を取得
    public StageData GetStageFileData(int _num)
    {
        //引数からファイルを取得
        string file_name = "StageData_" + _num.ToString();
        TextAsset csvFile = Resources.Load(file_name) as TextAsset; // CSVファイル
        if (!csvFile)
        {
            Debug.Log("ファイルが見つかりません");
            return null;
        }

        List<string[]> csvData = new List<string[]>(); // CSVファイルの中身を入れるリスト
        StringReader reader = new StringReader(csvFile.text); // TextAssetをStringReaderに変換
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine(); // 1行ずつ読み込む
            csvData.Add(line.Split(',')); // csvDataリストに追加する 
        }

        StageData stage = new StageData();

        
        //xの最大値取得
        int max_x = 0;
        for (int row = 0; row < csvData.Count; row++)
        {
            if (max_x < csvData[row].Length) max_x = csvData[row].Length;
        }
        //開始地点を決める（マップが中央に来るようにする）
        Vector2Int firstpos = new Vector2Int();
        firstpos.x = (MAP_WIDTH - max_x) / 2;
        firstpos.y = (MAP_HEIGHT - csvData.Count) / 2;

        //行ごとにデータ取得
        for(int row = 0; row < MAP_HEIGHT; row++)
        {
            if (row >= csvData.Count) break;//データがなければとばす

            int y = MAP_MAX_Y - row - firstpos.y;//y取得

            for(int column = 0; column < MAP_WIDTH; column++)
            {
                if (column >= csvData[row].Length) break;//データがなければとばす

                int x = MAP_MIN_X + column + firstpos.x;//x取得
                
                int num = int.Parse(csvData[row][column]);//csvの数値取得
                ObjId id;//id
                switch (num)//csvのデータをidに変換
                {
                    case 0:
                        id = ObjId.EMPTY; break;
                    case 1:
                        id = ObjId.WALL; break;
                    case 2:
                        id = ObjId.GOAL; break;
                    case 3:
                        id = ObjId.BLOCK; break;
                    case 4:
                        id = ObjId.MINE; break;
                    case 9:
                        id = ObjId.EMPTY;
                        player.transform.position = new Vector2(x + 0.5f, y + 0.5f);
                        break;
                    default:
                        id = ObjId.HOLE; break;
                }
                stage.SetData(new Vector2Int(x, y), id);
            }
        }

        return stage;
    }


    //ブロック配置(配置するデータ, 配置前に全削除するか)
    public void SetAllBlockData(StageData _stage, bool _delete_all = true)
    {
        if(_delete_all)//全削除
            DeleteAllBlock();

        //親オブジェクトが見つからなければ作成
        if (!block_parent)
            block_parent = new GameObject();

        foreach (var data in _stage.data)
        {
            if (data.Value == ObjId.NULL) continue;

            Vector3Int posint = (Vector3Int)data.Key;
            Vector3 pos = (Vector2)data.Key;

            switch(data.Value)
            {
                case ObjId.EMPTY:
                    tilemap_ground.SetTile(posint, tile_floor);
                    break;
                case ObjId.WALL:
                    tilemap_wall.SetTile(posint, tile_wall);
                    break;
                case ObjId.GOAL:
                    tilemap_ground.SetTile(posint, tile_goal);
                    break;
                case ObjId.BLOCK:
                    tilemap_ground.SetTile(posint, tile_floor);
                    tilemap_block.SetTile(posint, tile_block);
                    //Instantiate(tile_block, pos, Quaternion.identity, block_parent.transform);
                    break;
                case ObjId.MINE:
                    tilemap_ground.SetTile(posint, tile_floor);
                    tilemap_under.SetTile(posint, tile_mine);
                    //Instantiate(tile_mine, pos, Quaternion.identity, mine_parent.transform);
                    break;
                case ObjId.HOLE:
                    break;
            }
        }
    }

    //ブロック全削除
    public void DeleteAllBlock()
    {
        //オブジェクト削除
        if (block_parent)
                Destroy(block_parent);   

        //タイル削除
        if (grid == null) return;
        var tilemaps = grid.GetComponentsInChildren<Tilemap>();
        foreach(var tilemap in tilemaps)
        {
            if (tilemap == null) continue;

            tilemap.ClearAllTiles();
        }
    }

    //全タイルのブロック情報を取得
    public StageData GetAllBlockData()
    {
        StageData stage = new StageData();

        //ブロックの情報取得
        foreach (var pos in tilemap_block.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!tilemap_block.HasTile(pos)) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.BLOCK);
        }
        //地雷の位置情報取得
        foreach (var pos in tilemap_under.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!tilemap_under.HasTile(pos)) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.MINE);
        }
        //壁の情報取得
        foreach (var pos in tilemap_wall.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!tilemap_wall.HasTile(pos)) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.WALL);
        }
        //地面の情報取得
        foreach (var pos in tilemap_ground.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!tilemap_ground.HasTile(pos)) continue;
            //すでに他のデータがあるなら処理しない
            if (stage.GetData((Vector2Int)pos) != ObjId.NULL) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.EMPTY);
        }

        return stage;
    }
}
