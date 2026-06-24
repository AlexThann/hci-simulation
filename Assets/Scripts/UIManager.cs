using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject ehrPanel;
    public GameObject toastPanel;

    public GameObject vitalsMonitorPanel;
    public GameObject patientPanel;
    public GameObject ventilatorPanel;
    public GameObject callSystemPanel;
    public GameObject debriefPanel;

    void Start()
    {
        CloseAllPanels();
    }

    public void CloseAllPanels()
    {
        ehrPanel.SetActive(false);
        vitalsMonitorPanel.SetActive(false);
        patientPanel.SetActive(false);
        ventilatorPanel.SetActive(false);
        callSystemPanel.SetActive(false);
        debriefPanel.SetActive(false);
    }

    public void OpenEHR()
    {
        CloseAllPanels();
        ehrPanel.SetActive(true);
    }

    public void OpenVitals()
    {
        CloseAllPanels();
        vitalsMonitorPanel.SetActive(true);
    }

    public void OpenPatient()
    {
        CloseAllPanels();
        patientPanel.SetActive(true);
    }

    public void OpenVentilator()
    {
        CloseAllPanels();
        ventilatorPanel.SetActive(true);
    }

    public void OpenCallSystem()
    {
        CloseAllPanels();
        callSystemPanel.SetActive(true);
    }

    public void OpenDebrief()
    {
        CloseAllPanels();
        debriefPanel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }
}