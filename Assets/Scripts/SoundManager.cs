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
    [SerializeField]//menu���J��
    public AudioClip ui_open;
    [SerializeField]//menu�����
    public AudioClip ui_close;
    [SerializeField]//�A���h�D
    public AudioClip stage_undo;


    private float base_sound_volume;//�x�[�X�ƂȂ鉹��

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        base_sound_volume = audioSource.volume;
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
    //���ʂ�ς���
    public void SetVolume(float _vol)
    {        
        audioSource.volume = base_sound_volume * _vol;
    }
}
