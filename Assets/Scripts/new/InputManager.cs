using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    StageManager stageManager;
    PlayerMove playerMove;

    [SerializeField] GameObject selectTile;

    //選択中の座標
    Vector2Int select_pos;


    private void Awake()
    {
        //オブジェクト取得
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int p_pos = playerMove.GetIntPos();//プレイヤー座標取得
        
        if (p_pos != GetIntMousePos())
        {
            //選択位置を決める
            select_pos = p_pos + GetDirectionPM();
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            //見た目変更
            selectTile.SetActive(true);
            selectTile.transform.position = pos;
        }
        else
        {
            select_pos = p_pos;
            selectTile.SetActive(false);
        }

        //入力
        {
            //プレイヤーの移動
            Vector2Int input_vec = new Vector2Int();
            input_vec.x = (int)Input.GetAxisRaw("Horizontal");
            input_vec.y = (int)Input.GetAxisRaw("Vertical");
            //ベクトル設定
            playerMove.SetInputVec(input_vec);

            //ブロックを開ける
            if (Input.GetMouseButtonDown(0) && 
                !playerMove.is_moving)//移動中は実行しない
            {
                stageManager.OpenBlock(select_pos);//マウス位置のブロックを開く
            }
        }
    }


    //プレイヤーからマウスまでの方向を取得
    private Vector2Int GetDirectionPM()
    {
        //座標取得
        Vector2 p_pos = playerMove.gameObject.transform.position;
        Vector2 m_pos = GetMousePos();
        //プレイヤーからマウスまでのベクトル
        Vector2 vec = m_pos - p_pos;
        vec.Normalize();
        //方向
        Vector2Int direction = new Vector2Int();

        //無理やり変換
        if (vec.x >= 0.4f) direction.x = 1;
        else if (vec.x <= -0.4f) direction.x = -1;
        else direction.x = 0;
        if (vec.y >= 0.4f) direction.y = 1;
        else if (vec.y <= -0.4f) direction.y = -1;
        else direction.y = 0;

        return direction;
    }

    public Vector2 GetMousePos()
    {
        Vector3 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);

        return pos;
    }

    //マウスの座標取得
    public Vector2Int GetIntMousePos()
    {
        Vector3 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.z = 10.0f;

        //int変換しても正しくなるように修正
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;

        //intに変換
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
}
