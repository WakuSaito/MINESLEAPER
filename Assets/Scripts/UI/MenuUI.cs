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

    //�摜UI
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

    // ��莞�Ԍ�ɏ������Ăяo���R���[�`��
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    //���j���[���J��
    public void OpenUI()
    {
        is_active = true;
        is_animation = true;
        //ui_menu.SetActive(true);

        //�A�j���[�V����
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

    //���j���[�����
    public void CloseUI()
    {
        is_animation = true;
        EventSystem.current.SetSelectedGameObject(null);

        //�A�j���[�V����
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
        // �I�𒆂̃I�u�W�F�N�g�擾
        if(!EventSystem.current.currentSelectedGameObject)
        {
            // ����̃I�u�W�F�N�g��I��
            EventSystem.current.SetSelectedGameObject(ui_reset_button);
        }
    }
}
