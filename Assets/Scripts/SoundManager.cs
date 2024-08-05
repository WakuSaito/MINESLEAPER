using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;

    [SerializeField]//�U������������
    public AudioClip[] player_attack_hit;
    [SerializeField]//�U�����O����
    public AudioClip player_attack_miss;
    [SerializeField]//�ړ�
    public AudioClip[] player_move;
    [SerializeField]//���n
    public AudioClip player_land;
    [SerializeField]//�N���A�����Ƃ�
    public AudioClip player_goal;
    [SerializeField]//�u���b�N�ړ�
    public AudioClip block_move;
    [SerializeField]//����
    public AudioClip block_explosion;
    [SerializeField]//�{�^���I��
    public AudioClip ui_button_select;
    [SerializeField]//�؂�ւ��A�C�L���b�`
    public AudioClip ui_change;
    [SerializeField]//�A���h�D
    public AudioClip stage_undo;


    private float sound_volume = 1.0f;//���̃{�����[��

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    //SE�Đ�
    public void Play(AudioClip _ac, float _volume = 1.0f)
    {
        audioSource.PlayOneShot(_ac, _volume);
    }
    //�����_���Đ�
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
