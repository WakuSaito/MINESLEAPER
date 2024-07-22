using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMemento
{
    public StageData stageData;
}

public class PlayerMemento
{
    public Vector3 position;
}

public class Memento
{
    public StageMemento stage;
    public PlayerMemento player;
}


public class SaveData : MonoBehaviour
{
    //保存データ
    List<Memento> memento_data = new List<Memento>();

    StageManager stageManager;
    PlayerMove playerMove;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
    }

    public void CreateMemento()
    {
        Debug.Log("Mementoの作成");
        Memento memento = new Memento();
        memento.stage = stageManager.CreateMemento();
        memento.player = playerMove.CreateMemento();

        memento_data.Add(memento);

    }

    public void SetMemento(int _num)
    {
        int data_size = memento_data.Count;
        if (_num >= data_size || 
            _num < 0) return;

        Debug.Log("Mementoの呼び出し:" + _num);

        playerMove.SetMemento(memento_data[_num].player);
        stageManager.SetMemento(memento_data[_num].stage);

        //不要なデータの削除
        int next = _num + 1;
        if(next < data_size)
        {
            Debug.Log("Mementoデータ削除:" + next + ":" + (data_size - next));
            memento_data.RemoveRange(next, data_size - next);//次のデータから末尾まで削除
        }

    }

    //memento_dataの終端を取得
    public int GetMementoEnd()
    {
        return memento_data.Count - 1;
    }
}
