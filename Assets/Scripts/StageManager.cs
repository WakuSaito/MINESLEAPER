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
    StageData stage = new StageData();      //�X�e�[�W�̃u���b�N�f�[�^
    StageData stage_flag = new StageData(); //�X�e�[�W�̊��f�[�^

    CreateStage createStage;//�X�e�[�W�쐬�X�N���v�g
    PlayerMove playerMove;  //�v���C���[�X�N���v�g

    const int BLOCK_EMPTY = 0;//���ID
    const int BLOCK_MINE = 9; //�n����ID

    [SerializeField, Header("���e��u��������(�f�o�b�O�p)")]
    bool on_changemine = true;
    [SerializeField, Header("�󔒃u���b�N���A���ŊJ��(�f�o�b�O�p)")]
    bool on_areaopen = true;

    [SerializeField]//���̃^�C���}�b�v
    Tilemap flag_tilemap;
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
    [SerializeField]//���u���b�N
    TileBase tile_flag;
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
    //����4�����̍��W
    readonly Vector2Int[] surround4_pos =
    {
        new Vector2Int( 0, 1),//��
        new Vector2Int( 1, 0),//�E
        new Vector2Int( 0,-1),
        new Vector2Int(-1, 0),
    };

    private void Awake()
    {
        //�X�N���v�g�擾
        createStage = gameObject.GetComponent<CreateStage>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();

        //�X�e�[�W�̃u���b�N�f�[�^�擾
        stage = createStage.GetAllBlockData();

        //�S�󔒂̐������X�V
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//�󔒕����̂ݐ������o��

            UpdateTileNum(data.Key);//�X�V
        }
        if (on_changemine)//�n���u������
        {           
            foreach (var pos in under_tilemap.cellBounds.allPositionsWithin)
            {
                //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
                if (!under_tilemap.HasTile(pos)) continue;

                //�u���b�N�^�C�����d�˂�悤�ɐݒu
                block_tilemap.SetTile(pos, tile_block);
            }
        }
    }


    // ��莞�Ԍ�ɏ������Ăяo���R���[�`��
    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    //�u���b�N���J����(���W, ���������J���邩)
    public bool OpenBlock(Vector2Int _pos, bool _flag_open=false)
    {
        ObjId block_id = stage.GetData(_pos);

        Debug.Log(nameof(OpenBlock)  + "id:" + block_id + " : "+ _pos);
        //�u���b�N�ƒn���̂ݎ��s
        if (block_id != ObjId.BLOCK && 
            block_id != ObjId.MINE) return false;
        //�����ݒu���Ă����
        //if (!_flag_open && 
        //    stage_flag.GetData(_pos) == ObjId.FLAG) return false;

        DeleteFlag(_pos);//���폜

        stage.SetData(_pos, ObjId.EMPTY);//�󔒂ɕς���
        UpdateTileNum(_pos);//�����̉摜�X�V
      

        //�n���Ȃ�
        if (block_id == ObjId.MINE)
        {
            Explosion(_pos);//��������
        }

        block_tilemap.SetTile((Vector3Int)_pos, null);//�u���b�N�̍폜

        //�A�����ĊJ����
        if (block_id == ObjId.BLOCK && GetMineCount(_pos) == 0 && on_areaopen)
        {
            //����4�}�X��T��
            for (int i = 0; i < surround4_pos.Length; i++)
            {
                Vector2Int pos = _pos + surround4_pos[i];
                if (stage.GetData(pos) == ObjId.NULL) continue; //�f�[�^�����邩

                OpenBlock(pos, true);//�����ƊJ����
            }
        }
        return true;
    }


    //����
    public void Explosion(Vector2Int _pos)
    {
        //����8�}�X��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //���͂̍��W
            Vector2Int pos = _pos + surround_pos[i];
            //�v���C���[�̍��W
            Vector2Int p_pos = playerMove.GetIntPos();
            //���͂Ƀv���C���[�������ꍇ
            if(pos == p_pos)
            {
                int num = GetMineCount(p_pos)+1;//�����̐������擾
                playerMove.StartLeap(_pos, num);//���̕��ӂ��Ƃ΂�
            }

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

    //���ݒu�؂�ւ�
    public void SwitchFlag(Vector2Int _pos)
    {
        if (stage.GetData(_pos) != ObjId.BLOCK &&
            stage.GetData(_pos) != ObjId.MINE) return;

        //���ݒu�Ȃ�ݒu
        if (stage_flag.GetData(_pos) != ObjId.FLAG)
        {
            stage_flag.SetData(_pos, ObjId.FLAG);
            flag_tilemap.SetTile((Vector3Int)_pos, tile_flag);
        }
        else//�ݒu�ςȂ�폜
        {
            stage_flag.Delete(_pos);
            flag_tilemap.SetTile((Vector3Int)_pos, null);
        }
    }
    //���폜
    public void DeleteFlag(Vector2Int _pos)
    {
        if (stage_flag.GetData(_pos) != ObjId.FLAG) return;

        stage_flag.Delete(_pos);
        flag_tilemap.SetTile((Vector3Int)_pos, null);
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
    public int GetMineCount(Vector2Int _pos)
    {
        int mine_count = 0;

        //����8�}�X��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (stage.GetData(pos) != ObjId.MINE) continue;//�n���̂݌v�Z

            mine_count++;
        }

        return mine_count;
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
        if(next_id != ObjId.EMPTY && next_id != ObjId.HOLE)
        {
            //�ړ���̃u���b�N������
            if (!PushBlock(next_pos, _vec)) return false;
        }

        //�ړ��悪��
        if (next_id == ObjId.HOLE)
        {
            DeleteFlag(_pos);
            stage.SetData(_pos, ObjId.EMPTY);
        }
        else if (stage_flag.GetData(_pos) == ObjId.FLAG)//���̈ړ�
        {
            SwitchFlag(_pos);//���̏��ύX

            stage.Move(_pos, next_pos);

            SwitchFlag(next_pos);//���̏��ύX
        }
        else
        {
            stage.Move(_pos, next_pos);
        }

        //���͕ύX���Ȃ��@�^�C���̕ύX�͊֐����쐬���Ă���������
        if (next_id != ObjId.HOLE)
        {
            //�ړ���摜�ύX
            if (!on_changemine && id == ObjId.MINE)
                block_tilemap.SetTile((Vector3Int)next_pos, tile_mine);
            else
                block_tilemap.SetTile((Vector3Int)next_pos, tile_block);
        }
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

    //�X�e�[�W�f�[�^�L�^ �ύX�_�̂ݕۑ����Ă���������
    public StageMemento CreateMemento()
    {
        var memento = new StageMemento();
        memento.stageData = new StageData();
        memento.stageData.data = new Dictionary<Vector2Int, ObjId>(stage.data);

        return memento;
    }

    //�X�e�[�W�f�[�^�̃��[�h
    public void SetMemento(StageMemento memento)
    {
        stage = memento.stageData;

        //�^�C���z�u������
        foreach(var data in stage.data)
        {
            switch (data.Value)
            {
                case ObjId.EMPTY:
                    block_tilemap.SetTile((Vector3Int)data.Key, null);
                    under_tilemap.SetTile((Vector3Int)data.Key, null);
                    break;
                case ObjId.BLOCK:
                    block_tilemap.SetTile((Vector3Int)data.Key, tile_block);
                    under_tilemap.SetTile((Vector3Int)data.Key, null);
                    break;
                case ObjId.MINE:
                    if(!on_changemine)
                        block_tilemap.SetTile((Vector3Int)data.Key, null);
                    else
                        block_tilemap.SetTile((Vector3Int)data.Key, tile_block);
                    under_tilemap.SetTile((Vector3Int)data.Key, tile_mine);
                    break;
            }
        }
        foreach(var data in stage_flag.data)
        {
            if(data.Value == ObjId.FLAG)
            {
                flag_tilemap.SetTile((Vector3Int)data.Key, tile_flag);
            }
        }

        //�S�󔒂̐������X�V
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//�󔒕����̂ݐ������o��

            UpdateTileNum(data.Key);//�X�V
        }
    }
}
