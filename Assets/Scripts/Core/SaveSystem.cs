using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public const int MaxSaveSlots = 3;
    private const string NextSlotKey = "SaveSystem_NextSlot";

    public static string GetSaveFilePath(int slot)
    {
        slot = Mathf.Clamp(slot, 1, MaxSaveSlots);
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    /// <summary>
    /// 获取下一个要保存的槽位（循环）
    /// </summary>
    public static int GetNextSaveSlot()
    {
        int nextSlot = PlayerPrefs.GetInt(NextSlotKey, 1);
        if (nextSlot < 1 || nextSlot > MaxSaveSlots)
        {
            nextSlot = 1;
        }

        return nextSlot;
    }

    /// <summary>
    /// 更新下一个槽位计数器
    /// </summary>
    private static void AdvanceNextSlot()
    {
        int nextSlot = GetNextSaveSlot();
        nextSlot++;
        if (nextSlot > MaxSaveSlots)
        {
            nextSlot = 1;
        }

        PlayerPrefs.SetInt(NextSlotKey, nextSlot);
        PlayerPrefs.Save();
    }

    public static bool SaveGame(GameRunState runState, int slot)
    {
        if (runState == null)
        {
            Debug.LogError("保存失败：无效的游戏状态。");
            return false;
        }

        if (slot < 1 || slot > MaxSaveSlots)
        {
            Debug.LogError($"保存失败：存档槽编号无效 ({slot})。");
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(runState, true);
            string path = GetSaveFilePath(slot);
            File.WriteAllText(path, json);
            Debug.Log($"游戏已保存到存档槽 {slot}: {path}");
            AdvanceNextSlot();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"保存失败：{ex.Message}");
            return false;
        }
    }

    public static GameRunState LoadGame(int slot)
    {
        if (slot < 1 || slot > MaxSaveSlots)
        {
            Debug.LogError($"加载失败：存档槽编号无效 ({slot})。");
            return null;
        }

        string path = GetSaveFilePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"载入失败：未找到存档文件 {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            GameRunState runState = JsonUtility.FromJson<GameRunState>(json);
            if (runState == null)
            {
                Debug.LogError("载入失败：存档内容解析错误。");
                return null;
            }

            Debug.Log($"已从存档槽 {slot} 读取游戏状态。");
            return runState;
        }
        catch (Exception ex)
        {
            Debug.LogError($"载入失败：{ex.Message}");
            return null;
        }
    }

    public static bool HasSave(int slot)
    {
        if (slot < 1 || slot > MaxSaveSlots) return false;
        return File.Exists(GetSaveFilePath(slot));
    }

    public static bool HasAnySave()
    {
        for (int i = 1; i <= MaxSaveSlots; i++)
        {
            if (HasSave(i)) return true;
        }

        return false;
    }

    public static SaveSlotInfo GetSaveSlotInfo(int slot)
    {
        SaveSlotInfo info = new SaveSlotInfo
        {
            slot = slot,
            hasSave = false,
            displayText = "空",
            lastModified = DateTime.MinValue
        };

        if (!HasSave(slot))
        {
            return info;
        }

        string path = GetSaveFilePath(slot);
        try
        {
            string json = File.ReadAllText(path);
            GameRunState runState = JsonUtility.FromJson<GameRunState>(json);
            info.hasSave = runState != null;
            info.lastModified = File.GetLastWriteTime(path);
            if (runState != null)
            {
                info.displayText = $"回合 {runState.currentTurn}  玩家: {runState.player?.playerName ?? "未知"}  英雄: ({runState.heroGridPos.x},{runState.heroGridPos.y})";
            }
            else
            {
                info.displayText = "存档损坏";
            }
        }
        catch (Exception ex)
        {
            info.displayText = $"读取失败: {ex.Message}";
        }

        return info;
    }
}

[Serializable]
public class SaveSlotInfo
{
    public int slot;
    public bool hasSave;
    public string displayText;
    public DateTime lastModified;
}
