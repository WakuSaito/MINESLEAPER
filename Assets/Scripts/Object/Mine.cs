using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : ObjBase
{
    public override void Broken()
    {
        Destroy(gameObject);
    }

    public override void Fall()
    {

    }
}
