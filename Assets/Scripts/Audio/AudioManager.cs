// AudioManager.cs
// 全局音频管理器（单例 + 跨场景常驻）。
// 用法：把这个脚本挂到第一个场景里的一个空物体上，
//       并在 Inspector 里把上面创建好的 AudioLibrary 资产拖到 library 字段。
//
// 组员只需调用：
//   AudioManager.Instance.PlayBGM(BGM.Map);
//   AudioManager.Instance.PlaySFX(SFX.Attack);

using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频库（把 AudioLibrary 资产拖进来）")]
    [SerializeField] private AudioLibrary library;

    [Header("音量 0~1")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("BGM 切换淡入淡出时长（秒）")]
    [SerializeField] private float bgmFadeTime = 0.5f;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private BGM _currentBgm = BGM.None;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        if (library != null) library.Init();
        else Debug.LogError("[AudioManager] 没有设置 AudioLibrary！");
    }

    private void Start()
    {
        // 诊断自检：Start 阶段输出音频系统状态，帮助排查"没声音"的问题
        var listener = FindObjectOfType<AudioListener>();
        if (listener == null)
            Debug.LogError("[AudioManager] 🔇 场景中没有 AudioListener！请确认 Main Camera 上有 Audio Listener 组件。");
        else
            Debug.Log($"[AudioManager] ✅ AudioListener 在 {listener.name} 上");

        Debug.Log($"[AudioManager] master={masterVolume:F2} bgm={bgmVolume:F2} sfx={sfxVolume:F2} " +
                  $"clips={library?.ClipCount ?? 0} library={(library != null ? "已赋值" : "未赋值")}");
        Debug.Log("[AudioManager] 💡 如果以上都正常，检查 Unity Game 视图右上角是否点了 Mute 静音按钮。");
    }

    // ================= BGM =================

    public void PlayBGM(BGM key)
    {
        if (library == null)
        {
            Debug.LogError($"[AudioManager] 播放 BGM {key} 失败：AudioLibrary 未赋值！");
            return;
        }
        // 同一首正在放就不重启 —— 主菜单/种族/存档共用 BGM1 时音乐连续不断
        if (key == _currentBgm) return;

        AudioClip clip = library.GetBgm(key);
        if (clip == null) { Debug.LogWarning($"[Audio] 缺少 BGM：{key}"); return; }

        _currentBgm = key;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(SwitchBgm(clip));
    }

    public void StopBGM()
    {
        _currentBgm = BGM.None;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator SwitchBgm(AudioClip next)
    {
        if (_bgmSource.isPlaying)
            yield return Fade(_bgmSource, _bgmSource.volume, 0f, bgmFadeTime);

        _bgmSource.clip = next;
        _bgmSource.Play();
        yield return Fade(_bgmSource, 0f, bgmVolume * masterVolume, bgmFadeTime);
    }

    private IEnumerator FadeOutAndStop()
    {
        if (_bgmSource.isPlaying)
            yield return Fade(_bgmSource, _bgmSource.volume, 0f, bgmFadeTime);
        _bgmSource.Stop();
    }

    private IEnumerator Fade(AudioSource src, float from, float to, float t)
    {
        float e = 0f;
        while (e < t)
        {
            e += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(from, to, e / t);
            yield return null;
        }
        src.volume = to;
    }

    public float GetBgmVolume() => bgmVolume;
    public float GetSfxVolume() => sfxVolume;
    public float GetMasterVolume() => masterVolume;

    public void SetMasterVolume(float v) => masterVolume = Mathf.Clamp01(v);
    public void SetBgmVolume(float v) => bgmVolume = Mathf.Clamp01(v);
    public void SetSfxVolume(float v) => sfxVolume = Mathf.Clamp01(v);

    // ================= 音效 =================

    private SFX _lastSfx;
    private float _lastSfxTime;

    public void PlaySFX(SFX key)
    {
        if (library == null)
        {
            Debug.LogError($"[AudioManager] 播放音效 {key} 失败：AudioLibrary 未赋值！");
            return;
        }
        AudioClip clip = library.GetSfx(key);
        if (clip == null) { Debug.LogWarning($"[Audio] 缺少音效：{key}"); return; }

        float vol = sfxVolume * masterVolume;
        Debug.Log($"[AudioManager] ▶ PlaySFX: {key} clip={clip.name} length={clip.length:F2}s vol={vol:F2} source.enabled={_sfxSource.enabled}");
        // PlayOneShot 允许多个音效同时叠加播放
        _sfxSource.PlayOneShot(clip, vol);
        _lastSfx = key;
        _lastSfxTime = Time.unscaledTime;
    }

    private void Update()
    {
        ApplyVolumes();

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[AudioManager] ⌨️ 按下了 T 键，播放测试音效 Move...");
            PlaySFX(SFX.Move);
        }
    }

    private void ApplyVolumes()
    {
        if (_bgmSource != null)
            _bgmSource.volume = bgmVolume * masterVolume;
    }
}
