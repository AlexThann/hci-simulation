using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

// --- JSON DATA FILTERS ---
// The parser uses these to ignore everything except the hotspots.
public class ScenarioData
{
    public List<HotspotData> hotspots;
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
            SpawnAssets(parsedData.hotspots);
        }
    }

    void SpawnAssets(List<HotspotData> hotspotsToLoad)
    {
        foreach (HotspotData hs in hotspotsToLoad)
        {
            // Look for the physical prefab in your Resources folder
            GameObject prefab = Resources.Load<GameObject>("Hotspots/" + hs.id);

            if (prefab == null)
            {
                Debug.LogWarning($"Missing Prefab: Create a prefab named '{hs.id}' inside your Resources/Hotspots folder.");
                continue;
            }

            // Find where to place it in the room
            GameObject anchor = GameObject.Find("Anchor_" + hs.id);

            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (anchor != null)
            {
                spawnPosition = anchor.transform.position;
                spawnRotation = anchor.transform.rotation;
            }
            else
            {
                Debug.LogWarning($"Missing Anchor: Create an empty object named 'Anchor_{hs.id}' in your scene to place this object.");
            }

            // Spawn the object
            GameObject spawnedObject = Instantiate(prefab, spawnPosition, spawnRotation);
            spawnedObject.name = "Active_" + hs.id;
        }
    }
}