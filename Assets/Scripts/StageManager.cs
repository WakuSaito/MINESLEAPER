using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class StageManager : MonoBehaviour
{
    [SerializeField]//�X�e�[�W�̃^�C���}�b�v
    Tilemap stage_tilemap;
    //�u���b�N��TileBase
    [SerializeField]//block
    TileBase tile_block;
    [SerializeField]//���e
    TileBase tile_mine;
    [SerializeField]//���e
    Sprite sprite_mine;

    Dictionary<Vector2Int, int> map_data = new Dictionary<Vector2Int, int>();


    private void Start()
    {
        GetBlockData();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_pos = GetMousePos();
            //�������0�ȉ��ł���肪�N���Ȃ�
            if (mouse_pos.x < 0) mouse_pos.x -= 1.0f;
            if (mouse_pos.y < 0) mouse_pos.y -= 1.0f;
            Vector2Int pos = new Vector2Int((int)mouse_pos.x, (int)mouse_pos.y);
            Debug.Log("�}�E�X���Wf:" + mouse_pos);
            Debug.Log("�}�E�X���Wi:" + pos);
            if (map_data.ContainsKey(pos))
                stage_tilemap.SetTile((Vector3Int)pos, null);
        }
    }

    //�^�C���̃u���b�N�����擾
    public void GetBlockData()
    {
        int max_x = stage_tilemap.cellBounds.xMax;
        int max_y = stage_tilemap.cellBounds.yMax;

        //�^�C���}�b�v�̏��擾
        foreach(var pos in stage_tilemap.cellBounds.allPositionsWithin)
        {
            //���̈ʒu�Ƀ^�C����������Ώ������Ȃ�
            if (!stage_tilemap.HasTile(pos)) continue;

            int obj_id;

            if (stage_tilemap.GetTile(pos) == tile_mine)
            {
                Debug.Log("�ݒu");
                stage_tilemap.SetTile(pos, tile_block);
                obj_id = 2;
            }
            else
            {
                obj_id = 1;
            }

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
}
