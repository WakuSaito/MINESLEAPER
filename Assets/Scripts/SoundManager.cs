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
    [SerializeField]//menuを開く
    public AudioClip ui_open;
    [SerializeField]//menuを閉じる
    public AudioClip ui_close;
    [SerializeField]//アンドゥ
    public AudioClip stage_undo;


    private float base_sound_volume;//ベースとなる音量

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        base_sound_volume = audioSource.volume;
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
    //音量を変える
    public void SetVolume(float _vol)
    {        
        audioSource.volume = base_sound_volume * _vol;
    }
}
