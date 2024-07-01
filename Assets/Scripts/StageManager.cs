using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//public struct TileData
//{
//    public int id;
//    public bool is_open;
//}

public class StageManager : MonoBehaviour
{
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
    //�u���b�N��TileBase
    [SerializeField]//�u���b�N
    TileBase tile_block;
    [SerializeField]//���e
    TileBase tile_mine;
    [SerializeField]//����
    TileBase[] tile_num = new TileBase[9];

    [SerializeField]//�U���̑҂�����
    float chain_explo_delay = 0.3f;

    //�}�b�v�f�[�^
    Dictionary<Vector2Int, int> map_data = new Dictionary<Vector2Int, int>();

    //���͂̍��W
    Vector2Int[] surround_pos = new Vector2Int[8]
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
        GetBlockData();

        SetMineCount();
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
        Debug.Log(nameof(OpenBlock)  + "id:" + map_data[_pos] + " : "+ _pos);
        //���̍��W�Ƀu���b�N�f�[�^��������Ύ��s���Ȃ�
        if (!map_data.ContainsKey(_pos)) return;
        //�����Ȃ��}�X�Ȃ���s���Ȃ�
        if (block_tilemap.GetTile((Vector3Int)_pos) == null &&
            under_tilemap.GetTile((Vector3Int)_pos) == null) return;
        

        int block_id = map_data[_pos];

        UpdateTileNum(_pos);//�����̉摜�X�V
      
        Debug.Log("�u���b�N��ID" + block_id);

        //�n���Ȃ�
        if (block_id == BLOCK_MINE)
        {
            map_data[_pos] = SearchMine(_pos);//���̈ʒu��id���󔒂ɕύX
            UpdateTileNum(_pos);//�摜�X�V

            Explosion(_pos);//��������
        }

        block_tilemap.SetTile((Vector3Int)_pos, null);//�u���b�N�̍폜

        if (block_id == BLOCK_EMPTY && on_areaopen)
        {
            //����8�}�X��T��
            for (int i = 0; i < surround_pos.Length; i++)
            {
                Vector2Int pos = _pos + surround_pos[i];
                if (!map_data.ContainsKey(pos)) continue; //�f�[�^�����邩

                OpenBlock(pos);
            }
        }
    }


    //����
    public void Explosion(Vector2Int _pos)
    {
        //�S�I�u�W�F�N�g�ɔ����̏���n��
        GameObject[] all_obj = GameObject.FindGameObjectsWithTag("Object");
        foreach(GameObject obj in all_obj)
        {
            obj.GetComponent<ObjectManager>().HitExplosion(_pos);
        }

        //����8�}�X��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            //�}�b�v��񖳂��Ȃ牽�����Ȃ�
            if (!map_data.ContainsKey(pos)) continue;

            //�U���i�����e���|�x�点��悤�ɂ���j
            if(map_data[pos] == BLOCK_MINE)
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
            //�n���Ƌ󔒈ȊO�Ȃ�J�E���g�����炷
            else if (map_data[pos] != BLOCK_EMPTY)
            {
                map_data[pos]--;

                OpenBlock(pos);
            }
        }
    }

    //�^�C���̐����̌����ڏ��X�V
    public void UpdateTileNum(Vector2Int _pos)
    {       
        int block_id = map_data[_pos];
        if (block_id >= BLOCK_MINE) return;//�n���Ȃ�X�V���Ȃ�

        TileBase tile = null;//�ύX����^�C��

        //0�ȊO�͐������o��
        if (block_id > BLOCK_EMPTY)
        {
            tile = tile_num[block_id];//�����^�C��
        }

        //�^�C���̒u������
        under_tilemap.SetTile((Vector3Int)_pos, tile);
    }

    //�S�^�C���̃u���b�N�����擾
    public void GetBlockData()
    {
        //�u���b�N�^�C���}�b�v�̏��擾
        foreach (var pos in block_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!block_tilemap.HasTile(pos)) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            map_data.Add((Vector2Int)pos, BLOCK_EMPTY);
        }

        //�n���̈ʒu���擾
        foreach (var pos in under_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!under_tilemap.HasTile(pos)) continue;

            Debug.Log("�ݒu");
            if (on_changemine)
                //�u���b�N�^�C�����d�˂�悤�ɐݒu
                block_tilemap.SetTile(pos, tile_block);

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            map_data.Add((Vector2Int)pos, BLOCK_MINE);
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

    //����8�}�X�̒n���̐��𒲂ׂ�֐�(���W)
    public int SearchMine(Vector2Int _pos)
    {
        int mine_count = 0;

        //����8�}�X��T��
        for (int i = 0; i < surround_pos.Length; i++)
        {
            Vector2Int pos = _pos + surround_pos[i];
            if (!map_data.ContainsKey(pos)) continue; //�f�[�^�����邩
            if (map_data[pos] != BLOCK_MINE) continue;//�n���̂݌v�Z
        
            mine_count++;
        }

        return mine_count;
    }

    //�n���̎���8�}�X�̃J�E���g��ύX
    public void SetMineCount()
    {
        Debug.Log("�T�C�Y : " + map_data.Count);
        Dictionary<Vector2Int, int> mine_count = new Dictionary<Vector2Int, int>();

        //�n����T��
        foreach (KeyValuePair<Vector2Int, int> data in map_data)
        {
            if (data.Value == BLOCK_MINE) continue;//�n���}�X�ɂ͐��l��ݒ肵�Ȃ�

            int id = SearchMine(data.Key);//���͂̒n���̐��𒲂ׂ�
            mine_count.Add(data.Key, id);

        }
        //�}�b�v�̒l��ύX�i����map_data��ς���ƃG���[���N����j
        foreach (KeyValuePair<Vector2Int, int> data in mine_count)
        {
            map_data[data.Key] += data.Value;
        }
    }

    
}
