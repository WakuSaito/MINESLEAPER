using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : ObjBase
{
    StageManager stageManager;

    //旗が立っているか
    public bool on_flag = false;

    //地雷が入っているか
    public bool on_mine = false;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    public override void Broken()
    {
        if (on_mine)
            Explosion();

        gameObject.SetActive(false);
    }

    public override void Fall()
    {
       
    }

    protected void Explosion()
    {

    }
}
