using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Header("ブロックを押せる(デバッグ用)")]
    bool on_canpush = true;

    [SerializeField]
    StageManager stageManager;

    [SerializeField]//爆発での移動距離
    float move_length = 3.0f;
    [SerializeField]//爆風を受けられる距離
    float can_hit_distance = 1.5f;

    [SerializeField]//所持している特性
    string[] have_ability;

    bool isMoving; // 移動中判定
    // Playerから移動速度を取得できるように設定
    [SerializeField] float moveSpeed;

    // 外部で入力内容を保持する用の変数
    Vector2 input;

    private void Awake()
    {
    }

    
    void Update()
    {
        

        // 移動中だと入力を受け付けない
        if (!isMoving)
        {
            input = Vector2.zero;

            // キーボードの入力情報をinputに格納
            if (Input.GetKeyDown(KeyCode.D))           
                input.x = 1.0f;
            else if (Input.GetKeyDown(KeyCode.A))
                input.x = -1.0f;
            else if (Input.GetKeyDown(KeyCode.W))
                input.y = 1.0f;
            else if (Input.GetKeyDown(KeyCode.S))
                input.y = -1.0f;

            // 入力があった時
            if (input != Vector2.zero)
            {
                StartCoroutine( Move( GetIntPos(input) ) );
            }
        }
    }

    //　コルーチンを使って徐々に目的地に近づける
    IEnumerator Move(Vector2Int _vec)
    {
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
                    yield break;
            }
            else
                yield break;//移動しない
        }


        // 移動中は入力を受け付けない
        isMoving = true;

        // targetposとの差があるなら繰り返す:目的地に辿り着くまで繰り返す
        while ((targetPos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPosに近づける
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            // 徐々に近づけるため
            yield return null;
        }

        // 移動処理が完了したら目的地に到着させる
        transform.position = targetPos;
        isMoving = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            Debug.Log("ゴール");
        }
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
}
