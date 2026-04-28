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









using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    // 切换面板：关掉当前按钮所在的面板，打开目标面板
    public void SwitchTo(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        // 向上查找一直到 Canvas 的直接子物体（Panel）
        Transform current = transform;
        while (current.parent != null && current.parent.GetComponent<Canvas>() == null)
        {
            current = current.parent;
        }

        current.gameObject.SetActive(false); // 隐藏当前所在的 Panel
        targetPanel.SetActive(true);         // 显示目标面板
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}