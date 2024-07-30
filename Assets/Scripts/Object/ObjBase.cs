using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class ObjBase : MonoBehaviour
{
    //�ړ��i�ړ�����W�A�I�����A�N�V�����j
    protected bool Move(Vector2Int _pos, Action _complete = null)
    {
        Vector3 pos = (Vector2)_pos;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(pos, 0.5f));//�ړ�
        seq.Play().OnComplete(()=> {
            _complete?.Invoke();
        });

        return true;
    }
    protected bool Move(Vector3 _pos, Action _complete = null)
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
}
