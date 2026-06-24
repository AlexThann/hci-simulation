using TMPro;
using UnityEngine;

public class VentilatorPanelController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text fio2Text;

    [Header("Managers")]
    public HUDController hudController;
    public ToastManager toastManager;
    public LogManager logManager;

    private int fio2 = 21;

    public void IncreaseFiO2()
    {
        fio2 += 10;

        if (fio2 > 100)
            fio2 = 100;

        UpdateVentilatorUI();

        hudController.UpdateVitals(100, 94, 20, "120/78", 36.9f);
        toastManager.ShowToast("FiO2 increased to " + fio2 + "%");
        logManager.Log("VENTILATOR_UPDATE", "FiO2 increased to " + fio2 + "%");
    }

    public void DecreaseFiO2()
    {
        fio2 -= 10;

        if (fio2 < 21)
            fio2 = 21;

        UpdateVentilatorUI();

        hudController.UpdateVitals(120, 85, 26, "130/85", 37.2f);
        toastManager.ShowToast("FiO2 decreased to " + fio2 + "%");
        logManager.Log("VENTILATOR_UPDATE", "FiO2 decreased to " + fio2 + "%");
    }

    private void UpdateVentilatorUI()
    {
        fio2Text.text = "FiO2: " + fio2 + "%";
    }
}