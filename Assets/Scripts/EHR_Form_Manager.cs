using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class EHRFormManager : MonoBehaviour
{
    public TMP_InputField observationInput;
    public TMP_InputField fio2Input;
    public ToastManager toastManager;
    public LogManager logManager;

    private Dictionary<string, string> ehrData = new Dictionary<string, string>();

    public void SaveEHR()
    {
        ehrData["observation"] = observationInput.text.Trim();
        ehrData["fiO2_setting"] = fio2Input.text.Trim();

        toastManager.ShowToast("EHR saved.");

        logManager.Log(
            "EHR_SUBMIT",
            "Assessment/Intervention form saved"
        );

        Debug.Log("EHR saved");
    }

    public bool HasField(string fieldName)
    {
        return ehrData.ContainsKey(fieldName)
            && !string.IsNullOrWhiteSpace(ehrData[fieldName]);
    }
}