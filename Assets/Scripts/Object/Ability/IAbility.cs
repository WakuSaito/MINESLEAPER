using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�����̃C���^�[�t�F�[�X
public interface IAbility
{
    //�������󂯂��Ƃ�
    public void Hit(GameObject _gameobject);

    //�A�b�v�f�[�g���e
    public void Action(GameObject _gameobject);
}

