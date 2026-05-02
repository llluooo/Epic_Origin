//using UnityEngine;

//public class PanelSwitcher : MonoBehaviour
//{
//    // 切换面板：关掉当前按钮所在的面板，打开目标面板
//    public void SwitchTo(GameObject targetPanel)
//    {
//        if (targetPanel != null)
//        {
//            transform.parent.gameObject.SetActive(false); // 隐藏当前的面板
//            targetPanel.SetActive(true);                  // 显示目标面板
//        }
//    }

//    // 退出游戏
//    public void QuitGame()
//    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//    }
//}









//using UnityEngine;

//public class PanelSwitcher : MonoBehaviour
//{
//    // 切换面板：关掉当前按钮所在的面板，打开目标面板
//    public void SwitchTo(GameObject targetPanel)
//    {
//        if (targetPanel == null) return;

//        // 向上查找一直到 Canvas 的直接子物体（Panel）
//        Transform current = transform;
//        while (current.parent != null && current.parent.GetComponent<Canvas>() == null)
//        {
//            current = current.parent;
//        }

//        current.gameObject.SetActive(false); // 隐藏当前所在的 Panel
//        targetPanel.SetActive(true);         // 显示目标面板
//    }

//    public void QuitGame()
//    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit();
//#endif
//    }
//}









using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    // ======================================================
    // 1. 基础功能：纯 UI 面板切换
    // 适用于：主菜单跳转、设置页面、返回按钮等不涉及游戏逻辑的操作
    // ======================================================
    public void SwitchTo(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        // 自动查找当前按钮所在的顶级面板（即 Canvas 下的第一层子物体）
        Transform current = transform;
        while (current.parent != null && current.parent.GetComponent<Canvas>() == null)
        {
            current = current.parent;
        }

        // 隐藏当前面板，显示目标面板
        current.gameObject.SetActive(false);
        targetPanel.SetActive(true);
    }

    // ======================================================
    // 2. 逻辑功能：确认种族并正式启动游戏
    // 适用于：你未来在 RaceSelectPanel 中创建的那个“进入游戏”按钮
    // ======================================================
    public void StartGameAndCloseUI(GameObject targetPanel)
    {
        // 先执行 UI 切换逻辑
        SwitchTo(targetPanel);

        // 触发组长的 GameManager 单例逻辑
        if (GameManager.Instance != null)
        {
            // 注意：请确保组长已将 StartGame() 方法改为 public
            GameManager.Instance.StartGame();
            Debug.Log("UI 流程结束，已通知 GameManager 开启第一回合。");
        }
        else
        {
            Debug.LogWarning("场景中未找到 GameManager 物体，请确认是否已将 Canvas 搬至组长的场景中。");
        }
    }

    // ======================================================
    // 3. 通用功能：退出游戏
    // 适用于：MainMenuPanel 里的 ExitButton
    // ======================================================
    public void QuitGame()
    {
        Debug.Log("准备退出游戏...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}