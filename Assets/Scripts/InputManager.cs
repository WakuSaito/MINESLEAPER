using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField, Header("プレーヤーの前方のみ開けられる(デバッグ用)")]
    bool on_open_frontonly = true;

    StageManager stageManager;
    PlayerMove playerMove;
    SaveData saveData;

    [SerializeField] GameObject selectTile;
 
    //選択中の座標
    Vector2Int select_pos;
    //選択中のID
    ObjId select_id;

    private void Awake()
    {
        //オブジェクト取得
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int p_pos = playerMove.GetIntPos();//プレイヤー座標取得

        //選択ブロックを決める
        if (on_open_frontonly)//通常
        {
            //選択位置を決める
            select_pos = playerMove.attack_target_pos;
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            if(!playerMove.is_action)
            {
                //見た目変更
                selectTile.SetActive(true);
                selectTile.transform.position = pos;
            }
            else
                selectTile.SetActive(false);

        }
        else//どこでも開けられる
        {
            select_pos = GetIntMousePos();
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            //見た目変更
            selectTile.SetActive(true);
            selectTile.transform.position = pos;
        }


        //入力処理
        {
            //プレイヤーの移動
            Vector2Int input_vec = new Vector2Int();
            input_vec.x = (int)Input.GetAxisRaw("Horizontal");
            input_vec.y = (int)Input.GetAxisRaw("Vertical");
            //ベクトル設定
            playerMove.StartMove(input_vec);

            //攻撃
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (on_open_frontonly)
                    playerMove.Attack();
                else
                {
                    //id取得
                    select_id = stageManager.GetTileId(select_pos);

                    //ブロックを開ける
                    if (stageManager.OpenBlock(GetIntMousePos()))
                        EndAction();
                }
            }

            //旗設置切り替え  
            if(Input.GetMouseButtonDown(1))
            {
                stageManager.SwitchFlag(select_pos);
            } 
            if (Input.GetKeyDown(KeyCode.F))
            {
                stageManager.SwitchFlag(playerMove.attack_target_pos);
            }

            //アンドゥ
            if (Input.GetKeyDown(KeyCode.R))
                saveData.SetMemento(saveData.GetMementoEnd() - 1);
        }

    }

    //何か行動を終えたら呼ばれる
    private void EndAction()
    {
        //状態保存
        saveData.CreateMemento();
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
