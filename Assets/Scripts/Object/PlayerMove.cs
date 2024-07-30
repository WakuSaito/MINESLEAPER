using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//����
public enum Direction
{
    UP,
    LEFT,
    DOWN,
    RIGHT,
}

public class PlayerMove : ObjBase
{
    [SerializeField, Header("�u���b�N��������(�f�o�b�O�p)")]
    bool on_canpush = true;

    StageManager stageManager;
    SaveData saveData;

    [SerializeField]//�����ł̈ړ�����
    float move_length = 3.0f;
    [SerializeField]//�������󂯂��鋗��
    float can_hit_distance = 1.5f;

    //�����Ŕ�ԋ���
    int leap_distance = 3;

    public bool is_action; //�s�����t���O

    //�U���Ώۂ̏��
    public Vector2Int attack_target_pos;
    ObjId attack_target_id;

    // Player����ړ����x���擾�ł���悤�ɐݒ�
    [SerializeField] float moveSpeed;

    //����
    Direction direction = Direction.DOWN;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
        UpdateAttackTarget();
    }

    //�ړ��J�n
    public bool StartMove(Vector2Int _vec)
    {
        if (is_action) return false;//�s�����Ȃ��~
        if (_vec == Vector2Int.zero) return false;//�ړ��x�N�g�����[��

        //�΂߈ړ��ł��Ȃ��悤�ɂ���@x�D��
        if (_vec.x != 0) _vec.y = 0;

        //���������߂�
        SetDirection(_vec);

        //�����W
        Vector2Int pos = GetIntPos();
        //�ړ�����W
        Vector2 target_pos = (Vector2)transform.position + _vec;

        //�ړ���T��
        ObjId id = stageManager.GetTileId(GetIntPos(target_pos));
        Debug.Log("�ړ���id:" + id + ":" + target_pos);
        if (id != ObjId.EMPTY)
        {
            //�u���b�N������
            if (on_canpush)
            {
                if (!stageManager.PushBlock(pos + _vec, _vec))
                    return false;
            }
            else
                return false;//�ړ����Ȃ�
        }

        is_action = true;
        //�ړ�
        Move(target_pos, () =>
        {//�I��������
            is_action = false;
            saveData.CreateMemento();//��ԕۑ�
            UpdateAttackTarget();
        });

        return true;
    }

    //�ӂ��Ƃ΂��i���S�n�j
    public void StartLeap(Vector2Int _hypocenter, int _power)
    {
        if (is_action) return;

        leap_distance = _power;
        Debug.Log(_power);

        Vector2Int p_pos = GetIntPos();
        Vector2Int vec = (_hypocenter - p_pos) * -1;//���������߂�
        Vector2 target_pos = p_pos + new Vector2(0.5f, 0.5f);//�ڕW�n�_

        SetDirection(vec);//�����ݒ�

        //���[�g�T��
        for (int i=1;i<=leap_distance;i++)
        {
            Vector2Int route_pos = p_pos + (vec * i);
            ObjId id = stageManager.GetTileId(route_pos);
            //�r���ɏ�Q������΂ЂƂO�̍��W�Ŏ~�܂�
            if (id == ObjId.WALL || id== ObjId.NULL)
                break;
            else
                target_pos += vec;
        }
        is_action = true;
        
        //Leap�A�j���[�V����
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(target_pos, 0.8f));//�ړ�
        seq.Join(transform.DOScale(Vector3.one * 1.0f, 0.4f).SetLoops(2, LoopType.Yoyo));//�T�C�Y�ύX
        seq.Play().OnComplete(() =>
         {//�I������
             is_action = false;
             saveData.CreateMemento();//��ԕۑ�
             landing();//���n����
             UpdateAttackTarget();
         });
    }

    //���n����
    private void landing()
    {
        Debug.Log("���n");
        Vector2Int p_pos = GetIntPos();
        //���n�n�_��id�擾
        ObjId id = stageManager.GetTileId(p_pos);
        if (id == ObjId.BLOCK)//�u���b�N�Ȃ�J����
        {
            stageManager.OpenBlock(p_pos);
        }
        if (id == ObjId.MINE)//�n���Ȃ�O��Ɠ��������ɐ������
        {
            Vector2Int hypo_pos = p_pos + (GetDirectionVec() * -1);
            StartLeap(hypo_pos, leap_distance);
            stageManager.OpenBlock(p_pos);
        }
    }

    //�O���ɍU��
    public bool Attack()
    {
        UpdateAttackTarget();//���݂̍U���ΏۍX�V

        //�U���A�j���[�V����

        //�u���b�N����
        if(stageManager.OpenBlock(attack_target_pos))
        {
            UpdateAttackTarget();//���X�V
            saveData.CreateMemento();//�ۑ�
            return true;
        }
        else//�󂹂Ȃ������ꍇ
        {
            return false;
        }
       
    }


    public override void Broken()
    {
        Destroy(gameObject);
    }
    public override void Fall()
    {

    }

    public void UpdateAttackTarget()
    {
        //�U���Ώۂ̏��擾
        attack_target_pos = GetIntPos() + GetDirectionVec();
        attack_target_id = stageManager.GetTileId(attack_target_pos);
    }

    private void SetDirection(Vector2Int _vec)
    {
        if (_vec == Vector2Int.up)
            direction = Direction.UP;
        else if (_vec == Vector2Int.left)
            direction = Direction.LEFT;
        else if (_vec == Vector2Int.down)
            direction = Direction.DOWN;
        else if (_vec == Vector2Int.right)
            direction = Direction.RIGHT;

        UpdateAttackTarget();
    }
    private Vector2Int GetDirectionVec()
    {
        if (direction == Direction.UP)
            return Vector2Int.up;
        else if (direction == Direction.LEFT)
            return Vector2Int.left;
        else if (direction == Direction.DOWN)
            return Vector2Int.down;
        else if (direction == Direction.RIGHT)
            return Vector2Int.right;
        else
            return Vector2Int.zero;
    }

    //Vector2Int�^�̍��W��Ԃ��֐�
    public Vector2Int GetIntPos()
    {
        Vector2 pos = transform.position;
        //�������0�ȉ��ł���肪�N���Ȃ��悤�ɂ���
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;
        //int�ɕϊ�
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
    //������int�^�ɕϊ�
    public Vector2Int GetIntPos(Vector2 _pos)
    {
        Vector2 pos = _pos;
        //�������0�ȉ��ł���肪�N���Ȃ��悤�ɂ���
        if (pos.x < 0) pos.x -= 0.9999f;
        if (pos.y < 0) pos.y -= 0.9999f;
        //int�ɕϊ�
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }

    public PlayerMemento CreateMemento()
    {
        //Debug.Log("PlayerMemento�̍쐬");
        var memento = new PlayerMemento();
        memento.position = transform.position;
        return memento;
    }

    public void SetMemento(PlayerMemento memento)
    {
        //Debug.Log("PlayerMemento�̌Ăяo��" + memento.position);
        transform.position = memento.position;
    }
}
