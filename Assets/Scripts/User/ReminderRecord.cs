using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ReminderRecord
{
    public string reminder_content;
    public int reminder_time_hour;
    public int reminder_time_minute;
    public string reminder_am_pm;
    public DateTime reminderTime;
    public DateTime createdOn;

    public ReminderRecord(string t, int h, int m, string am_pm )
    {
        reminder_content = t;
        reminder_time_hour = h;
        reminder_time_minute = m;
        reminder_am_pm = am_pm.ToUpper();

        string timeString = reminder_time_hour + ":" + reminder_time_minute + " " + reminder_am_pm;

        reminderTime = DateTime.Parse(timeString);
        reminderTime = DateTime.SpecifyKind(reminderTime, DateTimeKind.Utc);



        createdOn = System.DateTime.Now;

        createdOn = DateTime.SpecifyKind(createdOn, DateTimeKind.Utc);

        //if the current time of hour is greater than the reminder time, mean it is for tomorrow reminder
        if (reminderTime.Hour<createdOn.Hour)
        {
            reminderTime.AddDays(1);
        }
        //same cases
        else if (reminderTime.Hour==createdOn.Hour&&reminderTime.Minute<createdOn.Minute)
        {
            reminderTime.AddDays(1);
        }
        

        Debug.Log("R: "+reminderTime.ToShortTimeString());
    }
}
