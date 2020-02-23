using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReminderOkBtn : MonoBehaviour
{
    public GameObject reminderPanel;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(CloseReminderPanel);
    }

    private void CloseReminderPanel()
    {
        Destroy(reminderPanel.gameObject);
    }
}
