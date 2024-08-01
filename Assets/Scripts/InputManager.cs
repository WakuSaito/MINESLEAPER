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
 
    //�I�𒆂̍��W
    Vector2Int select_pos;
    //�I�𒆂�ID
    ObjId select_id;

    private void Awake()
    {
        //�I�u�W�F�N�g�擾
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();

        menuUI = GameObject.Find("MenuUI").GetComponent<MenuUI>();

        debugMan = GameObject.Find("DebugMan").GetComponent<DebugMan>();
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int p_pos = playerMove.GetIntPos();//�v���C���[���W�擾

        //�I���u���b�N�����߂�
        if (!debugMan.on_open_anywhere)//�ʏ�
        {
            //�I���ʒu�����߂�
            select_pos = playerMove.attack_target_pos;
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            if(!playerMove.is_action)
            {
                //�����ڕύX
                selectTile.SetActive(true);
                selectTile.transform.position = pos;
            }
            else
                selectTile.SetActive(false);

        }
        else//�ǂ��ł��J������
        {
            select_pos = GetIntMousePos();
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            //�����ڕύX
            selectTile.SetActive(true);
            selectTile.transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!menuUI.is_animation)//���j���[���A�j���[�V�������Ŗ���
            {
                if (menuUI.is_active)//�\����\����؂�ւ�
                    menuUI.CloseUI();
                else
                    menuUI.OpenUI();
            }      
        }

        //���j���[����
        if(menuUI.is_active && !menuUI.is_animation)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                menuUI.SelectButton();
            }
        }
        //���͏���
        else 
        {
            if (playerMove.is_action) return;

            //�v���C���[�̈ړ�
            Vector2Int input_vec = new Vector2Int();
            input_vec.x = (int)Input.GetAxisRaw("Horizontal");
            input_vec.y = (int)Input.GetAxisRaw("Vertical");
            //�x�N�g���ݒ�
            playerMove.StartMove(input_vec);

            //�U��
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (debugMan.on_open_anywhere)//�ǂ��ł��J������
                    playerMove.AttackPos(select_pos);
                else
                {
                    playerMove.Attack();
                }
            }

            //���ݒu�؂�ւ�  
            if(Input.GetMouseButtonDown(1))
            {
                stageManager.SwitchFlag(select_pos);
            } 
            if (Input.GetKeyDown(KeyCode.F))
            {
                stageManager.SwitchFlag(playerMove.attack_target_pos);
            }

            //�A���h�D
            if (Input.GetKeyDown(KeyCode.R))
                saveData.SetMemento(saveData.GetMementoEnd() - 1);
        }

    }


    //�}�E�X�̍��W�擾
    public Vector2Int GetIntMousePos()
    {
        Vector3 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.z = 10.0f;

        //int�ϊ����Ă��������Ȃ�悤�ɏC��
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;

        //int�ɕϊ�
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
}
