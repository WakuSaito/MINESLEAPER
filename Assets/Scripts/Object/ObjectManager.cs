using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{

    Rigidbody2D rb;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    //爆風の影響を受ける関数
    public void HitExplosion(Vector2Int _pos)//摩擦を加えたい
    {
        Vector2 explo_pos = _pos + new Vector2(0.5f, 0.5f);//爆破地点の中心座標
        Vector2 move_vec = explo_pos -(Vector2)transform.position;

        float magni = move_vec.magnitude;

        Vector2 target_vec = move_vec.normalized;
        target_vec *= 3.0f;
        target_vec -= move_vec;


        if(magni<1.7f)//とりあえず
        {
            rb.velocity = target_vec * -1.0f;
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
