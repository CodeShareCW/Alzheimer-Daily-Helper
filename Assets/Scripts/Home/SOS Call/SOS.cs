using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SOS : MonoBehaviour
{
    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenPhoneApp);
    }

    private void OpenPhoneApp()
    {
        if (AppManager.instance.currentUser.caregiver != null&& AppManager.instance.currentUser.caregiver.caregiverName!=""&& AppManager.instance.currentUser.caregiver.caregiverPhoneNo!="")
        {
            Application.OpenURL("tel://" + AppManager.instance.currentUser.caregiver.caregiverPhoneNo);
        }
        else
        {
            msg_panel.SetActive(true);
            msg_title.text = "SOS Failed";
            msg_description.text = "Please ask your caregiver to update \"Caregiver\" section profile...";
        }
    }
}
