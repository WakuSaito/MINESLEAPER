using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMan : MonoBehaviour
{
    StageManager stageManager;
    SoundManager soundManager;

    [SerializeField, Header("���e��������悤�ɂ���(�f�o�b�O�p)")]
    public bool on_visiblemine = false;
    [SerializeField, Header("�󔒃u���b�N���A���ŊJ��(�f�o�b�O�p)")]
    public bool on_areaopen = false;
    [SerializeField, Header("�ǂ��ł��J������(�f�o�b�O�p)")]
    public bool on_open_anywhere = false;
    [SerializeField, Header("�S�X�e�[�W���(�f�o�b�O�p)")]
    public bool on_allstage_open = false;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

    }
    private void Update()
    {
        //�X�e�[�W�S���
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.G))
        {
            stageManager.deepest_stage = 100;
            soundManager.Play(soundManager.stage_fullopen);
        }
        if(on_allstage_open)
        {
            stageManager.deepest_stage = 100;
            on_allstage_open = false;
            soundManager.Play(soundManager.stage_fullopen);
        }

        //if(Input.GetKeyDown(KeyCode.N))
        //{
        //    stageManager.ChangeNextStage();
        //}
    }
}
