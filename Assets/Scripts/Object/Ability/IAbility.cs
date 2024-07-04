using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//特性のインターフェース
public interface IAbility
{
    //爆風を受けたとき
    public void Hit(GameObject _gameobject);

    //アップデート内容
    public void Action(GameObject _gameobject);
}

