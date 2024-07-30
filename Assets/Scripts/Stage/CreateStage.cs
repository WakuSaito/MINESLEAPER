using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateStage : MonoBehaviour
{
    //ブロックのタイルマップ
    Tilemap block_tilemap;
    //ブロック下のタイルマップ（地雷、数字）
    Tilemap under_tilemap;
    //壁のタイルマップ
    Tilemap wall_tilemap;
    //地面のタイルマップ
    Tilemap ground_tilemap;

    //ブロックのTileBase
    [SerializeField]//ブロック
    GameObject tile_block;
    [SerializeField]//爆弾
    GameObject tile_mine;

    //ブロックの親オブジェクト
    [SerializeField]
    GameObject block_parent;
    [SerializeField]
    GameObject mine_parent;


    // Start is called before the first frame update
    void Awake()
    {
        block_tilemap = GameObject.Find("BlockTilemap").GetComponent<Tilemap>();
        under_tilemap = GameObject.Find("UnderBlockTilemap").GetComponent<Tilemap>();
        wall_tilemap  = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        ground_tilemap =GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
    }

    //全タイルのブロック情報を取得
    public StageData GetAllBlockData()
    {
        StageData stage = new StageData();

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
        //地面の情報取得
        foreach (var pos in ground_tilemap.cellBounds.allPositionsWithin)
        {
            //その位置にタイルが無ければ処理しない
            if (!ground_tilemap.HasTile(pos)) continue;
            //すでに他のデータがあるなら処理しない
            if (stage.GetData((Vector2Int)pos) != ObjId.NULL) continue;

            //位置情報とオブジェクト情報を保存
            stage.SetData((Vector2Int)pos, ObjId.EMPTY);
        }

        return stage;
    }

    public void SetAllBlockData(StageData _stage)
    {
        foreach(var data in _stage.data)
        {
            if (data.Value != ObjId.BLOCK && data.Value != ObjId.MINE) continue;

            Vector3 pos = (Vector2)data.Key;

            switch(data.Value)
            {
                case ObjId.BLOCK:
                    Instantiate(tile_block, pos, Quaternion.identity, block_parent.transform);
                    break;
                case ObjId.MINE:
                    Instantiate(tile_mine, pos, Quaternion.identity, mine_parent.transform);
                    break;
            }
        }
    }
}
