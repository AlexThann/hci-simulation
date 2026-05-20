using UnityEngine;

public class GateValidator : MonoBehaviour
{
    public EHRFormManager ehrFormManager;
    public ToastManager toastManager;
    public LogManager logManager;

    public void CheckGate()
    {
        bool hasObservation = ehrFormManager.HasField("observation");
        bool hasFio2 = ehrFormManager.HasField("fiO2_setting");

        if (!hasObservation || !hasFio2)
        {
            toastManager.ShowToast("Συμπλήρωσε Observation και FiO2.");

            logManager.Log(
                "GATE_BLOCKED",
                "Missing required EHR fields"
            );

            Debug.Log("Gate blocked");
            return;
        }

        toastManager.ShowToast("Gate passed.");

        logManager.Log(
            "GATE_PASSED",
            "Required EHR fields completed"
        );

        Debug.Log("Gate passed");
    }
}