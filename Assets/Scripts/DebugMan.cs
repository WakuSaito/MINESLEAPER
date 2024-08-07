using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMan : MonoBehaviour
{
    StageManager stageManager;
    SoundManager soundManager;

    [SerializeField, Header("爆弾を見えるようにする(デバッグ用)")]
    public bool on_visiblemine = false;
    [SerializeField, Header("空白ブロックが連続で開く(デバッグ用)")]
    public bool on_areaopen = false;
    [SerializeField, Header("どこでも開けられる(デバッグ用)")]
    public bool on_open_anywhere = false;
    [SerializeField, Header("全ステージ解放(デバッグ用)")]
    public bool on_allstage_open = false;

    private void Awake()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

    }
    private void Update()
    {
        //ステージ全解放
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.G))
        {
            stageManager.deepest_stage = 100;
            soundManager.Play(soundManager.stage_fullopen);
        }
        if(on_allstage_open)
        {
            stageManager.deepest_stage = 100;
            on_allstage_open = false;
            soundManager.Play(soundManager.stage_fullopen);
        }

        //if(Input.GetKeyDown(KeyCode.N))
        //{
        //    stageManager.ChangeNextStage();
        //}
    }
}
