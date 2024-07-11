using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]//�����ł̈ړ�����
    float move_length = 3.0f;
    [SerializeField]//�������󂯂��鋗��
    float can_hit_distance = 1.5f;

    [SerializeField]//�������Ă������
    string[] have_ability;

    bool on_moveing = false;//�ړ����t���O
    Vector2 movetarget_pos;//�ړ��ڕW�n�_

    bool isMoving; // �ړ�������
    // Player����ړ����x���擾�ł���悤�ɐݒ�
    [SerializeField] float moveSpeed;

    // �O���œ��͓��e��ێ�����p�̕ϐ�
    Vector2 input;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        // �ړ������Ɠ��͂��󂯕t���Ȃ�
        if (!isMoving)
        {
            // �L�[�{�[�h�̓��͏���input�Ɋi�[
            input.x = Input.GetAxisRaw("Horizontal"); // ������
            input.y = Input.GetAxisRaw("Vertical");  // �c����

            // ���͂���������
            if (input != Vector2.zero)
            {
                // ���͂�����������ړI�n�ɂ���
                Vector2 targetPos = transform.position;
                targetPos += input;
                StartCoroutine(Move(targetPos));
            }
        }
    }

    //�@�R���[�`�����g���ď��X�ɖړI�n�ɋ߂Â���
    IEnumerator Move(Vector3 targetPos)
    {
        // �ړ����͓��͂��󂯕t���Ȃ�
        isMoving = true;

        // targetpos�Ƃ̍�������Ȃ�J��Ԃ�:�ړI�n�ɒH�蒅���܂ŌJ��Ԃ�
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // targetPos�ɋ߂Â���
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
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
        if (pos.x < 0) pos.x -= 1.0f;
        if (pos.y < 0) pos.y -= 1.0f;
        //int�ɕϊ�
        Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

        return int_pos;
    }
}
