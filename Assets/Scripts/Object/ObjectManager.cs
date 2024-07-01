using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]//�����ł̈ړ�����
    float move_length = 3.0f;
    [SerializeField]//�������󂯂��鋗��
    float can_hit_distance = 1.5f;

    bool on_moveing = false;//�ړ����t���O
    Vector2 movetarget_pos;//�ړ��ڕW�n�_

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        //�ړ��t���O�`�F�b�N
        if (!on_moveing) return;
        
        Vector2 vec = movetarget_pos - (Vector2)transform.position;

        Debug.Log((Vector2)transform.position);
        rb.velocity = vec * 1.5f;

        if (vec.magnitude < 0.1f)
        {
            on_moveing = false;

            rb.velocity = Vector2.zero;
        }

    }

    //�����̉e�����󂯂�֐�
    public void HitExplosion(Vector2Int _pos)//���C����������
    {
        Vector2 explo_pos = _pos + new Vector2(0.5f, 0.5f);//���j�n�_�̒��S���W
        Vector2 explo_vec = explo_pos - (Vector2)transform.position;//���j�n�_�܂ł̃x�N�g��

        Debug.Log("���j�x�N�g��:" + explo_vec);

        //���j�n�_���痣��Ă�������s���Ȃ�
        if (explo_vec.magnitude > can_hit_distance) return;

        //�ڕW�n�_�܂ł̃x�N�g���v�Z
        Vector2 tmp_vec = explo_vec.normalized * -1.0f * move_length;

        //�ړ��ڕW�n�_�̍X�V
        movetarget_pos = (Vector2)transform.position + tmp_vec;
        //�ړ��t���O�I��
        on_moveing = true;

        Debug.Log("�ڕW���W:"+movetarget_pos);
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
