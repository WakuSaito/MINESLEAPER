using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField, Header("�v���[���[�̎���̂݊J������(�f�o�b�O�p)")]
    bool on_open_surroundonly = true;

    StageManager stageManager;
    PlayerMove playerMove;
    SaveData saveData;

    [SerializeField] GameObject selectTile;
 

    //�I�𒆂̍��W
    Vector2Int select_pos;


    private void Awake()
    {
        //�I�u�W�F�N�g�擾
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
    }

    private void Start()
    {
        saveData.CreateMemento();
    }


    // Update is called once per frame
    void Update()
    {
        Vector2Int p_pos = playerMove.GetIntPos();//�v���C���[���W�擾

        //�I���u���b�N�����߂�
        if (on_open_surroundonly)//�ʏ�
        {
            if (p_pos != GetIntMousePos())
            {
                //�I���ʒu�����߂�
                select_pos = p_pos + GetDirectionPM();
                Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
                //�����ڕύX
                selectTile.SetActive(true);
                selectTile.transform.position = pos;
            }
            else
            {
                select_pos = p_pos;
                selectTile.SetActive(false);
            }
        }
        else//�ǂ��ł��J������
        {
            select_pos = GetIntMousePos();
            Vector3 pos = new Vector3(select_pos.x + 0.5f, select_pos.y + 0.5f, 0);
            //�����ڕύX
            selectTile.SetActive(true);
            selectTile.transform.position = pos;
        }


        //����
        {
            //�v���C���[�̈ړ�
            Vector2Int input_vec = new Vector2Int();
            input_vec.x = (int)Input.GetAxisRaw("Horizontal");
            input_vec.y = (int)Input.GetAxisRaw("Vertical");
            //�x�N�g���ݒ�
            if(playerMove.StartMove(input_vec))
            {
                
            }

            //�u���b�N���J����
            if (Input.GetMouseButtonDown(0) && 
                !playerMove.is_moving)//�ړ����͎��s���Ȃ�
            {
                //�}�E�X�ʒu�̃u���b�N���J��
                if (stageManager.OpenBlock(select_pos))
                {
                    //�J�������ԕۑ�
                    EndAction();
                }
            }
            //���ݒu�؂�ւ�  
            if(Input.GetMouseButtonDown(1))
            {
                stageManager.SwitchFlag(select_pos);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
            saveData.SetMemento(saveData.GetMementoEnd() - 1);
    }

    //�����s�����I������Ă΂��
    private void EndAction()
    {
        //��ԕۑ�
        saveData.CreateMemento();
    }


    //�v���C���[����}�E�X�܂ł̕������擾
    private Vector2Int GetDirectionPM()
    {
        //���W�擾
        Vector2 p_pos = playerMove.gameObject.transform.position;
        Vector2 m_pos = GetMousePos();
        //�v���C���[����}�E�X�܂ł̃x�N�g��
        Vector2 vec = m_pos - p_pos;
        vec.Normalize();
        //����
        Vector2Int direction = new Vector2Int();

        //�������ϊ�
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