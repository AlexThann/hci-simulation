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
    public GameObject crosshair;

    [Header("Player Control")]
    public MonoBehaviour cameraLookScript;


    public bool isUIOpen = false;


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
        crosshair.SetActive(true);
        EnableGameCursor();
    }

    public void OpenEHR()
    {
        CloseAllPanels();
        ehrPanel.SetActive(true);
        crosshair.SetActive(false);
        EnableUICursor();
    }

    public void OpenVitals()
    {
        CloseAllPanels();
        vitalsMonitorPanel.SetActive(true);
        crosshair.SetActive(false);
        EnableUICursor();
    }

    public void OpenPatient()
    {
        CloseAllPanels();
        patientPanel.SetActive(true);
        crosshair.SetActive(false);
        EnableUICursor();
    }

    public void OpenVentilator()
    {
        CloseAllPanels();
        ventilatorPanel.SetActive(true);
        crosshair.SetActive(false);
        EnableUICursor();
    }

    public void OpenCallSystem()
    {
        CloseAllPanels();
        callSystemPanel.SetActive(true);
        crosshair.SetActive(false);
        EnableUICursor();
    }

    public void OpenDebrief()
    {
        CloseAllPanels();
        debriefPanel.SetActive(true);
        crosshair.SetActive(false);
        EnableUICursor();
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
        EnableGameCursor();
    }

    private void EnableUICursor()
    {   
        isUIOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnableGameCursor()
    {
        isUIOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}