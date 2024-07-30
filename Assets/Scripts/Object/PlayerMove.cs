using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//方向
public enum Direction
{
    UP,
    LEFT,
    DOWN,
    RIGHT,
}

public class PlayerMove : ObjBase
{
    [SerializeField, Header("ブロックを押せる(デバッグ用)")]
    bool on_canpush = true;

    StageManager stageManager;
    SaveData saveData;

    [SerializeField]//爆発での移動距離
    float move_length = 3.0f;
    [SerializeField]//爆風を受けられる距離
    float can_hit_distance = 1.5f;

    //爆風で飛ぶ距離
    int leap_distance = 3;

    public bool is_action; //行動中フラグ

    //攻撃対象の情報
    public Vector2Int attack_target_pos;
    ObjId attack_target_id;

    // Playerから移動速度を取得できるように設定
    [SerializeField] float moveSpeed;

    //向き
    Direction direction = Direction.DOWN;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
        UpdateAttackTarget();
    }

    //移動開始
    public bool StartMove(Vector2Int _vec)
    {
        if (is_action) return false;//行動中なら停止
        if (_vec == Vector2Int.zero) return false;//移動ベクトルがゼロ

        //斜め移動できないようにする　x優先
        if (_vec.x != 0) _vec.y = 0;

        //向きを決める
        SetDirection(_vec);

        //現座標
        Vector2Int pos = GetIntPos();
        //移動先座標
        Vector2 target_pos = (Vector2)transform.position + _vec;

        //移動先探索
        ObjId id = stageManager.GetTileId(GetIntPos(target_pos));
        Debug.Log("移動先id:" + id + ":" + target_pos);
        if (id != ObjId.EMPTY)
        {
            //ブロックを押す
            if (on_canpush)
            {
                if (!stageManager.PushBlock(pos + _vec, _vec))
                    return false;
            }
            else
                return false;//移動しない
        }

        is_action = true;
        //移動
        Move(target_pos, () =>
        {//終了時処理
            is_action = false;
            saveData.CreateMemento();//状態保存
            UpdateAttackTarget();
        });

        return true;
    }

    //ふっとばし（爆心地）
    public void StartLeap(Vector2Int _hypocenter, int _power)
    {
        if (is_action) return;

        leap_distance = _power;
        Debug.Log(_power);

        Vector2Int p_pos = GetIntPos();
        Vector2Int vec = (_hypocenter - p_pos) * -1;//方向を求める
        Vector2 target_pos = p_pos + new Vector2(0.5f, 0.5f);//目標地点

        SetDirection(vec);//向き設定

        //ルート探索
        for (int i=1;i<=leap_distance;i++)
        {
            Vector2Int route_pos = p_pos + (vec * i);
            ObjId id = stageManager.GetTileId(route_pos);
            //途中に障害があればひとつ前の座標で止まる
            if (id == ObjId.WALL || id== ObjId.NULL)
                break;
            else
                target_pos += vec;
        }
        is_action = true;
        
        //Leapアニメーション
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(target_pos, 0.8f));//移動
        seq.Join(transform.DOScale(Vector3.one * 1.0f, 0.4f).SetLoops(2, LoopType.Yoyo));//サイズ変更
        seq.Play().OnComplete(() =>
         {//終了処理
             is_action = false;
             saveData.CreateMemento();//状態保存
             landing();//着地処理
             UpdateAttackTarget();
         });
    }

    //着地処理
    private void landing()
    {
        Debug.Log("着地");
        Vector2Int p_pos = GetIntPos();
        //着地地点のid取得
        ObjId id = stageManager.GetTileId(p_pos);
        if (id == ObjId.BLOCK)//ブロックなら開ける
        {
            stageManager.OpenBlock(p_pos);
        }
        if (id == ObjId.MINE)//地雷なら前回と同じ方向に吹っ飛ぶ
        {
            Vector2Int hypo_pos = p_pos + (GetDirectionVec() * -1);
            StartLeap(hypo_pos, leap_distance);
            stageManager.OpenBlock(p_pos);
        }
    }

    //前方に攻撃
    public bool Attack()
    {
        UpdateAttackTarget();//現在の攻撃対象更新

        //攻撃アニメーション

        //ブロックを壊す
        if(stageManager.OpenBlock(attack_target_pos))
        {
            UpdateAttackTarget();//情報更新
            saveData.CreateMemento();//保存
            return true;
        }
        else//壊せなかった場合
        {
            return false;
        }
       
    }


    public override void Broken()
    {
        Destroy(gameObject);
    }
    public override void Fall()
    {

    }

    public void UpdateAttackTarget()
    {
        //攻撃対象の情報取得
        attack_target_pos = GetIntPos() + GetDirectionVec();
        attack_target_id = stageManager.GetTileId(attack_target_pos);
    }

    private void SetDirection(Vector2Int _vec)
    {
        if (_vec == Vector2Int.up)
            direction = Direction.UP;
        else if (_vec == Vector2Int.left)
            direction = Direction.LEFT;
        else if (_vec == Vector2Int.down)
            direction = Direction.DOWN;
        else if (_vec == Vector2Int.right)
            direction = Direction.RIGHT;

        UpdateAttackTarget();
    }
    private Vector2Int GetDirectionVec()
    {
        if (direction == Direction.UP)
            return Vector2Int.up;
        else if (direction == Direction.LEFT)
            return Vector2Int.left;
        else if (direction == Direction.DOWN)
            return Vector2Int.down;
        else if (direction == Direction.RIGHT)
            return Vector2Int.right;
        else
            return Vector2Int.zero;
    }

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
    //引数をint型に変換
    public Vector2Int GetIntPos(Vector2 _pos)
    {
        Vector2 pos = _pos;
        //無理やり0以下でも問題が起きないようにする
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;
        //intに変換
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }

    public PlayerMemento CreateMemento()
    {
        //Debug.Log("PlayerMementoの作成");
        var memento = new PlayerMemento();
        memento.position = transform.position;
        return memento;
    }

    public void SetMemento(PlayerMemento memento)
    {
        //Debug.Log("PlayerMementoの呼び出し" + memento.position);
        transform.position = memento.position;
    }
}
