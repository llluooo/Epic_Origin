using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public const int MaxManualSaveSlots = 3;
    public const int MaxSaveSlots = MaxManualSaveSlots;
    private const string NextSlotKey = "SaveSystem_NextSlot";
    private const string AutoSaveFileName = "autosave.json";

    public static string SaveDirectoryOverride { get; set; }

    private static string SaveDirectory => string.IsNullOrEmpty(SaveDirectoryOverride)
        ? Application.persistentDataPath
        : SaveDirectoryOverride;

    public static string GetAutoSaveFilePath()
    {
        return Path.Combine(SaveDirectory, AutoSaveFileName);
    }

    public static string GetManualSaveFilePath(int slot)
    {
        slot = Mathf.Clamp(slot, 1, MaxManualSaveSlots);
        return Path.Combine(SaveDirectory, $"save_slot_{slot}.json");
    }

    public static string GetSaveFilePath(int slot)
    {
        return GetManualSaveFilePath(slot);
    }

    /// <summary>
    /// 获取下一个要保存的槽位（循环）
    /// </summary>
    public static int GetNextSaveSlot()
    {
        int nextSlot = PlayerPrefs.GetInt(NextSlotKey, 1);
        if (nextSlot < 1 || nextSlot > MaxManualSaveSlots)
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
        if (nextSlot > MaxManualSaveSlots)
        {
            nextSlot = 1;
        }

        PlayerPrefs.SetInt(NextSlotKey, nextSlot);
        PlayerPrefs.Save();
    }

    public static bool SaveGame(GameRunState runState, int slot)
    {
        return SaveManualGame(runState, slot);
    }

    public static bool SaveAutoGame(GameRunState runState)
    {
        return SaveGameToPath(runState, GetAutoSaveFilePath(), "自动档");
    }

    public static bool SaveManualGame(GameRunState runState, int slot)
    {
        if (runState == null)
        {
            Debug.LogError("保存失败：无效的游戏状态。");
            return false;
        }

        if (slot < 1 || slot > MaxManualSaveSlots)
        {
            Debug.LogError($"保存失败：存档槽编号无效 ({slot})。");
            return false;
        }

        bool saved = SaveGameToPath(runState, GetManualSaveFilePath(slot), $"存档槽 {slot}");
        if (saved)
        {
            AdvanceNextSlot();
        }

        return saved;
    }

    private static bool SaveGameToPath(GameRunState runState, string path, string label)
    {
        if (runState == null)
        {
            Debug.LogError("保存失败：无效的游戏状态。");
            return false;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = JsonUtility.ToJson(runState, true);
            File.WriteAllText(path, json);
            Debug.Log($"游戏已保存到{label}: {path}");
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
        return LoadManualGame(slot);
    }

    public static GameRunState LoadAutoGame()
    {
        return LoadGameFromPath(GetAutoSaveFilePath(), "自动档");
    }

    public static GameRunState LoadManualGame(int slot)
    {
        if (slot < 1 || slot > MaxManualSaveSlots)
        {
            Debug.LogError($"加载失败：存档槽编号无效 ({slot})。");
            return null;
        }

        return LoadGameFromPath(GetManualSaveFilePath(slot), $"存档槽 {slot}");
    }

    private static GameRunState LoadGameFromPath(string path, string label)
    {
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

            Debug.Log($"已从{label}读取游戏状态。");
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
        return HasManualSave(slot);
    }

    public static bool HasAutoSave()
    {
        return File.Exists(GetAutoSaveFilePath());
    }

    public static bool HasManualSave(int slot)
    {
        if (slot < 1 || slot > MaxManualSaveSlots) return false;
        return File.Exists(GetManualSaveFilePath(slot));
    }

    public static bool HasAnySave()
    {
        if (HasAutoSave())
        {
            return true;
        }

        for (int i = 1; i <= MaxManualSaveSlots; i++)
        {
            if (HasManualSave(i)) return true;
        }

        return false;
    }

    public static SaveSlotInfo GetSaveSlotInfo(int slot)
    {
        return GetManualSaveSlotInfo(slot);
    }

    public static SaveSlotInfo GetAutoSaveSlotInfo()
    {
        return GetSaveSlotInfoFromPath(0, GetAutoSaveFilePath(), "自动档");
    }

    public static SaveSlotInfo GetManualSaveSlotInfo(int slot)
    {
        return GetSaveSlotInfoFromPath(slot, GetManualSaveFilePath(slot), $"槽{slot}");
    }

    private static SaveSlotInfo GetSaveSlotInfoFromPath(int slot, string path, string emptyLabel)
    {
        SaveSlotInfo info = new SaveSlotInfo
        {
            slot = slot,
            hasSave = false,
            displayText = $"{emptyLabel}: 空",
            lastModified = DateTime.MinValue
        };

        if (!File.Exists(path))
        {
            return info;
        }

        try
        {
            string json = File.ReadAllText(path);
            GameRunState runState = JsonUtility.FromJson<GameRunState>(json);
            info.hasSave = runState != null;
            info.lastModified = File.GetLastWriteTime(path);
            if (runState != null)
            {
                info.displayText = $"{emptyLabel}: 回合 {runState.currentTurn}  玩家: {runState.player?.playerName ?? "未知"}  英雄: ({runState.heroGridPos.x},{runState.heroGridPos.y})";
            }
            else
            {
                info.displayText = $"{emptyLabel}: 存档损坏";
            }
        }
        catch (Exception ex)
        {
            info.displayText = $"{emptyLabel}: 读取失败: {ex.Message}";
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
