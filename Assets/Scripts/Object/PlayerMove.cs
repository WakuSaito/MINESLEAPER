using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


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
    StageManager stageManager;
    SaveData saveData;
    SoundManager soundManager;

    Animator animator;

    [SerializeField]//爆発での移動距離
    float move_length = 3.0f;
    [SerializeField]//爆風を受けられる距離
    float can_hit_distance = 1.5f;

    //爆風で飛ぶ距離
    int leap_distance = 3;

    public bool is_action; //行動中フラグ
    bool is_leap = false;

    //攻撃対象の情報
    public Vector2Int attack_target_pos;

    // Playerから移動速度を取得できるように設定
    [SerializeField] float moveSpeed;

    //向き
    Direction direction = Direction.DOWN;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        animator = gameObject.GetComponent<Animator>();
    }

    // 一定時間後に処理を呼び出すコルーチン
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    public void EndAction()
    {
        if (!is_action) return;

        saveData.CreateMemento();
        is_action = false;
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
        Vector2Int target_int_pos = pos + _vec;

        //移動先探索
        ObjId id = stageManager.GetTileId(target_int_pos);
        Debug.Log("移動先id:" + id + ":" + target_int_pos);
        if (id != ObjId.EMPTY && id != ObjId.GOAL && id != ObjId.HOLE)
        {
            if (!stageManager.PushBlock(pos + _vec, _vec))
                return false;//移動しない
        }

        is_action = true;
        //移動
        Move(target_pos, () =>
        {//終了時処理
            if(!CheckFloor())//足元チェック
                EndAction();
            UpdateAttackTarget();
        });

        soundManager.Play(soundManager.player_move);

        return true;
    }


    //ゴール処理
    private bool CheckFloor()
    {
        Vector2Int p_pos = GetIntPos();
        ObjId id = stageManager.GetTileId(p_pos);
        if (id == ObjId.GOAL)//ゴール処理
        {
            is_action = true;
            //アニメーション
            animator.SetTrigger("Goal");

            soundManager.Play(soundManager.player_goal);//SE
            StartCoroutine( DelayCoroutine(1.0f, ()=>
            { //終了時に実行
                Goal();
            }));

            return true;
        }
        else if (id == ObjId.HOLE)//落ちる処理
        {
            is_action = true;
            //アニメーション
            animator.SetTrigger("Fall");

            StartCoroutine(DelayCoroutine(1.0f, () =>
            { //終了時に実行
                Fall();
            }));

            return true;
        }
        else
            return false;
        
    }

    //ふっとばし（爆心地）
    public void StartLeap(Vector2Int _hypocenter, int _power)
    {
        if (is_action) return;       

        leap_distance = _power;
        Debug.Log("power:"+_power+ "hypo:"+_hypocenter);

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
        is_leap = true;

        //Leapアニメーション
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(target_pos, 0.8f));//移動
        seq.Join(transform.DOScale(Vector3.one * 1.0f, 0.4f).SetLoops(2, LoopType.Yoyo));//サイズ変更
        seq.Play().OnComplete(() =>
         {//終了処理
             landing();//着地処理
             UpdateAttackTarget();
         });
    }

    //着地処理
    private void landing()
    {
        Debug.Log("着地");

        if (CheckFloor()) return;//足元チェック

        Vector2Int p_pos = GetIntPos();
        //着地地点のid取得
        ObjId id = stageManager.GetTileId(p_pos);
        if (id == ObjId.BLOCK)//ブロックなら開ける
        {
            stageManager.OpenBlock(p_pos);
        }
        else if (id == ObjId.MINE)//地雷なら前回と同じ方向に吹っ飛ぶ
        {
            is_action = false;
            Vector2Int hypo_pos = p_pos + (GetDirectionVec() * -1);
            StartLeap(hypo_pos, leap_distance);
            stageManager.OpenBlock(p_pos);
            stageManager.CheckExplosion();//爆発したかチェック
            return;
        }

        soundManager.Play(soundManager.player_land);
        is_leap = false;
        EndAction();
    }

    //前方に攻撃
    public bool Attack()
    {
        UpdateAttackTarget();//現在の攻撃対象更新

        //攻撃アニメーション

        //ブロックを壊す
        if(stageManager.OpenBlock(attack_target_pos))
        {
            stageManager.CheckExplosion();//爆発したかチェック
            UpdateAttackTarget();//情報更新
            if (!is_leap)
                saveData.CreateMemento();

            soundManager.Play(soundManager.player_attack_hit);
            return true;
        }
        else//壊せなかった場合
        {
            soundManager.Play(soundManager.player_attack_miss);
            return false;
        }
    }
    public bool AttackPos(Vector2Int _pos)//デバッグ用
    {
        //ブロックを壊す
        if (stageManager.OpenBlock(_pos))
        {
            stageManager.CheckExplosion();//爆発したかチェック
            UpdateAttackTarget();//情報更新
            if (!is_leap)
                saveData.CreateMemento();

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
        Debug.Log("FALL!!");
        EndAction();//アクション終了
    }

    public void Goal()
    {
        Debug.Log("GOAL!!");
        EndAction();//アクション終了
        animator.SetTrigger("Default");

        //終了時に実行させたい
        stageManager.Clear();
    }

    public void UpdateAttackTarget()
    {
        //攻撃対象の情報取得
        attack_target_pos = GetIntPos() + GetDirectionVec();
    }

    public void SetDirection(Vector2Int _vec)
    {
        if (_vec == Vector2Int.up)
        {
            direction = Direction.UP;
            animator.SetTrigger("Up");
        }
        else if (_vec == Vector2Int.left)
        {
            direction = Direction.LEFT;
            animator.SetTrigger("Left");
        }
        else if (_vec == Vector2Int.down)
        {
            direction = Direction.DOWN;
            animator.SetTrigger("Down");
        }
        else if (_vec == Vector2Int.right)
        {
            direction = Direction.RIGHT;
            animator.SetTrigger("Right");
        }

        UpdateAttackTarget();
    }
    public void SetDirection(Direction _dir)
    {
        direction = _dir;

        //見た目変更
        if (direction == Direction.UP)
            animator.SetTrigger("Up");
        else if (direction == Direction.LEFT)
            animator.SetTrigger("Left");
        else if (direction == Direction.DOWN)
            animator.SetTrigger("Down");
        else if (direction == Direction.RIGHT)
            animator.SetTrigger("Right");

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


    public PlayerMemento CreateMemento()
    {
        //Debug.Log("PlayerMementoの作成");
        var memento = new PlayerMemento();
        memento.position = transform.position;
        memento.direction = direction;
        return memento;
    }

    public void SetMemento(PlayerMemento memento)
    {
        //Debug.Log("PlayerMementoの呼び出し" + memento.position);
        transform.position = memento.position;
        SetDirection(memento.direction);
        is_action = false;
        animator.SetTrigger("Default");
        UpdateAttackTarget();
    }
}
