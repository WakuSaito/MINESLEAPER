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

    [SerializeField]//所持している特性
    string[] have_ability;

    public bool is_moving; // 移動中判定
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
        is_moving = false;
        saveData.CreateMemento();//状態保存
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Goal")
    //    {
    //        Debug.Log("ゴール");
    //    }
    //}

    public bool StartMove(Vector2Int _vec)
    {
        if (is_moving) return false;//移動中なら停止
        if (_vec == Vector2Int.zero) return false;//移動ベクトルがゼロ

        //斜め移動できないようにする　x優先
        if (_vec.x != 0)
            _vec.y = 0;

        //向きを決める
        SetDirection(_vec);

        //現座標
        Vector2Int pos = GetIntPos();
        //移動先座標
        Vector2 targetPos = (Vector2)transform.position + _vec;
        Debug.Log(_vec);

        //移動先のidを取得
        ObjId id = stageManager.GetTileId(GetIntPos(targetPos));
        Debug.Log("移動先id:" + id + ":" + targetPos);


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

        // 移動中は入力を受け付けない
        is_moving = true;

        StartCoroutine(Move(targetPos));

        return true;
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
