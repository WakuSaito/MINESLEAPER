using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : ObjBase
{
    public override void Broken()
    {
        Destroy(gameObject);
    }

    public override void Fall()
    {
       
    }
}
