using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ObjId
{
    NULL=-1,//存在しない
    EMPTY=0,//空白(床)
    WALL,   //壁
    BLOCK,  //何もないブロック
    MINE,   //地雷
    FLAG,   //旗
    HOLE,   //穴
    GOAL,   //ゴール
}

//ステージデータクラス
public class StageData
{
    //マップデータ　一旦public
    public Dictionary<Vector2Int, ObjId> data = 
        new Dictionary<Vector2Int, ObjId>();

    public ObjId GetData(Vector2Int _pos)
    {
        if (data.ContainsKey(_pos))
            return data[_pos];
        else
            //データが存在しない
            return ObjId.NULL;
    }

    public void SetData(Vector2Int _pos, ObjId _id)
    {
        if (data.ContainsKey(_pos))
            data[_pos] = _id;
        else
            data.Add(_pos, _id);
    }

    //移動（現在の座標、移動先）
    public bool Move(Vector2Int _pos, Vector2Int _destination)
    {
        //データが存在しない
        if (!data.ContainsKey(_pos) ||
            !data.ContainsKey(_destination)) return false;

        data[_destination] = data[_pos];
        data[_pos] = ObjId.EMPTY;

        return true;
    }

    //入れ替え（現在の座標、移動先）
    public bool Swap(Vector2Int _pos1, Vector2Int _pos2)
    {
        //データが存在しない
        if (!data.ContainsKey(_pos1) ||
            !data.ContainsKey(_pos2)) return false;

        ObjId tmp = data[_pos1];
        data[_pos1] = data[_pos2];
        data[_pos2] = tmp;

        return true;
    }
    //キー削除
    public void Delete(Vector2Int _pos)
    {
        if (!data.ContainsKey(_pos)) return;

        data.Remove(_pos);
    }

}
