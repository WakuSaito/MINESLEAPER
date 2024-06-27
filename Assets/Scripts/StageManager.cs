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

    [SerializeField]//�X�e�[�W�̃^�C���}�b�v(����n���A�����A�u���b�N�𓯂��^�C���}�b�v�ŊǗ����Ă��邪�A�����ɂ��ڐG���Ă��܂����ߗv�ύX)
    Tilemap stage_tilemap;
    //�u���b�N��TileBase
    [SerializeField]//�u���b�N
    TileBase tile_block;
    [SerializeField]//���e
    TileBase tile_mine;
    [SerializeField]//����
    TileBase[] tile_num = new TileBase[9];

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
        if (Input.GetMouseButton(0))
        {
            Vector3 mouse_pos = GetMousePos();
            //�������0�ȉ��ł���肪�N���Ȃ��悤�ɂ���
            if (mouse_pos.x < 0) mouse_pos.x -= 1.0f;
            if (mouse_pos.y < 0) mouse_pos.y -= 1.0f;
            //int�ɕϊ�
            Vector2Int int_mouse_pos = new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y);

            //���̍��W�Ƀu���b�N�f�[�^�������
            if (map_data.ContainsKey(int_mouse_pos))
            {
                OpenBlock(int_mouse_pos);
            }
        }
    }

    //�u���b�N���J����
    public void OpenBlock(Vector2Int _pos)
    {
        //���łɊJ���Ă���Ȃ���s���Ȃ�
        if (stage_tilemap.GetTile((Vector3Int)_pos) == null) return;

        int block_id = map_data[_pos];
        TileBase tile = null;
        Debug.Log("�u���b�N��ID" + block_id);

        //�n���Ȃ�
        if (block_id == BLOCK_MINE)
        {
            Explosion(_pos);
        }
        //�ʏ�̃u���b�N�Ȃ�
        else
        {
            //0�ȊO�͐������o��
            if (block_id > BLOCK_EMPTY)
                tile = tile_num[block_id];
        }

        stage_tilemap.SetTile((Vector3Int)_pos, tile);//�^�C���̒u������
    }

    //����
    public void Explosion(Vector2Int _pos)
    {
        map_data[_pos] = BLOCK_EMPTY;//���̈ʒu��id���󔒂ɕύX

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
            if (!map_data.ContainsKey(pos)) continue;

            //�n���ȊO�Ȃ�J�E���g�����炷
            if (map_data[pos] != BLOCK_MINE)
                map_data[pos]--;


            OpenBlock(pos);
        }
    }

    //�S�^�C���̃u���b�N�����擾
    public void GetBlockData()
    {
        //�^�C���}�b�v�̏��擾
        foreach(var pos in stage_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!stage_tilemap.HasTile(pos)) continue;

            int obj_id;

            if (stage_tilemap.GetTile(pos) == tile_mine)
            {
                Debug.Log("�ݒu");
                if(on_changemine)
                    //�n���̌����ڂ�ʏ�̃u���b�N�ɕύX
                    stage_tilemap.SetTile(pos, tile_block);

                obj_id = BLOCK_MINE;
            }
            else
            {
                obj_id = BLOCK_EMPTY;
            }

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            map_data.Add( (Vector2Int)pos ,obj_id);

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

    //�n���̎���8�}�X�̃J�E���g��ύX
    public void SetMineCount()
    {
        Debug.Log("�T�C�Y : " + map_data.Count);
        Dictionary<Vector2Int, int> mine_count = new Dictionary<Vector2Int, int>();

        //�n����T��
        foreach (KeyValuePair<Vector2Int, int> data in map_data)
        {
            if (data.Value != BLOCK_MINE) continue;

            //����8�}�X��T��
            for(int i=0; i<surround_pos.Length; i++)
            {
                Vector2Int pos = data.Key + surround_pos[i];
                if (!map_data.ContainsKey(pos)) continue;
                if (map_data[pos] == BLOCK_MINE) continue;//�n���͌v�Z���Ȃ�

                //�J�E���g�ǉ�
                if (mine_count.ContainsKey(pos))
                    mine_count[pos]++;
                else
                    mine_count.Add(pos, 1);
                
            }
        }
        //�}�b�v�̒l��ύX�i����map_data��ς���ƃG���[���N����j
        foreach (KeyValuePair<Vector2Int, int> data in mine_count)
        {
            map_data[data.Key] += data.Value;
        }
    }
}
