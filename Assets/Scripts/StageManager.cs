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

    List<GameObject> blocks = new List<GameObject>();

    CreateStage createStage;//�X�e�[�W�쐬�X�N���v�g
    PlayerMove playerMove;  //�v���C���[�X�N���v�g
    SaveData saveData;      //�Z�[�u�f�[�^�X�N���v�g
    SoundManager soundManager;//�T�E���h�X�N���v�g

    DebugMan debugMan;//�f�o�b�O�p�t���O

    //�u���b�N�̐e�I�u�W�F�N�g
    [SerializeField]
    GameObject block_parent;

    //�^�C���}�b�v
    [SerializeField]//���̃^�C���}�b�v
    Tilemap flag_tilemap;
    [SerializeField]//�u���b�N���̃^�C���}�b�v�i�n���A�����j
    Tilemap under_tilemap;
    [SerializeField]//�n�ʂ̃^�C���}�b�v
    Tilemap ground_tilemap;

    //�u���b�N��TileBase
    [SerializeField]//���u���b�N
    TileBase tile_flag;
    [SerializeField]//����
    TileBase[] tile_num = new TileBase[9];
    [SerializeField]//��
    TileBase tile_floor;

    [SerializeField]//�U���̑҂�����
    float chain_explo_delay = 0.3f;

    int current_stage = 1;//���݂̃X�e�[�W
    public int deepest_stage = 1;//���ǂ蒅�������Ƃ̂���ŉ��̃X�e�[�W

    bool is_explosion = false;//����������


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
        saveData = GameObject.Find("SaveData").GetComponent<SaveData>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        debugMan = GameObject.Find("DebugMan").GetComponent<DebugMan>();
    }

    private void Start()
    {
        ChangeStage(1);//�X�e�[�W1���Ăяo��
    }

    //�X�e�[�W�ύX
    public void ChangeNextStage()
    {
            ChangeStage(current_stage+1);
    }
    public void ChangeStage(int _num)
    {
        current_stage = _num;//���݂̃X�e�[�W�̍X�V
        if (deepest_stage < current_stage) 
            deepest_stage = current_stage;//�ŉ��̍X�V
        stage = createStage.GetStageFileData(current_stage);
        //�f�[�^�����݂��Ȃ���Ζ������p��
        if (stage == null)
        {
            stage = createStage.GetStageFileData(0);
            current_stage = 0;
        }
        //�X�e�[�W�z�u
        createStage.SetAllBlockData(stage);

        playerMove.SetDirection(Vector2Int.down);//�v���C���[�̌����ύX
        playerMove.UpdateAttackTarget();

        FindAllBlock();

        //�S�󔒂̐������X�V
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//�󔒕����̂ݐ������o��

            UpdateTileNum(data.Key);//�X�V
        }
        
        saveData.ResetMemento();//���Z�b�g
        saveData.CreateMemento();//�f�[�^�ۑ�
    }

    //�N���A����
    public void Clear()
    {
        ChangeNextStage();
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

        //block_tilemap.SetTile((Vector3Int)_pos, null);//�u���b�N�̍폜
        GetBlock(_pos).Broken();//�u���b�N�폜

        //�A�����ĊJ����
        if (debugMan.on_areaopen && block_id == ObjId.BLOCK && GetMineCount(_pos) == 0)
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
        is_explosion = true;

        //����8�}�X�̃v���C���[��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //���͂̍��W
            Vector2Int pos = _pos + surround_pos[i];
            //�v���C���[�̍��W
            Vector2Int p_pos = playerMove.GetIntPos();
            Debug.Log("player���W:" + p_pos);
            //���͂Ƀv���C���[�������ꍇ
            if (pos == p_pos)
            {
                int num = GetMineCount(p_pos) + 1;//�����̐������擾
                playerMove.StartLeap(_pos, num);//���̕��ӂ��Ƃ΂�
            }
        }
        //����8�}�X�̃u���b�N��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            //���͂̍��W
            Vector2Int pos = _pos + surround_pos[i];
            ObjId id = stage.GetData(pos);

            //�u���b�N���J����
            if (id == ObjId.MINE || id == ObjId.BLOCK)
            {
                OpenBlock(pos);//�J����
            }
            //�󔒂Ȃ琔���X�V
            else if (id == ObjId.EMPTY)
            {
                UpdateTileNum(pos);
            }
        }

    }
    //���������������������ׂ�
    public bool CheckExplosion()
    {
        if (!is_explosion) return false;
        is_explosion = false;
        soundManager.Play(soundManager.block_explosion);//������
        return true;
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

    public Block GetBlock(Vector2Int _pos)
    {
        foreach(var obj in blocks)
        {
            if (obj == null) continue;

            Block block = obj.GetComponent<Block>();
            if (block.GetIntPos() == _pos) 
                return block;
        }
        return null;
    }

    private void FindAllBlock()
    {
        blocks.Clear();
        foreach(Transform obj in block_parent.transform)
            blocks.Add(obj.gameObject);
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

        Block block = GetBlock(_pos);//�u���b�N�擾

        //�ړ��悪��
        if (next_id == ObjId.HOLE)
        {
            DeleteFlag(_pos);
            stage.SetData(_pos, ObjId.EMPTY);

            stage.SetData(next_pos, ObjId.EMPTY);//����n�ʂɕς���

            block.Move(next_pos, () =>
            { //�ړ���ɗ��Ƃ�
                block.Fall();
                ground_tilemap.SetTile((Vector3Int)next_pos, tile_floor);//�^�C����\��
            });//�ړ�
        }
        else if (stage_flag.GetData(_pos) == ObjId.FLAG)//���̈ړ�
        {
            SwitchFlag(_pos);//���̏��ύX

            stage.Move(_pos, next_pos);

            SwitchFlag(next_pos);//���̏��ύX
            block.Move(next_pos);

        }
        else
        {
            //�ړ�
            stage.Move(_pos, next_pos);
            block.Move(next_pos);
        }

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
        memento.flagData = new StageData();
        memento.flagData.data = new Dictionary<Vector2Int, ObjId>(stage_flag.data);

        return memento;
    }

    //�X�e�[�W�f�[�^�̃��[�h
    public void SetMemento(StageMemento memento)
    {
        //�f�[�^�ύX
        stage = memento.stageData;
        stage_flag = memento.flagData;

        //�X�e�[�W�z�u
        createStage.SetAllBlockData(stage);
        createStage.SetAllBlockData(stage_flag, false);//�u�������͍s��Ȃ�

        //�S�󔒂̐������X�V
        foreach (KeyValuePair<Vector2Int, ObjId> data in stage.data)
        {
            if (data.Value != ObjId.EMPTY) continue;//�󔒕����̂ݐ������o��

            UpdateTileNum(data.Key);//�X�V
        }

        FindAllBlock();//�u���b�N�f�[�^�ύX
    }
}
