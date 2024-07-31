using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class ObjBase : MonoBehaviour
{
    //�ړ��i�ړ�����W�A�I�����A�N�V�����j
    public bool Move(Vector2Int _pos, Action _complete = null)
    {
        Vector3 pos = (Vector2)_pos;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(pos, 0.5f));//�ړ�
        seq.Play().OnComplete(()=> {
            _complete?.Invoke();
        });

        return true;
    }
    public bool Move(Vector3 _pos, Action _complete = null)
    {
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(_pos, 0.3f));//�ړ�
        seq.Play().OnComplete(() => {
            _complete?.Invoke();
        });

        return true;
    }

    public abstract void Fall();

    public abstract void Broken();

    //Vector2Int�^�̍��W��Ԃ��֐�
    public Vector2Int GetIntPos()
    {
        Vector2 pos = transform.position;
        //�������0�ȉ��ł���肪�N���Ȃ��悤�ɂ���
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;
        //int�ɕϊ�
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
}
