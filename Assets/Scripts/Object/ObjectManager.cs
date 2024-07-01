using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]//爆発での移動距離
    float move_length = 3.0f;
    [SerializeField]//爆風を受けられる距離
    float can_hit_distance = 1.5f;

    bool on_moveing = false;//移動中フラグ
    Vector2 movetarget_pos;//移動目標地点

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        //移動フラグチェック
        if (!on_moveing) return;
        
        Vector2 vec = movetarget_pos - (Vector2)transform.position;

        Debug.Log((Vector2)transform.position);
        rb.velocity = vec * 1.5f;

        if (vec.magnitude < 0.1f)
        {
            on_moveing = false;

            rb.velocity = Vector2.zero;
        }

    }

    //爆風の影響を受ける関数
    public void HitExplosion(Vector2Int _pos)//摩擦を加えたい
    {
        Vector2 explo_pos = _pos + new Vector2(0.5f, 0.5f);//爆破地点の中心座標
        Vector2 explo_vec = explo_pos - (Vector2)transform.position;//爆破地点までのベクトル

        Debug.Log("爆破ベクトル:" + explo_vec);

        //爆破地点から離れていたら実行しない
        if (explo_vec.magnitude > can_hit_distance) return;

        //目標地点までのベクトル計算
        Vector2 tmp_vec = explo_vec.normalized * -1.0f * move_length;

        //移動目標地点の更新
        movetarget_pos = (Vector2)transform.position + tmp_vec;
        //移動フラグオン
        on_moveing = true;

        Debug.Log("目標座標:"+movetarget_pos);
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
