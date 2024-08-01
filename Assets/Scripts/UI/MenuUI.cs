using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


public class MenuUI : MonoBehaviour
{
    public bool is_active = false;

    public bool is_animation = false;

    //画像UI
    [SerializeField]
    GameObject ui_menu;
    [SerializeField]
    GameObject ui_button;
    [SerializeField]
    GameObject ui_reset_button;
    [SerializeField]
    GameObject ui_exit_button;
    [SerializeField]
    GameObject ui_stage;

    Animator animator_button;
    Animator animator_stage;

    private void Awake()
    {
        animator_button = ui_button.GetComponent<Animator>();
        animator_stage  = ui_stage.GetComponent<Animator>();

        
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
        //ui_menu.SetActive(true);

        //アニメーション
        {
            animator_button.SetTrigger("StartIn");
            StartCoroutine(DelayCoroutine(0.25f, () =>
            {
                animator_stage.SetTrigger("StartIn");

                StartCoroutine(DelayCoroutine(0.4f, () =>
                {
                    is_animation = false;

                    ui_button.GetComponent<CanvasGroup>().alpha = 1;
                    ui_stage.GetComponent<CanvasGroup>().alpha = 1;
                    
                }));
            }));
        }        
    }

    //メニューを閉じる
    public void CloseUI()
    {
        is_animation = true;
        EventSystem.current.SetSelectedGameObject(null);

        //アニメーション
        {
            animator_button.SetTrigger("StartOut");
            animator_stage.SetTrigger("StartOut");
        }

        StartCoroutine(DelayCoroutine(0.3f, () =>
        {
            ui_button.GetComponent<CanvasGroup>().alpha = 0;
            ui_stage.GetComponent<CanvasGroup>().alpha = 0;

            is_animation = false;
            is_active = false;
            // ui_menu.SetActive(false);
        }));     
    }

    public void SelectButton()
    {
        // 選択中のオブジェクト取得
        if(!EventSystem.current.currentSelectedGameObject)
        {
            // 一つ下のオブジェクトを選択
            EventSystem.current.SetSelectedGameObject(ui_reset_button);
        }
    }
}
