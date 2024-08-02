using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : ObjBase
{
    StageManager stageManager;

    //���������Ă��邩
    public bool on_flag = false;

    //�n���������Ă��邩
    public bool on_mine = false;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    public override void Broken()
    {
        Destroy(gameObject);
    }

    public override void Fall()
    {
       
    }

    protected void Explosion()
    {

    }
}
