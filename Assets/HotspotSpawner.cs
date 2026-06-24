using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

// --- JSON DATA FILTERS ---
// The parser uses these to ignore everything except the hotspots.
public class ScenarioData
{
    public List<HotspotData> hotspots;
    public InitialState initial_state;
}

public class InitialState
{
    public UiState ui;
}

public class UiState
{
    public List<string> active_hotspots;
}

public class HotspotData
{
    public string id;
    public string label;
}

// --- THE SPAWNER ---
public class HotspotSpawner : MonoBehaviour
{
    void Start()
    {
        // 1. Get the JSON passed over from the Main Menu
        if (GameManager.Instance == null || GameManager.Instance.activeScenarioJson == null)
        {
            Debug.LogError("No JSON payload found. Did you launch from the Main Menu?");
            return;
        }

        string jsonText = GameManager.Instance.activeScenarioJson.text;

        // 2. Parse ONLY the hotspots
        ScenarioData parsedData = JsonConvert.DeserializeObject<ScenarioData>(jsonText);

        if (parsedData != null && parsedData.hotspots != null)
        {
            SpawnAssets(parsedData.hotspots, parsedData.initial_state?.ui?.active_hotspots);
        }
    }

    void SpawnAssets(List<HotspotData> hotspotsToLoad, List<string> activeHotspotIds)
    {
        if (activeHotspotIds == null)
        {
            Debug.LogWarning("No active_hotspots list found in JSON.");
            return;
        }

        foreach (HotspotData hs in hotspotsToLoad)
        {
            // Skip hotspots that are not active in initial_state.ui.active_hotspots
            if (!activeHotspotIds.Contains(hs.id))
            {
                Debug.Log($"Skipping inactive hotspot: {hs.id}");
                continue;
            }

            GameObject prefab = Resources.Load<GameObject>("Hotspots/" + hs.id);

            if (prefab == null)
            {
                Debug.LogWarning($"Missing Prefab: Create a prefab named '{hs.id}' inside Resources/Hotspots.");
                continue;
            }

            GameObject anchor = GameObject.Find("Anchor_" + hs.id);

            if (anchor == null)
            {
                Debug.LogWarning($"Missing Anchor: Create an empty object named 'Anchor_{hs.id}' in your scene.");
                continue;
            }

            GameObject spawnedObject = Instantiate(
                prefab,
                anchor.transform.position,
                anchor.transform.rotation
            );

            spawnedObject.name = "Active_" + hs.id;
        }
    }
}