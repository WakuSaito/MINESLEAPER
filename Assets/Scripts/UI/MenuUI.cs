using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


public class MenuUI : MonoBehaviour
{
    StageManager stageManager;
    SaveData saveData;

    //メニューが有効
    public bool is_active = false;
    //アニメーション中
    public bool is_animation = false;

    //画像UI
    [SerializeField]
    GameObject ui_menu;
    [SerializeField]
    GameObject ui_button;
    [SerializeField]
    GameObject ui_stage;

    //ボタン
    [SerializeField]
    Button button_reset;
    [SerializeField]
    Button button_exit;
    Button[] button_stage;//ステージボタン

    Animator animator_button;
    Animator animator_stage;

    private void Awake()
    {
        animator_button = ui_button.GetComponent<Animator>();
        animator_stage  = ui_stage.GetComponent<Animator>();

        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();

        //ステージボタンの全取得
        button_stage = ui_stage.GetComponentsInChildren<Button>();

        //ボタン無効化
        SetIntaractable(false);
    }

    // 一定時間後に処理を呼び出すコルーチン
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    //メニューを開く
    public void OpenUI()
    {
        is_active = true;
        is_animation = true;

        //アニメーション
        {
            //順番にアニメーションさせる
            animator_button.SetTrigger("StartIn");
            StartCoroutine(DelayCoroutine(0.25f, () =>
            {
                animator_stage.SetTrigger("StartIn");

                StartCoroutine(DelayCoroutine(0.4f, () =>
                {//終了処理
                    is_animation = false;

                    //ボタン有効化
                    SetIntaractable(true);
                }));
            }));
        }        
    }

    //メニューを閉じる
    public void CloseUI()
    {
        is_animation = true;
        //選択解除
        EventSystem.current.SetSelectedGameObject(null);
        //ボタンの無効化
        SetIntaractable(false);


        //アニメーション
        {
            animator_button.SetTrigger("StartOut");
            animator_stage.SetTrigger("StartOut");
        }

        StartCoroutine(DelayCoroutine(0.3f, () =>
        {//終了処理
            is_animation = false;
            is_active = false;
        }));     
    }

    public void SelectButton()
    {
        //選択中のボタンが無いなら
        if(!EventSystem.current.currentSelectedGameObject)
        {
            // リセットボタンを選択
            EventSystem.current.SetSelectedGameObject(button_reset.gameObject);
        }
    }
    //ボタンの有効化無効化
    private void SetIntaractable(bool _intaractable)
    {
        button_reset.interactable = _intaractable;
        button_exit.interactable = _intaractable;
        int count = 1;
        foreach(var button in button_stage)
        {
            //たどり着いていないステージは無効化
            if (count > stageManager.deepest_stage)
            {
                button.interactable = false; 
            }
            else 
                button.interactable = _intaractable;
            count++;
        }
    }

    //リセット実行
    public void OnReset()
    {
        Debug.Log("ステージリセット");
        //最初の状態に戻す
        saveData.SetMemento(0);

        CloseUI();//メニューを閉じる
    }

    //ゲーム終了
    public void OnExit()
    {
        Debug.Log("ゲーム終了");
    }

    //ステージ切り替え
    public void SelectStage(int _num)
    {
        stageManager.ChangeStage(_num);//ステージ呼び出し

        CloseUI();//メニューを閉じる
    }
}
