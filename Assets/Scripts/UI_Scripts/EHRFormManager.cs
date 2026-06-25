using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EHRFormManager : MonoBehaviour
{
    [Header("Assessment Form")]
    public TMP_InputField observationInput;
    public TMP_Dropdown skinColorInput;
    public TMP_Dropdown consciousnessInput;

    [Header("Intervention Form")]
    public TMP_Dropdown deviceInput;
    public TMP_InputField fio2Input;
    public TMP_InputField flowRateInput;

    [Header("Communication Log")]
    public TMP_Dropdown recipientInput;
    public TMP_InputField reasonInput;
    public TMP_InputField outcomeInput;

    [Header("Managers")]
    public ToastManager toastManager;
    public LogManager logManager;

    private Dictionary<string, string> ehrData = new Dictionary<string, string>();


    public void SaveEHRFormAssessment(){
        SaveField("observation", observationInput);
        SaveField("skin_color", skinColorInput);
        SaveField("consciousness", consciousnessInput);
        toastManager.ShowToast("EHR assessment form saved.");
        logManager.Log("EHR_SUBMIT", "EHR documentation assessment saved");
    }


    public void SaveEHRFormIntervention(){
        SaveField("device", deviceInput);
        SaveField("fiO2_setting", fio2Input);
        SaveField("flow_rate", flowRateInput);
        toastManager.ShowToast("EHR intervention form saved.");
        logManager.Log("EHR_SUBMIT", "EHR documentation intervention saved");
    }


    public void SaveEHRFormCommunication(){
        SaveField("recipient", recipientInput);
        SaveField("reason", reasonInput);
        SaveField("outcome", outcomeInput);
        toastManager.ShowToast("EHR communication form saved.");
        logManager.Log("EHR_SUBMIT", "EHR documentation communication saved");
    }

    private void SaveField(string key, TMP_InputField input)
    {
        if (input == null) return;
        ehrData[key] = input.text.Trim();
    }

    private void SaveField(string key, TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;
        ehrData[key] = dropdown.options[dropdown.value].text.Trim();
    }

    public bool HasField(string key)
    {
        return ehrData.ContainsKey(key)
            && !string.IsNullOrWhiteSpace(ehrData[key]);
    }

    public string GetField(string key)
    {
        if (!ehrData.ContainsKey(key))
            return "";

        return ehrData[key];
    }
}