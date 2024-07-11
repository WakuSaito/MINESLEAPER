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

//�X�e�[�W�f�[�^�N���X
public class StageData
{
    //�}�b�v�f�[�^
    private Dictionary<Vector2Int, ObjId> data = 
        new Dictionary<Vector2Int, ObjId>();

    public ObjId GetData(Vector2Int _pos)
    {
        if (data.ContainsKey(_pos))
            return data[_pos];
        else
            //�f�[�^�����݂��Ȃ�
            return ObjId.NULL;
    }

    public void SetData(Vector2Int _pos, ObjId _id)
    {
        if (data.ContainsKey(_pos))
            data[_pos] = _id;
        else
            data.Add(_pos, _id);
    }

    //�ړ��i���݂̍��W�A�ړ���j
    public bool Move(Vector2Int _pos, Vector2Int _destination)
    {
        //�f�[�^�����݂��Ȃ�
        if (!data.ContainsKey(_pos) ||
            !data.ContainsKey(_destination)) return false;

        data[_destination] = data[_pos];
        data[_pos] = ObjId.EMPTY;

        return true;
    }

    //����ւ��i���݂̍��W�A�ړ���j
    public bool Swap(Vector2Int _pos1, Vector2Int _pos2)
    {
        //�f�[�^�����݂��Ȃ�
        if (!data.ContainsKey(_pos1) ||
            !data.ContainsKey(_pos2)) return false;

        ObjId tmp = data[_pos1];
        data[_pos1] = data[_pos2];
        data[_pos2] = tmp;

        return true;
    }

}
