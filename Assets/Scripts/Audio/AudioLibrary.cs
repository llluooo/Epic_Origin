// AudioLibrary.cs
// 在 Unity 里：右键 Create → EpicOrigin → Audio Library 生成一个资产，
// 然后在 Inspector 里把每个枚举对应的音频文件拖进去即可。
// 这样以后换音频不用改任何代码。

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "EpicOrigin/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [System.Serializable]
    public struct BgmEntry { public BGM key; public AudioClip clip; }

    [System.Serializable]
    public struct SfxEntry { public SFX key; public AudioClip clip; }

    [Header("背景音乐")]
    public BgmEntry[] bgm;

    [Header("音效")]
    public SfxEntry[] sfx;

    private Dictionary<BGM, AudioClip> _bgmMap;
    private Dictionary<SFX, AudioClip> _sfxMap;

    // 运行时由 AudioManager 调用一次，建好查表字典
    public void Init()
    {
        _bgmMap = new Dictionary<BGM, AudioClip>();
        foreach (var e in bgm)
            if (e.clip != null && !_bgmMap.ContainsKey(e.key)) _bgmMap[e.key] = e.clip;

        _sfxMap = new Dictionary<SFX, AudioClip>();
        foreach (var e in sfx)
            if (e.clip != null && !_sfxMap.ContainsKey(e.key)) _sfxMap[e.key] = e.clip;
    }

    public AudioClip GetBgm(BGM key)
        => (_bgmMap != null && _bgmMap.TryGetValue(key, out var c)) ? c : null;

    public AudioClip GetSfx(SFX key)
        => (_sfxMap != null && _sfxMap.TryGetValue(key, out var c)) ? c : null;

    public int ClipCount
    {
        get
        {
            int count = 0;
            foreach (var e in bgm) if (e.clip != null) count++;
            foreach (var e in sfx) if (e.clip != null) count++;
            return count;
        }
    }
}
