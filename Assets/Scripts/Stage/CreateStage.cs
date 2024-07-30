using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class CreateStage : MonoBehaviour
{
    //�u���b�N�̃^�C���}�b�v
    Tilemap block_tilemap;
    //�u���b�N���̃^�C���}�b�v�i�n���A�����j
    Tilemap under_tilemap;
    //�ǂ̃^�C���}�b�v
    Tilemap wall_tilemap;
    //�n�ʂ̃^�C���}�b�v
    Tilemap ground_tilemap;

    //�u���b�N��TileBase
    [SerializeField]//�u���b�N
    GameObject tile_block;
    [SerializeField]//���e
    GameObject tile_mine;

    //�u���b�N�̐e�I�u�W�F�N�g
    [SerializeField]
    GameObject block_parent;
    [SerializeField]
    GameObject mine_parent;

    const int MAP_WIDTH = 32;
    const int MAP_HEIGHT = 18;
    const int MAP_MIN_X = -16;
    const int MAP_MAX_Y = 8;

    // Start is called before the first frame update
    void Awake()
    {
        block_tilemap = GameObject.Find("BlockTilemap").GetComponent<Tilemap>();
        under_tilemap = GameObject.Find("UnderBlockTilemap").GetComponent<Tilemap>();
        wall_tilemap  = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        ground_tilemap =GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
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

        //�s���ƂɃf�[�^�擾
        for(int row = 0; row < csvData.Count; row++)
        {
            int y = MAP_MAX_Y - row;//y�擾
            for(int column = 0; column < MAP_WIDTH; column++)
            {
                if (column >= csvData[row].Length) break;//�f�[�^���Ȃ���΂Ƃ΂�

                int x = MAP_MIN_X + column;//x�擾
                
                int num = int.Parse(csvData[row][column]);//csv�̐��l�擾
                ObjId id;//id
                switch (num)//csv�̃f�[�^��id�ɕϊ�
                {
                    case 1:
                        id = ObjId.WALL; break;
                    case 2:
                        id = ObjId.GOAL; break;
                    case 3:
                        id = ObjId.BLOCK; break;
                    case 4:
                        id = ObjId.MINE; break;
                    default:
                        id = ObjId.HOLE; break;
                }
                stage.SetData(new Vector2Int(x, y), id);
            }
        }

        return stage;
    }

    //�S�^�C���̃u���b�N�����擾
    public StageData GetAllBlockData()
    {
        StageData stage = new StageData();

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
        //�n�ʂ̏��擾
        foreach (var pos in ground_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!ground_tilemap.HasTile(pos)) continue;
            //���łɑ��̃f�[�^������Ȃ珈�����Ȃ�
            if (stage.GetData((Vector2Int)pos) != ObjId.NULL) continue;

            //�ʒu���ƃI�u�W�F�N�g����ۑ�
            stage.SetData((Vector2Int)pos, ObjId.EMPTY);
        }

        return stage;
    }

    public void SetAllBlockData(StageData _stage)
    {
        foreach(var data in _stage.data)
        {
            if (data.Value != ObjId.BLOCK && data.Value != ObjId.MINE) continue;

            Vector3 pos = (Vector2)data.Key;

            switch(data.Value)
            {
                case ObjId.BLOCK:
                    Instantiate(tile_block, pos, Quaternion.identity, block_parent.transform);
                    break;
                case ObjId.MINE:
                    Instantiate(tile_mine, pos, Quaternion.identity, mine_parent.transform);
                    break;
            }
        }
    }
}
