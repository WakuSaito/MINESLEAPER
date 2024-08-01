using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMan : MonoBehaviour
{
    [SerializeField, Header("爆弾を見えるようにする(デバッグ用)")]
    public bool on_visiblemine = false;
    [SerializeField, Header("空白ブロックが連続で開く(デバッグ用)")]
    public bool on_areaopen = false;
    [SerializeField, Header("どこでも開けられる(デバッグ用)")]
    public bool on_open_anywhere = false;
}
