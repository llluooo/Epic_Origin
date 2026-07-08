// SceneBGMPlayer.cs
// 挂到场景任意 GameObject 上，Start 时自动播放指定的 BGM。
// 在 Inspector 里选 BGM 类型即可，无需写代码。

using UnityEngine;

public class SceneBGMPlayer : MonoBehaviour
{
    [SerializeField] private BGM bgm = BGM.MainMenu;
    [SerializeField] private float delay = 0f;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning($"[SceneBGMPlayer] 场景中没有 AudioManager，无法播放 BGM: {bgm}");
            return;
        }
        if (delay > 0f)
            Invoke(nameof(PlayBgm), delay);
        else
            PlayBgm();
    }

    private void PlayBgm()
    {
        AudioManager.Instance.PlayBGM(bgm);
    }
}
