using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReminderRecordNameComparer : IComparer<ReminderRecord>
{
    public ReminderRecordNameComparer()
    { }
    public int Compare(ReminderRecord r1, ReminderRecord r2)
    {
        return r1.reminderTime.CompareTo(r2.reminderTime);
    }
}

public class Caregiver_AddReminder_Controller : MonoBehaviour
{
    public Dropdown hour_DP, minute_DP, am_pm_DP;

    List<string> hour_options, minute_options, am_pm_options;

    public InputField reminderContent;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    public void Awake()
    {
        hour_options = new List<string>();
        minute_options = new List<string>();
        for (int i = 1 % 12 ; i <= 12; i++)
        {
            if (i == 0) i = 12;
            hour_options.Add(i.ToString("D2"));
        }
        for (int i = 0; i < 60; i++)
        {
           minute_options.Add(i.ToString("D2"));

        }
        am_pm_options = new List<string>() { "AM", "PM" };

        hour_DP.AddOptions(hour_options);
        minute_DP.AddOptions(minute_options);
        am_pm_DP.AddOptions(am_pm_options);


    }

    public async void AddReminder_Callback()
    {
        AppManager.instance.updatingMongoDB = true;

        

        if (reminderContent.text!="")
        {
            if (reminderContent.text[0]==' ')
            {
                //no allow space at first letter
                //pop out msg to warn
                msg_panel.SetActive(true);
                msg_title.text = "Add Reminder Failed";
                msg_description.text = "Please don't put\"SPACE\" at first letter";
            }
            else
            {
                string getHourValue = hour_DP.options[hour_DP.value].text;
                string getMinuteValue = minute_DP.options[minute_DP.value].text;
                string getAMPMValue = am_pm_DP.options[am_pm_DP.value].text;

                ReminderRecord rr = new ReminderRecord(reminderContent.text,  int.Parse(getHourValue), int.Parse(getMinuteValue), getAMPMValue);

                if (AppManager.instance.currentUser.reminderRecord == null)
                    AppManager.instance.currentUser.reminderRecord = new List<ReminderRecord>();

                var temp = AppManager.instance.currentUser;
                AppManager.instance.currentUser.reminderRecord.Add(rr);

                AppManager.instance.currentUser.reminderRecord.Sort(new ReminderRecordNameComparer());

                try
                {
                    sendingDataPanel.SetActive(true);
                    InteractableInput(false);
                    await Task.Run(() => MongoUpdateReminderRecord());

                    //add the reminder
                    SSTools.ShowMessage("Add Reminder Success", SSTools.Position.bottom, SSTools.Time.twoSecond);
                }
                catch(Exception e)
                {
                    AppManager.instance.currentUser.reminderRecord = temp.reminderRecord;
                    //add the reminder

                    SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);
                }

            }

        }
        else
        {
            //pop out msg to warn
            msg_panel.SetActive(true);
            msg_title.text = "Add Reminder Failed";
            msg_description.text = "Please fill in the reminder...";
        }
        sendingDataPanel.SetActive(false);
        InteractableInput(true);
        AppManager.instance.updatingMongoDB = false;
    }

    private async Task MongoUpdateReminderRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.reminderRecord, AppManager.instance.currentUser.reminderRecord));

    }
    private void InteractableInput(bool inter)
    {
        foreach (var b in FindObjectsOfType<Button>())
        {
            b.interactable = inter;
        }
        foreach (var i in FindObjectsOfType<InputField>())
        {
            i.interactable = inter;
        }

    }
}
