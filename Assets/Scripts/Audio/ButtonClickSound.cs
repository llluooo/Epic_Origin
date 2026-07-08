// ButtonClickSound.cs
// 把这个组件挂到任意 UI Button 上，点击时自动播放通用点击音。
// 这样组员做 UI 时不用写任何音效代码 —— 直接拖一个组件就行。

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log($"[ButtonClickSound] 按钮 '{gameObject.name}' 被点击");
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[ButtonClickSound] 场景中没有 AudioManager！请在首个场景创建空物体并挂载 AudioManager 脚本。");
                return;
            }
            AudioManager.Instance.PlaySFX(SFX.ButtonClick);
        });
    }
}
