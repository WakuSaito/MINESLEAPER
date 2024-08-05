using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;

    [SerializeField]//攻撃が当たった
    public AudioClip[] player_attack_hit;
    [SerializeField]//攻撃を外した
    public AudioClip player_attack_miss;
    [SerializeField]//移動
    public AudioClip[] player_move;
    [SerializeField]//着地
    public AudioClip player_land;
    [SerializeField]//クリアしたとき
    public AudioClip player_goal;
    [SerializeField]//ブロック移動
    public AudioClip block_move;
    [SerializeField]//爆発
    public AudioClip block_explosion;
    [SerializeField]//ボタン選択
    public AudioClip ui_button_select;
    [SerializeField]//切り替えアイキャッチ
    public AudioClip ui_change;
    [SerializeField]//アンドゥ
    public AudioClip stage_undo;


    private float sound_volume = 1.0f;//音のボリューム

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    //SE再生
    public void Play(AudioClip _ac, float _volume = 1.0f)
    {
        audioSource.PlayOneShot(_ac, _volume);
    }
    //ランダム再生
    public void Play(AudioClip[] _ac, float _volume = 1.0f)
    {
        audioSource.PlayOneShot(_ac[Random.Range(0, _ac.Length)], _volume);
    }

    public void SetVolume(float _vol)
    {
        sound_volume = _vol;
        audioSource.volume = _vol;
    }
}
