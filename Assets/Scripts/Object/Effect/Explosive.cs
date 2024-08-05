using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
	private float anim_length;
	private float time_count;

	// Use this for initialization
	void Start()
	{
		Animator animOne = GetComponent<Animator>();
		//���݂̃A�j���[�V�������擾
		AnimatorStateInfo infAnim = animOne.GetCurrentAnimatorStateInfo(0);
		//�A�j���[�V�����̒����擾
		anim_length = infAnim.length;
		time_count = 0;
	}

	// Update is called once per frame
	void Update()
	{
		time_count += Time.deltaTime;//�J�E���g
		//�A�j���[�V�������I������Ƃ��ɃI�u�W�F�N�g�폜
		if (time_count > anim_length)
		{
			Destroy(gameObject);
		}
	}
}
