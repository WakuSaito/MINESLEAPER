using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    StageManager stageManager;
    PlayerMove playerMove;
    SaveData saveData;

    MenuUI menuUI;

    DebugMan debugMan;

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

        menuUI = GameObject.Find("MenuUI").GetComponent<MenuUI>();

        debugMan = GameObject.Find("DebugMan").GetComponent<DebugMan>();
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int p_pos = playerMove.GetIntPos();//プレイヤー座標取得

        //選択ブロックを決める
        if (!debugMan.on_open_anywhere)//通常
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!menuUI.is_animation)//メニューがアニメーション中で無い
            {
                if (menuUI.is_active)//表示非表示を切り替え
                    menuUI.CloseUI();
                else
                    menuUI.OpenUI();
            }      
        }

        //メニュー操作
        if(menuUI.is_active && !menuUI.is_animation)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                menuUI.SelectButton();
            }
        }
        //入力処理
        else 
        {
            if (playerMove.is_action) return;

            //プレイヤーの移動
            Vector2Int input_vec = new Vector2Int();
            input_vec.x = (int)Input.GetAxisRaw("Horizontal");
            input_vec.y = (int)Input.GetAxisRaw("Vertical");
            //ベクトル設定
            playerMove.StartMove(input_vec);

            //攻撃
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (debugMan.on_open_anywhere)//どこでも開けられる
                    playerMove.AttackPos(select_pos);
                else
                {
                    playerMove.Attack();
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
