using System;
using System.IO;
using UnityEngine;

public class SaveManager2 : MonoBehaviour
{
    public static SaveManager2 Instance { get; private set; }

    public int currentLevel { get; private set; }
    public int highScore { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Load();
    }

    [Serializable]
    public class PlayerData
    {
        public int currentLevel;
        public int highScore;
    }

    public void UpdateProgress(int level, int score)
    {
        currentLevel = level;
        if (score > highScore)
        {
            highScore = score;
        }
        Save();
    }

    public void Save()
    {
        string path = Application.persistentDataPath + "/playerInfo.json";
        PlayerData data = new PlayerData
        {
            currentLevel = currentLevel,
            highScore = highScore
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved Level: {currentLevel}, Score: {highScore}");
    }

    public void Load()
    {
        string path = Application.persistentDataPath + "/playerInfo.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            currentLevel = data.currentLevel;
            highScore = data.highScore;
        }
        else
        {
            Debug.LogWarning("Save file not found, starting fresh.");
            currentLevel = 1;
            highScore = 0;
        }
    }

    public void ResetProgress()
    {
        currentLevel = 1;
        highScore = 0;
        Save();
    }
}