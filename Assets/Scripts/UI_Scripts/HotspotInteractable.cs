using UnityEngine;

public enum HotspotType
{
    EHR,
    Monitor,
    Patient,
    Ventilator,
    CallSystem
}

public class HotspotInteractable : MonoBehaviour
{
    public HotspotType hotspotType;
}