using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]//爆発での移動距離
    float move_length = 3.0f;
    [SerializeField]//爆風を受けられる距離
    float can_hit_distance = 1.5f;

    [SerializeField]//所持している特性
    string[] have_ability;

    bool on_moveing = false;//移動中フラグ
    Vector2 movetarget_pos;//移動目標地点

    bool isMoving; // 移動中判定
    // Playerから移動速度を取得できるように設定
    [SerializeField] float moveSpeed;

    // 外部で入力内容を保持する用の変数
    Vector2 input;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        // 移動中だと入力を受け付けない
        if (!isMoving)
        {
            // キーボードの入力情報をinputに格納
            input.x = Input.GetAxisRaw("Horizontal"); // 横方向
            input.y = Input.GetAxisRaw("Vertical");  // 縦方向

            // 入力があった時
            if (input != Vector2.zero)
            {
                // 入力があった分を目的地にする
                Vector2 targetPos = transform.position;
                targetPos += input;
                StartCoroutine(Move(targetPos));
            }
        }
    }

    //　コルーチンを使って徐々に目的地に近づける
    IEnumerator Move(Vector3 targetPos)
    {
        // 移動中は入力を受け付けない
        isMoving = true;

        // targetposとの差があるなら繰り返す:目的地に辿り着くまで繰り返す
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPosに近づける
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
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
        if (pos.x < 0) pos.x -= 1.0f;
        if (pos.y < 0) pos.y -= 1.0f;
        //intに変換
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
}
