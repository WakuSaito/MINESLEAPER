using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Block
{
    [SerializeField]//爆発エフェクト
    GameObject explosive_effect;

    public override void Broken()
    {           
        Explosion();
        //削除
        Destroy(gameObject);
    }

    public void Explosion()
    {
        //爆発エフェクト作成
        Instantiate(explosive_effect, gameObject.transform.position, Quaternion.identity);

    }
}
