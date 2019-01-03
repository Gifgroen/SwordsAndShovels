using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class StatsManager 
{
    public static SessionKeeper sessionKeeper = new SessionKeeper();
    public static string SaveFilePath { get; set; }

    public static void SaveSessions()
    {
        string json = JsonUtility.ToJson(sessionKeeper);
        File.WriteAllText(SaveFilePath, json);
    }

    public static void LoadSessions()
    {
        if(File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            sessionKeeper = JsonUtility.FromJson<SessionKeeper>(json);
        }
    }
}
