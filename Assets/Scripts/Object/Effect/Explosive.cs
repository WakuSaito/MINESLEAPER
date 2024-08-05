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
		//現在のアニメーション情報取得
		AnimatorStateInfo infAnim = animOne.GetCurrentAnimatorStateInfo(0);
		//アニメーションの長さ取得
		anim_length = infAnim.length;
		time_count = 0;
	}

	// Update is called once per frame
	void Update()
	{
		time_count += Time.deltaTime;//カウント
		//アニメーションが終了するときにオブジェクト削除
		if (time_count > anim_length)
		{
			Destroy(gameObject);
		}
	}
}
