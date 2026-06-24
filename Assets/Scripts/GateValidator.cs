using UnityEngine;

public class GateValidator : MonoBehaviour
{
    public EHRFormManager ehrFormManager;
    public ToastManager toastManager;
    public LogManager logManager;

    public void CheckFirstDocumentationGate()
    {
        bool hasObservation = ehrFormManager.HasField("observation");
        bool hasFio2 = ehrFormManager.HasField("fiO2_setting");

        if (!hasObservation || !hasFio2)
        {
            toastManager.ShowToast("Missing required EHR fields: observation and FiO2.");
            logManager.Log("GATE_BLOCKED", "Missing observation or FiO2 documentation");
            return;
        }

        toastManager.ShowToast("Documentation gate passed.");
        logManager.Log("GATE_PASSED", "Observation and FiO2 documentation completed");
    }

    public void CheckCommunicationGate()
    {
        bool hasRecipient = ehrFormManager.HasField("recipient");
        bool hasOutcome = ehrFormManager.HasField("outcome");

        if (!hasRecipient || !hasOutcome)
        {
            toastManager.ShowToast("Missing communication log fields: recipient and outcome.");
            logManager.Log("GATE_BLOCKED", "Missing recipient or outcome");
            return;
        }

        toastManager.ShowToast("Communication gate passed.");
        logManager.Log("GATE_PASSED", "Communication documentation completed");
    }
}