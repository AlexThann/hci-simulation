using UnityEngine;
using TMPro;

public class CallButtonManager : MonoBehaviour
{
    [Header("Call Button")]
    public TMP_Text callButton;

    [Header("Toast Manager")]
    public ToastManager toastManager;
    [Header("Log Manager")]
    public LogManager logManager;

    public void callDoctor(){
        toastManager.ShowToast("Έγινε κλήση στον εφημερεύοντα.");
        logManager.Log("EHR_CALL_BUTTON","Called Doctor");
    }

}
