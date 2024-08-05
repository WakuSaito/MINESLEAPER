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
    SoundManager soundManager;

    //���j���[���L��
    public bool is_active = false;
    //�A�j���[�V������
    public bool is_animation = false;

    //�摜UI
    [SerializeField]
    GameObject ui_menu;
    [SerializeField]
    GameObject ui_button;
    [SerializeField]
    GameObject ui_stage;

    //�{�^��
    [SerializeField]
    Button button_reset;
    [SerializeField]
    Button button_exit;
    Button[] button_stage;//�X�e�[�W�{�^��

    Animator animator_button;
    Animator animator_stage;

    private void Awake()
    {
        animator_button = ui_button.GetComponent<Animator>();
        animator_stage  = ui_stage.GetComponent<Animator>();

        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        //�X�e�[�W�{�^���̑S�擾
        button_stage = ui_stage.GetComponentsInChildren<Button>();

        //�{�^��������
        SetIntaractable(false);
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

        soundManager.SetVolume(0.3f);//��������������
        //�A�j���[�V����
        {
            //���ԂɃA�j���[�V����������
            animator_button.SetTrigger("StartIn");
            StartCoroutine(DelayCoroutine(0.25f, () =>
            {
                animator_stage.SetTrigger("StartIn");

                StartCoroutine(DelayCoroutine(0.4f, () =>
                {//�I������
                    is_animation = false;

                    //�{�^���L����
                    SetIntaractable(true);
                }));
            }));
        }        
    }

    //���j���[�����
    public void CloseUI()
    {
        is_animation = true;
        //�I������
        EventSystem.current.SetSelectedGameObject(null);
        //�{�^���̖�����
        SetIntaractable(false);

        soundManager.SetVolume(1.0f);//���̑傫����ʏ�ɖ߂�

        //�A�j���[�V����
        {
            animator_button.SetTrigger("StartOut");
            animator_stage.SetTrigger("StartOut");
        }

        StartCoroutine(DelayCoroutine(0.3f, () =>
        {//�I������
            is_animation = false;
            is_active = false;
        }));     
    }

    public void SelectButton()
    {
        //�I�𒆂̃{�^���������Ȃ�
        if(!EventSystem.current.currentSelectedGameObject)
        {
            // ���Z�b�g�{�^����I��
            EventSystem.current.SetSelectedGameObject(button_reset.gameObject);
        }
    }
    //�{�^���̗L����������
    private void SetIntaractable(bool _intaractable)
    {
        button_reset.interactable = _intaractable;
        button_exit.interactable = _intaractable;
        int count = 1;
        foreach(var button in button_stage)
        {
            //���ǂ蒅���Ă��Ȃ��X�e�[�W�͖�����
            if (count > stageManager.deepest_stage)
            {
                button.interactable = false; 
            }
            else 
                button.interactable = _intaractable;
            count++;
        }
    }

    //���Z�b�g���s
    public void OnReset()
    {
        Debug.Log("�X�e�[�W���Z�b�g");
        //�ŏ��̏�Ԃɖ߂�
        saveData.SetMemento(0);

        soundManager.Play(soundManager.ui_change);

        CloseUI();//���j���[�����
    }

    //�Q�[���I��
    public void OnExit()
    {
        Debug.Log("�Q�[���I��");

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
    #else
        Application.Quit();//�Q�[���v���C�I��
    #endif
    }

    //�X�e�[�W�؂�ւ�
    public void SelectStage(int _num)
    {
        stageManager.ChangeStage(_num);//�X�e�[�W�Ăяo��

        CloseUI();//���j���[�����
    }
}
