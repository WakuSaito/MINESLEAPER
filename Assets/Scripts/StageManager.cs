using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BlockId
{
    EMPTY=0,
    NUM1,
    NUM2,
    NUM3,
    NUM4,
    NUM5,
    NUM6,
    NUM7,
    NUM8,
    MINE,
}


public class StageManager : MonoBehaviour
{
    StageData stage = new StageData();

    const int BLOCK_EMPTY = 0;//���ID
    const int BLOCK_MINE = 9; //�n����ID

    [SerializeField, Header("���e��u��������(�f�o�b�O�p)")]
    bool on_changemine = true;
    [SerializeField, Header("�󔒃u���b�N���A���ŊJ��(�f�o�b�O�p)")]
    bool on_areaopen = true;

    [SerializeField]//�u���b�N�̃^�C���}�b�v
    Tilemap block_tilemap;
    [SerializeField]//�u���b�N���̃^�C���}�b�v�i�n���A�����j
    Tilemap under_tilemap;
    [SerializeField]//�ǂ̃^�C���}�b�v
    Tilemap wall_tilemap;
    [SerializeField]//�n�ʂ̃^�C���}�b�v
    Tilemap ground_tilemap;

    //�u���b�N��TileBase
    [SerializeField]//�u���b�N
    TileBase tile_block;
    [SerializeField]//���e
    TileBase tile_mine;
    [SerializeField]//����
    TileBase[] tile_num = new TileBase[9];

    [SerializeField]//�U���̑҂�����
    float chain_explo_delay = 0.3f;


    //���͂̍��W
    readonly Vector2Int[] surround_pos = 
    {
        new Vector2Int( 0, 1),//��
        new Vector2Int( 1, 1),//�E��
        new Vector2Int( 1, 0),//�E
        new Vector2Int( 1,-1),
        new Vector2Int( 0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1)
    };


    private void Start()
    {
        //�X�e�[�W�̃u���b�N�f�[�^�擾
        GetBlockData();

        //�S�󔒂̐������X�V
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//�󔒕����̂ݐ������o��

            UpdateTileNum(data.Key);//�X�V
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_pos = GetMousePos();
            //�������0�ȉ��ł���肪�N���Ȃ��悤�ɂ���
            if (mouse_pos.x < 0) mouse_pos.x -= 1.0f;
            if (mouse_pos.y < 0) mouse_pos.y -= 1.0f;
            //int�ɕϊ�
            Vector2Int int_mouse_pos = new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y);
      
            OpenBlock(int_mouse_pos);//�}�E�X�ʒu�̃u���b�N���J��
            
        }
    }

    // ��莞�Ԍ�ɏ������Ăяo���R���[�`��
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    //�u���b�N���J����
    public void OpenBlock(Vector2Int _pos)
    {
        ObjId block_id = stage.GetData(_pos);

        Debug.Log(nameof(OpenBlock)  + "id:" + block_id + " : "+ _pos);
        //�u���b�N�ƒn���̂ݎ��s
        if (block_id != ObjId.BLOCK && 
            block_id != ObjId.MINE) return;

        stage.SetData(_pos, ObjId.EMPTY);//�󔒂ɕς���
        UpdateTileNum(_pos);//�����̉摜�X�V
      
        Debug.Log("�u���b�N��ID" + block_id);

        //�n���Ȃ�
        if (block_id == ObjId.MINE)
        {
            Explosion(_pos);//��������
        }

        block_tilemap.SetTile((Vector3Int)_pos, null);//�u���b�N�̍폜

        //�A�����ĊJ����
        if (block_id == ObjId.BLOCK && GetMineCount(_pos) == 0 && on_areaopen)
        {
            //����8�}�X��T��
            for (int i = 0; i < surround_pos.Length; i++)
            {
                Vector2Int pos = _pos + surround_pos[i];
                if (stage.GetData(pos) == ObjId.NULL) continue; //�f�[�^�����邩

                OpenBlock(pos);
            }
        }
    }


    //����
    public void Explosion(Vector2Int _pos)
    {
        //�S�I�u�W�F�N�g�ɔ����̏���n��
        //GameObject[] all_obj = GameObject.FindGameObjectsWithTag("Object");
        //foreach(GameObject obj in all_obj)
        //{
        //    obj.GetComponent<ObjectManager>().HitExplosion(_pos);
        //}

        //����8�}�X��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //���͂̍��W
            Vector2Int pos = _pos + surround_pos[i];

            ObjId id = stage.GetData(pos);

            //�U���i�����e���|�x�点��悤�ɂ���j
            if(id == ObjId.MINE)
            {
                block_tilemap.SetTile((Vector3Int)pos, null);//�u���b�N�̍폜 �ʒu��ύX����\���A��

                // �R���[�`���̋N��
                StartCoroutine(DelayCoroutine(chain_explo_delay, () =>
                {
                    Debug.Log("�U��");
                    // 3�b��ɂ����̏��������s�����
                    OpenBlock(pos);//��������
                }));
                continue;
            }
            //�u���b�N�͊J��
            else if (id == ObjId.BLOCK)
            {
                OpenBlock(pos);
            }
            //�󔒂Ȃ琔���X�V
            else if(id == ObjId.EMPTY)
            {
                UpdateTileNum(pos);
            }
        }
    }

    //�^�C���̐����̌����ڏ��X�V
    private void UpdateTileNum(Vector2Int _pos)
    {       
        if (stage.GetData(_pos) != ObjId.EMPTY) return;//�󔒂̂ݍX�V

        int num = GetMineCount(_pos);

        TileBase tile = tile_num[num];//�ύX����^�C��

        //�^�C���̒u������
        under_tilemap.SetTile((Vector3Int)_pos, tile);
    }

    //����8�}�X�̒n���̐��𒲂ׂ�֐�(���W)
    private int GetMineCount(Vector2Int _pos)
    {
        int mine_count = 0;

        //����8�}�X��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (stage.GetData(pos) != ObjId.MINE) continue;//�n���̂݌v�Z

            mine_count++;
        }

        Debug.Log("���͂̒n���̐�:" + mine_count);
        return mine_count;
    }

    //�S�^�C���̃u���b�N�����擾
    private void GetBlockData()
    {
        //�u���b�N�̏��擾
        foreach (var pos in block_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!block_tilemap.HasTile(pos)) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.BLOCK);
        }
        //�n���̈ʒu���擾
        foreach (var pos in under_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!under_tilemap.HasTile(pos)) continue;

            if (on_changemine)
                //�u���b�N�^�C�����d�˂�悤�ɐݒu
                block_tilemap.SetTile(pos, tile_block);

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.MINE);
        }
        //�ǂ̏��擾
        foreach (var pos in wall_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!wall_tilemap.HasTile(pos)) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.WALL);
        }
        //�ǂ̏��擾
        foreach (var pos in ground_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!ground_tilemap.HasTile(pos)) continue;
            //���łɑ��̃f�[�^������Ȃ珈�����Ȃ�
            if (stage.GetData((Vector2Int)pos) != ObjId.NULL) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.EMPTY);
        }
    }

    //�}�E�X�̍��W�擾
    public Vector3 GetMousePos()
    {
        Vector3 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.z = 10.0f;

        return pos;
    }

    public ObjId GetTileId(Vector2Int _pos)
    {
        return stage.GetData(_pos);
    }

    //�u���b�N�������֐�
    public bool PushBlock(Vector2Int _pos, Vector2Int _vec)
    {
        ObjId id = GetTileId(_pos);
        //�u���b�N�A�n���̂݉�����
        if (id != ObjId.BLOCK && id != ObjId.MINE) return false;
       
        //�ړ�����W
        Vector2Int next_pos = _pos + _vec;
        //�ړ����id���擾
        ObjId next_id = GetTileId(next_pos);
        
        //�ړ��悪�󔒈ȊO
        if(next_id != ObjId.EMPTY)
        {
            //�ړ���̃u���b�N������
            if (!PushBlock(next_pos, _vec)) return false;
        }
      
        stage.Move(_pos, next_pos);

        //�ړ���摜�ύX
        block_tilemap.SetTile((Vector3Int)next_pos, tile_block);
        //�����W�摜�ύX
        block_tilemap.SetTile((Vector3Int)_pos, null);


        //�����̍X�V
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //���͂̍��W
            Vector2Int pos = _pos + surround_pos[i];

            if (stage.GetData(pos) == ObjId.EMPTY)
                UpdateTileNum(pos);
        }
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //���͂̍��W
            Vector2Int pos = next_pos + surround_pos[i];

            if (stage.GetData(pos) == ObjId.EMPTY)
                UpdateTileNum(pos);
        }

        Debug.Log("����");

        return true;
    }


}
