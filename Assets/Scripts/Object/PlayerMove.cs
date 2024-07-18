using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField, Header("�u���b�N��������(�f�o�b�O�p)")]
    bool on_canpush = true;

    [SerializeField]
    StageManager stageManager;

    [SerializeField]//�����ł̈ړ�����
    float move_length = 3.0f;
    [SerializeField]//�������󂯂��鋗��
    float can_hit_distance = 1.5f;

    [SerializeField]//�������Ă������
    string[] have_ability;

    bool isMoving; // �ړ�������
    // Player����ړ����x���擾�ł���悤�ɐݒ�
    [SerializeField] float moveSpeed;

    // �O���œ��͓��e��ێ�����p�̕ϐ�
    Vector2 input;

    private void Awake()
    {
    }

    
    void Update()
    {
        

        // �ړ������Ɠ��͂��󂯕t���Ȃ�
        if (!isMoving)
        {
            input = Vector2.zero;

            // �L�[�{�[�h�̓��͏���input�Ɋi�[
            if (Input.GetKeyDown(KeyCode.D))           
                input.x = 1.0f;
            else if (Input.GetKeyDown(KeyCode.A))
                input.x = -1.0f;
            else if (Input.GetKeyDown(KeyCode.W))
                input.y = 1.0f;
            else if (Input.GetKeyDown(KeyCode.S))
                input.y = -1.0f;

            // ���͂���������
            if (input != Vector2.zero)
            {
                StartCoroutine( Move( GetIntPos(input) ) );
            }
        }
    }

    //�@�R���[�`�����g���ď��X�ɖړI�n�ɋ߂Â���
    IEnumerator Move(Vector2Int _vec)
    {
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
                    yield break;
            }
            else
                yield break;//�ړ����Ȃ�
        }


        // �ړ����͓��͂��󂯕t���Ȃ�
        isMoving = true;

        // targetpos�Ƃ̍�������Ȃ�J��Ԃ�:�ړI�n�ɒH�蒅���܂ŌJ��Ԃ�
        while ((targetPos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPos�ɋ߂Â���
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            // ���X�ɋ߂Â��邽��
            yield return null;
        }

        // �ړ�����������������ړI�n�ɓ���������
        transform.position = targetPos;
        isMoving = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            Debug.Log("�S�[��");
        }
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
}
