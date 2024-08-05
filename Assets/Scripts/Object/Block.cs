using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : ObjBase
{
    SoundManager soundManager;

    private void Awake()
    {
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
    public override void MoveSound()
    {
        soundManager.Play(soundManager.block_move);
    }

    public override void Broken()
    {
        //íœ
        Destroy(gameObject);
    }

    public override void Fall()
    {
        soundManager.Play(soundManager.block_fall);
        //íœ
        Destroy(gameObject);
    }

   
}
