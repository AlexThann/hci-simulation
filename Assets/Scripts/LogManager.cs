using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LogEntry
{
    public string timestamp;
    public string eventType;
    public string details;
}

public class LogManager : MonoBehaviour
{
    public List<LogEntry> logs = new List<LogEntry>();

    public void Log(string eventType, string details)
    {
        logs.Add(new LogEntry
        {
            timestamp = DateTime.Now.ToString("HH:mm:ss"),
            eventType = eventType,
            details = details
        });

        Debug.Log($"[{eventType}] {details}");
    }
}