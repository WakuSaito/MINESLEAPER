using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


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
    StageManager stageManager;
    SaveData saveData;
    SoundManager soundManager;

    Animator animator;

    [SerializeField]//�����ł̈ړ�����
    float move_length = 3.0f;
    [SerializeField]//�������󂯂��鋗��
    float can_hit_distance = 1.5f;

    //�����Ŕ�ԋ���
    int leap_distance = 3;

    public bool is_action; //�s�����t���O
    bool is_leap = false;

    //�U���Ώۂ̏��
    public Vector2Int attack_target_pos;

    // Player����ړ����x���擾�ł���悤�ɐݒ�
    [SerializeField] float moveSpeed;

    //����
    Direction direction = Direction.DOWN;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        animator = gameObject.GetComponent<Animator>();
    }

    // ��莞�Ԍ�ɏ������Ăяo���R���[�`��
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    public void EndAction()
    {
        if (!is_action) return;

        saveData.CreateMemento();
        is_action = false;
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
        Vector2Int target_int_pos = pos + _vec;

        //�ړ���T��
        ObjId id = stageManager.GetTileId(target_int_pos);
        Debug.Log("�ړ���id:" + id + ":" + target_int_pos);
        if (id != ObjId.EMPTY && id != ObjId.GOAL && id != ObjId.HOLE)
        {
            if (!stageManager.PushBlock(pos + _vec, _vec))
                return false;//�ړ����Ȃ�
        }

        is_action = true;
        //�ړ�
        Move(target_pos, () =>
        {//�I��������
            if(!CheckFloor())//�����`�F�b�N
                EndAction();
            UpdateAttackTarget();
        });

        soundManager.Play(soundManager.player_move);

        return true;
    }


    //�S�[������
    private bool CheckFloor()
    {
        Vector2Int p_pos = GetIntPos();
        ObjId id = stageManager.GetTileId(p_pos);
        if (id == ObjId.GOAL)//�S�[������
        {
            is_action = true;
            //�A�j���[�V����
            animator.SetTrigger("Goal");

            soundManager.Play(soundManager.player_goal);//SE
            StartCoroutine( DelayCoroutine(1.0f, ()=>
            { //�I�����Ɏ��s
                Goal();
            }));

            return true;
        }
        else if (id == ObjId.HOLE)//�����鏈��
        {
            is_action = true;
            //�A�j���[�V����
            animator.SetTrigger("Fall");

            StartCoroutine(DelayCoroutine(1.0f, () =>
            { //�I�����Ɏ��s
                Fall();
            }));

            return true;
        }
        else
            return false;
        
    }

    //�ӂ��Ƃ΂��i���S�n�j
    public void StartLeap(Vector2Int _hypocenter, int _power)
    {
        if (is_action) return;       

        leap_distance = _power;
        Debug.Log("power:"+_power+ "hypo:"+_hypocenter);

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
        is_leap = true;

        //Leap�A�j���[�V����
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(target_pos, 0.8f));//�ړ�
        seq.Join(transform.DOScale(Vector3.one * 1.0f, 0.4f).SetLoops(2, LoopType.Yoyo));//�T�C�Y�ύX
        seq.Play().OnComplete(() =>
         {//�I������
             landing();//���n����
             UpdateAttackTarget();
         });
    }

    //���n����
    private void landing()
    {
        Debug.Log("���n");

        if (CheckFloor()) return;//�����`�F�b�N

        Vector2Int p_pos = GetIntPos();
        //���n�n�_��id�擾
        ObjId id = stageManager.GetTileId(p_pos);
        if (id == ObjId.BLOCK)//�u���b�N�Ȃ�J����
        {
            stageManager.OpenBlock(p_pos);
        }
        else if (id == ObjId.MINE)//�n���Ȃ�O��Ɠ��������ɐ������
        {
            is_action = false;
            Vector2Int hypo_pos = p_pos + (GetDirectionVec() * -1);
            StartLeap(hypo_pos, leap_distance);
            stageManager.OpenBlock(p_pos);
            stageManager.CheckExplosion();//�����������`�F�b�N
            return;
        }

        soundManager.Play(soundManager.player_land);
        is_leap = false;
        EndAction();
    }

    //�O���ɍU��
    public bool Attack()
    {
        UpdateAttackTarget();//���݂̍U���ΏۍX�V

        //�U���A�j���[�V����

        //�u���b�N����
        if(stageManager.OpenBlock(attack_target_pos))
        {
            stageManager.CheckExplosion();//�����������`�F�b�N
            UpdateAttackTarget();//���X�V
            if (!is_leap)
                saveData.CreateMemento();

            soundManager.Play(soundManager.player_attack_hit);
            return true;
        }
        else//�󂹂Ȃ������ꍇ
        {
            soundManager.Play(soundManager.player_attack_miss);
            return false;
        }
    }
    public bool AttackPos(Vector2Int _pos)//�f�o�b�O�p
    {
        //�u���b�N����
        if (stageManager.OpenBlock(_pos))
        {
            stageManager.CheckExplosion();//�����������`�F�b�N
            UpdateAttackTarget();//���X�V
            if (!is_leap)
                saveData.CreateMemento();

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
        Debug.Log("FALL!!");
        EndAction();//�A�N�V�����I��
    }

    public void Goal()
    {
        Debug.Log("GOAL!!");
        EndAction();//�A�N�V�����I��
        animator.SetTrigger("Default");

        //�I�����Ɏ��s��������
        stageManager.Clear();
    }

    public void UpdateAttackTarget()
    {
        //�U���Ώۂ̏��擾
        attack_target_pos = GetIntPos() + GetDirectionVec();
    }

    public void SetDirection(Vector2Int _vec)
    {
        if (_vec == Vector2Int.up)
        {
            direction = Direction.UP;
            animator.SetTrigger("Up");
        }
        else if (_vec == Vector2Int.left)
        {
            direction = Direction.LEFT;
            animator.SetTrigger("Left");
        }
        else if (_vec == Vector2Int.down)
        {
            direction = Direction.DOWN;
            animator.SetTrigger("Down");
        }
        else if (_vec == Vector2Int.right)
        {
            direction = Direction.RIGHT;
            animator.SetTrigger("Right");
        }

        UpdateAttackTarget();
    }
    public void SetDirection(Direction _dir)
    {
        direction = _dir;

        //�����ڕύX
        if (direction == Direction.UP)
            animator.SetTrigger("Up");
        else if (direction == Direction.LEFT)
            animator.SetTrigger("Left");
        else if (direction == Direction.DOWN)
            animator.SetTrigger("Down");
        else if (direction == Direction.RIGHT)
            animator.SetTrigger("Right");

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


    public PlayerMemento CreateMemento()
    {
        //Debug.Log("PlayerMemento�̍쐬");
        var memento = new PlayerMemento();
        memento.position = transform.position;
        memento.direction = direction;
        return memento;
    }

    public void SetMemento(PlayerMemento memento)
    {
        //Debug.Log("PlayerMemento�̌Ăяo��" + memento.position);
        transform.position = memento.position;
        SetDirection(memento.direction);
        is_action = false;
        animator.SetTrigger("Default");
        UpdateAttackTarget();
    }
}
