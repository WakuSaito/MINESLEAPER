using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����
public enum Direction
{
    UP,
    LEFT,
    DOWN,
    RIGHT,
}

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Header("�u���b�N��������(�f�o�b�O�p)")]
    bool on_canpush = true;

    StageManager stageManager;
    SaveData saveData;

    [SerializeField]//�����ł̈ړ�����
    float move_length = 3.0f;
    [SerializeField]//�������󂯂��鋗��
    float can_hit_distance = 1.5f;

    [SerializeField]//�������Ă������
    string[] have_ability;

    public bool is_moving; // �ړ�������
    // Player����ړ����x���擾�ł���悤�ɐݒ�
    [SerializeField] float moveSpeed;

    //����
    Direction direction = Direction.DOWN;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
    }


    //�@�R���[�`�����g���ď��X�ɖړI�n�ɋ߂Â���
    IEnumerator Move(Vector2 _target_pos)
    {
        // targetpos�Ƃ̍�������Ȃ�J��Ԃ�:�ړI�n�ɒH�蒅���܂ŌJ��Ԃ�
        while ((_target_pos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPos�ɋ߂Â���
            transform.position = Vector2.MoveTowards(transform.position, _target_pos, moveSpeed * Time.deltaTime);
            // ���X�ɋ߂Â��邽��
            yield return null;
        }

        // �ړ�����������������ړI�n�ɓ���������
        transform.position = _target_pos;
        is_moving = false;
        saveData.CreateMemento();//��ԕۑ�
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Goal")
    //    {
    //        Debug.Log("�S�[��");
    //    }
    //}

    public bool StartMove(Vector2Int _vec)
    {
        if (is_moving) return false;//�ړ����Ȃ��~
        if (_vec == Vector2Int.zero) return false;//�ړ��x�N�g�����[��

        //�΂߈ړ��ł��Ȃ��悤�ɂ���@x�D��
        if (_vec.x != 0)
            _vec.y = 0;

        //���������߂�
        SetDirection(_vec);

        //�����W
        Vector2Int pos = GetIntPos();
        //�ړ�����W
        Vector2 targetPos = (Vector2)transform.position + _vec;
        Debug.Log(_vec);

        //�ړ����id���擾
        ObjId id = stageManager.GetTileId(GetIntPos(targetPos));
        Debug.Log("�ړ���id:" + id + ":" + targetPos);


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

        // �ړ����͓��͂��󂯕t���Ȃ�
        is_moving = true;

        StartCoroutine(Move(targetPos));

        return true;
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
