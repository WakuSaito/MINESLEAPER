using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField, Header("プレーヤーの周りのみ開けられる(デバッグ用)")]
    bool on_open_surroundonly = true;

    StageManager stageManager;
    PlayerMove playerMove;
    SaveData saveData;

    [SerializeField] GameObject selectTile;
 
    //選択中の座標
    Vector2Int select_pos;
    //選択中のID
    ObjId select_id;

    //左クリックを押し続けているか
    bool is_hold_clickL = false;
    //左クリック開始地点
    Vector2Int start_pos_clickL;
    //左クリックを押している時間
    float time_hold_clickL = 0.0f;
    [SerializeField]//成立する長押しの時間
    float hold_sec = 1.0f;

    private void Awake()
    {
        //オブジェクト取得
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
    }

    private void Start()
    {
        saveData.CreateMemento();
        start_pos_clickL = Vector2Int.zero;
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int p_pos = playerMove.GetIntPos();//プレイヤー座標取得

        //選択ブロックを決める
        if (on_open_surroundonly)//通常
        {
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
        }
        else//どこでも開けられる
        {
            select_pos = GetIntMousePos();
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            //見た目変更
            selectTile.SetActive(true);
            selectTile.transform.position = pos;
        }

        //id取得
        select_id = stageManager.GetTileId(select_pos);

        //入力処理
        {
            //プレイヤーの移動
            Vector2Int input_vec = new Vector2Int();
            input_vec.x = (int)Input.GetAxisRaw("Horizontal");
            input_vec.y = (int)Input.GetAxisRaw("Vertical");
            //ベクトル設定
            playerMove.StartMove(input_vec);


            //ブロックを開ける
            if (Input.GetMouseButtonDown(0))
            {
                switch (select_id)
                {
                    case ObjId.BLOCK:
                    case ObjId.MINE:
                        //マウス位置のブロックを開く
                        if (stageManager.OpenBlock(select_pos))
                        {
                            //開けたら状態保存
                            EndAction();
                        }
                        break;

                    case ObjId.EMPTY:
                        //数字がある時のみ実行
                        if (stageManager.GetMineCount(select_pos) <= 0) break;

                        is_hold_clickL = true;//クリック状態切り替え
                        start_pos_clickL = select_pos;//クリック位置保存
                        break;            
                }
                
            }
            else if(Input.GetMouseButtonUp(0))
            {
                is_hold_clickL = false;//クリック状態切り替え
            }

            //旗設置切り替え  
            if(Input.GetMouseButtonDown(1))
            {
                stageManager.SwitchFlag(select_pos);
            }

            //アンドゥ
            if (Input.GetKeyDown(KeyCode.R))
                saveData.SetMemento(saveData.GetMementoEnd() - 1);
        }

        //長押し処理 没になるかも
        {
            if (is_hold_clickL &&
                start_pos_clickL == select_pos) //押し始めた位置と現在の位置が同じ    
            {
                //時間カウント
                time_hold_clickL += Time.deltaTime;           
            }
            else
            {
                //時間リセット
                time_hold_clickL = 0.0f;
                //長押し解除
                is_hold_clickL = false;
            }
            //成立したときの処理
            if(time_hold_clickL >= hold_sec)
            {
                //リセット
                is_hold_clickL = false;
                time_hold_clickL = 0.0f;

                Debug.Log("実行");
            }
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
