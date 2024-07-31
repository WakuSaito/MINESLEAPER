using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class ObjBase : MonoBehaviour
{
    //移動（移動先座標、終了時アクション）
    public bool Move(Vector2Int _pos, Action _complete = null)
    {
        Vector3 pos = (Vector2)_pos;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(pos, 0.5f));//移動
        seq.Play().OnComplete(()=> {
            _complete?.Invoke();
        });

        return true;
    }
    public bool Move(Vector3 _pos, Action _complete = null)
    {
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(_pos, 0.3f));//移動
        seq.Play().OnComplete(() => {
            _complete?.Invoke();
        });

        return true;
    }

    public abstract void Fall();

    public abstract void Broken();

    //Vector2Int型の座標を返す関数
    public Vector2Int GetIntPos()
    {
        Vector2 pos = transform.position;
        //無理やり0以下でも問題が起きないようにする
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;
        //intに変換
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
}
