using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Block
{
    [SerializeField]//�����G�t�F�N�g
    GameObject explosive_effect;

    public override void Broken()
    {           
        Explosion();
        //�폜
        Destroy(gameObject);
    }

    public void Explosion()
    {
        //�����G�t�F�N�g�쐬
        Instantiate(explosive_effect, gameObject.transform.position, Quaternion.identity);

    }
}
