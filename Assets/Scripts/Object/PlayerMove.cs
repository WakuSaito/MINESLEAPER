using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//方向
public enum Direction
{
    UP,
    LEFT,
    DOWN,
    RIGHT,
}

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Header("ブロックを押せる(デバッグ用)")]
    bool on_canpush = true;

    StageManager stageManager;
    SaveData saveData;

    [SerializeField]//爆発での移動距離
    float move_length = 3.0f;
    [SerializeField]//爆風を受けられる距離
    float can_hit_distance = 1.5f;

    [SerializeField]//爆風で飛ぶ距離
    int leap_distance = 3;

    public bool is_moving; //移動中判定
    public bool is_leaping;//吹っ飛び中

    // Playerから移動速度を取得できるように設定
    [SerializeField] float moveSpeed;

    //向き
    Direction direction = Direction.DOWN;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
    }

    //　コルーチンを使って徐々に目的地に近づける
    IEnumerator Move(Vector2 _target_pos)
    {
        is_moving = true;//移動中にする
        // targetposとの差があるなら繰り返す:目的地に辿り着くまで繰り返す
        while ((_target_pos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPosに近づける
            transform.position = Vector2.MoveTowards(transform.position, _target_pos, moveSpeed * Time.deltaTime);
            if (is_leaping)//途中で吹っ飛び始めたらキャンセル
            {
                is_moving = false;
                yield break;
            }
            // 徐々に近づけるため
            yield return null;
        }

        // 移動処理が完了したら目的地に到着させる
        transform.position = _target_pos;
        is_moving = false;
        saveData.CreateMemento();//状態保存
    }
    IEnumerator Leap(Vector2 _target_pos)
    {
        is_leaping = true;
        // targetposとの差があるなら繰り返す:目的地に辿り着くまで繰り返す
        while ((_target_pos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPosに近づける
            transform.position = Vector2.MoveTowards(transform.position, _target_pos, moveSpeed * Time.deltaTime);
            // 徐々に近づけるため
            yield return null;
        }

        // 移動処理が完了したら目的地に到着させる
        transform.position = _target_pos;
        is_leaping = false;
        saveData.CreateMemento();//状態保存
        landing();//着地処理
    }
    //着地処理
    private void landing()
    {
        Debug.Log("着地");
        Vector2Int p_pos = GetIntPos();
        //着地地点のid取得
        ObjId id = stageManager.GetTileId(p_pos);
        if(id == ObjId.BLOCK)//ブロックなら開ける
        {
            stageManager.OpenBlock(p_pos);
        }
        if (id == ObjId.MINE)//地雷なら前回と同じ方向に吹っ飛ぶ
        {
            Vector2Int hypo_pos = p_pos + (GetDirectionVec() * -1);
            StartLeap(hypo_pos);
            stageManager.OpenBlock(p_pos);
        }
    }

    //ふっとばし（爆心地）
    public void StartLeap(Vector2Int _hypocenter)
    {
        if (is_leaping) return;

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
        StartCoroutine(Leap(target_pos));
    }

    public bool StartMove(Vector2Int _vec)
    {
        if (IsAction()) return false;//行動中なら停止
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
        
        
        StartCoroutine(Move(target_pos));

        return true;
    }
    //何か行動中か
    public bool IsAction()
    {
        return is_leaping || is_moving;
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
