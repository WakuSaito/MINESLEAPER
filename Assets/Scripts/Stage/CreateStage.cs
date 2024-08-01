using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class CreateStage : MonoBehaviour
{
    GameObject player;//�v���C���[�I�u�W�F�N�g

    //�O���b�h�i�^�C���}�b�v�̐e�j
    Grid grid;
    //�u���b�N�̃^�C���}�b�v
    Tilemap tilemap_block;
    //�u���b�N���̃^�C���}�b�v�i�n���A�����j
    Tilemap tilemap_under;
    //�ǂ̃^�C���}�b�v
    Tilemap tilemap_wall;
    //�n�ʂ̃^�C���}�b�v
    Tilemap tilemap_ground;

    //�^�C��
    [SerializeField]//��
    TileBase tile_floor;
    [SerializeField]//��
    TileBase tile_wall;
    [SerializeField]//�S�[��
    TileBase tile_goal;
    [SerializeField]//�u���b�N �폜�\��
    TileBase tile_block;
    [SerializeField]//���e�@�폜�\��
    TileBase tile_mine;
    [SerializeField]//���u���b�N
    TileBase tile_flag;

    //�u���b�N��TileBase
    [SerializeField]//�u���b�N
    GameObject obj_block;
    [SerializeField]//���e
    GameObject obj_mine;

    //�u���b�N�̐e�I�u�W�F�N�g
    [SerializeField]
    GameObject block_parent;

    const int MAP_WIDTH = 32;
    const int MAP_HEIGHT = 18;
    const int MAP_MIN_X = -16;
    const int MAP_MAX_Y = 8;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        tilemap_block = GameObject.Find("BlockTilemap").GetComponent<Tilemap>();
        tilemap_under = GameObject.Find("UnderBlockTilemap").GetComponent<Tilemap>();
        tilemap_wall  = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        tilemap_ground =GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
    }

    //�t�@�C������X�e�[�W�����擾
    public StageData GetStageFileData(int _num)
    {
        //��������t�@�C�����擾
        string file_name = "StageData_" + _num.ToString();
        TextAsset csvFile = Resources.Load(file_name) as TextAsset; // CSV�t�@�C��
        if (!csvFile)
        {
            Debug.Log("�t�@�C����������܂���");
            return null;
        }

        List<string[]> csvData = new List<string[]>(); // CSV�t�@�C���̒��g�����郊�X�g
        StringReader reader = new StringReader(csvFile.text); // TextAsset��StringReader�ɕϊ�
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine(); // 1�s���ǂݍ���
            csvData.Add(line.Split(',')); // csvData���X�g�ɒǉ����� 
        }

        StageData stage = new StageData();

        
        //x�̍ő�l�擾
        int max_x = 0;
        for (int row = 0; row < csvData.Count; row++)
        {
            if (max_x < csvData[row].Length) max_x = csvData[row].Length;
        }
        //�J�n�n�_�����߂�i�}�b�v�������ɗ���悤�ɂ���j
        Vector2Int firstpos = new Vector2Int();
        firstpos.x = (MAP_WIDTH - max_x) / 2;
        firstpos.y = (MAP_HEIGHT - csvData.Count) / 2;

        //�s���ƂɃf�[�^�擾
        for(int row = 0; row < MAP_HEIGHT; row++)
        {
            if (row >= csvData.Count) break;//�f�[�^���Ȃ���΂Ƃ΂�

            int y = MAP_MAX_Y - row - firstpos.y;//y�擾

            for(int column = 0; column < MAP_WIDTH; column++)
            {
                if (column >= csvData[row].Length) break;//�f�[�^���Ȃ���΂Ƃ΂�

                int x = MAP_MIN_X + column + firstpos.x;//x�擾
                
                int num = int.Parse(csvData[row][column]);//csv�̐��l�擾
                ObjId id;//id
                switch (num)//csv�̃f�[�^��id�ɕϊ�
                {
                    case 0:
                        id = ObjId.EMPTY; break;
                    case 1:
                        id = ObjId.WALL; break;
                    case 2:
                        id = ObjId.GOAL; break;
                    case 3:
                        id = ObjId.BLOCK; break;
                    case 4:
                        id = ObjId.MINE; break;
                    case 9:
                        id = ObjId.EMPTY;
                        player.transform.position = new Vector2(x + 0.5f, y + 0.5f);
                        break;
                    default:
                        id = ObjId.HOLE; break;
                }
                stage.SetData(new Vector2Int(x, y), id);
            }
        }

        return stage;
    }


    //�u���b�N�z�u(�z�u����f�[�^, �z�u�O�ɑS�폜���邩)
    public void SetAllBlockData(StageData _stage, bool _delete_all = true)
    {
        if(_delete_all)//�S�폜
            DeleteAllBlock();

        //�e�I�u�W�F�N�g��������Ȃ���΍쐬
        if (!block_parent)
            block_parent = new GameObject();

        foreach (var data in _stage.data)
        {
            if (data.Value == ObjId.NULL) continue;

            Vector3Int posint = (Vector3Int)data.Key;
            Vector3 pos = (Vector2)data.Key;

            switch(data.Value)
            {
                case ObjId.EMPTY:
                    tilemap_ground.SetTile(posint, tile_floor);
                    break;
                case ObjId.WALL:
                    tilemap_wall.SetTile(posint, tile_wall);
                    break;
                case ObjId.GOAL:
                    tilemap_ground.SetTile(posint, tile_goal);
                    break;
                case ObjId.BLOCK:
                    tilemap_ground.SetTile(posint, tile_floor);
                    tilemap_block.SetTile(posint, tile_block);
                    //Instantiate(tile_block, pos, Quaternion.identity, block_parent.transform);
                    break;
                case ObjId.MINE:
                    tilemap_ground.SetTile(posint, tile_floor);
                    tilemap_under.SetTile(posint, tile_mine);
                    //Instantiate(tile_mine, pos, Quaternion.identity, mine_parent.transform);
                    break;
                case ObjId.HOLE:
                    break;
            }
        }
    }

    //�u���b�N�S�폜
    public void DeleteAllBlock()
    {
        //�I�u�W�F�N�g�폜
        if (block_parent)
                Destroy(block_parent);   

        //�^�C���폜
        if (grid == null) return;
        var tilemaps = grid.GetComponentsInChildren<Tilemap>();
        foreach(var tilemap in tilemaps)
        {
            if (tilemap == null) continue;

            tilemap.ClearAllTiles();
        }
    }

    //�S�^�C���̃u���b�N�����擾
    public StageData GetAllBlockData()
    {
        StageData stage = new StageData();

        //�u���b�N�̏��擾
        foreach (var pos in tilemap_block.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!tilemap_block.HasTile(pos)) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.BLOCK);
        }
        //�n���̈ʒu���擾
        foreach (var pos in tilemap_under.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!tilemap_under.HasTile(pos)) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.MINE);
        }
        //�ǂ̏��擾
        foreach (var pos in tilemap_wall.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!tilemap_wall.HasTile(pos)) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.WALL);
        }
        //�n�ʂ̏��擾
        foreach (var pos in tilemap_ground.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!tilemap_ground.HasTile(pos)) continue;
            //���łɑ��̃f�[�^������Ȃ珈�����Ȃ�
            if (stage.GetData((Vector2Int)pos) != ObjId.NULL) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.EMPTY);
        }

        return stage;
    }
}
