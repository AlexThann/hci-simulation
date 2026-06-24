using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public void ExportLogsToTXT()
    {
        string path = Application.dataPath + "/simulation_log.txt";

        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (LogEntry log in logs)
            {
                writer.WriteLine(
                    $"[{log.timestamp}] {log.eventType}: {log.details}"
                );
            }
        }

        Debug.Log("Logs exported to: " + path);
    }
}