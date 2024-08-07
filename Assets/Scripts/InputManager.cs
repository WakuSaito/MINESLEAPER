using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{

    StageManager stageManager;
    PlayerMove playerMove;
    SaveData saveData;
    SoundManager soundManager;//サウンドスクリプト

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
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

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
            SwitchMenu();
        }
        //メニューUIがアニメーション中なら入力を受け付けない
        if (menuUI.is_animation) return;

        //メニュー操作
        if(menuUI.is_active)
        {
            //ボタンが選択状態で無いとき選択する
            float hor = Input.GetAxisRaw("Vertical");
            if (hor != 0)
            {
                menuUI.SelectButton();
            }
        }
        //入力処理
        else 
        {
            if (playerMove.is_action) return;
            //アンドゥ
            if (Input.GetKeyDown(KeyCode.R))
            {
                playerMove.is_fall = false;
                Undo();
            }

            if (playerMove.is_fall) return;//行動中でない
            if (EventSystem.current.currentSelectedGameObject) return;//ボタン選択中でない

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
                stageManager.SwitchFlag(GetIntMousePos());
            } 
            if (Input.GetKeyDown(KeyCode.F))
            {
                stageManager.SwitchFlag(playerMove.attack_target_pos);
            }

            
        }

    }
    //アンドゥ
    public void Undo()
    {
        if (playerMove.is_action) return;//行動中でない

        int num = saveData.GetMementoEnd() - 1;//セットするデータ番号（現在のひとつ前）
        if (num >= 0)//データがあれば
        {
            saveData.SetMemento(num);//ステージデータセット
            soundManager.Play(soundManager.stage_undo);//SE
        }
    }
    //メニューの開閉切り替え
    public void SwitchMenu()
    {
        if (menuUI.is_animation) return;//メニューがアニメーション中でない

        if (menuUI.is_active)//表示非表示を切り替え
            menuUI.CloseUI();
        else
            menuUI.OpenUI();

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
