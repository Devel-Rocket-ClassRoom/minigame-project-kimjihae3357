using System.IO;
using UnityEngine;

/// <summary>JSON 파일 단일 슬롯 저장/로드. 씬 전환을 넘어 유지되는 LoadRequested 플래그 보유.</summary>
public static class SaveSystem
{
    /// <summary>Title에서 Continue 누를 때 true. Ingame 씬에서 이 값을 보고 복원 여부 결정.</summary>
    public static bool LoadRequested;

    private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");

    public static bool HasSave() => File.Exists(Path);

    public static void Write(GameSaveData data)
    {
        if (data == null) return;
        data.hasData = true;
        File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        Debug.Log($"[Save] 저장 완료: {Path}");
    }

    public static GameSaveData Read()
    {
        if (!HasSave()) return null;
        try
        {
            return JsonUtility.FromJson<GameSaveData>(File.ReadAllText(Path));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Save] 로드 실패: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (HasSave()) File.Delete(Path);
    }
}
