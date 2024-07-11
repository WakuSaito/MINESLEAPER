using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ObjId
{
    NULL=-1,
    EMPTY=0,
    WALL,
    BLOCK,

}

//ステージデータクラス
public class StageData
{
    //マップデータ
    private Dictionary<Vector2Int, ObjId> data = 
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

}
